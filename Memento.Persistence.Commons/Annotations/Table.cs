using System;

namespace Memento.Persistence.Commons.Annotations
{
    /// <summary>
    /// Anotación que representa una tabla de BBDD
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
    public class Table : Attribute
    {
        /// <summary>
        /// Nombre de la tabla
        /// </summary>
        public string Name { get; set; }
    }
}