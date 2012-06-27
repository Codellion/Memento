using Memento.Persistence;
using Memento.Persistence.Commons;

namespace Memento.Test.Entities
{
    public class DetalleLinea : Entity
    {
        public long? DetalleLineaId
        {
            set { Set("DetalleLineaId", value); }
            get { return Get<long?>("DetalleLineaId"); }
        }
        public string Detalle
        {
            set { Set("Detalle", value); }
            get { return Get<string>("Detalle"); }
        }

        public Reference<Linea> Linea { set; get; }
    }
}
