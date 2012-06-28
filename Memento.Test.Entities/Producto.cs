using Memento.Persistence;
using Memento.Persistence.Commons;

namespace Memento.Test.Entities
{
    public class Producto : Entity
    {
        public long? ProductoId
        {
            set { Set("ProductoId", value); }
            get { return Get<long?>("ProductoId"); }
        }
        public string Nombre
        {
            set { Set("Nombre", value); }
            get { return Get<string>("Nombre"); }
        }

        private Dependences<Linea> _lineas;
        public Dependences<Linea> Lineas
        {
            get { return _lineas ?? (_lineas = new Dependences<Linea>("Producto", this)); }
            set { _lineas = value; }
        }

        private Dependences<ProductoProveedor> _proveedores;
        public Dependences<ProductoProveedor> Proveedores
        {
            get { return _proveedores ?? (_proveedores = new Dependences<ProductoProveedor>("Producto", this)); }
            set { _proveedores = value; }
        }
    }
}
