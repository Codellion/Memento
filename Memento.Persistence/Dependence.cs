using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Reflection;
using Memento.Persistence.Commons;
using Memento.Persistence.Interfaces;
using Verso.Net.Commons;
using Verso.Net.Commons.Octopus;

namespace Memento.Persistence
{
    /// <summary>
    /// Clase que representa la depedencia entre Entidades
    /// permitiendo la carga perezosa de dichos datos
    /// </summary>
    /// <typeparam name="T">Tipo del valor almacenado</typeparam>
    [Serializable]
    public class Dependence<T> : LazyEntity where T : Entity
    {
        #region Atributos

        /// <summary>
        /// Estado de la dependencia
        /// </summary>
        private StatusDependence _status = StatusDependence.Unknown;

        /// <summary>
        /// Atributo privado que sirve para almacenar el valor
        /// de la dependencia
        /// </summary>
        private T _value;

        #endregion

        #region Propiedades

        /// <summary>
        /// Propiedad que permite la carga perezosa del valor de la dependecia
        /// </summary>
        public T Value
        {
            get
            {
                if (_value == null)
                {
                    bool octoActivate = false;

                    var aux = Activator.CreateInstance<T>();

                    if (!string.IsNullOrEmpty(ReferenceName))
                    {
                        //Establecemos la relación entre ambas entidades
                        PropertyInfo prop = aux.GetType().GetProperty(ReferenceName);

                        object refAux = Activator.CreateInstance(prop.PropertyType);
                        refAux.GetType().GetProperty("Value").SetValue(refAux, EntityRef, null);

                        prop.SetValue(aux, refAux, null);

                        var res = new List<T>();

                        //Comprobamos si el modulo de Octopus se encuentra activo
                        // y en caso afirmativo realizamos la llamada a traves de el

                        if(ConfigurationManager.GetSection("octopus") != null &&
                           ConfigurationManager.GetSection("octopus/assembliesLocation") != null)
                        {
                            var secOctoAsm =
                                ConfigurationManager.GetSection("octopus/assembliesLocation") as NameValueConfigurationCollection;

                            if (secOctoAsm != null && secOctoAsm["mememento"] != null)
                            {
                                octoActivate = true;

                                var verso = new VersoMsg();

                                verso.ServiceBlock = "Memento";
                                verso.Verb = "GetEntities";
                                verso.DataVerso = aux;

                                verso = OctopusFacade.ExecuteServiceBlock(verso);

                                if(verso != null)
                                {
                                    res = verso.GetData<List<T>>();
                                }
                            }
                        }

                        //Si octopus no se encuentra disponible realizamos la llamada directamente
                        if (!octoActivate)
                        {
                            IPersistence<T> servicioPers = new Persistence<T>();

                            //Realizamos la busqueda de los datos relacionados
                            res = servicioPers.GetEntities(aux);
                        }

                        if (res != null)
                        {
                            _value = res[0];

                            Status = StatusDependence.Synchronized;
                            IsDirty = false;
                        }
                        else
                        {
                            _value = Activator.CreateInstance<T>();
                            Status = StatusDependence.Unknown;
                        }
                    }
                }

                return _value;
            }

            set
            {
                IsDirty = true;
                _value = value;

                if (_value.GetEntityId() != null)
                {
                    Status = _value.IsDirty ? StatusDependence.Modified : StatusDependence.Synchronized;
                }
                else
                {
                    Status = StatusDependence.Created;
                }
            }
        }

        /// <summary>
        /// Entidad que contiene la dependencia 
        /// </summary>
        public Entity EntityRef { get; set; }

        /// <summary>
        /// Nombre de la propiedad de la clase sobre la que tenemos
        /// la dependencia
        /// </summary>
        public string ReferenceName { get; set; }

        /// <summary>
        /// Indica si la lista de dependencias fue creado por el usuario y no traida desde la BBDD
        /// </summary>
        public bool IsDirty { get; set; }

        /// <summary>
        /// Estado de la dependencia
        /// </summary>
        public StatusDependence Status
        {
            get { return _status; }
            set { _status = value; }
        }

        #endregion

        #region Constructores

        /// <summary>
        /// Constructor donde se define la propiedad referenciada en la clase
        /// dependiente
        /// </summary>
        public Dependence()
        {
            IsDirty = true;
            Status = StatusDependence.Unknown;

            Initialize();
        }

        /// <summary>
        /// Constructor donde se define la propiedad referenciada en la clase
        /// dependiente
        /// </summary>
        public Dependence(T entity)
        {
            Value = entity;

            Initialize();
        }

        /// <summary>
        /// Constructor donde se define la propiedad referenciada en la clase
        /// dependiente
        /// </summary>
        /// <param name="refName">Nombre de la propiedad referenciada</param>
        /// <param name="entidad">Entidad que contiene la dependencia</param>
        public Dependence(string refName, Entity entidad)
        {
            IsDirty = false;
            Status = StatusDependence.Unknown;

            ReferenceName = refName;
            EntityRef = entidad;

            Initialize();
        }

        #endregion

        #region Métodos Privados

        /// <summary>
        /// Método que maneja el cambio de propiedades de la dependencia
        /// </summary>
        /// <param name="sender">Objeto que genero el evento</param>
        /// <param name="propertyChangedEventArgs">Propiedades del evento</param>
        private void ValueOnPropertyChanged(object sender, PropertyChangedEventArgs propertyChangedEventArgs)
        {
            IsDirty = true;

            if (Value.GetEntityId() != null)
            {
                Status = StatusDependence.Modified;
            }
        }

        #endregion

        #region Métodos públicos

        /// <summary>
        /// Método que inicializa los parámetros base de la dependencia
        /// </summary>
        private void Initialize()
        {
            if (Value != null)
            {
                Value.PropertyChanged += ValueOnPropertyChanged;
            }
        }

        /// <summary>
        /// Elimina la dependencia
        /// </summary>
        public void Delete()
        {
            IsDirty = true;

            Status = StatusDependence.Deleted;
        }

        #endregion
    }
}