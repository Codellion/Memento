using System;
using Memento.Persistence;
using Memento.Persistence.Commons;
using Memento.Persistence.Commons.Annotations;

namespace Memento.Test.Entities
{
    [Serializable]
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

        [Relation("Producto", RelationType.Dependences)]
        public Dependences<Linea> Lineas
        {
            set { Set(value); }
            get { return Get<Dependences<Linea>>(); }
        }

        [Relation("Producto", RelationType.Dependences)]
        public Dependences<ProductoProveedor> Proveedores
        {
            set { Set(value); }
            get { return Get<Dependences<ProductoProveedor>>(); }
        }
    }
}