using System;
using System.Collections.Generic;
using System.Data;
using Memento.Persistence.Commons;

namespace Memento.Persistence.Interfaces
{
    /// <summary>
    /// Interfaz de operaciones del módulo de persistencia
    /// </summary>
    /// <typeparam name="T">Tipo de la Entidad sobre la que trabajará el módulo de persistencia</typeparam>
    public interface IPersistence<T> where T : Entity
    {
        /// <summary>
        /// Método que persiste una T y la devuelve 
        /// el identificador obtenido del registro
        /// </summary>
        /// <param name="entity">T que se desea persistir</param>
        /// <returns>Entidad persistida</returns>
        T PersistEntity(T entity);

        /// <summary>
        /// Método que actualiza una Entidad
        /// </summary>
        /// <param name="entity">Entidad actualizada</param>
        void UpdateEntity(T entity);

        /// <summary>
        /// Método que realiza un borrado lógico de la Entidad
        /// </summary>
        /// <param name="entityId">Identficiador de la Entidad a eliminar</param>
        void DeleteEntity(Object entityId);

        /// <summary>
        /// Método que devuelve una Entidad
        /// a partir de su identificador
        /// </summary>
        /// <param name="entityId">Identificador de la Entidad</param>
        /// <returns>Entidad que se recupera</returns>
        T GetEntity(Object entityId);

        /// <summary>
        /// Método que devuelve todas las Entidades activas
        /// </summary>
        /// <returns>Entidades activas</returns>
        IList<T> GetEntities();

        /// <summary>
        /// Método que devuelve todas las Entidades que 
        /// cumplan con la entidad que se pasa como filtro de búsqueda
        /// </summary>
        /// <param name="filterEntity">Entidad utilizada de filtro</param>
        /// <returns>Entidades filtradas</returns>
        IList<T> GetEntities(T filterEntity);

        /// <summary>
        /// Método que devuelve un DataSeT con todas las Entidades activas
        /// </summary>
        /// <returns>DataSeT con las Entidades activas</returns>
        DataSet GetEntitiesDs();

        /// <summary>
        /// Método que devuelve un DataSeT con todas las Entidades que 
        /// cumplan con la Entidad que se pasa como filtro de búsqueda
        /// </summary>
        /// <param name="filterEntity">Entidad utilizada de filtro</param>
        /// <returns>DataSeT con las Entidades filtradas</returns>
        DataSet GetEntitiesDs(T filterEntity);

        /// <summary>
        /// Método que devuelve un dataset con los datos devueltos por el procedimiento indicado
        /// con los parametros pasados
        /// </summary>
        /// <param name="storeProcedure">Nombre del procedimiento</param>
        /// <param name="procParams">Parametros necesitados por el procedimiento</param>
        /// <returns>Dataset con los resultados</returns>
        DataSet GetEntitiesDs(string storeProcedure, IDictionary<string, object> procParams);
    }
}