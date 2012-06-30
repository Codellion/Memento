using System.Collections.Generic;
using System.Data;
using Memento.Persistence.Commons;

namespace Memento.DataAccess.Interfaces
{
    /// <summary>
    /// Interfaz de acceso a la capa de datos del módulo de persistencia
    /// </summary>
    public interface IDataPersistence
    {
        /// <summary>
        /// Método que persiste una entidad y la devuelve 
        /// el identificador obtenido del registro
        /// </summary>
        /// <param name="entity">Entidad que se desea persistir</param>
        /// <returns>Identificador de la entidad persistida</returns>
        object InsertEntity(Entity entity);

        /// <summary>
        /// Método que actualiza una entidad
        /// </summary>
        /// <param name="entity">Entidad actualizada</param>
        void UpdateEntity(Entity entity);

        /// <summary>
        /// Método que realiza un borrado lógico de la entidad
        /// </summary>
        /// <param name="entityId">Identificador de la entidad a eliminar</param>
        void DeleteEntity(object entityId);

        /// <summary>
        /// Método que devuelve una entidad
        /// a partir de su identificador
        /// </summary>
        /// <param name="entityId">Identificador de la entidad</param>
        /// <returns>Entidad que se recupera</returns>
        IDataReader GetEntity(object entityId);

        /// <summary>
        /// Método que devuelve todas las entidades activas
        /// </summary>
        /// <returns>Entidades activas</returns>
        IDataReader GetEntities();

        /// <summary>
        /// Método que devuelve todas las entidades que 
        /// cumplan con la entidad que se pasa como filtro de búsqueda
        /// </summary>
        /// <param name="filterEntity">Entidad utilizada de filtro</param>
        /// <returns>Entidades filtradas</returns>
        IDataReader GetEntities(Entity filterEntity);

        /// <summary>
        /// Método que devuelve un DataSet con todas las entidades activas
        /// </summary>
        /// <returns>DataSet con las entidades activas</returns>
        DataSet GetEntitiesDs();

        /// <summary>
        /// Método que devuelve un DataSet con todas las entidades que 
        /// cumplan con la entidad que se pasa como filtro de búsqueda
        /// </summary>
        /// <param name="filterEntity">Entidad utilizada de filtro</param>
        /// <returns>DataSet con las entidades filtradas</returns>
        DataSet GetEntitiesDs(Entity filterEntity);

        /// <summary>
        /// Método que devuelve un dataset con los datos devueltos por el procedimiento indicado
        /// con los parametros pasados
        /// </summary>
        /// <param name="storeProcedure">Nombre del procedimiento</param>
        /// <param name="proParams">Parametros necesitados por el procedimiento</param>
        /// <returns>Dataset con los resultados</returns>
        DataSet GetEntitiesDs(string storeProcedure, IDictionary<string, object> proParams);

        /// <summary>
        /// Método que devuelve una conexión con el proveedor implementado
        /// </summary>
        /// <returns>Conexión</returns>
        IDbConnection GetConnection();
    }
}