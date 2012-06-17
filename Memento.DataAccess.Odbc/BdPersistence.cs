using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.Common;
using System.Data.Odbc;
using System.Reflection;

using Memento.DataAccess.Interfaces;
using Memento.DataAccess.Utils;
using Memento.Persistence.Commons;


namespace Memento.DataAccess.Odbc
{
    /// <summary>
    /// Clase que implementa la interfaz de acceso a datos del módulo de persistencia,
    /// para ello se ha utilizado el servicio de acceso a BBDD de Odbc
    /// </summary>
    /// <typeparam name="T">Tipo de la entidad con la que se va operar</typeparam>
    public class BdPersistence<T> : IDisposable, IDataPersistence<T> where T: Entity
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
            servicioDatos = new OdbcCommand(string.Empty, new OdbcConnection(connstring.ConnectionString));

            servicioDatos.Connection.Open();
        }

        /// <summary>
        /// Se crea una conexión con el entorno indicado
        /// </summary>
        public BdPersistence(string entorno)
        {

            ConnectionStringSettings connstring = ConfigurationManager.ConnectionStrings[entorno];
            servicioDatos = new OdbcCommand(string.Empty, new OdbcConnection(connstring.ConnectionString));

            servicioDatos.Connection.Open();
        }

        /// <summary>
        /// Se crea una conexión con la BD dentro
        /// de una transacción activa
        /// </summary>
        /// <param name="transaccion">Transacción activa</param>
        public BdPersistence(IDbTransaction transaccion)
        {
            servicioDatos = new OdbcCommand(string.Empty,
                                             transaccion.Connection as OdbcConnection,
                                             transaccion as OdbcTransaction);

        }

        #endregion

        #region Implementación de la interfaz

        /// <summary>
        /// Método que persiste una entidad y la devuelve 
        /// el identificador obtenido del registro
        /// </summary>
        /// <param name="entidad">Entidad que se desea persistir</param>
        /// <returns>Identificador de la entidad persistida</returns>
        public object InsertEntity(Entity entidad)
        {
            Query query = DbUtil<T>.GetInsert((T)entidad);

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

            object nullValue = nullType != null ? Convert.ChangeType(id, nullType) : id;

            return nullValue;
        }

        /// <summary>
        /// Método que actualiza una entidad
        /// </summary>
        /// <param name="entidad">Entidad actualizada</param>
        public void UpdateEntity(Entity entidad)
        {
            Query query = DbUtil<T>.GetUpdate((T)entidad);

            servicioDatos.CommandText = query.ToUpdate();

            servicioDatos.ExecuteNonQuery();
        }

        /// <summary>
        /// Método que realiza un borrado lógico de la entidad
        /// </summary>
        /// <param name="entidadId">Identificador de la entidad a eliminar</param>
        public void DeleteEntity(object entidadId)
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
        public IDataReader GetEntity(object entidadId)
        {
            T aux = Activator.CreateInstance<T>();
            Type gType = aux.GetType();

            PropertyInfo pPk = gType.GetProperty(gType.Name + "Id");

            Type nullType = Nullable.GetUnderlyingType(pPk.PropertyType);

            object nullValue = nullType != null ? Convert.ChangeType(entidadId, nullType) : entidadId;

            pPk.SetValue(aux, nullValue, null);

            Query query = DbUtil<T>.GetQuery(aux);

            servicioDatos.CommandText = query.ToSelect();

            return servicioDatos.ExecuteReader();
        }

        /// <summary>
        /// Método que devuelve todas las entidades activas
        /// </summary>
        /// <returns>Entidades activas</returns>
        public IDataReader GetEntities()
        {
            T aux = Activator.CreateInstance<T>();

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
        public IDataReader GetEntities(Entity entidadFiltro)
        {
            Query query = DbUtil<T>.GetQuery((T)entidadFiltro);

            servicioDatos.CommandText = query.ToSelect();

            return servicioDatos.ExecuteReader();
        }

        /// <summary>
        /// Método que devuelve un DataSet con todas las entidades activas
        /// </summary>
        /// <returns>DataSet con las entidades activas</returns>
        public DataSet GetEntitiesDs()
        {
            T aux = Activator.CreateInstance<T>();

            Query query = DbUtil<T>.GetQuery(aux);

            OdbcDataAdapter adapter = new OdbcDataAdapter(query.ToSelect(), 
                servicioDatos.Connection as OdbcConnection);

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
        public DataSet GetEntitiesDs(Entity entidadFiltro)
        {
            Query query = DbUtil<T>.GetQuery((T)entidadFiltro);

            OdbcDataAdapter adapter = new OdbcDataAdapter(query.ToSelect(),
                servicioDatos.Connection as OdbcConnection);

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
        public DataSet GetEntitiesDs(string storeProcedure, IDictionary<string, object> parametros)
        {
            foreach (string key in parametros.Keys)
            {
                object valor = parametros[key];

                DbParameter param = new OdbcParameter(key, DbUtil<T>.GetDbType(valor.GetType()));
                param.Value = valor;
                
                servicioDatos.Parameters.Add(param);
            }
            
            servicioDatos.CommandType = CommandType.StoredProcedure;
            servicioDatos.CommandText = storeProcedure;
            
            OdbcDataAdapter adapter = new OdbcDataAdapter(servicioDatos as OdbcCommand);
            
            DataSet result = new DataSet();
            adapter.Fill(result, "result");

            return result;
        }


        /// <summary>
        /// Método que devuelve una conexión con el driver Odbc
        /// </summary>
        /// <returns></returns>
        public IDbConnection GetConnection()
        {
            ConnectionStringSettings connstring = ConfigurationManager.ConnectionStrings[Entorno];
            return new OdbcConnection(connstring.ConnectionString);
        }

        #endregion

        /// <summary>
        /// Se utiliza el método Dispose del objeto para asegurarnos de cerrar la conexión
        /// en caso de estar aún abierta
        /// </summary>
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
