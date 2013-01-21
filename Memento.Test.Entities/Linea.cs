using System;
using Memento.Persistence;
using Memento.Persistence.Commons;
using Memento.Persistence.Commons.Annotations;

namespace Memento.Test.Entities
{
    [Serializable]
    public class Linea : Entity
    {
        [PrimaryKey(Generator = KeyGenerationType.Database)]
        [Field(Name = "LineaId")]
        public long? LineaId
        {
            set { Set(value); }
            get { return Get<long?>(); }
        }

        [Field]
        public string Descripcion
        {
            set { Set(value); }
            get { return Get<string>(); }
        }

        [Field]
        public int? Cantidad
        {
            set { Set(value); }
            get { return Get<int?>(); }
        }

        [Relation("Lineas", RelationType.Reference)]
        public Reference<Producto> Producto { set; get; }

        [Relation("Lineas", RelationType.Reference)]
        public Reference<Factura> Factura { set; get; }

        [Relation("Linea", RelationType.Dependence)]
        public Dependence<DetalleLinea> DetalleLinea
        {
            set { Set(value); }
            get { return Get<Dependence<DetalleLinea>>(); }
        }
    }
}