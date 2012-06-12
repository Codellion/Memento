using System;
using System.Data;

using Memento.DataAccess;

namespace Memento.Persistence
{
    /// <summary>
    /// Clase que permite la utilización de transacciones en el 
    /// módulo de persistencia
    /// </summary>
    public class DataContext : IDisposable
    {
        #region Atributos

        private IDbTransaction _transaccion;
        private IDbConnection _servicioDatos;
        private bool _transaccionAbierta;

        #endregion

        #region Propiedades

        /// <summary>
        /// Base de datos utilizada en la transacción
        /// </summary>
        public IDbConnection ServicioDatos
        {
            get { return _servicioDatos; }
            set { _servicioDatos = value; }
        }

        /// <summary>
        /// Transacción en curso
        /// </summary>
        public IDbTransaction Transaccion
        {
            get { return _transaccion; }
            set { _transaccion = value; }
        }

        #endregion

        #region Constructores

        /// <summary>
        /// Constructor que crea un contexto de ejecución dentro
        /// del marco de aplicaciones MEMENTO
        /// </summary>
        public DataContext()
        {
            ServicioDatos = DataFactoryProvider.GetConnection<Entity>();
            ServicioDatos.Open();
            Transaccion = ServicioDatos.BeginTransaction();
            _transaccionAbierta = true;
        }

        /// <summary>
        /// Constructor que crea un contexto de ejecución dentro
        /// del marco de aplicaciones indicado en la variable entornoBD
        /// </summary>
        /// <param name="entornoBd">Entorno de aplicaciones</param>
        public DataContext(string entornoBd)
        {
            ServicioDatos = DataFactoryProvider.GetConnection<Entity>(entornoBd);
            ServicioDatos.Open();
            Transaccion = ServicioDatos.BeginTransaction();
            _transaccionAbierta = true;
        }

        /// <summary>
        /// Constructor que crea un contexto de ejecución con la bd indicada
        /// </summary>
        /// <param name="bd">Base de datos</param>
        public DataContext(IDbConnection bd)
        {
            ServicioDatos = bd;
            ServicioDatos.Open();
            Transaccion = ServicioDatos.BeginTransaction();
            _transaccionAbierta = true;
        }

        #endregion

        #region Métodos públicos

        /// <summary>
        /// Realiza una cancelación de todas las operaciones de la transacción
        /// actual
        /// </summary>
        public void Rollback()
        {
            if (Transaccion != null)
            {
                Transaccion.Rollback();
                _transaccionAbierta = false;
            }

            ServicioDatos.Close();
        }

        /// <summary>
        /// Realiza un commit de la transacción actual
        /// </summary>
        public void SaveChanges()
        {
            if (Transaccion != null)
            {
                Transaccion.Commit();
                _transaccionAbierta = false;
            }

            ServicioDatos.Close();
        }

        #endregion

        #region Miembros de IDisposable

        /// <summary>
        /// Realiza un rollback en caso de no haber podido completarse la transacción
        /// </summary>
        public void Dispose()
        {
            if (Transaccion != null)
            {
                if(_transaccionAbierta)
                {
                    Transaccion.Rollback();
                    _transaccionAbierta = false;
                }

                Transaccion.Dispose();
            }

            if(ServicioDatos.State == ConnectionState.Open)
            {
                ServicioDatos.Close(); 
            }
        }

        #endregion
    }
}
