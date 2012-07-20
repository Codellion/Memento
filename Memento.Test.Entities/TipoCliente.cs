using Memento.Persistence.Commons;
using Memento.Persistence.Commons.Annotations;

namespace Memento.Test.Entities
{
    [Table(Name = "TipoCliente")]
    public class TipoCliente : Entity
    {
        [PrimaryKey(Generator = KeyGenerationType.Memento)]
        [Field(Name = "tipoDeClienteID")]
        public string TipoClienteId
        {
            set { Set(value); }
            get { return Get<string>(); }
        }

        [Field(Name = "Descript")]
        public string Descripcion
        {
            set { Set(value); }
            get { return Get<string>(); }
        }
    }
}