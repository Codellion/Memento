using System;
using Memento.Persistence;
using Memento.Persistence.Commons;
using Memento.Persistence.Commons.Annotations;

namespace Memento.Test.Entities
{
    [Serializable]
    [Table(Name = "Cliente")]
    public class Cliente : Entity
    {
        [PrimaryKey(Generator = KeyGenerationType.Database)]
        [Field(Name = "ClienteId")]
        public int? ClienteId
        {
            set { Set(value); }
            get { return Get<int?>(); }
        }

        [Field(Name = "Nombre", Required = true)]
        public string Nombre
        {
            set { Set(value); }
            get { return Get<string>(); }
        }

        [Relation("Cliente", RelationType.Dependences)]
        public Dependences<Factura> Facturas
        {
            set { Set(value); }
            get { return Get<Dependences<Factura>>(); }
        }
    }
}