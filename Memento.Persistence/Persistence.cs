using System;
using System.Collections.Generic;
using System.Data;
using System.Reflection;

using Memento.DataAccess;
using Memento.DataAccess.Interfaces;

using Memento.Persistence.Interfaces;

namespace Memento.Persistence
{
    /// <summary>
    /// Clase que actua de fachada del módulo de persistencia
    /// </summary>
    /// <typeparam name="T">Tipo de dato sobre el que se creará la factoría</typeparam>
    public class Persistence<T> : IPersistence<T>
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
                        _persistenciaDatos = DataFactoryProvider.GetProveedor<T>(_contexto.Transaccion);
                    }
                    else
                    {
                        _persistenciaDatos = DataFactoryProvider.GetProveedor<T>(null);
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
        /// <param name="entidad">Entidad que se dará de alta</param>
        /// <returns>Entidad con el identificador de BBDD informado</returns>
        public T InsertarEntidad(T entidad)
        {
            object id = PersistenciaDatos.InsertarEntidad(entidad);

            Type tipoEntidad = typeof (T);

            PropertyInfo propId = tipoEntidad.GetProperty(tipoEntidad.Name + "Id");
            Type nullType = Nullable.GetUnderlyingType(propId.PropertyType);
            Object nullValue = null;

            nullValue = nullType != null ? Convert.ChangeType(id, nullType) : id;

            propId.SetValue(entidad, nullValue, null);


            return entidad;
        }

        /// <summary>
        /// Realiza las modificaciones sobre una entidad en BBDD
        /// </summary>
        /// <param name="entidad">Entidad modificada</param>
        public void ModificarEntidad(T entidad)
        {
            PersistenciaDatos.ModificarEntidad(entidad);
        }

        /// <summary>
        /// Realiza una borrado lógico de la entidad cuyo identificador
        /// se pasa como parámetro
        /// </summary>
        /// <param name="entidadId">Identificador de la entidad que se quiere eliminar</param>
        public void EliminarEntidad(object entidadId)
        {
            PersistenciaDatos.EliminarEntidad(entidadId);
        }

        /// <summary>
        /// Devuelve la entidad cuyo identificador se pasa como parámetro
        /// de entrada
        /// </summary>
        /// <param name="entidadId">Identificador de la entidad que se quiere obtener</param>
        /// <returns>Entidad</returns>
        public T GetEntidad(object entidadId)
        {
            IDataReader dr = PersistenciaDatos.GetEntidad(entidadId);
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
        public IList<T> GetEntidades()
        {
            IList<T> entidades = new List<T>();

            IDataReader dr = PersistenciaDatos.GetEntidades();

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
        /// <param name="entidadFiltro">Entidad que actua como filtro</param>
        /// <returns>Lista de entidades</returns>
        public IList<T> GetEntidades(T entidadFiltro)
        {
            IList<T> entidades = new List<T>();

            IDataReader dr = PersistenciaDatos.GetEntidades(entidadFiltro);

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
        public DataSet GetEntidadesDs()
        {
            return PersistenciaDatos.GetEntidadesDs();
        }

        /// <summary>
        /// Devuelve un dataset con todas las filas obtenidas con los 
        /// filtros establecidos en la variable entidadFiltro
        /// </summary>
        /// <param name="entidadFiltro">Entidad que actua como filtro</param>
        /// <returns>Dataset con el resultado del filtro</returns>
        public DataSet GetEntidadesDs(T entidadFiltro)
        {
            return PersistenciaDatos.GetEntidadesDs(entidadFiltro);
        }

        /// <summary>
        /// Método que devuelve un dataset con los datos devueltos por el procedimiento indicado
        /// con los parametros pasados
        /// </summary>
        /// <param name="storeProcedure">Nombre del procedimiento</param>
        /// <param name="parametros">Parametros necesitados por el procedimiento</param>
        /// <returns>Dataset con los resultados</returns>
        public DataSet GetEntidadesDs(string storeProcedure, IDictionary<string, object> parametros)
        {
            return PersistenciaDatos.GetEntidadesDs(storeProcedure, parametros);
        }

        #endregion

        #region Métodos privados

        /// <summary>
        /// Establece el valor de una propiedad dentro de un objeto, el nombre de la propiedad
        /// puede contener elementos de varios niveles (Ej: Elemento.Propiedad1.Atributo2
        /// Dichos objetos intermedios se instancia automáticamente
        /// </summary>
        /// <param name="objeto">Objeto donde queremos introducir el valor</param>
        /// <param name="propName">Nombre de la propiedad</param>
        /// <param name="valor">Valor de la propiedad</param>
        private static void SetValorEnObjeto(T objeto, string propName, Object valor)
        {
            try
            {
                Type tipoEntidad = objeto.GetType();

                PropertyInfo refsInfo = tipoEntidad.GetProperty("Referencias");

                IList<String> referencias = (IList<String>)refsInfo.GetValue(objeto, null);

                //Comprobamos si la propiedad contiene niveles de profundidad
                if (propName.Contains("."))
                {
                    string[] subProps = propName.Split('.');
                    Object aux = objeto;

                    //Recorremos todos los niveles informados
                    foreach (string subProp in subProps)
                    {
                        PropertyInfo sprop = tipoEntidad.GetProperty(subProp);

                        PropertyInfo subRefsInfo = tipoEntidad.GetProperty("Referencias");

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
                                        if(sprop.PropertyType.Name.Equals("Reference`1"))
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
                                         if(sprop.PropertyType.Name.Equals("Reference`1"))
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
                                    valor = TryParserNullBoolean(sprop, valor);
                                    Object nullValue = GetNullableValueFromProp(sprop, valor);

                                    sprop.SetValue(aux, nullValue, null);
                                }
                            }
                        }
                        else if (sprop != null)
                        {
                            valor = TryParserNullBoolean(sprop, valor);
                            Object nullValue = GetNullableValueFromProp(sprop, valor);
                            
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
                        if (typeof(DBNull) != valor.GetType())
                        {
                            valor = TryParserNullBoolean(prop, valor);
                            Object nullValue = GetNullableValueFromProp(prop, valor);

                            prop.SetValue(objeto, nullValue, null);
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
                SetValorEnObjeto(aux, dr.GetName(i), dr.GetValue(i));
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
                SetValorEnObjeto(aux, colName, dr[colName]);
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
