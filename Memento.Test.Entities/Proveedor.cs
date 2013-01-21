using System;
using Memento.Persistence;
using Memento.Persistence.Commons;
using Memento.Persistence.Commons.Annotations;

namespace Memento.Test.Entities
{
    [Serializable]
    public class Proveedor : Entity
    {
        [PrimaryKey(Generator = KeyGenerationType.Database)]
        [Field(Name = "ProveedorId")]
        public int? ProveedorId
        {
            set { Set(value); }
            get { return Get<int?>(); }
        }

        [Field(Required = true)]
        public string Nombre
        {
            set { Set(value); }
            get { return Get<string>(); }
        }

        [Field]
        public string Telefono
        {
            set { Set(value); }
            get { return Get<string>(); }
        }

        [Field]
        public string Email
        {
            set { Set(value); }
            get { return Get<string>(); }
        }

        [Relation("Proveedor", RelationType.Dependences)]
        public Dependences<ProductoProveedor> Productos
        {
            set { Set(value); }
            get { return Get<Dependences<ProductoProveedor>>(); }
        }
    }
}