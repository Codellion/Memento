using Memento.Persistence;
using Memento.Persistence.Commons;

namespace Memento.Test.Entities
{
    public class Cliente : Entity
    {
        public int? ClienteId { set; get; }
        public string Nombre { set; get; }

        private Dependences<Factura> _facturas;
        public Dependences<Factura> Facturas
        {
            get { return _facturas ?? (_facturas = new Dependences<Factura>("Cliente", this)); }
            set { _facturas = value; }
        }
    }
}
