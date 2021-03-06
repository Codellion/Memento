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
    /// Clase que representa la depedencia entre Entidades 1-N y N-M
    /// permitiendo la carga perezosa de dichos datos
    /// </summary>
    /// <typeparam name="T">Tipo de las dependencias</typeparam>
    [Serializable]
    public class Dependences<T> : LazyEntity where T : Entity
    {
        #region Atributos

        /// <summary>
        /// Lista original de las dependencias
        /// </summary>
        private List<T> _backup;

        /// <summary>
        /// Lista de dependencias a eliminar
        /// </summary>
        private IDictionary<int, object> _deletes;

        /// <summary>
        /// Lista de dependencias a crear
        /// </summary>
        private IDictionary<int, object> _inserts;

        /// <summary>
        /// Indica si la lista de dependencias fue creado por el usuario y no traida desde la BBDD
        /// </summary>
        private bool _isDirty;

        /// <summary>
        /// Lista de dependencias a actualizar
        /// </summary>
        private IDictionary<int, object> _updates;

        /// <summary>
        /// Atributo privado que sirve para almacenar los valores
        /// de las dependencias
        /// </summary>
        private BindingList<T> _value;

        #endregion

        #region Propiedades

        /// <summary>
        /// Propiedad que permite la carga perezosa de los valores de las dependecias
        /// </summary>
        public BindingList<T> Value
        {
            get
            {
                if (_value == null)
                {
                    bool octoActivate = false;

                    var aux = Activator.CreateInstance<T>();

                    if (!string.IsNullOrEmpty(ReferenceName))
                    {
                        //Establecemos la relaci�n entre ambas entidades
                        PropertyInfo prop = aux.GetType().GetProperty(ReferenceName);

                        object refAux = Activator.CreateInstance(prop.PropertyType);
                        refAux.GetType().GetProperty("Value").SetValue(refAux, EntityRef, null);

                        prop.SetValue(aux, refAux, null);

                        var res = new List<T>();

                        //Comprobamos si el modulo de Octopus se encuentra activo
                        // y en caso afirmativo realizamos la llamada a traves de el

                        if (ConfigurationManager.GetSection("octopus") != null)
                        {
                            octoActivate = true;

                            var verso = new VersoMsg();

                            verso.ServiceBlock = "Memento";
                            verso.Verb = "GetEntities";
                            verso.DataVerso = aux;

                            verso = OctopusFacade.ExecuteServiceBlock(verso);

                            if (verso != null)
                            {
                                res = verso.GetData<List<T>>();
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
                            _backup = new List<T>(res);

                            _value = new BindingList<T>(res);

                            Initialize();
                        }

                        IsDirty = false;
                    }
                }

                return _value;
            }

            set
            {
                IsDirty = true;
                _value = value;
            }
        }

        /// <summary>
        /// Entidad que contiene las dependencias
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
        public bool IsDirty
        {
            get { return _isDirty || _inserts.Count > 0 || _updates.Count > 0 || _deletes.Count > 0; }
            set { _isDirty = value; }
        }

        /// <summary>
        /// Lista de dependencias a crear
        /// </summary>
        protected List<T> Inserts
        {
            get
            {
                List<T> res = new List<T>();

                foreach (int index in _inserts.Keys)
                {
                    res.Add(Value[index]);
                }

                return res;
            }
        }

        /// <summary>
        /// Lista de dependencias a actualizar
        /// </summary>
        protected List<T> Updates
        {
            get
            {
                List<T> res = new List<T>();

                foreach (int index in _updates.Keys)
                {
                    res.Add(Value[index]);
                }

                return res;
            }
        }

        /// <summary>
        /// Lista de dependencias a eliminar
        /// </summary>
        protected List<T> Deletes
        {
            get
            {
                List<T> res = new List<T>();

                foreach (int index in _deletes.Keys)
                {
                    T aux = Activator.CreateInstance<T>();

                    aux.SetEntityId(_deletes[index]);

                    res.Add(aux);
                }

                return res;
            }
        }

        #endregion

        #region Constructores

        /// <summary>
        /// Constructor donde se define la propiedad referenciada en la clase
        /// dependiente
        /// </summary>
        public Dependences()
        {
            Value = new BindingList<T>();

            Initialize();
        }

        /// <summary>
        /// Constructor donde se define la propiedad referenciada en la clase
        /// dependiente
        /// </summary>
        public Dependences(T entity)
        {
            Value = new BindingList<T>();

            Initialize();

            Value.Add(entity);
        }

        /// <summary>
        /// Constructor donde se define la propiedad referenciada en la clase
        /// dependiente
        /// </summary>
        public Dependences(IEnumerable<T> entities)
        {
            Value = new BindingList<T>();

            Initialize();

            foreach (T entity in entities)
            {
                Value.Add(entity);
            }
        }

        /// <summary>
        /// Constructor donde se define la propiedad referenciada en la clase
        /// dependiente
        /// </summary>
        /// <param name="refName">Nombre de la propiedad referenciada</param>
        /// <param name="entidad">Entidad que contiene la dependencia</param>
        public Dependences(string refName, Entity entidad)
        {
            IsDirty = false;
            ReferenceName = refName;
            EntityRef = entidad;

            Initialize();
        }

        #endregion

        #region M�todos Privados

        /// <summary>
        /// M�todo que gestiona los cambios sobre la lista de dependencias
        /// </summary>
        /// <param name="sender">Objeto que genera el evento</param>
        /// <param name="listChangedEventArgs">Argumentos del cambio en la lista</param>
        private void ValueOnListChanged(object sender, ListChangedEventArgs listChangedEventArgs)
        {
            int newIndex = listChangedEventArgs.NewIndex;

            IsDirty = true;

            switch (listChangedEventArgs.ListChangedType)
            {
                case ListChangedType.ItemAdded:

                    if (Value[newIndex].GetEntityId() == null)
                    {
                        _inserts.Add(newIndex, -1);
                    }
                    else if (Value[newIndex].IsDirty)
                    {
                        _updates.Add(newIndex, Value[newIndex].GetEntityId());
                    }

                    break;
                case ListChangedType.ItemChanged:

                    if (Value[newIndex].GetEntityId() != null
                        && !_updates.ContainsKey(newIndex))
                    {
                        _updates.Add(newIndex, Value[newIndex].GetEntityId());
                    }

                    break;
                case ListChangedType.ItemDeleted:

                    if (_backup[newIndex].GetEntityId() != null)
                    {
                        if (_updates.ContainsKey(newIndex))
                        {
                            _updates.Remove(newIndex);
                        }

                        _deletes.Add(_deletes.Count + 1, _backup[newIndex].GetEntityId());
                    }
                    else
                    {
                        _inserts.Remove(newIndex);
                    }

                    _backup.RemoveAt(newIndex);

                    break;
            }
        }

        #endregion

        #region M�todos P�blicos

        /// <summary>
        /// Crea una nueva dependencia relacionada con la entidad a la que pertenece esta lista
        /// </summary>
        /// <returns></returns>
        public T CreateDependence()
        {
            T aux = Activator.CreateInstance<T>();

            if (!string.IsNullOrEmpty(ReferenceName))
            {
                //Establecemos la relaci�n entre ambas entidades
                PropertyInfo prop = aux.GetType().GetProperty(ReferenceName);

                object refAux = Activator.CreateInstance(prop.PropertyType);
                refAux.GetType().GetProperty("Value").SetValue(refAux, EntityRef, null);

                prop.SetValue(aux, refAux, null);
            }

            return aux;
        }

        /// <summary>
        /// Inicializa los par�metros b�sicos de las dependencias
        /// </summary>
        private void Initialize()
        {
            if (Value != null)
            {
                ((BindingList<T>) Value).ListChanged += ValueOnListChanged;
            }

            _inserts = new Dictionary<int, object>();
            _updates = new Dictionary<int, object>();
            _deletes = new Dictionary<int, object>();
        }

        #endregion
    }
}