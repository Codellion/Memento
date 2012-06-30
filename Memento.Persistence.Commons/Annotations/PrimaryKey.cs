using System;

namespace Memento.Persistence.Commons.Annotations
{
    [AttributeUsage(AttributeTargets.Property)]
    public class PrimaryKey : Attribute
    {
        public KeyGenerationType Generator = KeyGenerationType.Memento;
    }
}