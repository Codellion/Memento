using System;
using Memento.Persistence.Commons;
using Memento.Persistence.Commons.Annotations;

namespace Memento.Test.Entities
{
    [Serializable]
    [Table(Name = "Tipo_De_Factura")]
    public class TipoFactura : Entity
    {
        [PrimaryKey(Generator = KeyGenerationType.Memento)]
        [Field(Name = "Tipo_Factura_Id")]
        public long? TipoFacturaId
        {
            set { Set(value); }
            get { return Get<long?>(); }
        }

        [Field(Name = "Descripcion_Tipo_Factura")]
        public string Descripcion
        {
            set { Set(value); }
            get { return Get<string>(); }
        }
    }
}