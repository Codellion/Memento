using Memento.Persistence;

namespace Memento.Test.Entities
{
    public class Factura : Entity
    {
        public long? FacturaId { set; get; }
        public float? Importe { set; get; }

        public Reference<Cliente> Cliente { set; get; }

        private Dependences<Linea> _lineas;
        public Dependences<Linea> Lineas
        {
            get { return _lineas ?? (_lineas = new Dependences<Linea>("Factura", this)); }
            set { _lineas = value; }
        }
    }
}
