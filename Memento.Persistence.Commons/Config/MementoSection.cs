using System;
using System.Collections.Generic;
using System.Configuration;
using System.Text;

namespace Memento.Persistence.Commons.Config
{
    public class MementoSection : ConfigurationSection
    {
        [ConfigurationProperty("octopusLocation", DefaultValue = null, IsRequired = false)]
        public string OctopusLocation
        {
            get { return this["octopusLocation"].ToString(); }
            set { this["octopusLocation"] = value; }
        }

        [ConfigurationProperty("persistenceEntities", IsRequired = false)]
        public PersistenceEntities PersistenceEntities
        {
            get { return (PersistenceEntities)this["persistenceEntities"]; }
        }

        [ConfigurationProperty("providerConfig", IsRequired = false)]
        public ProviderConfig ProviderConfig
        {
            get { return (ProviderConfig)this["providerConfig"]; }
            set { this["providerConfig"] = value; }
        }
    }
}
