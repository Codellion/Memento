namespace Memento.Persistence
{
    /// <summary>
    /// Enumerado que proporciona el estado de una dependencia
    /// </summary>
    public enum StatusDependence
    {
        /// <summary>
        /// No se tiene conocimiento
        /// </summary>
        Unknown,
        /// <summary>
        ///Esta sincronizada con la fuente de datos
        /// /// </summary>
        Synchronized,
        /// <summary>
        ///No se encuentra persistida
        /// /// </summary>
        Created,
        /// <summary>
        ///Tiene propiedades modificadas
        /// /// </summary>
        Modified,
        /// <summary>
        /// Se ha eliminado de la fuente de datos
        /// </summary>
        Deleted
    }
}