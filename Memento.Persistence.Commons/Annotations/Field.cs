using System;

namespace Memento.Persistence.Commons.Annotations
{
    /// <summary>
    /// Anotación que representa un campo de BBDD
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class Field : Attribute
    {
        /// <summary>
        /// Nombre de la columna en BBDD
        /// </summary>
        public string Name;

        /// <summary>
        /// Indica si el campo es obligatorio (Para futuras versiones)
        /// </summary>
        public bool Required;
    }
}