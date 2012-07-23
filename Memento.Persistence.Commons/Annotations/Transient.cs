using System;

namespace Memento.Persistence.Commons.Annotations
{
    /// <summary>
    /// Anotación que representa una propiedad de una entidad que no se persiste en BBDD
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class Transient : Attribute
    {
    }
}