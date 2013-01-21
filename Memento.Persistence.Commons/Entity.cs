using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Configuration;
using System.Diagnostics;
using System.Reflection;
using System.Xml.Serialization;
using Memento.Persistence.Commons.Annotations;

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
        /// Propiedades privadas de la entidad
        /// </summary>
        private readonly IDictionary<string, object> _propValues;

        /// <summary>
        /// Booleano que informa si la entidad esta activa o no
        /// </summary>
        private bool _activo = true;

        /// <summary>
        /// Lista de propiedades que contienen una dependecia de la entidad
        /// </summary>
        private List<string> _dependences;

        /// <summary>
        /// Indica si la entidad esta sincronizada con la BBDD
        /// </summary>
        private bool _isDirty;

        /// <summary>
        /// Lista de propiedades que contienen una referencia hacía la entidad
        /// </summary>
        private List<string> _references;

        /// <summary>
        /// Nombre de la tabla en BBDD
        /// </summary>
        private string _table;

        /// <summary>
        /// Lista de propiedades que contienen no son persistentes
        /// </summary>
        private List<string> _transientProps;

        /// <summary>
        /// Atributos de las relaciones de la entidad
        /// </summary>
        private IDictionary<string, string> _dependsConfig;

        /// <summary>
        /// Nombre del campo que contiene la clave de la entidad
        /// </summary>
        private string _primaryKeyName;

        /// <summary>
        /// Estrategia usada para generar la clave de la entidad
        /// </summary>
        private KeyGenerationType _keyGenerator = KeyGenerationType.Memento;

        /// <summary>
        /// Mapeo de propiedades
        /// </summary>
        private readonly IDictionary<string, string> _fieldsMap;

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

        /// <summary>
        /// Indica si la entidad esta sincronizada con la BBDD
        /// </summary>
        public bool IsDirty
        {
            get { return (_isDirty || GetEntityId() == null); }
            set { _isDirty = value; }
        }

        /// <summary>
        /// Estrategia usada para generar la clave de la entidad
        /// </summary>
        public KeyGenerationType KeyGenerator
        {
            get { return _keyGenerator; }
            set { _keyGenerator = value; }
        }

        #endregion

        #region Constructores

        /// <summary>
        /// Constructor de la clase donde se invoca el método de configurar entidad
        /// para las clases que heredan de Entidad establezcan sus configuraciones
        /// </summary>
        protected Entity()
        {
            _primaryKeyName = string.Empty;
            _fieldsMap = new Dictionary<string, string>();

            TransientProps = new List<string>(6)
                                 {
                                     "TransientProps",
                                     "Table",
                                     "Dependences",
                                     "References",
                                     "IsDirty",
                                     "PropertyChanged",
                                     "KeyGenerator",
                                     "FieldsMap"
                                 };


            foreach (object cAttribute in GetType().GetCustomAttributes(false))
            {
                if(cAttribute is Table)
                {
                    Table tAnnotation = cAttribute as Table;

                    if(!string.IsNullOrEmpty(tAnnotation.Name))
                    {
                        Table = tAnnotation.Name;
                    }
                }
            }

            if(string.IsNullOrEmpty(Table))
            {
                NameValueCollection section = ConfigurationManager.GetSection("memento/persistenceEntities") as NameValueCollection;

                string fullName = GetType().FullName;

                if (fullName != null && section != null) Table = section[fullName];

                if (string.IsNullOrEmpty(Table)) Table = GetType().Name;
            }
            
            References = new List<string>();
            Dependences = new List<string>();

            foreach (PropertyInfo prop in GetType().GetProperties())
            {
                if (prop.PropertyType.BaseType == typeof (EaterEntity))
                {
                    References.Add(prop.Name);
                }
                else if (prop.PropertyType.BaseType == typeof (LazyEntity))
                {
                    Dependences.Add(prop.Name);
                }
            }

            _propValues = new Dictionary<string, object>(GetType().GetProperties().Length);

            //Inicializamos las propiedades con atributos propios
            InitializeCustomProps();
        }

        #endregion

        #region Métodos Privados

        private void InitializeCustomProps()
        {
            foreach (PropertyInfo propertyInfo in GetType().GetProperties())
            {
                if (propertyInfo.GetCustomAttributes(false).Length > 0)
                {
                    foreach (object cAttribute in propertyInfo.GetCustomAttributes(false))
                    {
                        if (cAttribute is Relation)
                        {
                            Relation attRelation = cAttribute as Relation;

                            if (attRelation.Type != RelationType.Reference)
                            {
                                if(_dependsConfig == null) _dependsConfig = new Dictionary<string, string>();

                                _dependsConfig.Add(propertyInfo.Name, attRelation.PropertyName);
                            }
                        }else if(cAttribute is PrimaryKey)
                        {
                            PrimaryKey prk = cAttribute as PrimaryKey;

                            _primaryKeyName = propertyInfo.Name;
                            _keyGenerator = prk.Generator;
                        }else if(cAttribute is Field)
                        {
                            Field propField = cAttribute as Field;

                            if(!string.IsNullOrEmpty(propField.Name))
                            {
                                _fieldsMap[propertyInfo.Name] = propField.Name;    
                            }
                        }else if(cAttribute is Transient)
                        {   
                            TransientProps.Add(propertyInfo.Name);
                        }
                    }
                }
            }
        }

        private string GetPropertyName()
        {
            string res = null;

            StackTrace stackTrace = new StackTrace();
            StackFrame[] frames = stackTrace.GetFrames();

            if (frames != null)
            {
                StackFrame thisFrame = frames[2];
                MethodBase method = thisFrame.GetMethod();

                if (method.Name.Length > 4) res = method.Name.Substring(4);
            }

            return res;
        }

        #endregion

        #region Métodos Públicos

        /// <summary>
        /// Devuelve el identificador de la entidad
        /// </summary>
        /// <returns></returns>
        public object GetEntityId()
        {
            return GetType().GetProperty(GetEntityIdName()).GetValue(this, null);
        }

        /// <summary>
        /// Establece el identificador de la entidad
        /// </summary>
        /// <returns></returns>
        public void SetEntityId(object id)
        {
            Type tipoT = GetType();
            Type tId = tipoT.GetProperty(GetEntityIdName()).PropertyType;

            Type nullType = Nullable.GetUnderlyingType(tId);

            object nullValue = nullType != null ? Convert.ChangeType(id, nullType) : id;

            GetType().GetProperty(GetEntityIdName()).SetValue(this, nullValue, null);
        }

        /// <summary>
        /// Devuelve el nombre del identificador de la entidad
        /// </summary>
        /// <returns></returns>
        public string GetEntityIdName()
        {
            if(!string.IsNullOrEmpty(_primaryKeyName))
            {
                return _primaryKeyName;
            }

            return GetType().Name + "Id";
        }

        #endregion

        #region Métodos para el control de las propiedades

        /// <summary>
        /// Evento que se lanza cuando se modifica una propiedad de la entidad
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Método que establece las propiedades privadas de una entidad
        /// </summary>
        /// <param name="value">Valor</param>
        protected void Set(object value)
        {
            string info = GetPropertyName();

            if (!string.IsNullOrEmpty(info))
            {
                object prop = _propValues.ContainsKey(info) ? _propValues[info] : null;

                if (prop != value)
                {
                    IsDirty = true;
                    _propValues[info] = value;

                    if (PropertyChanged != null)
                    {
                        PropertyChanged(this, new PropertyChangedEventArgs(info));
                    }
                }
            }
        }

        /// <summary>
        /// Método que devuelve el valor de una propiedad privada de la entidad
        /// </summary>
        /// <typeparam name=" T">Tipo de dato de la propiedad</typeparam>
        /// <typeparam name="T">Tipo de dato de la propiedad</typeparam>
        /// <returns></returns>
        protected T Get<T>()
        {
            object prop = null;
            string info = GetPropertyName();

            if (!string.IsNullOrEmpty(info))
            {
                prop = _propValues.ContainsKey(info) ? _propValues[info] : null;

                if (prop == null && _dependsConfig != null && _dependsConfig.ContainsKey(info))
                {
                    prop = Activator.CreateInstance(typeof(T), 
                                        new object[]
                                        {
                                            _dependsConfig[info],
                                            this
                                        });

                    _propValues[info] = prop;
                }
            }

            return (T) prop;
        }

        /// <summary>
        /// Método que devuelve el mapeo de la propiedad con la BBDD
        /// </summary>
        /// <param name="propName">Nombre de la propiedad</param>
        /// <returns>Columna que mapea la propiedad</returns>
        public string GetMappedProp(string propName)
        {
            if(_fieldsMap.ContainsKey(propName))
            {
                return _fieldsMap[propName];
            }

            return propName;
        }

        #endregion
    }
}