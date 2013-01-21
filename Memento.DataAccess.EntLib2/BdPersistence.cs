using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using Memento.DataAccess.Interfaces;
using Memento.DataAccess.Utils;
using Memento.Persistence.Commons;
using Memento.Persistence.Commons.Annotations;
using Microsoft.Practices.EnterpriseLibrary.Data;

namespace Memento.DataAccess.EntLib2
{
    /// <summary>
    /// Clase que implementa la interfaz de acceso a datos del módulo de persistencia,
    /// para ello se ha utilizado el servicio de acceso a BBDD de Enterprise Library 2.0
    /// </summary>
    /// <typeparam name="T">Tipo de la entidad con la que se va operar</typeparam>
    public class BdPersistence<T> : IDataPersistence where T : Entity
    {
        #region Constantes

        /// <summary>
        /// Entorno de la arquitectura sobre la que se va a operar
        /// </summary>
        private const string Entorno = "MEMENTO";

        #endregion

        #region Atributos privados

        /// <summary>
        /// Base datos sobre la que se opera
        /// </summary>
        private readonly Database _servicioDatos;

        /// <summary>
        /// Transacción actual en curso en caso de existir
        /// </summary>
        private readonly DbTransaction _transaccion;

        #endregion

        #region Constructores

        /// <summary>
        /// Se crea una conexión por defecto con MEMENTO
        /// </summary>
        public BdPersistence()
        {
            _servicioDatos = DatabaseFactory.CreateDatabase(Entorno);
        }

        /// <summary>
        /// Se crea una conexión con el entorno indicado
        /// </summary>
        public BdPersistence(string entorno)
        {
            _servicioDatos = DatabaseFactory.CreateDatabase(entorno);
        }

        /// <summary>
        /// Se crea una conexión con la BD dentro
        /// de una transacción activa
        /// </summary>
        /// <param name="transaccion">Transacción activa</param>
        public BdPersistence(IDbTransaction transaccion)
        {
            _servicioDatos = DatabaseFactory.CreateDatabase(Entorno);
            _transaccion = transaccion as DbTransaction;
        }

        /// <summary>
        /// Se crea una conexión con la BD especificada por parámetros dentro
        /// de una transacción activa
        /// </summary>
        /// <param name="entorno">Base de datos</param>
        /// <param name="transaccion">Transacción activa</param>
        public BdPersistence(string entorno, IDbTransaction transaccion)
        {
            _servicioDatos = DatabaseFactory.CreateDatabase(entorno);
            _transaccion = transaccion as DbTransaction;
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
            Query query = DbUtil<T>.GetInsert((T) entidad);

            if(_transaccion != null)
            {
                _servicioDatos.ExecuteNonQuery(_transaccion, CommandType.Text, query.ToInsert());
            }
            else
            {
                _servicioDatos.ExecuteNonQuery(CommandType.Text, query.ToInsert());
            }

            object id = null;

            if(entidad.KeyGenerator == KeyGenerationType.Database)
            {
                id = _transaccion != null 
                    ? _servicioDatos.ExecuteScalar(_transaccion, CommandType.Text, query.ToSelectLastId()) 
                    : _servicioDatos.ExecuteScalar(CommandType.Text, query.ToSelectLastId());
            }
            else
            {
                id = entidad.GetEntityId();
            }

            if (id == null)
            {
                throw new Exception("Ocurrió un error desconocido al insertar el registro.");
            }

            return id;
        }

        /// <summary>
        /// Método que actualiza una entidad
        /// </summary>
        /// <param name="entidad">Entidad actualizada</param>
        public void UpdateEntity(Entity entidad)
        {
            Query query = DbUtil<T>.GetUpdate((T) entidad);

            if (_transaccion != null)
            {
                _servicioDatos.ExecuteNonQuery(_transaccion, CommandType.Text, query.ToUpdate());
            }
            else
            {
                _servicioDatos.ExecuteNonQuery(CommandType.Text, query.ToUpdate());
            }
        }

        /// <summary>
        /// Método que realiza un borrado lógico de la entidad
        /// </summary>
        /// <param name="entidadId">Identificador de la entidad a eliminar</param>
        public void DeleteEntity(object entidadId)
        {
            Query query = DbUtil<T>.GetDelete(entidadId);

            if (_transaccion != null)
            {
                _servicioDatos.ExecuteNonQuery(_transaccion, CommandType.Text, query.ToDelete());
            }
            else
            {
                _servicioDatos.ExecuteNonQuery(CommandType.Text, query.ToDelete());
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
           
            aux.SetEntityId(entidadId);
            Query query = DbUtil<T>.GetQuery(aux);

            return _servicioDatos.ExecuteReader(CommandType.Text, query.ToSelect());
        }

        /// <summary>
        /// Método que devuelve todas las entidades activas
        /// </summary>
        /// <returns>Entidades activas</returns>
        public IDataReader GetEntities()
        {
            T aux = Activator.CreateInstance<T>();

            Query query = DbUtil<T>.GetQuery(aux);

            return _servicioDatos.ExecuteReader(CommandType.Text, query.ToSelect());
        }

        /// <summary>
        /// Método que devuelve todas las entidades que 
        /// cumplan con la entidad que se pasa como filtro de búsqueda
        /// </summary>
        /// <param name="entidadFiltro">Entidad utilizada de filtro</param>
        /// <returns>Entidades filtradas</returns>
        public IDataReader GetEntities(Entity entidadFiltro)
        {
            Query query = DbUtil<T>.GetQuery((T) entidadFiltro);

            return _servicioDatos.ExecuteReader(CommandType.Text, query.ToSelect());
        }

        /// <summary>
        /// Método que devuelve un DataSet con todas las entidades activas
        /// </summary>
        /// <returns>DataSet con las entidades activas</returns>
        public DataSet GetEntitiesDs()
        {
            T aux = Activator.CreateInstance<T>();

            Query query = DbUtil<T>.GetQuery(aux);

            return _servicioDatos.ExecuteDataSet(CommandType.Text, query.ToSelect());
        }

        /// <summary>
        /// Método que devuelve un DataSet con todas las entidades que 
        /// cumplan con la entidad que se pasa como filtro de búsqueda
        /// </summary>
        /// <param name="entidadFiltro">Entidad utilizada de filtro</param>
        /// <returns>DataSet con las entidades filtradas</returns>
        public DataSet GetEntitiesDs(Entity entidadFiltro)
        {
            Query query = DbUtil<T>.GetQuery((T) entidadFiltro);

            return _servicioDatos.ExecuteDataSet(CommandType.Text, query.ToSelect());
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

                parametrosProc[count] = valor;

                count++;
            }

            storeProcedure = string.Format("{0}", storeProcedure);

            return _servicioDatos.ExecuteDataSet(storeProcedure, parametrosProc);
        }


        /// <summary>
        /// Método que devuelve una conexión de la Enterprise library 2006
        /// </summary>
        /// <returns></returns>
        public IDbConnection GetConnection()
        {
            return _servicioDatos.CreateConnection();
        }

        #endregion
    }
}