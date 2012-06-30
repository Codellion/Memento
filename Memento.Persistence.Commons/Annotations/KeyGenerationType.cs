namespace Memento.Persistence.Commons.Annotations
{
    public enum KeyGenerationType
    {
        /// <summary>
        /// Memento autogestiona las claves de las entidades
        /// </summary>
        Memento,

        /// <summary>
        /// La base de datos se encarga de gestionar la generación de la clave (Identities...)
        /// </summary>
        Database,

        /// <summary>
        /// El usuario controla la gestión de claves en las entidades
        /// </summary>
        Unhandled
    }
}