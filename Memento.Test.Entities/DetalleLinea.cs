﻿using Memento.Persistence;
using Memento.Persistence.Commons;
using Memento.Persistence.Commons.Annotations;

namespace Memento.Test.Entities
{
    public class DetalleLinea : Entity
    {
        [PrimaryKey(Generator = KeyGenerationType.Database)]
        [Field(Name = "DetalleLineaId")]
        public long? DetalleLineaId
        {
            set { Set(value); }
            get { return Get<long?>(); }
        }

        [Field]
        public string Detalle
        {
            set { Set(value); }
            get { return Get<string>(); }
        }

        [Relation("LineaId", "DetalleLinea", RelationType.Reference)]
        public Reference<Linea> Linea { set; get; }
    }
}