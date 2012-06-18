using Memento.Persistence;
using Memento.Persistence.Commons;

namespace Memento.Test.Entities
{
    public class Linea : Entity
    {
        public long? LineaId { set; get; }
        public string Descripcion { set; get; }
        public int? Cantidad { set; get; }

        private Dependence<DetalleLinea> _detalleLinea;
        public Dependence<DetalleLinea> DetalleLinea
        {
            get { return _detalleLinea ?? (_detalleLinea = new Dependence<DetalleLinea>("Linea", this)); }
            set { _detalleLinea = value; }
        }

        public Reference<Producto> Producto { set; get; }
        public Reference<Factura> Factura { set; get; }
    }
}
