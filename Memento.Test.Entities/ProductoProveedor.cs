using System.Data;
using Memento.Persistence;
using Memento.Persistence.Commons;
using Memento.Persistence.Commons.Annotations;

namespace Memento.Test.Entities
{
    public class ProductoProveedor : NmEntity
    {
        [PrimaryKey(Generator = KeyGenerationType.Database)]
        [Field(Name = "ProductoProveedorId")]
        public long? ProductoProveedorId
        {
            set { Set(value); }
            get { return Get<long?>(); }
        }

        [Field]
        public float? Precio
        {
            set { Set(value); }
            get { return Get<float?>(); }
        }

        [Relation("ProductoId", "Proveedores", RelationType.Reference)]
        public Reference<Producto> Producto { set; get; }

        [Relation("ProveedorId", "Productos", RelationType.Reference)]
        public Reference<Proveedor> Proveedor { set; get; }
    }
}