namespace Memento.Persistence.Commons.Annotations
{
    /// <summary>
    /// Tipos de relaciones entre entidades
    /// </summary>
    public enum RelationType
    {
        /// <summary>
        /// Es una clase referenciada
        /// </summary>
        Reference,

        /// <summary>
        /// Es una clase dependiente
        /// </summary>
        Dependence,

        /// <summary>
        /// Es un conjunto de series dependientes
        /// </summary>
        Dependences
    }
}