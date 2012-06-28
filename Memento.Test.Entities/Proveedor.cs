using Memento.Persistence;
using Memento.Persistence.Commons;

namespace Memento.Test.Entities
{
    public class Proveedor : Entity
    {
        public int? ProveedorId
        {
            set { Set("ProveedorId", value); }
            get { return Get<int?>("ProveedorId"); }
        }
        public string Nombre
        {
             set { Set("Nombre", value); }
             get { return Get<string>("Nombre"); }
        }
        public string Telefono
        {
            set { Set("Telefono", value); }
            get { return Get<string>("Telefono"); }
        }
        public string Email
        {
            set { Set("Email", value); }
            get { return Get<string>("Email"); }
        }

        private Dependences<ProductoProveedor> _productos;
        public Dependences<ProductoProveedor> Productos
        {
            get { return _productos ?? (_productos = new Dependences<ProductoProveedor>("Proveedor", this)); }
            set { _productos = value; }
        }
    }
}
