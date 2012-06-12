using Memento.Persistence.Commons;

namespace Memento.Persistence
{
    /// <summary>
    /// Clase que representa la referencia de una entidad
    /// </summary>
    /// <typeparam name="T">Tipo del valor almacenado</typeparam>
    public class Reference<T> where T:Entity
    {
        #region Atributos

        /// <summary>
        /// Atributo privado que sirve para almacenar el valor
        /// de la referencia
        /// </summary>
        private T _value;

        #endregion
        
        #region Propiedades
               
        /// <summary>
        /// Propiedad que sirve para almacenar el valor
        /// de la referencia
        /// </summary>
        public T Value
        {
            get { return _value; }
            set { _value = value; }
        }

        #endregion

        #region Constructores

        /// <summary>
        /// Constructor por defecto
        /// </summary>
        public Reference()
        {
            
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
