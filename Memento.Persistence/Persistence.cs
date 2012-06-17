using System;
using System.Collections.Generic;
using System.Data;
using System.Reflection;

using Memento.DataAccess;
using Memento.DataAccess.Interfaces;
using Memento.Persistence.Commons;
using Memento.Persistence.Interfaces;

namespace Memento.Persistence
{
    /// <summary>
    /// Clase que actua de fachada del módulo de persistencia
    /// </summary>
    /// <typeparam name="T">Tipo de dato sobre el que se creará la factoría</typeparam>
    public class Persistence<T> : IPersistence<T> where T: Entity
    {
        #region Constantes

        /// <summary>
        /// Prefijo que llevan las propiedades que queremos filtrar con LIKE en lugar de =
        /// </summary>
        public const string PrefixLike = "#like#";

        #endregion

        #region Atributos

        /// <summary>
        /// Atributo privado del servicio de datos de persistencia
        /// </summary>
        private IDataPersistence<T> _persistenciaDatos;

        private DataContext _contexto;

        #endregion

        #region Propiedades

        /// <summary>
        /// Propiedad del servicio de datos de persistencia donde
        /// se llama al Proveedor de servicio de persistencia para que 
        /// devuelva una implementación de dicha interfaz
        /// </summary>
        public IDataPersistence<T> PersistenciaDatos
        {
            get
            {
                if(_persistenciaDatos == null)
                {
                    if(_contexto != null)
                    {
                        _persistenciaDatos = DataFactoryProvider.GetProvider<T>(_contexto.Transaccion);
                    }
                    else
                    {
                        _persistenciaDatos = DataFactoryProvider.GetProvider<T>(null);
                    }
                }

                return _persistenciaDatos;
            }
            set { _persistenciaDatos = value; }
        }

        #endregion

        #region Constructores

        /// <summary>
        /// Crea una instancia del módulo de persistencia con los valores por defecto
        /// </summary>
        public Persistence()
        {

        }

        /// <summary>
        /// Crea una instancia del módulo de persistencia dentro del contexto pasado
        /// como parámetro
        /// </summary>
        /// <param name="contexto">Contexto transaccional</param>
        public Persistence(DataContext contexto)
        {
            _contexto = contexto;
        }

        #endregion

        #region Implementación de la interfaz

        /// <summary>
        /// Da de alta una entidad en BBDD
        /// </summary>
        /// <param name="entity">Entidad que se dará de alta</param>
        /// <returns>Entidad con el identificador de BBDD informado</returns>
        public T InsertEntity(T entity)
        {
            object id = PersistenciaDatos.InsertEntity(entity);

            Type tipoEntidad = typeof (T);

            PropertyInfo propId = tipoEntidad.GetProperty(tipoEntidad.Name + "Id");
            Type nullType = Nullable.GetUnderlyingType(propId.PropertyType);

            object nullValue = nullType != null ? Convert.ChangeType(id, nullType) : id;

            propId.SetValue(entity, nullValue, null);

            object res = entity;

            return (T)res;
        }

        /// <summary>
        /// Realiza las modificaciones sobre una entidad en BBDD
        /// </summary>
        /// <param name="entity">Entidad modificada</param>
        public void UpdateEntity(T entity)
        {
            PersistenciaDatos.UpdateEntity(entity);
        }

        /// <summary>
        /// Realiza una borrado lógico de la entidad cuyo identificador
        /// se pasa como parámetro
        /// </summary>
        /// <param name="entitydId">Identificador de la entidad que se quiere eliminar</param>
        public void DeleteEntity(object entitydId)
        {
            PersistenciaDatos.DeleteEntity(entitydId);
        }

        /// <summary>
        /// Devuelve la entidad cuyo identificador se pasa como parámetro
        /// de entrada
        /// </summary>
        /// <param name="entitydId">Identificador de la entidad que se quiere obtener</param>
        /// <returns>Entidad</returns>
        public T GetEntity(object entitydId)
        {
            IDataReader dr = PersistenciaDatos.GetEntity(entitydId);
            dr.Read();

            //Realizamos el mapeo de la entidad desde los datos de la fila
            T res =  ParseRowToEntidad(dr);

            dr.Close();

            return res;
        }

        /// <summary>
        /// Devuelve una lista de todas las entidades activas existentes en BBDD
        /// </summary>
        /// <returns>Lista da entidades</returns>
        public IList<T> GetEntities()
        {
            IList<T> entidades = new List<T>();

            IDataReader dr = PersistenciaDatos.GetEntities();

            //Realizamos el mapeo de las entidades desde los datos de cada fila
            while (dr.Read())
            {
                entidades.Add(ParseRowToEntidad(dr));
            }

            dr.Close();

            return entidades;
        }

        /// <summary>
        /// Devuelve una lista de todas las entidades obtenidas con los 
        /// filtros establecidos en la variable entidadFiltro
        /// </summary>
        /// <param name="filterEntity">Entidad que actua como filtro</param>
        /// <returns>Lista de entidades</returns>
        public IList<T> GetEntities(T filterEntity)
        {
            IList<T> entidades = new List<T>();

            IDataReader dr = PersistenciaDatos.GetEntities(filterEntity);

            //Realizamos el mapeo de las entidades desde los datos de cada fila
            while (dr.Read())
            {
                entidades.Add(ParseRowToEntidad(dr));
            }

            dr.Close();

            return entidades;
        }

        /// <summary>
        /// Devuelve un dataset con todas las filas activas en BBDD de la entidad
        /// </summary>
        /// <returns>Dataset con el resultado</returns>
        public DataSet GetEntitiesDs()
        {
            return PersistenciaDatos.GetEntitiesDs();
        }

        /// <summary>
        /// Devuelve un dataset con todas las filas obtenidas con los 
        /// filtros establecidos en la variable entidadFiltro
        /// </summary>
        /// <param name="filterEntity">Entidad que actua como filtro</param>
        /// <returns>Dataset con el resultado del filtro</returns>
        public DataSet GetEntitiesDs(T filterEntity)
        {
            return PersistenciaDatos.GetEntitiesDs(filterEntity);
        }

        /// <summary>
        /// Método que devuelve un dataset con los datos devueltos por el procedimiento indicado
        /// con los parametros pasados
        /// </summary>
        /// <param name="storeProcedure">Nombre del procedimiento</param>
        /// <param name="procParams">Parametros necesitados por el procedimiento</param>
        /// <returns>Dataset con los resultados</returns>
        public DataSet GetEntitiesDs(string storeProcedure, IDictionary<string, object> procParams)
        {
            return PersistenciaDatos.GetEntitiesDs(storeProcedure, procParams);
        }

        #endregion

        #region Métodos privados

        /// <summary>
        /// Establece el valor de una propiedad dentro de un objeto, el nombre de la propiedad
        /// puede contener elementos de varios niveles (Ej: Elemento.Propiedad1.Atributo2
        /// Dichos objetos intermedios se instancia automáticamente
        /// </summary>
        /// <param name="targetObject">Objeto donde queremos introducir el valor</param>
        /// <param name="propName">Nombre de la propiedad</param>
        /// <param name="value">Valor de la propiedad</param>
        private static void SetValueInObject(T targetObject, string propName, Object value)
        {
            try
            {
                Type tipoEntidad = targetObject.GetType();

                PropertyInfo refsInfo = tipoEntidad.GetProperty("References");

                IList<String> referencias = (IList<String>)refsInfo.GetValue(targetObject, null);

                //Comprobamos si la propiedad contiene niveles de profundidad
                if (propName.Contains("."))
                {
                    string[] subProps = propName.Split('.');
                    Object aux = targetObject;

                    //Recorremos todos los niveles informados
                    foreach (string subProp in subProps)
                    {
                        PropertyInfo sprop = tipoEntidad.GetProperty(subProp);

                        PropertyInfo subRefsInfo = tipoEntidad.GetProperty("References");

                        //Comprobamos si la propiedad es una referencia del objeto para 
                        //Saber si tenemos que instanciarla
                        if (subRefsInfo != null)
                        {
                            IList<String> subReferencias = (IList<String>)subRefsInfo.GetValue(aux, null);

                            //Comprobamos si es necesario establecer el valor
                            if (sprop != null)
                            {
                                Object svalue = sprop.GetValue(aux, null);

                                //Comprobamos si se necesita instanciar una subpropiedad
                                if (subReferencias.Contains(sprop.Name))
                                {
                                    if (svalue == null)
                                    {
                                        if(sprop.PropertyType.BaseType == typeof(EaterEntity))
                                        {
                                            object refValue =
                                                Activator.CreateInstance(sprop.PropertyType.GetGenericArguments()[0]); 

                                            object[] param = new object[1];
                                            param[0] = refValue;

                                            svalue = Activator.CreateInstance(sprop.PropertyType, param);

                                            sprop.SetValue(aux, svalue, null);
                                            tipoEntidad = sprop.PropertyType.GetGenericArguments()[0];
                                            aux = refValue;
                                        }
                                        else
                                        {
                                            svalue = Activator.CreateInstance(sprop.PropertyType);
                                            sprop.SetValue(aux, svalue, null);
                                            tipoEntidad = sprop.PropertyType;
                                            aux = svalue;
                                        }
                                    }
                                    else
                                    {
                                        if (sprop.PropertyType.BaseType == typeof(EaterEntity))
                                         {
                                             tipoEntidad = sprop.PropertyType.GetGenericArguments()[0];
                                             aux = svalue.GetType().GetProperty("Value").GetValue(svalue, null);
                                         }
                                         else
                                         {
                                             tipoEntidad = sprop.PropertyType;
                                             aux = svalue;
                                         }
                                    }
                                }
                                else
                                {
                                    value = TryParserNullBoolean(sprop, value);
                                    Object nullValue = GetNullableValueFromProp(sprop, value);

                                    sprop.SetValue(aux, nullValue, null);
                                }
                            }
                        }
                        else if (sprop != null)
                        {
                            value = TryParserNullBoolean(sprop, value);
                            Object nullValue = GetNullableValueFromProp(sprop, value);
                            
                            sprop.SetValue(aux, nullValue, null);
                        }
                    }
                }
                else
                {
                    //Bifurcación de las propiedades de primer nivel
                    PropertyInfo prop = tipoEntidad.GetProperty(propName);

                    if (!referencias.Contains(propName))
                    {
                        //Comprobamos que el valor a introducir no se nulo
                        if (typeof(DBNull) != value.GetType())
                        {
                            value = TryParserNullBoolean(prop, value);
                            Object nullValue = GetNullableValueFromProp(prop, value);

                            prop.SetValue(targetObject, nullValue, null);
                        }
                    }
                }
            }
            catch (Exception)
            {
            }
        }

        private static object TryParserNullBoolean(PropertyInfo prop, object valor)
        {
            if (prop.PropertyType == typeof(bool)
                || prop.PropertyType == typeof(bool?))
            {
                bool res;

                if (!bool.TryParse(valor.ToString(), out res))
                {
                    valor = int.Parse(valor.ToString());
                }
                else
                {
                    valor = res;
                }
            }

            return valor;
        }

        private static object GetNullableValueFromProp(PropertyInfo prop, object valor)
        {
            //Establecemos el valor
            Type nullType = Nullable.GetUnderlyingType(prop.PropertyType);
            Object nullValue = nullType != null ? Convert.ChangeType(valor, nullType) : valor;

            return nullValue;
        }

        /// <summary>
        /// Convierte una fila de datos obtenida de una consulta
        /// en una entidad del módelo de dominio
        /// </summary>
        /// <param name="dr">Lector posicionado en la fila</param>
        /// <returns>Entidad que representa los datos de la fila</returns>
        private T ParseRowToEntidad(IDataReader dr)
        {
            int numCols = dr.FieldCount;

            T aux = Activator.CreateInstance<T>();

            //Por cada columna establecemos el valor
            //dentro de la entidad que vamos a devolver
            for (int i = 0; i < numCols; i++)
            {
                //Establecemos el valor de la columna
                SetValueInObject(aux, dr.GetName(i), dr.GetValue(i));
            }

            return aux;
        }

        /// <summary>
        /// Convierte una fila de datos obtenida de una consulta
        /// en una entidad del módelo de dominio
        /// </summary>
        /// <param name="dr">Lector posicionado en la fila</param>
        /// <returns>Entidad que representa los datos de la fila</returns>
        public static T ParseRowToEntidad(DataRow dr)
        {
            int numCols = dr.Table.Columns.Count;

            T aux = Activator.CreateInstance<T>();

            //Por cada columna establecemos el valor
            //dentro de la entidad que vamos a devolver
            for (int i = 0; i < numCols; i++)
            {
                string colName = dr.Table.Columns[i].ColumnName;
                //Establecemos el valor de la columna
                SetValueInObject(aux, colName, dr[colName]);
            }

            return aux;
        }

        #endregion

        #region Métodos públicos

        /// <summary>
        /// Devuelve un valor utilizable en una búsqueda parcial con LIKE
        /// </summary>
        /// <param name="value">Valor a procesar</param>
        /// <returns>Valor procesado</returns>
        public static string GetPartialSearchName(string value)
        {
            return string.Format("{0}%{1}%", PrefixLike, value.Replace(" ", "%"));
        }

        #endregion
    }
}
