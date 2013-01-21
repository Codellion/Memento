using System;
using System.Collections.Specialized;
using System.Configuration;
using Memento.Persistence.Commons;
using Memento.Persistence.Commons.Annotations;

namespace Memento.DataAccess.Utils
{
    /// <summary>
    /// Clase que representa una consulta de SQL
    /// </summary>
    public class Query
    {
        #region Atributos

        #endregion

        #region Propiedades

        /// <summary>
        /// Columnas de la query
        /// </summary>
        private string Cols { get; set; }

        /// <summary>
        /// Tablas de la query
        /// </summary>
        public string Tables { private get; set; }

        /// <summary>
        /// Clausulas del where
        /// </summary>
        public string Filters { private get; set; }

        /// <summary>
        /// Valores en un insert o update
        /// </summary>
        private string Values { get; set; }

        /// <summary>
        /// Tipo de clave
        /// </summary>
        public KeyGenerationType TypeKeyGen { get; set; }

        #endregion

        #region Constructores

        public Query()
        {
        }

        /// <summary>
        /// Constructor de la Query
        /// </summary>
        /// <param name="sCols">Columnas</param>
        /// <param name="sTables">Tablas</param>
        /// <param name="sFilters">Filtros</param>
        public Query(string sCols, string sTables, string sFilters)
        {
            Cols = sCols;
            Tables = sTables;
            Filters = sFilters;
        }

        /// <summary>
        /// Constructor de la Query
        /// </summary>
        /// <param name="sCols">Columnas</param>
        /// <param name="values">Valores</param>
        /// <param name="sTables">Tablas</param>
        /// <param name="sFilters">Filtros</param>
        public Query(string sCols, string values, string sTables, string sFilters)
        {
            Cols = sCols;
            Values = values;
            Tables = sTables;
            Filters = sFilters;
        }

        #endregion

        /// <summary>
        /// Devuelve la query SQL con los datos contenidos en la clase
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            String res;

            if (String.IsNullOrEmpty(Filters))
            {
                res = String.Format(" SELECT {0} FROM {1}",
                                    Cols, Tables);
            }
            else
            {
                res = String.Format(" SELECT {0} FROM {1} WHERE {2}",
                                    Cols, Tables, Filters);
            }

            return res;
        }

        public string ToSelect()
        {
            return ToString();
        }

        public string ToInsert()
        {
            return String.Format(" INSERT INTO {0} ({1}) VALUES ({2}); ", Tables, Cols, Values);
        }

        public string ToSelectLastId()
        {
            string selectId = string.Empty;

            if (TypeKeyGen == KeyGenerationType.Database)
            {
                NameValueCollection providerConfig = ConfigurationManager.GetSection("memento/providerConfig") as NameValueCollection;

                if (providerConfig == null)
                {
                    throw new Exception("Error in app.config, you must set a provider config");
                }

                string pKdbComand = providerConfig["databaseKeyCommand"];

                selectId += pKdbComand;
            }

            return selectId;
        }

        public string ToUpdate()
        {
            String res;

            if (String.IsNullOrEmpty(Filters))
            {
                res = String.Format(" UPDATE {0} SET {1}",
                                    Tables, Cols);
            }
            else
            {
                res = String.Format(" UPDATE {0} SET {1} WHERE {2}",
                                    Tables, Cols, Filters);
            }

            return res;
        }

        public string ToDelete()
        {
            return String.Format(" UPDATE {0} SET Activo = 0 WHERE {1} ",
                                 Tables, Filters);
        }
    }
}