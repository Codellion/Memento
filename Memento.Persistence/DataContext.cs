using System;
using System.Data;
using Memento.DataAccess;
using Memento.Persistence.Commons;

namespace Memento.Persistence
{
    /// <summary>
    /// Clase que permite la utilización de transacciones en el 
    /// módulo de persistencia
    /// </summary>
    public class DataContext : IDisposable
    {
        #region Atributos

        private bool _isOpenTransaction;

        #endregion

        #region Propiedades

        /// <summary>
        /// Base de datos utilizada en la transacción
        /// </summary>
        private IDbConnection Connection { get; set; }

        /// <summary>
        /// Transacción en curso
        /// </summary>
        public IDbTransaction Transaction { get; private set; }

        #endregion

        #region Constructores

        /// <summary>
        /// Constructor que crea un contexto de ejecución dentro
        /// del marco de aplicaciones MEMENTO
        /// </summary>
        public DataContext()
        {
            Connection = DataFactoryProvider.GetConnection<Entity>();
            Connection.Open();
            Transaction = Connection.BeginTransaction();
            _isOpenTransaction = true;
        }

        /// <summary>
        /// Constructor que crea un contexto de ejecución dentro
        /// del marco de aplicaciones indicado en la variable entornoBD
        /// </summary>
        /// <param name="entornoBd">Entorno de aplicaciones</param>
        public DataContext(string entornoBd)
        {
            Connection = DataFactoryProvider.GetConnection<Entity>(entornoBd);
            Connection.Open();
            Transaction = Connection.BeginTransaction();
            _isOpenTransaction = true;
        }

        /// <summary>
        /// Constructor que crea un contexto de ejecución con la bd indicada
        /// </summary>
        /// <param name="bd">Base de datos</param>
        public DataContext(IDbConnection bd)
        {
            Connection = bd;
            Connection.Open();
            Transaction = Connection.BeginTransaction();
            _isOpenTransaction = true;
        }

        #endregion

        #region Métodos públicos

        /// <summary>
        /// Realiza una cancelación de todas las operaciones de la transacción
        /// actual
        /// </summary>
        public void Rollback()
        {
            if (Transaction != null)
            {
                Transaction.Rollback();
                _isOpenTransaction = false;
            }

            Connection.Close();
        }

        /// <summary>
        /// Realiza un commit de la transacción actual
        /// </summary>
        public void SaveChanges()
        {
            if (Transaction != null)
            {
                Transaction.Commit();
                _isOpenTransaction = false;
            }

            Connection.Close();
        }

        #endregion

        #region Miembros de IDisposable

        /// <summary>
        /// Realiza un rollback en caso de no haber podido completarse la transacción
        /// </summary>
        public void Dispose()
        {
            if (Transaction != null)
            {
                if (_isOpenTransaction)
                {
                    Transaction.Rollback();
                    _isOpenTransaction = false;
                }

                Transaction.Dispose();
            }

            if (Connection.State == ConnectionState.Open)
            {
                Connection.Close();
            }
        }

        #endregion
    }
}