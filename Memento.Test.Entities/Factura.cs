﻿using System;
using Memento.Persistence;
using Memento.Persistence.Commons;
using Memento.Persistence.Commons.Annotations;

namespace Memento.Test.Entities
{
    [Serializable]
    public class Factura : Entity
    {
        [PrimaryKey(Generator = KeyGenerationType.Database)]
        [Field(Name = "FacturaId")]
        public long? FacturaId
        {
            set { Set(value); }
            get { return Get<long?>(); }
        }

        [Field(Required = true)]
        public float? Importe
        {
            set { Set(value); }
            get { return Get<float?>(); }
        }

        [Relation("Facturas", RelationType.Reference)]
        public Reference<Cliente> Cliente { set; get; }

        [Relation("Factura", RelationType.Dependences)]
        public Dependences<Linea> Lineas
        {
            set { Set(value); }
            get { return Get<Dependences<Linea>>(); }
        }
    }
}