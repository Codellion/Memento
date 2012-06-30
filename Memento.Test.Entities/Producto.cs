using System.Data;
using Memento.Persistence;
using Memento.Persistence.Commons;
using Memento.Persistence.Commons.Annotations;

namespace Memento.Test.Entities
{
    public class Producto : Entity
    {
        [PrimaryKey(Generator = KeyGenerationType.Database)]
        [Field(Name = "ProductoId")]
        public long? ProductoId
        {
            set { Set(value); }
            get { return Get<long?>(); }
        }

        [Field]
        public string Nombre
        {
            set { Set(value); }
            get { return Get<string>(); }
        }

        [Relation("ProductoId", "Producto", RelationType.Dependences)]
        public Dependences<Linea> Lineas
        {
            set { Set(value); }
            get { return Get<Dependences<Linea>>(); }
        }

        [Relation("ProductoId", "Producto", RelationType.Dependences)]
        public Dependences<ProductoProveedor> Proveedores
        {
            set { Set(value); }
            get { return Get<Dependences<ProductoProveedor>>(); }
        }
    }
}