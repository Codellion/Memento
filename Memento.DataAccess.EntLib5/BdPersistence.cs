using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Reflection;

using Memento.DataAccess.Interfaces;
using Memento.DataAccess.Utils;
using Memento.Persistence.Commons;
using Microsoft.Practices.EnterpriseLibrary.Data;

namespace Memento.DataAccess.EntLib5
{
    /// <summary>
    /// Clase que implementa la interfaz de acceso a datos del módulo de persistencia,
    /// para ello se ha utilizado el servicio de acceso a BBDD de Enterprise Library 5.0
    /// </summary>
    /// <typeparam name="T">Tipo de la entidad con la que se va operar</typeparam>
    public class BdPersistence<T> : IDataPersistence<T> where T: Entity
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
        private Database servicioDatos;

        /// <summary>
        /// Transacción actual en curso en caso de existir
        /// </summary>
        private DbTransaction transaccion;

        #endregion

        #region Constructores

        /// <summary>
        /// Se crea una conexión por defecto con MEMENTO
        /// </summary>
        public BdPersistence()
        {
            servicioDatos = DatabaseFactory.CreateDatabase(Entorno);
        }

        /// <summary>
        /// Se crea una conexión con el entorno indicado
        /// </summary>
        public BdPersistence(string entorno)
        {
            servicioDatos = DatabaseFactory.CreateDatabase(entorno);
        }

        /// <summary>
        /// Se crea una conexión con la BD dentro
        /// de una transacción activa
        /// </summary>
        /// <param name="transaccion">Transacción activa</param>
        public BdPersistence(IDbTransaction transaccion)
        {
            servicioDatos = DatabaseFactory.CreateDatabase(Entorno);
            this.transaccion = transaccion as DbTransaction;
        }

        /// <summary>
        /// Se crea una conexión con la BD especificada por parámetros dentro
        /// de una transacción activa
        /// </summary>
        /// <param name="entorno">Base de datos</param>
        /// <param name="transaccion">Transacción activa</param>
        public BdPersistence(string entorno, IDbTransaction transaccion)
        {
            servicioDatos = DatabaseFactory.CreateDatabase(entorno);
            this.transaccion = transaccion as DbTransaction;
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

            if (transaccion != null)
            {
                id = servicioDatos.ExecuteScalar(transaccion, CommandType.Text, query.ToInsert());
            }
            else
            {
                id = servicioDatos.ExecuteScalar(CommandType.Text, query.ToInsert());
            }

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

            if (transaccion != null)
            {
                servicioDatos.ExecuteNonQuery(transaccion, CommandType.Text, query.ToUpdate());
            }
            else
            {
                servicioDatos.ExecuteNonQuery(CommandType.Text, query.ToUpdate());
            }
        }

        /// <summary>
        /// Método que realiza un borrado lógico de la entidad
        /// </summary>
        /// <param name="entidadId">Identificador de la entidad a eliminar</param>
        public void DeleteEntity(object entidadId)
        {
            Query query = DbUtil<T>.GetDelete(entidadId);

            if (transaccion != null)
            {
                servicioDatos.ExecuteNonQuery(transaccion, CommandType.Text, query.ToDelete());
            }
            else
            {
                servicioDatos.ExecuteNonQuery(CommandType.Text, query.ToDelete());
            }
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

            return servicioDatos.ExecuteReader(CommandType.Text, query.ToSelect());
        }

        /// <summary>
        /// Método que devuelve todas las entidades activas
        /// </summary>
        /// <returns>Entidades activas</returns>
        public IDataReader GetEntities()
        {
            T aux = Activator.CreateInstance<T>();
            
            Query query = DbUtil<T>.GetQuery(aux);

            return servicioDatos.ExecuteReader(CommandType.Text, query.ToSelect());
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

            return servicioDatos.ExecuteReader(CommandType.Text, query.ToSelect());
        }

        /// <summary>
        /// Método que devuelve un DataSet con todas las entidades activas
        /// </summary>
        /// <returns>DataSet con las entidades activas</returns>
        public DataSet GetEntitiesDs()
        {
            T aux = Activator.CreateInstance<T>();
            
            Query query = DbUtil<T>.GetQuery(aux);

            return servicioDatos.ExecuteDataSet(CommandType.Text, query.ToSelect());
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

            return servicioDatos.ExecuteDataSet(CommandType.Text, query.ToSelect());
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
            object[] parametrosProc = new object[parametros.Count];
            int count = 0;

            foreach (string key in parametros.Keys)
            {
                object valor = parametros[key];
                SqlParameter param = new SqlParameter(key, DbUtil<T>.GetDbType(valor.GetType()));
                param.Value = valor;

                parametrosProc[count] = param;

                count++;
            }

            storeProcedure = string.Format("[{0}].{1}", DataFactoryProvider.GetSChema(), storeProcedure);

            return servicioDatos.ExecuteDataSet(storeProcedure, parametrosProc);
        }


        /// <summary>
        /// Método que devuelve una conexión de la Enterprise library 5.0
        /// </summary>
        /// <returns></returns>
        public IDbConnection GetConnection()
        {
            return servicioDatos.CreateConnection();
        }

        #endregion

    }
}
