using Memento.Persistence;
using Memento.Persistence.Commons;

namespace Memento.Test.Entities
{
    public class Linea : Entity
    {
        public long? LineaId
        {
            set { Set("LineaId", value); }
            get { return Get<long?>("LineaId"); }
        }
        public string Descripcion
        {
            set { Set("Descripcion", value); }
            get { return Get<string>("Descripcion"); }
        }
        public int? Cantidad
        {
            set { Set("Cantidad", value); }
            get { return Get<int?>("Cantidad"); }
        }

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
