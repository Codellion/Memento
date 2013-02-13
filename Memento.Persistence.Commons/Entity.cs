using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;

using Memento.Persistence.Commons.Annotations;
using Memento.Persistence.Commons.Config;

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
        /// Prototipo del mapeo de la clase
        /// </summary>
        private Prototype Prototype { get; set; }
        
        /// <summary>
        /// Booleano que informa si la entidad esta activa o no
        /// </summary>
        private bool _activo = true;
        
        /// <summary>
        /// Indica si la entidad esta sincronizada con la BBDD
        /// </summary>
        private bool _isDirty;
        
        #endregion

        #region Propiedades

        /// <summary>
        /// Lista de propiedades que contienen una referencia hacía la entidad
        /// </summary>
        public List<string> References
        {
            get { return Prototype.References ?? (Prototype.References = new List<string>()); }
            set { Prototype.References = value; }
        }

        /// <summary>
        /// Lista de propiedades que contienen una dependecia de la entidad
        /// </summary>
        public List<string> Dependences
        {
            get { return Prototype.Dependences ?? (Prototype.Dependences = new List<string>()); }
            set { Prototype.Dependences = value; }
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
            get { return Prototype.Table; }
            set { Prototype.Table = value; }
        }

        /// <summary>
        /// Lista de propiedades que contienen no son persistentes
        /// </summary>
        public List<string> TransientProps
        {
            get { return Prototype.TransientProps; }
            set { Prototype.TransientProps = value; }
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
            get { return Prototype.KeyGenerator; }
            set { Prototype.KeyGenerator = value; }
        }

        #endregion

        #region Constructores

        /// <summary>
        /// Constructor de la clase donde se invoca el método de configurar entidad
        /// para las clases que heredan de Entidad establezcan sus configuraciones
        /// </summary>
        protected Entity()
        {
            Prototype = MetadataCache.Instance.GetMetadata(this);
        }

        #endregion

        #region Métodos Privados

        private string GetPropertyName()
        {
            string res = null;

            var stackTrace = new StackTrace();
            var frames = stackTrace.GetFrames();

            if (frames != null)
            {
                var thisFrame = frames[2];
                var method = thisFrame.GetMethod();

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
            var tipoT = GetType();
            var tId = tipoT.GetProperty(GetEntityIdName()).PropertyType;

            var nullType = Nullable.GetUnderlyingType(tId);

            object nullValue = nullType != null ? Convert.ChangeType(id, nullType) : id;

            GetType().GetProperty(GetEntityIdName()).SetValue(this, nullValue, null);
        }

        /// <summary>
        /// Devuelve el nombre del identificador de la entidad
        /// </summary>
        /// <returns></returns>
        public string GetEntityIdName()
        {
            if(!string.IsNullOrEmpty(Prototype.PrimaryKeyName))
            {
                return Prototype.PrimaryKeyName;
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
                object prop = Prototype.PropValues.ContainsKey(info) ? Prototype.PropValues[info] : null;

                if (prop != value)
                {
                    IsDirty = true;
                    Prototype.PropValues[info] = value;

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
                prop = Prototype.PropValues.ContainsKey(info) ? Prototype.PropValues[info] : null;

                if (prop == null && Prototype.DependsConfig != null && Prototype.DependsConfig.ContainsKey(info))
                {
                    prop = Activator.CreateInstance(typeof(T), 
                                        new object[]
                                        {
                                            Prototype.DependsConfig[info],
                                            this
                                        });

                    Prototype.PropValues[info] = prop;
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
            if (Prototype.FieldsMap.ContainsKey(propName))
            {
                return Prototype.FieldsMap[propName];
            }

            return propName;
        }

        #endregion
    }
}