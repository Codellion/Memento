using System;

namespace Memento.Persistence.Commons
{
    /// <summary>
    /// Clase abstracta de la que heredan aquellas entidades N-M que se
    /// desean persistir mediante el módulo de persistencia
    /// </summary>
    [Serializable]
    public abstract class NmEntity : Entity
    {
    }
}
