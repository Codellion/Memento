using System;

namespace Memento.Persistence.Commons.Annotations
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
    public class Table : Attribute
    {
        public string Name;
    }
}