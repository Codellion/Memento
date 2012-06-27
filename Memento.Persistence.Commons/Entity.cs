using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Configuration;
using System.Reflection;

namespace Memento.Persistence.Commons
{
    /// <summary>
    /// Clase abstracta de la que heredan aquellas entidades que se
    /// desean persistir mediante el módulo de persistencia
    /// </summary>
    [Serializable]
    public abstract class Entity : INotifyPropertyChanged
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
        /// Lista de propiedades que contienen una dependecia de la entidad
        /// </summary>
        private List<string> _dependences;

        /// <summary>
        /// Booleano que informa si la entidad esta activa o no
        /// </summary>
        private bool _activo = true;

        /// <summary>
        /// Lista de propiedades que contienen no son persistentes
        /// </summary>
        private List<string> _transientProps;

        private bool _isDirty = false;

        private IDictionary<string, object> _propValues;

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
        /// Lista de propiedades que contienen una dependecia de la entidad
        /// </summary>
        public List<string> Dependences
        {
            get { return _dependences ?? (_dependences = new List<string>()); }
            set { _dependences = value; }
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

        /// <summary>
        /// Lista de propiedades que contienen no son persistentes
        /// </summary>
        public List<string> TransientProps
        {
            get { return _transientProps; }
            set { _transientProps = value; }
        }

        public bool IsDirty
        {
            get { return _isDirty; }
            set { _isDirty = value; }
        }

        #endregion

        /// <summary>
        /// Constructor de la clase donde se invoca el método de configurar entidad
        /// para las clases que heredan de Entidad establezcan sus configuraciones
        /// </summary>
        protected Entity()
        {
            TransientProps = new List<string>(4);

            TransientProps.Add("TransientProps");
            TransientProps.Add("Table");
            TransientProps.Add("Dependences");
            TransientProps.Add("References");
            TransientProps.Add("IsDirty");
            TransientProps.Add("PropertyChanged");
            TransientProps.Add("_propValues");

            NameValueCollection section = ConfigurationManager.GetSection("PersistenceEntities") as NameValueCollection;

            string fullName = GetType().FullName;
            if (fullName != null && section != null) Table = section[fullName];

            References = new List<string>();
            Dependences = new List<string>();

            foreach (PropertyInfo prop in GetType().GetProperties())
            {
                if(prop.PropertyType.BaseType == typeof(EaterEntity))
                {
                    References.Add(prop.Name);
                }
                else if(prop.PropertyType.BaseType == typeof(LazyEntity))
                {
                    Dependences.Add(prop.Name);
                }
            }

            _propValues = new Dictionary<string, object>(GetType().GetProperties().Length);
        }

        /// <summary>
        /// Devuelve el identificador de la entidad
        /// </summary>
        /// <returns></returns>
        public object GetEntityId()
        {
            return GetType().GetProperty((GetType().Name + "Id")).GetValue(this, null);
        }

        /// <summary>
        /// Establece el identificador de la entidad
        /// </summary>
        /// <returns></returns>
        public void SetEntityId(object id)
        {
            GetType().GetProperty((GetType().Name + "Id")).SetValue(this, id, null);
        }

        /// <summary>
        /// Devuelve el nombre del identificador de la entidad
        /// </summary>
        /// <returns></returns>
        public object GetEntityIdName()
        {
            return GetType().Name + "Id";
        }

        #region Métodos para el control de las propiedades

        public event PropertyChangedEventHandler PropertyChanged;

        protected void Set(String info, object value)
        {
            object prop = _propValues.ContainsKey(info) ? _propValues[info] : null;

            if (prop != value)
            {
                _propValues[info] = value;

                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs(info));
                }    
            }
        }

        protected T Get<T>(String info)
        {
            object prop = _propValues.ContainsKey(info) ? _propValues[info] : null;

            return (T)prop;
        }

        #endregion
    }
}
