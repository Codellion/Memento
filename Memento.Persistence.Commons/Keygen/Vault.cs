using System;
using System.Collections.Generic;

namespace Memento.Persistence.Commons.Keygen
{
    /// <summary>
    /// Baúl de claves utilizado para almacenar los siguientes 
    /// identificadores a utilizar por cada tipo de entidad
    /// </summary>
    [Serializable]
    public class Vault : IDisposable
    {
        #region Atributos

        /// <summary>
        /// Diccionario de claves por tipo de entidad
        /// </summary>
        IDictionary<string, KeyVault> EntitiesVault { get; set; }

        #endregion

        #region Propiedades

        /// <summary>
        /// Array de tipos de entidad almacenados en el baúl
        /// </summary>
        public string[] Keys { get; set; }

        /// <summary>
        /// Array de identificadores asociados a cada tipo de entidad del baúl
        /// </summary>
        public KeyVault[] Values { get; set; }

        #endregion

        #region Constructores

        /// <summary>
        /// Constructor que inicializa el baúl de claves
        /// </summary>
        public Vault()
        {
            EntitiesVault = new Dictionary<string, KeyVault>();
        }

        #endregion

        #region Métodso Públicos

        /// <summary>
        /// Obtiene la siguiente clave única asociada a una clase
        /// </summary>
        /// <param name="className">Nombre completo de la clases</param>
        /// <returns>Siguiente clave única</returns>
        public KeyVault GetClassVault(string className)
        {
            KeyVault res = null;

            if(EntitiesVault.ContainsKey(className))
            {
                res = EntitiesVault[className];
            }

            return res;
        }

        /// <summary>
        /// Establece la siguiente clave única para una clase
        /// </summary>
        /// <param name="classVault">Siguiente clave única</param>
        public void SetClassVault(KeyVault classVault)
        {
            EntitiesVault[classVault.FullClass] = classVault;
        }

        /// <summary>
        /// Sincroniza los arrays de claves y valores con el contenido del diccionario
        /// </summary>
        public void PrepareSerialize()
        {
            if(EntitiesVault.Count == 0)
            {
                return;
            }

            Keys = new string[EntitiesVault.Count];
            Values = new KeyVault[EntitiesVault.Count];

            EntitiesVault.Keys.CopyTo(Keys, 0);
            EntitiesVault.Values.CopyTo(Values, 0);
        }

        /// <summary>
        /// Restablece el contenido del diccionario a partir de los arrays serializados
        /// </summary>
        public void PrepareDeserialize()
        {
            if(Keys == null)
            {
                return;
            }
            
            for(int i = 0; i < Keys.Length; i++)
            {
                EntitiesVault[Keys[i]] = Values[i];
            }
        }

        /// <summary>
        /// Al liberarse los recursos nos aseguramos que se sincroniza el baúl
        /// </summary>
        public void Dispose()
        {
            KeyGeneration.Synchronize();
        }

        #endregion
    }
}
