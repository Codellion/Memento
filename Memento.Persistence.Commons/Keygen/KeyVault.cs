using System;

namespace Memento.Persistence.Commons.Keygen
{
    /// <summary>
    /// Clase que representa la siguiente clave disponible para un tipo de entidad
    /// </summary>
    [Serializable]
    public class KeyVault
    {
        /// <summary>
        /// Nombre completo de la clase asociada a la clave
        /// </summary>
        public string FullClass { get; set; }

        /// <summary>
        /// Siguiente clave
        /// </summary>
        public long NextKey { get; set; }
    }
}
