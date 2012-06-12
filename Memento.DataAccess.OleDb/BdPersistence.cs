using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.Common;
using System.Data.OleDb;
using System.Reflection;

using Memento.DataAccess.Interfaces;
using Memento.DataAccess.Utils;


namespace Memento.DataAccess.OleDb
{
    /// <summary>
    /// Clase que implementa la interfaz de acceso a datos del módulo de persistencia,
    /// para ello se ha utilizado el servicio de acceso a BBDD de OleDb
    /// </summary>
    /// <typeparam name="T">Tipo de la entidad con la que se va operar</typeparam>
    public class BdPersistence<T> : IDataPersistence<T>, IDisposable
    {

        #region Constantes

        /// <summary>
        /// Entorno de la arquitectura sobre la que se va a operar
        /// </summary>
        public const string Entorno = "MEMENTO";

        #endregion

        #region Atributos privados

        /// <summary>
        /// Base datos sobre la que se opera
        /// </summary>
        private IDbCommand servicioDatos;

        #endregion

        #region Constructores

        /// <summary>
        /// Se crea una conexión por defecto con MEMENTO
        /// </summary>
        public BdPersistence()
        {

            ConnectionStringSettings connstring = ConfigurationManager.ConnectionStrings[Entorno];
            servicioDatos = new OleDbCommand(string.Empty, new OleDbConnection(connstring.ConnectionString));

            servicioDatos.Connection.Open();
        }

        /// <summary>
        /// Se crea una conexión con el entorno indicado
        /// </summary>
        public BdPersistence(string entorno)
        {

            ConnectionStringSettings connstring = ConfigurationManager.ConnectionStrings[entorno];
            servicioDatos = new OleDbCommand(string.Empty, new OleDbConnection(connstring.ConnectionString));

            servicioDatos.Connection.Open();
        }

        /// <summary>
        /// Se crea una conexión con la BD dentro
        /// de una transacción activa
        /// </summary>
        /// <param name="transaccion">Transacción activa</param>
        public BdPersistence(IDbTransaction transaccion)
        {
            servicioDatos = new OleDbCommand(string.Empty,
                                             transaccion.Connection as OleDbConnection,
                                             transaccion as OleDbTransaction);

        }

        #endregion

        #region Implementación de la interfaz

        /// <summary>
        /// Método que persiste una entidad y la devuelve 
        /// el identificador obtenido del registro
        /// </summary>
        /// <param name="entidad">Entidad que se desea persistir</param>
        /// <returns>Identificador de la entidad persistida</returns>
        public object InsertarEntidad(T entidad)
        {
            Query query = DbUtil<T>.GetInsert(entidad);

            object id;

            servicioDatos.CommandText = query.ToInsert();

            id = servicioDatos.ExecuteScalar();

            if (id == null)
            {
                throw new Exception("Ocurrió un error desconocido al insertar el registro.");
            }

            Type tipoT = typeof(T);
            Type tId = tipoT.GetProperty(tipoT.Name + "Id").PropertyType;

            Type nullType = Nullable.GetUnderlyingType(tId);
            Object nullValue = null;

            nullValue = nullType != null ? Convert.ChangeType(id, nullType) : id;

            return nullValue;
        }

        /// <summary>
        /// Método que actualiza una entidad
        /// </summary>
        /// <param name="entidad">Entidad actualizada</param>
        public void ModificarEntidad(T entidad)
        {
            Query query = DbUtil<T>.GetUpdate(entidad);

            servicioDatos.CommandText = query.ToUpdate();

            servicioDatos.ExecuteNonQuery();
        }

        /// <summary>
        /// Método que realiza un borrado lógico de la entidad
        /// </summary>
        /// <param name="entidadId">Identificador de la entidad a eliminar</param>
        public void EliminarEntidad(object entidadId)
        {
            Query query = DbUtil<T>.GetDelete(entidadId);

            servicioDatos.CommandText = query.ToDelete();

            servicioDatos.ExecuteNonQuery();
            
        }

        /// <summary>
        /// Método que devuelve una entidad
        /// a partir de su identificador
        /// </summary>
        /// <param name="entidadId">Identificador de la entidad</param>
        /// <returns>Entidad que se recupera</returns>
        public IDataReader GetEntidad(object entidadId)
        {
            T aux = DbUtil<T>.GetPlantillaEntidad();
            Type gType = aux.GetType();

            PropertyInfo pPk = gType.GetProperty(gType.Name + "Id");

            Type nullType = Nullable.GetUnderlyingType(pPk.PropertyType);
            Object nullValue = null;

            nullValue = nullType != null ? Convert.ChangeType(entidadId, nullType) : entidadId;

            pPk.SetValue(aux, nullValue, null);

            Query query = DbUtil<T>.GetQuery(aux);

            servicioDatos.CommandText = query.ToSelect();

            return servicioDatos.ExecuteReader();
        }

        /// <summary>
        /// Método que devuelve todas las entidades activas
        /// </summary>
        /// <returns>Entidades activas</returns>
        public IDataReader GetEntidades()
        {
            T aux = DbUtil<T>.GetPlantillaEntidad();

            Query query = DbUtil<T>.GetQuery(aux);

            servicioDatos.CommandText = query.ToSelect();

            return servicioDatos.ExecuteReader();
        }

        /// <summary>
        /// Método que devuelve todas las entidades que 
        /// cumplan con la entidad que se pasa como filtro de búsqueda
        /// </summary>
        /// <param name="entidadFiltro">Entidad utilizada de filtro</param>
        /// <returns>Entidades filtradas</returns>
        public IDataReader GetEntidades(T entidadFiltro)
        {
            Query query = DbUtil<T>.GetQuery(entidadFiltro);

            servicioDatos.CommandText = query.ToSelect();

            return servicioDatos.ExecuteReader();
        }

        /// <summary>
        /// Método que devuelve un DataSet con todas las entidades activas
        /// </summary>
        /// <returns>DataSet con las entidades activas</returns>
        public DataSet GetEntidadesDs()
        {
            T aux = DbUtil<T>.GetPlantillaEntidad();

            Query query = DbUtil<T>.GetQuery(aux);

            OleDbDataAdapter adapter = new OleDbDataAdapter(query.ToSelect(), 
                servicioDatos.Connection as OleDbConnection);

            DataSet result = new DataSet();
            adapter.Fill(result, "result");
            
            return result;
        }

        /// <summary>
        /// Método que devuelve un DataSet con todas las entidades que 
        /// cumplan con la entidad que se pasa como filtro de búsqueda
        /// </summary>
        /// <param name="entidadFiltro">Entidad utilizada de filtro</param>
        /// <returns>DataSet con las entidades filtradas</returns>
        public DataSet GetEntidadesDs(T entidadFiltro)
        {
            Query query = DbUtil<T>.GetQuery(entidadFiltro);

            OleDbDataAdapter adapter = new OleDbDataAdapter(query.ToSelect(),
                servicioDatos.Connection as OleDbConnection);

            DataSet result = new DataSet();
            adapter.Fill(result, "result");

            return result;
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
            foreach (string key in parametros.Keys)
            {
                object valor = parametros[key];

                DbParameter param = new OleDbParameter(key, DbUtil<T>.GetDbType(valor.GetType()));
                param.Value = valor;
                
                servicioDatos.Parameters.Add(param);
            }
            
            servicioDatos.CommandType = CommandType.StoredProcedure;
            servicioDatos.CommandText = storeProcedure;
            
            OleDbDataAdapter adapter = new OleDbDataAdapter(servicioDatos as OleDbCommand);
            
            DataSet result = new DataSet();
            adapter.Fill(result, "result");

            return result;
        }


        /// <summary>
        /// Método que devuelve una conexión de la Enterprise library 2006
        /// </summary>
        /// <returns></returns>
        public IDbConnection GetConnection()
        {
            ConnectionStringSettings connstring = ConfigurationManager.ConnectionStrings[Entorno];
            return new OleDbConnection(connstring.ConnectionString);
        }

        #endregion


        public void Dispose()
        {
            if(servicioDatos !=null)
            {
                if(servicioDatos.Connection != null 
                    && servicioDatos.Connection.State == ConnectionState.Open
                    && servicioDatos.Transaction == null)
                {
                    servicioDatos.Connection.Close();
                }
            }
        }
    }
}
