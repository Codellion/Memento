using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Reflection;
using Memento.Persistence.Commons;
using Memento.Persistence.Commons.Annotations;

namespace Memento.DataAccess.Utils
{
    public class DbUtil<T> where T : Entity
    {
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
            IList<string> transientProps = entidad.TransientProps;

            if(entidad.KeyGenerator == KeyGenerationType.Database)
            {
                transientProps.Add(entidad.GetEntityIdName());    
            }

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
                            var refValue = persistProp.PropertyType.GetProperty("Value").GetValue(value, null) as Entity;

                            if (refValue == null)
                            {
                                continue;
                            }

                            nomCol = refValue.GetEntityIdName();
                            object valueAux = refValue.GetEntityId();

                            if (valueAux == null)
                            {
                                continue;
                            }

                            value = valueAux;
                        }

                        nomCol = entidad.GetMappedProp(nomCol);

                        cols.Add(nomCol);


                        //En función del tipo construimos la clausula del where
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
                        else if (persistProp.PropertyType == typeof (float)
                                 || persistProp.PropertyType == typeof (float?))
                        {
                            values.Add(String.Format(" {0} ",
                                                     value.ToString().Replace(",", ".")));
                        }
                        else
                        {
                            values.Add(String.Format(" {0} ", value));
                        }
                    }
                }
            }

            var res = new Query(GetSepList(cols, ","),
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

            object id = entidad.GetEntityId();

            if (id == null)
            {
                throw new Exception("El objeto que se quiere actualizar no tiene identificador");
            }

            //Establecemos las propiedades base que no se persisten
            IList<string> transientProps = entidad.TransientProps;

            if (entidad.KeyGenerator == KeyGenerationType.Database)
            {
                transientProps.Add(entidad.GetEntityIdName());
            }

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
                            var refValue = persistProp.PropertyType.GetProperty("Value").GetValue(value, null) as Entity;

                            if (refValue == null)
                            {
                                continue;
                            }

                            nomCol = refValue.GetEntityIdName();
                            object valueAux = refValue.GetEntityId();

                            if (valueAux == null)
                            {
                                continue;
                            }

                            value = valueAux;
                        }

                        nomCol = entidad.GetMappedProp(nomCol);

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
                        else if (persistProp.PropertyType == typeof (float)
                                 || persistProp.PropertyType == typeof (float?))
                        {
                            cols.Add(String.Format(" {0} = {1} ",
                                                   nomCol,
                                                   value.ToString().Replace(",", ".")));
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

            var res = new Query(GetSepList(cols, ","),
                                entidad.Table,
                                String.Format(" {0}Id = {1}", tipoEntidad.Name, id));

            return res;
        }

        public static Query GetDelete(object entidadId)
        {
            Entity auxT = Activator.CreateInstance<T>();
            
            object id;

            if (entidadId is Entity)
            {
                id = (entidadId as Entity).GetEntityId();
            }
            else
            {
                id = entidadId;
            }

            Type tId = id.GetType();

            string @where = String.Format(tId == typeof (string) ? " {0} = '{1}' " : " {0} = {1} ",
                auxT.GetMappedProp(auxT.GetEntityIdName()), id);

            var query = new Query { Tables = auxT.Table, Filters = @where };

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
            IList<string> transientProps = entidad.TransientProps;

            IList<string> colProps = new List<string>();
            IList<string> tableProps = new List<string>();
            IList<string> filtProps = new List<string>();

            string sfrom = String.Format(" {0} {1} ",
                                         entidad.Table,
                                         "tableAux0");

            tableProps.Add(sfrom);

            //Establecemos el alias
            const string alias = "tableAux";
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
                    || persistProp.PropertyType.BaseType == typeof (LazyEntity))
                {
                    continue;
                }

                Object value = persistProp.GetValue(entidad, null);

                if (!entidad.References.Contains(persistProp.Name))
                {
                    string propName = entidad.GetMappedProp(persistProp.Name);

                    //Añadimos la columna
                    colProps.Add(string.Format("tableAux0.{0} [{1}]", propName, persistProp.Name));

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
                                                            propName,
                                                            value));
                            }
                            else
                            {
                                filtProps.Add(String.Format(" {0}.{1} = '{2}' ",
                                                            "tableAux0",
                                                            propName,
                                                            value));
                            }
                        }
                        else if (persistProp.PropertyType == typeof (bool)
                                 || persistProp.PropertyType == typeof (bool?))
                        {
                            filtProps.Add(String.Format(" {0}.{1} = {2} ",
                                                        "tableAux0",
                                                        propName,
                                                        ((bool) value) ? "1" : "0"));
                        }
                        else if (persistProp.PropertyType == typeof (float)
                                 || persistProp.PropertyType == typeof (float?))
                        {
                            filtProps.Add(String.Format(" {0}.{1} = {2} ",
                                                        "tableAux0",
                                                        propName,
                                                        value.ToString().Replace(",", ".")));
                        }
                        else
                        {
                            filtProps.Add(String.Format(" {0}.{1} = {2} ",
                                                        "tableAux0",
                                                        propName,
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

                        var aux = Activator.CreateInstance(propertyType) as Entity;

                        if(aux == null)
                        {
                            continue;
                        }

                        //Obtenemos el nombre de la tabla relacionada
                        var sTableNameAux = aux.Table;

                        string idCol = aux.GetMappedProp(aux.GetEntityIdName());

                        //Añadimos el join
                        tableProps.Add(String.Format(" {0} {1} on {2}.{3} = {4}.{5} ",
                                                     sTableNameAux,
                                                     aliasAux,
                                                     aliasAux,
                                                     idCol,
                                                     "tableAux0",
                                                     idCol));

                        if (value != null)
                        {
                            value = value.GetType().GetProperty("Value").GetValue(value, null);

                            var eValue = value as Entity;

                            if(eValue == null)
                            {
                                continue;
                            }

                            //Si la propiedad contiene un valor lo utilizamos como filtro de la Query                           
                            Object refId = eValue.GetEntityId();

                            if (refId != null)
                            {
                                Type pRefId = refId.GetType();
                                string idColFilter = eValue.GetMappedProp(eValue.GetEntityIdName());

                                if (pRefId == typeof (string))
                                {
                                    filtProps.Add(String.Format(" {0}.{1} = '{2}' ",
                                                                aliasAux,
                                                                idColFilter,
                                                                refId));
                                }
                                else
                                {
                                    filtProps.Add(String.Format(" {0}.{1} = {2} ",
                                                                aliasAux,
                                                                idColFilter,
                                                                refId));
                                }
                            }

                            //Si la propiedad es una Entidad a su vez de inspeccion internamente
                            //para satisfacer sus referencias internas
                            SetFilters(persistProp.Name, eValue, persistProp, aliasAux,
                                        ref colProps, ref tableProps, ref filtProps);
                        }
                        else
                        {
                            //Si la propiedad es una Entidad a su vez de inspeccion internamente
                            //para satisfacer sus referencias internas
                            SetFilters(persistProp.Name, aux, persistProp, aliasAux,
                                        ref colProps, ref tableProps, ref filtProps);
                        }
                    }
                }

                i++;
            }

            return new Query(GetSepList(colProps, " , "), GetSepList(tableProps, " join "),
                             GetSepList(filtProps, " and "));
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
        public static void SetFilters(String nivel, Entity entidad, PropertyInfo propEntidad, string actualAlias,
                                      ref IList<string> colProps, ref IList<string> tableProps,
                                      ref IList<string> filtProps)
        {
            Type tipoEntidad = propEntidad.PropertyType;

            if (tipoEntidad.BaseType == typeof (EaterEntity))
            {
                tipoEntidad = propEntidad.PropertyType.GetGenericArguments()[0];
            }

            string alias = actualAlias + "_";
            int i = 0;

            var referencias = entidad.References;

            IList<string> transientProps = entidad.TransientProps;

            foreach (PropertyInfo persistProp in tipoEntidad.GetProperties())
            {
                string aliasAux = alias + i;

                if (transientProps.Contains(persistProp.Name)
                    || persistProp.PropertyType.BaseType == typeof (LazyEntity))
                {
                    continue;
                }

                Object value = persistProp.GetValue(entidad, null);

                string colNom = entidad.GetMappedProp(persistProp.Name);

                if (!referencias.Contains(persistProp.Name))
                {
                    colProps.Add(String.Format("{0}.{1} [{2}.{3}]",
                                               actualAlias, colNom,
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
                                                            colNom,
                                                            value));
                            }
                            else
                            {
                                filtProps.Add(String.Format(" {0}.{1} = '{2}' ",
                                                            actualAlias,
                                                            colNom,
                                                            value));
                            }
                        }
                        else if (persistProp.PropertyType == typeof (bool)
                                 || persistProp.PropertyType == typeof (bool?))
                        {
                            filtProps.Add(String.Format(" {0}.{1} = {2} ",
                                                        actualAlias,
                                                        colNom,
                                                        ((bool) value) ? "1" : "0"));
                        }
                        else if (persistProp.PropertyType == typeof (float)
                                 || persistProp.PropertyType == typeof (float?))
                        {
                            filtProps.Add(String.Format(" {0}.{1} = {2} ",
                                                        actualAlias,
                                                        colNom,
                                                        value.ToString().Replace(",", ".")));
                        }
                        else
                        {
                            filtProps.Add(String.Format(" {0}.{1} = {2} ",
                                                        actualAlias,
                                                        colNom,
                                                        value.ToString().ToString(CultureInfo.InvariantCulture)));
                        }
                    }
                }
                else
                {
                    if (referencias.Contains(persistProp.Name))
                    {
                        Type propertyType = persistProp.PropertyType.GetGenericArguments()[0];

                        var aux = Activator.CreateInstance(propertyType) as Entity;

                        if (aux != null)
                        {
                            var sTableNameAux = aux.Table;
                            var idCol = aux.GetMappedProp(aux.GetEntityIdName());

                            tableProps.Add(String.Format(" {0} {1} on {2}.{3} = {4}.{5} ",
                                                         sTableNameAux,
                                                         aliasAux,
                                                         aliasAux,
                                                         idCol,
                                                         actualAlias,
                                                         idCol));
                        }

                        if (value != null)
                        {
                            value = value.GetType().GetProperty("Value").GetValue(value, null);

                            var eValue = value as Entity;

                            if (eValue != null)
                            {
                                Object refId = eValue.GetEntityId();

                                if (refId != null)
                                {
                                    if (refId is string)
                                    {
                                        filtProps.Add(String.Format(" {0}.{1} = '{2}' ",
                                                                    aliasAux,
                                                                    eValue.GetMappedProp(eValue.GetEntityIdName()),
                                                                    refId));
                                    }
                                    else
                                    {
                                        filtProps.Add(String.Format(" {0}.{1}Id = {2} ",
                                                                    aliasAux,
                                                                    eValue.GetMappedProp(eValue.GetEntityIdName()),
                                                                    refId));
                                    }
                                }
                            }

                            SetFilters(String.Format("{0}.{1}", nivel, persistProp.Name),
                                        eValue, persistProp, aliasAux,
                                        ref colProps, ref tableProps, ref filtProps);
                        }
                        else
                        {
                           
                            SetFilters(String.Format("{0}.{1}", nivel, persistProp.Name),
                                        aux, persistProp, aliasAux,
                                        ref colProps, ref tableProps, ref filtProps);
                        }
                    }
                }

                i++;
            }
        }

        #endregion
    }
}