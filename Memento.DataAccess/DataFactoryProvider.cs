using System;
using System.Collections.Specialized;
using System.Configuration;
using System.Data;
using System.IO;
using System.Reflection;
using Memento.DataAccess.Interfaces;
using Memento.Persistence.Commons;
using Memento.Persistence.Commons.Config;

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
                var mementoConfig = ConfigurationManager.GetSection("spock/memento") as MementoSection;

                if (mementoConfig == null || mementoConfig.ProviderConfig == null)
                {
                    return null;
                }

                var providerConfig = mementoConfig.ProviderConfig;

                string proveedorPers = providerConfig.Class;
                string assemblyPers = providerConfig.Assembly;

                Type tProveedor = null;

                if (!string.IsNullOrEmpty(proveedorPers))
                {
                    Assembly asm = Assembly.LoadFrom(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + Path.DirectorySeparatorChar + assemblyPers);
                    tProveedor = asm.GetType(proveedorPers);
                }

                if (tProveedor != null)
                {
                    Type[] genParam = new Type[1];
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
            var mementoConfig = ConfigurationManager.GetSection("spock/memento") as MementoSection;

            if (mementoConfig == null || mementoConfig.ProviderConfig == null)
            {
                throw new Exception("Error in app.config, you must set a provider config of Memento");
            }

            var providerConfig = mementoConfig.ProviderConfig;

            string proveedorPers = providerConfig.Class;
            string assemblyPers = providerConfig.Assembly;

            Type tProveedor = null;

            if (!string.IsNullOrEmpty(proveedorPers))
            {
                Assembly asm = Assembly.LoadFrom(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + Path.DirectorySeparatorChar + assemblyPers);
                tProveedor = asm.GetType(proveedorPers);
                
            }

            if (tProveedor != null)
            {
                Type[] genParam = new Type[1];
                genParam[0] = typeof (T);

                tProveedor = tProveedor.MakeGenericType(genParam);

                IDataPersistence res;
                if (string.IsNullOrEmpty(entornoBd))
                {
                    res = Activator.CreateInstance(tProveedor) as IDataPersistence;
                }
                else
                {
                    object[] param = new object[1];
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