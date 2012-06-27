using Memento.Persistence;
using Memento.Persistence.Commons;

namespace Memento.Test.Entities
{
    public class Factura : Entity
    {
        public long? FacturaId
        {
            set { Set("FacturaId", value); }
            get { return Get<long?>("FacturaId"); }
        }
        public float? Importe
        {
            set { Set("Importe", value); }
            get { return Get<float?>("Importe"); }
        }

        public Reference<Cliente> Cliente { set; get; }

        private Dependences<Linea> _lineas;
        public Dependences<Linea> Lineas
        {
            get { return _lineas ?? (_lineas = new Dependences<Linea>("Factura", this)); }
            set { _lineas = value; }
        }
    }
}
