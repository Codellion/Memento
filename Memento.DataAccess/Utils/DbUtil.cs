using System;
using System.Collections.Generic;
using System.Data;
using System.Reflection;

using Memento.Persistence.Commons;

namespace Memento.DataAccess.Utils
{
    public class DbUtil<T> where T: Entity {

        #region Constantes

        /// <summary>
        /// Prefijo que llevan las propiedades que queremos filtrar con LIKE en lugar de =
        /// </summary>
        public const string PrefixLike = "#like#";

        #endregion

        #region Metodos privados
        
        /// <summary>
        /// Devuelve el tipo de dato de SQL Server equivalente al del framework .Net
        /// </summary>
        /// <param name="tipo">Tipo de datos del framework</param>
        /// <returns>Tipo de dato equivalente de SQL Server</returns>
        public static SqlDbType GetDbType(Type tipo)
        {
            return ConvertDbType.ToSqlDbType(tipo);
        }
        
        /// <summary>
        /// Devuelve una lista de parámetros SQL con las propiedades de
        /// primer nivel que se persisten en una entidad
        /// </summary>
        /// <param name="entidad">Entidad</param>
        /// <returns>Lista de parámetros SQL</returns>
        public static Query GetInsert(T entidad)
        {
            Type tipoEntidad = entidad.GetType();
       
            //Establecemos las propiedades base que no se persisten
            IList<string> transientProps = new List<string>();
            transientProps.Add("Table");
            transientProps.Add("References");
            transientProps.Add(tipoEntidad.Name + "Id");

            IList<string> cols = new List<string>();
            IList<string> values = new List<string>();

            foreach (PropertyInfo persistProp in tipoEntidad.GetProperties())
            {
                object value = persistProp.GetValue(entidad, null);

                if (value != null)
                {
                    if (!(value is LazyEntity) && !transientProps.Contains(persistProp.Name))
                    {
                        string nomCol = persistProp.Name;

                        if (entidad.References.Contains(persistProp.Name))
                        {
                            //Comprobamos que si es una entidad referenciada esta contenga el id
                            nomCol = persistProp.PropertyType.Name + "Id";
                            PropertyInfo pId = persistProp.PropertyType.GetProperty(nomCol);
                            object valueAux = pId.GetValue(value, null);

                            if (valueAux == null)
                            {
                                continue;
                            }

                            value = valueAux;
                        }
                        cols.Add(nomCol);

                        if (persistProp.PropertyType == typeof (string)
                            || persistProp.PropertyType == typeof (DateTime)
                            || persistProp.PropertyType == typeof (DateTime?))
                        {
                            values.Add(String.Format(" '{0}' ",
                                                     value));
                        }
                        else if (persistProp.PropertyType == typeof (bool)
                                 || persistProp.PropertyType == typeof (bool?))
                        {
                            values.Add(String.Format(" {0} ",
                                                     ((bool) value) ? "1" : "0"));
                        }
                        else
                        {
                            values.Add(String.Format(" {0} ",
                                                     value));
                        }

                    }
                }
            }

            Query res = new Query(GetSepList(cols, ","),
                                  GetSepList(values, ","),
                                  entidad.Table, null);

            return res;
        }

        /// <summary>
        /// Devuelve una lista de parámetros SQL con las propiedades de
        /// primer nivel que se persisten en una entidad
        /// </summary>
        /// <param name="entidad">Entidad</param>
        /// <returns>Lista de parámetros SQL</returns>
        public static Query GetUpdate(T entidad)
        {
            Type tipoEntidad = entidad.GetType();

            PropertyInfo idInfo = tipoEntidad.GetProperty(tipoEntidad.Name + "Id");
            object id = idInfo.GetValue(entidad, null);

            if (id == null)
            {
                throw new Exception("El objeto que se quiere actualizar no tiene identificador");
            }

            //Establecemos las propiedades base que no se persisten
            IList<string> transientProps = new List<string>();
            transientProps.Add("Table");
            transientProps.Add("References");
            transientProps.Add(tipoEntidad.Name + "Id");
            
            IList<string> cols = new List<string>();

            foreach (PropertyInfo persistProp in tipoEntidad.GetProperties())
            {
                object value = persistProp.GetValue(entidad, null);

                if (value != null)
                {
                    if (!(value is LazyEntity) && !transientProps.Contains(persistProp.Name))
                    {
                        string nomCol = persistProp.Name;

                        if (entidad.References.Contains(persistProp.Name))
                        {
                            //Comprobamos que si es una entidad referenciada esta contenga el id
                            nomCol = persistProp.PropertyType.Name + "Id";
                            PropertyInfo pId = persistProp.PropertyType.GetProperty(nomCol);
                            object valueAux = pId.GetValue(value, null);

                            if (valueAux == null)
                            {
                                continue;
                            }

                            value = valueAux;
                        }

                        if (persistProp.PropertyType == typeof (string)
                            || persistProp.PropertyType == typeof (DateTime)
                            || persistProp.PropertyType == typeof (DateTime?))
                        {
                            cols.Add(String.Format(" {0} = '{1}' ",
                                                   nomCol,
                                                   value));
                        }
                        else if (persistProp.PropertyType == typeof (bool)
                                 || persistProp.PropertyType == typeof (bool?))
                        {
                            cols.Add(String.Format(" {0} = {1} ",
                                                   nomCol,
                                                   ((bool) value) ? "1" : "0"));
                        }
                        else
                        {
                            cols.Add(String.Format(" {0} = {1} ",
                                                   nomCol,
                                                   value));
                        }
                    }
                }
            }

            Query res = new Query(GetSepList(cols, ","),
                                  entidad.Table,
                                  String.Format(" {0}Id = {1}", tipoEntidad.Name, id));

            return res;
        }

        public static Query GetDelete(object entidadId)
        {
            Type tipoT = typeof (T);
            Type tId = tipoT.GetProperty(tipoT.Name + "Id").PropertyType;

            string where;

            if (tId == typeof (string))
            {
                where = String.Format(" {0}Id = '{1}' ",
                                      tipoT.Name,
                                      entidadId);
            }
            else
            {
                where = String.Format(" {0}Id = {1} ",
                                      tipoT.Name,
                                      entidadId);
            }

            PropertyInfo pTableName = tipoT.GetProperty("Table");
            string sTableName = (string) pTableName.GetValue(Activator.CreateInstance<T>(), null);

            Query query = new Query();
            query.Tables = sTableName;
            query.Filters = where;

            return query;
        }

        /// <summary>
        /// Devuelve una cadena con elementos separados por un separador indicado a
        /// partir de una lista
        /// </summary>
        /// <param name="lista">Lista de elementos</param>
        /// <param name="sep">Separador</param>
        /// <returns>Cadena con los elementos separados</returns>
        public static string GetSepList(IList<string> lista, String sep)
        {
            string res = String.Empty;

            if (lista.Count > 0)
            {
                int count = 1;

                foreach (string cadena in lista)
                {
                    if (lista.Count > count)
                    {
                        res += cadena + sep;
                    }
                    else
                    {
                        res += cadena;
                    }

                    count++;
                }

            }

            return res;
        }

        /// <summary>
        /// Devuelve una Query a partir de una entidad utilizando como
        /// filtros las propiedades informadas en la misma
        /// </summary>
        /// <param name="entidad">Entidad utilizada para crear la Query</param>
        /// <returns>Query de la entidad</returns>
        public static Query GetQuery(T entidad)
        {
            Type tipoEntidad = entidad.GetType();
      
            //Establecemos las propiedades base que no se persisten
            IList<string> transientProps = new List<string>();
            transientProps.Add("Table");
            transientProps.Add("References");

            IList<string> colProps = new List<string>();
            IList<string> tableProps = new List<string>();
            IList<string> filtProps = new List<string>();
            
            string sfrom = String.Format(" {0} {1} ",
                                         entidad.Table,
                                         "tableAux0");

            tableProps.Add(sfrom);

            //Establecemos el alias
            string alias = "tableAux";
            int i = 1;

            //Inspeccionamos cada propiedad de la entidad para construir las
            //columnas, joins y filtros que contenga en función de su tipología
            foreach (PropertyInfo persistProp in tipoEntidad.GetProperties())
            {
                string aliasAux = alias + i;

                //Si el tipo de la propiedad es Dependencia<T> no hacemos nada
                //ya que dicho tipo de dato ya se encarga automáticamente de realizar 
                //sus consultas
                if (transientProps.Contains(persistProp.Name)
                    || persistProp.PropertyType.BaseType == typeof(LazyEntity))
                {
                    continue;
                }

                Object value = persistProp.GetValue(entidad, null);

                if (!entidad.References.Contains(persistProp.Name))
                {
                    //Añadimos la columna
                    colProps.Add("tableAux0." + persistProp.Name);

                    //Si la propiedad contiene un valor lo incluimos como filtro
                    if (value != null)
                    {
                        if (persistProp.PropertyType == typeof (string)
                            || persistProp.PropertyType == typeof (DateTime)
                            || persistProp.PropertyType == typeof (DateTime?))
                        {
                            if (value.ToString().StartsWith(PrefixLike))
                            {
                                value = value.ToString().Replace(PrefixLike, String.Empty);

                                filtProps.Add(String.Format(" {0}.{1} LIKE '{2}' ",
                                                            "tableAux0",
                                                            persistProp.Name,
                                                            value));
                            }
                            else
                            {
                                filtProps.Add(String.Format(" {0}.{1} = '{2}' ",
                                                            "tableAux0",
                                                            persistProp.Name,
                                                            value));
                            }
                        }
                        else if (persistProp.PropertyType == typeof (bool)
                                 || persistProp.PropertyType == typeof (bool?))
                        {
                            filtProps.Add(String.Format(" {0}.{1} = {2} ",
                                                        "tableAux0",
                                                        persistProp.Name,
                                                        ((bool) value) ? "1" : "0"));
                        }
                        else
                        {
                            filtProps.Add(String.Format(" {0}.{1} = {2} ",
                                                        "tableAux0",
                                                        persistProp.Name,
                                                        value));
                        }
                    }
                }
                else
                {
                    //Si la propiedad es una referencia es necesario inspeccionarla
                    //más detalladamente debido a que puede contener otras referencias internas
                    if (entidad.References.Contains(persistProp.Name))
                    {
                        Type propertyType = persistProp.PropertyType.GetGenericArguments()[0];

                        Object aux = Activator.CreateInstance(propertyType);

                        //Obtenemos el nombre de la tabla relacionada
                        PropertyInfo pTableNameAux = propertyType.GetProperty("Table");
                        string sTableNameAux = (string) pTableNameAux.GetValue(aux, null);

                        //Añadimos el join
                        tableProps.Add(String.Format(" {0} {1} on {2}.{3}Id = {4}.{5}Id ",
                                                     sTableNameAux,
                                                     aliasAux,
                                                     aliasAux,
                                                     propertyType.Name,
                                                     "tableAux0",
                                                     propertyType.Name));

                        if (value != null)
                        {
                            value = value.GetType().GetProperty("Value").GetValue(value, null);

                            //Si la propiedad contiene un valor lo utilizamos como filtro de la Query                           

                            PropertyInfo pRefId = propertyType.GetProperty(propertyType.Name + "Id");
                            Object refId = pRefId.GetValue(value, null);

                            if (refId != null)
                            {
                                if (pRefId.PropertyType == typeof (string))
                                {
                                    filtProps.Add(String.Format(" {0}.{1}Id = '{2}' ",
                                                                aliasAux,
                                                                propertyType.Name,
                                                                refId));
                                }
                                else
                                {
                                    filtProps.Add(String.Format(" {0}.{1}Id = {2} ",
                                                                aliasAux,
                                                                propertyType.Name,
                                                                refId));
                                }
                            }

                            //Si la propiedad es una Entidad a su vez de inspeccion internamente
                            //para satisfacer sus referencias internas
                            if (value is Entity)
                            {
                                SetFilters(persistProp.Name, value, persistProp, aliasAux,
                                           ref colProps, ref tableProps, ref filtProps);
                            }
                        }
                        else
                        {
                            //Si la propiedad es una Entidad a su vez de inspeccion internamente
                            //para satisfacer sus referencias internas
                            if (aux is Entity)
                            {
                                SetFilters(persistProp.Name, aux, persistProp, aliasAux,
                                           ref colProps, ref tableProps, ref filtProps);
                            }
                        }
                    }
                }

                i++;
            }

            return new Query(GetSepList(colProps, " , "), GetSepList(tableProps, " join "), GetSepList(filtProps, " and "));
        }

        /// <summary>
        /// Genera las clausulas where de una Query de forma recursiva
        /// </summary>
        /// <param name="nivel">Nivel actual de la recursión</param>
        /// <param name="entidad">Entidad de la que se van a extrar los filtros</param>
        /// <param name="propEntidad">Propiedad que representan a la entidad</param>
        /// <param name="actualAlias">Alias de la tabla de la entidad</param>
        /// <param name="colProps">Columnas</param>
        /// <param name="tableProps">Tablas</param>
        /// <param name="filtProps">Filtros</param>
        public static void SetFilters(String nivel, Object entidad, PropertyInfo propEntidad, string actualAlias,
                                      ref IList<string> colProps, ref IList<string> tableProps, ref IList<string> filtProps)
        {
            Type tipoEntidad = propEntidad.PropertyType;

            if (tipoEntidad.BaseType == typeof(EaterEntity))
            {
                tipoEntidad = propEntidad.PropertyType.GetGenericArguments()[0];
            }

            string alias = actualAlias + "_";
            int i = 0;

            PropertyInfo refsInfo = tipoEntidad.GetProperty("References");

            IList<string> referencias = (IList<string>) refsInfo.GetValue(entidad, null);

            IList<string> transientProps = new List<string>();
            transientProps.Add("Table");
            transientProps.Add("References");

            foreach (PropertyInfo persistProp in tipoEntidad.GetProperties())
            {
                string aliasAux = alias + i;

                if (transientProps.Contains(persistProp.Name)
                    || persistProp.PropertyType.BaseType == typeof(LazyEntity))
                {
                    continue;
                }

                Object value = persistProp.GetValue(entidad, null);

                if (!referencias.Contains(persistProp.Name))
                {
                    colProps.Add(String.Format("{0}.{1} [{2}.{3}]",
                                               actualAlias, persistProp.Name,
                                               nivel, persistProp.Name));

                    if (value != null)
                    {
                        if (persistProp.PropertyType == typeof (string)
                            || persistProp.PropertyType == typeof (DateTime)
                            || persistProp.PropertyType == typeof (DateTime?))
                        {
                            if (value.ToString().StartsWith(PrefixLike))
                            {
                                value = value.ToString().Replace(PrefixLike, String.Empty);

                                filtProps.Add(String.Format(" {0}.{1} LIKE '{2}' ",
                                                            actualAlias,
                                                            persistProp.Name,
                                                            value));
                            }
                            else
                            {
                                filtProps.Add(String.Format(" {0}.{1} = '{2}' ",
                                                            actualAlias,
                                                            persistProp.Name,
                                                            value));
                            }
                        }
                        else if (persistProp.PropertyType == typeof (bool)
                                 || persistProp.PropertyType == typeof (bool?))
                        {
                            filtProps.Add(String.Format(" {0}.{1} = {2} ",
                                                        actualAlias,
                                                        persistProp.Name,
                                                        ((bool) value) ? "1" : "0"));
                        }
                        else
                        {
                            filtProps.Add(String.Format(" {0}.{1} = {2} ",
                                                        actualAlias,
                                                        persistProp.Name,
                                                        value));
                        }
                    }
                }
                else
                {
                    if (referencias.Contains(persistProp.Name))
                    {
                        Type propertyType = persistProp.PropertyType.GetGenericArguments()[0];

                        Object aux = Activator.CreateInstance(propertyType);

                        PropertyInfo pTableNameAux = propertyType.GetProperty("Table");
                        string sTableNameAux = (string) pTableNameAux.GetValue(aux, null);

                        if (!(aux is Entity))
                        {
                            colProps.Add(String.Format("{0}.{1} [{2}.{3}]",
                                                       actualAlias, persistProp.Name,
                                                       nivel, persistProp.Name));
                        }

                        tableProps.Add(String.Format(" {0} {1} on {2}.{3}Id = {4}.{5}Id ",
                                                     sTableNameAux,
                                                     aliasAux,
                                                     aliasAux,
                                                     propertyType.Name,
                                                     actualAlias,
                                                     propertyType.Name));

                        if (value != null)
                        {
                            value = value.GetType().GetProperty("Value").GetValue(value, null);

                            PropertyInfo pRefId = propertyType.GetProperty(propertyType.Name + "Id");
                            Object refId = pRefId.GetValue(value, null);

                            if (refId != null)
                            {
                                if (pRefId.PropertyType == typeof (string))
                                {
                                    filtProps.Add(String.Format(" {0}.{1}Id = '{2}' ",
                                                                aliasAux,
                                                                propertyType.Name,
                                                                refId));
                                }
                                else
                                {
                                    filtProps.Add(String.Format(" {0}.{1}Id = {2} ",
                                                                aliasAux,
                                                                propertyType.Name,
                                                                refId));
                                }
                            }
                            if (value is Entity)
                            {
                                SetFilters(String.Format("{0}.{1}", nivel, persistProp.Name),
                                           value, persistProp, aliasAux,
                                           ref colProps, ref tableProps, ref filtProps);
                            }
                        }
                        else
                        {
                            if (aux is Entity)
                            {
                                SetFilters(String.Format("{0}.{1}", nivel, persistProp.Name),
                                           aux, persistProp, aliasAux,
                                           ref colProps, ref tableProps, ref filtProps);
                            }
                        }
                    }
                }

                i++;
            }
        }

        #endregion
    }   
}   
