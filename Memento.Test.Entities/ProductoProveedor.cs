using System;
using Memento.Persistence;
using Memento.Persistence.Commons;
using Memento.Persistence.Commons.Annotations;

namespace Memento.Test.Entities
{
    [Serializable]
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

        [Relation("Proveedores", RelationType.Reference)]
        public Reference<Producto> Producto { set; get; }

        [Relation("Productos", RelationType.Reference)]
        public Reference<Proveedor> Proveedor { set; get; }
    }
}