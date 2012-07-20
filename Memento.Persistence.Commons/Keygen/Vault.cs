using System;
using System.Collections.Generic;

namespace Memento.Persistence.Commons.Keygen
{
    [Serializable]
    public class Vault : IDisposable
    {
        IDictionary<string, KeyVault> EntitiesVault { get; set; }

        public string[] Keys { get; set; }
        public KeyVault[] Values { get; set; }

        public Vault()
        {
            EntitiesVault = new Dictionary<string, KeyVault>();
        }

        public KeyVault GetClassVault(string className)
        {
            KeyVault res = null;

            if(EntitiesVault.ContainsKey(className))
            {
                res = EntitiesVault[className];
            }

            return res;
        }

        public void SetClassVault(KeyVault classVault)
        {
            EntitiesVault[classVault.FullClass] = classVault;
        }

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

        public void PrepareDeserialize()
        {
            if(Keys == null)
            {
                return;
            }
            
            for(var i = 0; i < Keys.Length; i++)
            {
                EntitiesVault[Keys[i]] = Values[i];
            }
        }

        public void Dispose()
        {
            KeyGeneration.Synchronize();
        }
    }
}
