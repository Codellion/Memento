using System.Collections.Generic;
using System.Data;

namespace Memento.DataAccess.Interfaces
{
    /// <summary>
    /// Interfaz de acceso a la capa de datos del módulo de persistencia
    /// </summary>
    /// <typeparam name="T">Tipo de la entidad con la que se va a operar</typeparam>
    public interface IDataPersistence<T>
    {
        /// <summary>
        /// Método que persiste una entidad y la devuelve 
        /// el identificador obtenido del registro
        /// </summary>
        /// <param name="entidad">Entidad que se desea persistir</param>
        /// <returns>Identificador de la entidad persistida</returns>
        object InsertarEntidad(T entidad);

        /// <summary>
        /// Método que actualiza una entidad
        /// </summary>
        /// <param name="entidad">Entidad actualizada</param>
        void ModificarEntidad(T entidad);

        /// <summary>
        /// Método que realiza un borrado lógico de la entidad
        /// </summary>
        /// <param name="entidadId">Identificador de la entidad a eliminar</param>
        void EliminarEntidad(object entidadId);

        /// <summary>
        /// Método que devuelve una entidad
        /// a partir de su identificador
        /// </summary>
        /// <param name="entidadId">Identificador de la entidad</param>
        /// <returns>Entidad que se recupera</returns>
        IDataReader GetEntidad(object entidadId);

        /// <summary>
        /// Método que devuelve todas las entidades activas
        /// </summary>
        /// <returns>Entidades activas</returns>
        IDataReader GetEntidades();

        /// <summary>
        /// Método que devuelve todas las entidades que 
        /// cumplan con la entidad que se pasa como filtro de búsqueda
        /// </summary>
        /// <param name="entidadFiltro">Entidad utilizada de filtro</param>
        /// <returns>Entidades filtradas</returns>
        IDataReader GetEntidades(T entidadFiltro);

        /// <summary>
        /// Método que devuelve un DataSet con todas las entidades activas
        /// </summary>
        /// <returns>DataSet con las entidades activas</returns>
        DataSet GetEntidadesDs();

        /// <summary>
        /// Método que devuelve un DataSet con todas las entidades que 
        /// cumplan con la entidad que se pasa como filtro de búsqueda
        /// </summary>
        /// <param name="entidadFiltro">Entidad utilizada de filtro</param>
        /// <returns>DataSet con las entidades filtradas</returns>
        DataSet GetEntidadesDs(T entidadFiltro);

        /// <summary>
        /// Método que devuelve un dataset con los datos devueltos por el procedimiento indicado
        /// con los parametros pasados
        /// </summary>
        /// <param name="storeProcedure">Nombre del procedimiento</param>
        /// <param name="parametros">Parametros necesitados por el procedimiento</param>
        /// <returns>Dataset con los resultados</returns>
        DataSet GetEntidadesDs(string storeProcedure, IDictionary<string, object> parametros);

        /// <summary>
        /// Método que devuelve una conexión con el proveedor implementado
        /// </summary>
        /// <returns>Conexión</returns>
        IDbConnection GetConnection();
    }
}
