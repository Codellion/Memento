using System;
using Memento.Persistence.Commons;

namespace Memento.Persistence
{
    /// <summary>
    /// Clase que representa la referencia de una entidad
    /// </summary>
    /// <typeparam name="T">Tipo del valor almacenado</typeparam>
    [Serializable]
    public class Reference<T> : EaterEntity where T : Entity
    {
        #region Atributos

        #endregion

        #region Propiedades

        /// <summary>
        /// Propiedad que sirve para almacenar el valor
        /// de la referencia
        /// </summary>
        public T Value { get; set; }

        #endregion

        #region Constructores

        /// <summary>
        /// Constructor por defecto
        /// </summary>
        public Reference()
        {
        }

        /// <summary>
        /// Constructor donde se establece el valor del id
        /// </summary>
        public Reference(object valueId)
        {
            Value = Activator.CreateInstance<T>();
            Value.SetEntityId(valueId);
        }

        /// <summary>
        /// Constructor donde se establece el valor
        /// </summary>
        public Reference(T value)
        {
            Value = value;
        }

        #endregion
    }
}