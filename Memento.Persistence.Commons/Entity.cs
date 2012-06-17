using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Reflection;

namespace Memento.Persistence.Commons
{
    /// <summary>
    /// Clase abstracta de la que heredan aquellas entidades que se
    /// desean persistir mediante el módulo de persistencia
    /// </summary>
    [Serializable]
    public abstract class Entity
    {

        #region Atributos

        /// <summary>
        /// Nombre de la tabla en BBDD
        /// </summary>
        private string _table;

        /// <summary>
        /// Lista de propiedades que contienen una referencia hacía la entidad
        /// </summary>
        private List<string> _references;

        /// <summary>
        /// Booleano que informa si la entidad esta activa o no
        /// </summary>
        private bool _activo = true;

        #endregion

        #region Propiedades
      
        /// <summary>
        /// Lista de propiedades que contienen una referencia hacía la entidad
        /// </summary>
        public List<string> References
        {
            get { return _references ?? (_references = new List<string>()); }
            set { _references = value; }
        }

        /// <summary>
        /// Booleano que informa si la entidad esta activa o no
        /// </summary>
        public bool Activo
        {
            get { return _activo; }
            set { _activo = value; }
        }

        /// <summary>
        /// Nombre de la tabla en BBDD
        /// </summary>
        public string Table
        {
            get { return _table; }
            set { _table = value; }
        }

        #endregion

        /// <summary>
        /// Constructor de la clase donde se invoca el método de configurar entidad
        /// para las clases que heredan de Entidad establezcan sus configuraciones
        /// </summary>
        protected Entity()
        {
            NameValueCollection section = ConfigurationManager.GetSection("PersistenceEntities") as NameValueCollection;

            string fullName = GetType().FullName;
            if (fullName != null && section != null) Table = section[fullName];

            References = new List<string>();

            foreach (PropertyInfo prop in GetType().GetProperties())
            {
                if(prop.PropertyType.BaseType == typeof(EaterEntity))
                {
                    References.Add(prop.Name);
                }
            }
        }
    }
}
