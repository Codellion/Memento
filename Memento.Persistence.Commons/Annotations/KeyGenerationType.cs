namespace Memento.Persistence.Commons.Annotations
{
    /// <summary>
    /// Enumerado que representa el tipo de generación de claves para una entidad
    /// </summary>
    public enum KeyGenerationType
    {
        /// <summary>
        /// Memento autogestiona las claves de las entidades
        /// </summary>
        Memento,

        /// <summary>
        /// La base de datos se encarga de gestionar la generación de la clave 
        /// (Por el momento sólo los campos de tipo Identity de SQL Server)
        /// </summary>
        Database,

        /// <summary>
        /// El usuario controla la gestión de claves en las entidades
        /// </summary>
        Unhandled
    }
}