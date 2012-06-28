using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;

using Memento.Persistence.Commons;
using Memento.Persistence.Interfaces;

namespace Memento.Persistence
{
    /// <summary>
    /// Clase que representa la depedencia entre Entidades 1-N y N-M
    /// permitiendo la carga perezosa de dichos datos
    /// </summary>
    /// <typeparam name="T">Tipo de las dependencias</typeparam>
    public class Dependences<T> : LazyEntity where T : Entity
    {
        #region Atributos

        /// <summary>
        /// Atributo privado que sirve para almacenar los valores
        /// de las dependencias
        /// </summary>
        private IList<T> _value;

        /// <summary>
        /// Nombre de la propiedad de la clase sobre la que tenemos
        /// la dependencia
        /// </summary>
        private string _referenceName;

        /// <summary>
        /// Entidad que contiene las dependencias
        /// </summary>
        private Entity _entityRef;

        /// <summary>
        /// Indica si la lista de dependencias fue creado por el usuario y no traida desde la BBDD
        /// </summary>
        private bool _isDirty;

        private IDictionary<int, object> _inserts;
        private IDictionary<int, object> _updates;
        private IDictionary<int, object> _deletes;
        private List<T> _backup;

        #endregion

        #region Propiedades

        /// <summary>
        /// Propiedad que permite la carga perezosa de los valores de las dependecias
        /// </summary>
        public IList<T> Value
        {
            get
            {
                try
                {
                    if (_value == null)
                    {
                        IPersistence<T> servicioPers = new Persistence<T>();

                        T aux = Activator.CreateInstance<T>();

                        if (!string.IsNullOrEmpty(ReferenceName))
                        {
                            //Establecemos la relación entre ambas entidades
                            PropertyInfo prop = aux.GetType().GetProperty(ReferenceName);

                            object refAux = Activator.CreateInstance(prop.PropertyType);
                            refAux.GetType().GetProperty("Value").SetValue(refAux, EntityRef, null);

                            prop.SetValue(aux, refAux, null);

                            //Realizamos la busqueda de los datos relacionados
                            IList<T> res = servicioPers.GetEntities(aux);

                            if (res != null)
                            {
                                _backup = new List<T>(res);

                                _value = new BindingList<T>(res);

                                Initialize();
                            }

                            IsDirty = false;
                        }
                    }
                }
                catch (Exception)
                {
                    //TODO Registrar error
                }
                

                return _value;
            }

            set { IsDirty = true; _value = value; }
        }

        /// <summary>
        /// Entidad que contiene las dependencias
        /// </summary>
        public Entity EntityRef
        {
            get { return _entityRef; }
            set { _entityRef = value; }
        }

        /// <summary>
        /// Nombre de la propiedad de la clase sobre la que tenemos
        /// la dependencia
        /// </summary>
        public string ReferenceName
        {
            get { return _referenceName; }
            set { _referenceName = value; }
        }

        /// <summary>
        /// Indica si la lista de dependencias fue creado por el usuario y no traida desde la BBDD
        /// </summary>
        public bool IsDirty
        {
            get { return _isDirty || _inserts.Count > 0 || _updates.Count > 0 || _deletes.Count > 0; }
            set { _isDirty = value; }
        }


        protected IList<T> Inserts
        { 
            get
            {
                IList<T> res = new List<T>();

                foreach (int index in _inserts.Keys)
                {
                    res.Add(Value[index]);
                }

                return res;
            }
        }

        protected IList<T> Updates
        {
            get
            {
                IList<T> res = new List<T>();

                foreach (int index in _updates.Keys)
                {
                    res.Add(Value[index]);
                }

                return res;
            }
        }

        protected IList<T> Deletes
        {
            get
            {
                IList<T> res = new List<T>();

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
            Initialize();

            IsDirty = false;
            _referenceName = refName;
            _entityRef = entidad;
        }

        #endregion

        public T CreateDependence()
        {
            T aux = Activator.CreateInstance<T>();

            if (!string.IsNullOrEmpty(ReferenceName))
            {
                //Establecemos la relación entre ambas entidades
                PropertyInfo prop = aux.GetType().GetProperty(ReferenceName);

                object refAux = Activator.CreateInstance(prop.PropertyType);
                refAux.GetType().GetProperty("Value").SetValue(refAux, EntityRef, null);

                prop.SetValue(aux, refAux, null);
            }

            return aux;
        }
        
        protected void Initialize()
        {
            if(Value != null)
            {
                ((BindingList<T>) Value).ListChanged += ValueOnListChanged; 
            }

            _inserts = new Dictionary<int, object>();
            _updates = new Dictionary<int, object>();
            _deletes = new Dictionary<int, object>();
        }

        private void ValueOnListChanged(object sender, ListChangedEventArgs listChangedEventArgs)
        {
            int newIndex = listChangedEventArgs.NewIndex;

            IsDirty = true;

            switch (listChangedEventArgs.ListChangedType)
            {
                case ListChangedType.ItemAdded:

                    if(Value[newIndex].GetEntityId() == null)
                    {
                        _inserts.Add(newIndex, -1);
                    }
                    else if(Value[newIndex].IsDirty)
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
                        if(_updates.ContainsKey(newIndex))
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
    }
}
