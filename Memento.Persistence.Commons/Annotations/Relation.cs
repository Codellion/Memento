using System;

namespace Memento.Persistence.Commons.Annotations
{
    /// <summary>
    /// Anotación que representa la relación entre 2 o más entidades
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class Relation : Attribute
    {
        /// <summary>
        /// Nombre de la propiedad que representa la entidad actual
        /// en la entidad relacionada
        /// </summary>
        private readonly string _propertyName;

        /// <summary>
        /// Tipo de relación
        /// </summary>
        private readonly RelationType _type = RelationType.Reference;

        /// <summary>
        /// Nombre de la propiedad que representa la entidad actual
        /// en la entidad relacionada
        /// </summary>
        public string PropertyName
        {
            get { return _propertyName; }
        }

        /// <summary>
        /// Tipo de relación
        /// </summary>
        public RelationType Type
        {
            get { return _type; }
        }

        /// <summary>
        /// Constructor por defecto
        /// </summary>
        /// <param name="propertyName">Nombre de la propiedad que representa la entidad actual
        /// en la entidad relacionada</param>
        /// <param name="type">Tipo de la relación</param>
        public Relation(string propertyName, RelationType type)
        {
            _propertyName = propertyName;
            _type = type;
        }
    }
}