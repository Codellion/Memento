using Memento.Persistence;
using Memento.Persistence.Commons;

namespace Memento.Test.Entities
{
    public class ProductoProveedor : NmEntity
    {
        public long? ProductoProveedorId
        {
            set { Set("ProductoProveedorId", value); }
            get { return Get<long?>("ProductoProveedorId"); }
        }
        public float? Precio
        {
            set { Set("Precio", value); }
            get { return Get<float?>("Precio"); }
        }

        public Reference<Producto> Producto { set; get; }
        public Reference<Proveedor> Proveedor { set; get; }
    }
}
