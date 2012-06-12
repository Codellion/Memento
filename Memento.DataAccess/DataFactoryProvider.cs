using System;
using System.Data;
using System.Reflection;
using Memento.DataAccess.Interfaces;
using System.Configuration;

namespace Memento.DataAccess
{
    /// <summary>
    /// Factoria de acceso a datos del módulo de persistencia
    /// </summary>
    public class DataFactoryProvider
    {
        /// <summary>
        /// Devuelve una implementación del interfaz de acceso a datos
        /// en función de la tipología de la entidad
        /// </summary>
        /// <typeparam name="T">Tipo de la entidad sobre la que se quiere obtener el servicio</typeparam>
        /// <param name="datbase">Base de datos</param>
        /// /// <param name="transaction">Transaccion abierta</param>
        /// <returns>Implementación del interfaz de acceso a datos</returns>
        public static IDataPersistence<T> GetProveedor<T>(IDbTransaction transaction)
        {
            IDataPersistence<T> res = null;

            if (IsEntidad(typeof(T)))
            {
                string proveedorPers = ConfigurationManager.AppSettings["proveedorPersistencia"];
                string assemblyPers = ConfigurationManager.AppSettings["emsambladoProveedorPersistencia"];

                Type tProveedor = null;

                if (!string.IsNullOrEmpty(proveedorPers))
                {
                    Assembly asm = Assembly.LoadFile(AppDomain.CurrentDomain.BaseDirectory + assemblyPers);
                    tProveedor = asm.GetType(proveedorPers);
                }

                if(tProveedor != null)
                {
                    Type[] genParam = new Type[1];
                    genParam[0] = typeof(T);

                    tProveedor = tProveedor.MakeGenericType(genParam);
                    object[] parametros = null;

                    if(transaction != null)
                    {
                        parametros = new object[1];
                        parametros[0] = transaction;
                    }

                    res = Activator.CreateInstance(tProveedor, parametros) as IDataPersistence<T>;
                    
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
            IDataPersistence<T> res = null;

            
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
                Type[] genParam = new Type[1];
                genParam[0] = typeof(T);

                tProveedor = tProveedor.MakeGenericType(genParam);

                if(string.IsNullOrEmpty(entornoBd))
                {
                    res = Activator.CreateInstance(tProveedor) as IDataPersistence<T>;    
                }
                else
                {
                    object[] param = new object[1];
                    param[0] = entornoBd;

                    res = Activator.CreateInstance(tProveedor, param) as IDataPersistence<T>;
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
        private static bool IsEntidad(Type type)
        {
            bool res = false;

            Type typeBase = type.BaseType;

            while (typeBase.BaseType != null && !typeBase.BaseType.Equals(typeof(Object)))
            {
                typeBase = typeBase.BaseType;
            }

            if (typeBase.Name.Equals("Entity"))
            {
                res = true;
            }

            return res;
        }

        public static string GetSChema()
        {
            string schema = ConfigurationManager.AppSettings["esquemaBD"];
            return schema;
        }
    }
}
