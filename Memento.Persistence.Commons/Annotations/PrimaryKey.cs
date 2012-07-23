using System;

namespace Memento.Persistence.Commons.Annotations
{
    /// <summary>
    /// Anotación que representa la clave principal de una entidad
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class PrimaryKey : Attribute
    {
        /// <summary>
        /// Método para gestionar las claves de la entidad
        /// </summary>
        public KeyGenerationType Generator = KeyGenerationType.Memento;

        /// <summary>
        /// Indica la longitud del campo en BBDD (Para futuras versiones)
        /// </summary>
        public int Length;
    }
}