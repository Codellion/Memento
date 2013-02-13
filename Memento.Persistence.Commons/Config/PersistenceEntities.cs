using System;
using System.Collections.Generic;
using System.Configuration;
using System.Text;

namespace Memento.Persistence.Commons.Config
{
    [ConfigurationCollection(typeof(PersistenceEntity), AddItemName = "entity", CollectionType = ConfigurationElementCollectionType.BasicMap)]
    public class PersistenceEntities : ConfigurationElementCollection
    {
        protected override ConfigurationElement CreateNewElement()
        {
            return new PersistenceEntity();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            if (element == null)
                throw new ArgumentNullException("element");

            return ((PersistenceEntity)element).MapperClass;
        }

        public new PersistenceEntity this[string key]
        {
            get { return (PersistenceEntity)BaseGet(key); }
        }

        public PersistenceEntity this[int index]
        {
            get { return (PersistenceEntity)BaseGet(index); }
        }
    }
}
