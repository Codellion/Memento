using Memento.Persistence;
using Memento.Persistence.Commons;

namespace Memento.Test.Entities
{
    public class Linea : Entity
    {
        public long? LineaId { set; get; }
        public string Descripcion { set; get; }
        public int? Cantidad { set; get; }

        public Reference<Producto> Producto { set; get; }
        public Reference<Factura> Factura { set; get; }
    }
}
