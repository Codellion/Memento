using System;
using System.Configuration;
using System.Data;
using System.Reflection;
using Memento.DataAccess.Interfaces;
using Memento.Persistence.Commons;

namespace Memento.DataAccess
{
    /// <summary>
    /// Factoria de acceso a datos del módulo de persistencia
    /// </summary>
    public static class DataFactoryProvider
    {
        /// <summary>
        /// Devuelve una implementación del interfaz de acceso a datos
        /// en función de la tipología de la entidad
        /// </summary>
        /// <typeparam name="T">Tipo de la entidad sobre la que se quiere obtener el servicio</typeparam>
        /// <param name="transaction">Transaccion abierta</param>
        /// <returns>Implementación del interfaz de acceso a datos</returns>
        public static IDataPersistence GetProvider<T>(IDbTransaction transaction)
        {
            IDataPersistence res = null;

            if (IsEntity(typeof (T)))
            {
                string proveedorPers = ConfigurationManager.AppSettings["proveedorPersistencia"];
                string assemblyPers = ConfigurationManager.AppSettings["emsambladoProveedorPersistencia"];

                Type tProveedor = null;

                if (!string.IsNullOrEmpty(proveedorPers))
                {
                    Assembly asm = Assembly.LoadFile(AppDomain.CurrentDomain.BaseDirectory + assemblyPers);
                    tProveedor = asm.GetType(proveedorPers);
                }

                if (tProveedor != null)
                {
                    var genParam = new Type[1];
                    genParam[0] = typeof (T);

                    tProveedor = tProveedor.MakeGenericType(genParam);
                    object[] parametros = null;

                    if (transaction != null)
                    {
                        parametros = new object[1];
                        parametros[0] = transaction;
                    }

                    res = Activator.CreateInstance(tProveedor, parametros) as IDataPersistence;
                }
            }

            return res;
        }


        public static IDbConnection GetConnection<T>()
        {
            return GetConnection<T>(null);
        }

        public static IDbConnection GetConnection<T>(string entornoBd)
        {
            string proveedorPers = ConfigurationManager.AppSettings["proveedorPersistencia"];
            string assemblyPers = ConfigurationManager.AppSettings["emsambladoProveedorPersistencia"];

            Type tProveedor = null;

            if (!string.IsNullOrEmpty(proveedorPers))
            {
                Assembly asm = Assembly.LoadFile(AppDomain.CurrentDomain.BaseDirectory + assemblyPers);
                tProveedor = asm.GetType(proveedorPers);
            }

            if (tProveedor != null)
            {
                var genParam = new Type[1];
                genParam[0] = typeof (T);

                tProveedor = tProveedor.MakeGenericType(genParam);

                IDataPersistence res;
                if (string.IsNullOrEmpty(entornoBd))
                {
                    res = Activator.CreateInstance(tProveedor) as IDataPersistence;
                }
                else
                {
                    var param = new object[1];
                    param[0] = entornoBd;

                    res = Activator.CreateInstance(tProveedor, param) as IDataPersistence;
                }

                if (res != null)
                {
                    return res.GetConnection();
                }
            }


            return null;
        }

        /// <summary>
        /// Comprueba si el tipo hereda de la clase Entidad
        /// </summary>
        /// <param name="type">Tipo</param>
        /// <returns>Verdadero si el tipo hereda de Entidad</returns>
        private static bool IsEntity(Type type)
        {
            return (Activator.CreateInstance(type) is Entity);
        }
    }
}