using Memento.Persistence;

namespace Memento.Test.Entities
{
    public class Producto : Entity
    {
        public int? ProductoId { set; get; }
        public string Nombre { set; get; }

        private Dependences<Linea> _lineas;
        public Dependences<Linea> Lineas
        {
            get { return _lineas ?? (_lineas = new Dependences<Linea>("Producto", this)); }
            set { _lineas = value; }
        }
    }
}
