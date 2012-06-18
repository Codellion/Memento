using Memento.Persistence;
using Memento.Persistence.Commons;

namespace Memento.Test.Entities
{
    public class DetalleLinea : Entity
    {
        public long? DetalleLineaId { set; get; }
        public string Detalle { set; get; }

        public Reference<Linea> Linea { set; get; }
    }
}
