using System;

namespace Memento.Persistence.Commons.Keygen
{
    [Serializable]
    public class KeyVault
    {
        public string FullClass { get; set; }
        public long NextKey { get; set; }
    }
}
