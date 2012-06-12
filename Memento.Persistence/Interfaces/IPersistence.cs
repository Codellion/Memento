using System;
using System.Collections.Generic;
using System.Data;

namespace Memento.Persistence.Interfaces
{
    /// <summary>
    /// Interfaz de operaciones del módulo de persistencia
    /// </summary>
    /// <typeparam name="T">Tipo de la entidad sobre la que trabajará el módulo de persistencia</typeparam>
    public interface IPersistence<T>
    {
        /// <summary>
        /// Método que persiste una T y la devuelve 
        /// el identificador obtenido del registro
        /// </summary>
        /// <param name="entidad">T que se desea persistir</param>
        /// <returns>T persistida</returns>
        T InsertarEntidad(T entidad);

        /// <summary>
        /// Método que actualiza una entidad
        /// </summary>
        /// <param name="entidad">T actualizada</param>
        void ModificarEntidad(T entidad);

        /// <summary>
        /// Método que realiza un borrado lógico de la entidad
        /// </summary>
        /// <param name="entidadId">Identficiador de la T a eliminar</param>
        void EliminarEntidad(Object entidadId);

        /// <summary>
        /// Método que devuelve una entidad
        /// a partir de su identificador
        /// </summary>
        /// <param name="entidadId">Identificador de la entidad</param>
        /// <returns>T que se recupera</returns>
        T GetEntidad(Object entidadId);

        /// <summary>
        /// Método que devuelve todas las entidades activas
        /// </summary>
        /// <returns>Entidades activas</returns>
        IList<T> GetEntidades();

        /// <summary>
        /// Método que devuelve todas las entidades que 
        /// cumplan con la T que se pasa como filtro de búsqueda
        /// </summary>
        /// <param name="entidadFiltro">T utilizada de filtro</param>
        /// <returns>Entidades filtradas</returns>
        IList<T> GetEntidades(T entidadFiltro);

        /// <summary>
        /// Método que devuelve un DataSeT con todas las entidades activas
        /// </summary>
        /// <returns>DataSeT con las entidades activas</returns>
        DataSet GetEntidadesDs();

        /// <summary>
        /// Método que devuelve un DataSeT con todas las entidades que 
        /// cumplan con la T que se pasa como filtro de búsqueda
        /// </summary>
        /// <param name="entidadFiltro">T utilizada de filtro</param>
        /// <returns>DataSeT con las entidades filtradas</returns>
        DataSet GetEntidadesDs(T entidadFiltro);

        /// <summary>
        /// Método que devuelve un dataset con los datos devueltos por el procedimiento indicado
        /// con los parametros pasados
        /// </summary>
        /// <param name="storeProcedure">Nombre del procedimiento</param>
        /// <param name="parametros">Parametros necesitados por el procedimiento</param>
        /// <returns>Dataset con los resultados</returns>
        DataSet GetEntidadesDs(string storeProcedure, IDictionary<string, object> parametros);

    }
}
