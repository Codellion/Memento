using System;

namespace Memento.DataAccess.Utils
{
    /// <summary>
    /// Clase que representa una consulta de SQL
    /// </summary>
    public class Query
    {
        #region Atributos

        /// <summary>
        /// Columnas de la query
        /// </summary>
        private string _cols;

        /// <summary>
        /// Tablas de la query
        /// </summary>
        private string _tables;

        /// <summary>
        /// Clausulas del where
        /// </summary>
        private string _filters;
        
        /// <summary>
        /// Valores del insert o update
        /// </summary>
        private string _values;

        #endregion

        #region Propiedades

        /// <summary>
        /// Columnas de la query
        /// </summary>
        public string Cols
        {
            get { return _cols; }
            set { _cols = value; }
        }

        /// <summary>
        /// Tablas de la query
        /// </summary>
        public string Tables
        {
            get { return _tables; }
            set { _tables = value; }
        }

        /// <summary>
        /// Clausulas del where
        /// </summary>
        public string Filters
        {
            get { return _filters; }
            set { _filters = value; }
        }

        /// <summary>
        /// Valores en un insert o update
        /// </summary>
        public string Values
        {
            get { return _values; }
            set { _values = value; }
        }
       
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
        public Query(string sCols, string values ,string sTables, string sFilters)
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
                                    Tables, Filters); ;
        }
    }
}
