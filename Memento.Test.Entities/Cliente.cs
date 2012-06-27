using Memento.Persistence;
using Memento.Persistence.Commons;

namespace Memento.Test.Entities
{
    public class Cliente : Entity
    {
        public int? ClienteId
        {
            set { Set("ClienteId", value); }
            get { return Get<int?>("ClienteId"); }
        }
        public string Nombre
        {
             set { Set("Nombre", value); }
             get { return Get<string>("Nombre"); }
        }

        private Dependences<Factura> _facturas;
        public Dependences<Factura> Facturas
        {
            get { return _facturas ?? (_facturas = new Dependences<Factura>("Cliente", this)); }
            set { _facturas = value; }
        }
    }
}
