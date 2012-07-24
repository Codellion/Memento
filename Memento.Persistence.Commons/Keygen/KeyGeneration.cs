using System.IO;
using System.Xml.Serialization;

namespace Memento.Persistence.Commons.Keygen
{
    /// <summary>
    /// Clase encargada de gestionar la claves primarias en Memento
    /// </summary>
    public static class KeyGeneration
    {

        #region Atributos

        /// <summary>
        /// Booleano que indica si el baúl de claves está sincronizado
        /// </summary>
        private static bool _isSynchronize = true;

        /// <summary>
        /// Baúl de claves
        /// </summary>
        private static Vault _vault;

        /// <summary>
        /// Baúl de claves
        /// </summary>
        static Vault Vault
        {
            get
            {
                if(_vault == null)
                {
                    if(File.Exists("Vault.keys"))
                    {   
                        Stream fVault = new FileStream("Vault.keys", FileMode.Open);

                        var unMarshall = new XmlSerializer(typeof(Vault));
                        _vault = (Vault) unMarshall.Deserialize(fVault);

                        _vault.PrepareDeserialize();

                        fVault.Close();
                    }
                    else
                    {
                        _vault = new Vault();
                    }
                }

                return _vault;
            }
        }

        #endregion

        #region Enumerados

        /// <summary>
        /// Tipo de la clave a generar
        /// </summary>
        enum PassType
        {
            Numeric,
            Hexadecimal
        }

        #endregion

        #region Métodos Privados

        /// <summary>
        /// Método que devuelve el tipo de clave a generar según el tipo de la entidad
        /// </summary>
        /// <typeparam name="T">Tipo de la entidad</typeparam>
        /// <param name="entity">Entidad</param>
        /// <returns>Tipo de clave a generar</returns>
        static PassType GetPassType<T>(T entity) where T: Entity
        {
            PassType pType = PassType.Numeric;

            if(entity.GetType().GetProperty(entity.GetEntityIdName()).PropertyType == typeof(string))
            {
                pType = PassType.Hexadecimal;
            }

            return pType;
        }

        /// <summary>
        /// Método que devuelve la siguiente clave numérica asociada a la clase
        /// </summary>
        /// <typeparam name="T">Tipo de la entidad</typeparam>
        /// <returns>Siguiente clave numérica</returns>
        static long GetNumericKey<T>()
        {
            long res;

            string className = typeof(T).FullName;

            KeyVault classVault = Vault.GetClassVault(className);

            if (classVault != null)
            {
                res = classVault.NextKey;
            }
            else
            {
                classVault = new KeyVault { FullClass = className, NextKey = 1};
                res = classVault.NextKey;
            }

            classVault.NextKey++;

            Vault.SetClassVault(classVault);
            
            return res;
        }

        /// <summary>
        /// Método que devuelve la siguiente clave hexadecimal asociada a la clase
        /// </summary>
        /// <typeparam name="T">Tipo de la entidad</typeparam>
        /// <returns>Siguiente clave hexadecimal</returns>
        static string GetHexKey<T>()
        {
            string res;

            string className = typeof (T).FullName;

            KeyVault classVault = Vault.GetClassVault(className);

            if(classVault != null)
            {
                res = string.Format("{0:X}", classVault.NextKey);
             
            }
            else
            {
                classVault = new KeyVault {FullClass = className, NextKey = 1};

                res = string.Format("{0:X}", classVault.NextKey);
            }

            classVault.NextKey++;

            Vault.SetClassVault(classVault);

            return res;
        }

        #endregion

        #region Métodos Públicos

        /// <summary>
        /// Método que se encarga de sincronizar el baúl de claves
        /// </summary>
        public static void Synchronize()
        {
            if(!_isSynchronize)
            {
                Vault.PrepareSerialize();

                Stream data = new FileStream("Vault.keys", FileMode.Create);

                var marshall = new XmlSerializer(typeof(Vault));

                marshall.Serialize(data, Vault);

                _isSynchronize = true;
            }
        }

        /// <summary>
        /// Método que devuelve la siguiente clave asociada al tipo de la entidad
        /// </summary>
        /// <typeparam name="T">Tipo de la entidad</typeparam>
        /// <param name="entity">Entidad</param>
        /// <returns>Siguiente clave única</returns>
        public static object GetPrimaryKey<T>(T entity) where T : Entity
        {
            object res = null;

            PassType pType = GetPassType(entity);

            switch (pType)
            {
                case PassType.Numeric:

                    res = GetNumericKey<T>();
                    break;

                case PassType.Hexadecimal:

                    res = GetHexKey<T>();
                    break;
            }
            
            _isSynchronize = false;

            return res;
        }

        #endregion
    }
}
