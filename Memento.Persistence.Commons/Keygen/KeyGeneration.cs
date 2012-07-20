using System.IO;
using System.Xml.Serialization;

namespace Memento.Persistence.Commons.Keygen
{
    public static class KeyGeneration
    {
        private static bool _isSynchronize;
        private static Vault _vault;

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

        enum PassType
        {
            Numeric,
            Hexadecimal
        }
      
        static PassType GetPassType<T>(T entity) where T: Entity
        {
            PassType pType = PassType.Numeric;

            if(entity.GetType().GetProperty(entity.GetEntityIdName()).PropertyType == typeof(string))
            {
                pType = PassType.Hexadecimal;
            }

            return pType;
        }

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
        
        public static void Synchronize()
        {
            if(!_isSynchronize)
            {
                Stream data = new FileStream("Vault.keys", FileMode.Create);

                Vault.PrepareSerialize();

                var marshall = new XmlSerializer(typeof(Vault));

                marshall.Serialize(data, Vault);

                _isSynchronize = true;
            }
        }

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
    }
}
