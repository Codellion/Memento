using System;

namespace Memento.Persistence.Commons.Annotations
{
    [AttributeUsage(AttributeTargets.Property)]
    public class Relation : Attribute
    {
        private readonly string _fieldName;
        private readonly string _propertyName;
        private readonly RelationType _type = RelationType.Reference;

        public Relation(string fieldName, string propertyName, RelationType type)
        {
            _fieldName = fieldName;
            _propertyName = propertyName;
            _type = type;
        }

        public string FieldName
        {
            get { return _fieldName; }
        }

        public string PropertyName
        {
            get { return _propertyName; }
        }

        public RelationType Type
        {
            get { return _type; }
        }
    }
}