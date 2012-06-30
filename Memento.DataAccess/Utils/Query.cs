using System;

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
        public string Cols { get; set; }

        /// <summary>
        /// Tablas de la query
        /// </summary>
        public string Tables { get; set; }

        /// <summary>
        /// Clausulas del where
        /// </summary>
        public string Filters { get; set; }

        /// <summary>
        /// Valores en un insert o update
        /// </summary>
        public string Values { get; set; }

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
            return String.Format(" INSERT INTO {0} ({1}) VALUES ({2}) ; SELECT  @@IDENTITY AS ID;",
                                 Tables, Cols, Values);
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