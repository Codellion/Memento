using System;
using System.Collections.Generic;
using System.Configuration;
using System.Text;

namespace Memento.Persistence.Commons.Config
{
    public class PersistenceEntity : ConfigurationElement
    {
        [ConfigurationProperty("table", IsRequired = true)]
        public string Table
        {
            get { return this["table"].ToString(); }
            set { this["table"] = value; }
        }

        [ConfigurationProperty("mapperClass", IsRequired = true)]
        public string MapperClass
        {
            get { return this["mapperClass"].ToString(); }
            set { this["mapperClass"] = value; }
        }
    }
}
