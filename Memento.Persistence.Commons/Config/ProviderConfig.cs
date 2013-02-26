using System;
using System.Collections.Generic;
using System.Configuration;
using System.Text;

namespace Memento.Persistence.Commons.Config
{
    public class ProviderConfig : ConfigurationElement
    {
        [ConfigurationProperty("class", IsRequired = true)]
        public string Class
        {
            get { return this["class"].ToString(); }
            set { this["class"] = value; }
        }

        [ConfigurationProperty("assembly", IsRequired = true)]
        public string Assembly
        {
            get { return this["assembly"].ToString(); }
            set { this["assembly"] = value; }
        }

        [ConfigurationProperty("dbKeyCommand", IsRequired = true)]
        public string DbKeyCommand
        {
            get { return this["dbKeyCommand"].ToString(); }
            set { this["dbKeyCommand"] = value; }
        }
    }
}
