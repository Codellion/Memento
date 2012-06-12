using System;
using System.Collections.Generic;
using System.Reflection;
using Memento.Persistence.Commons;
using Memento.Persistence.Interfaces;

namespace Memento.Persistence
{
    /// <summary>
    /// Clase que representa la depedencia entre Entidades
    /// permitiendo la carga perezosa de dichos datos
    /// </summary>
    /// <typeparam name="T">Tipo del valor almacenado</typeparam>
    public class Dependence<T> where T:Entity
    {
        #region Atributos

        /// <summary>
        /// Atributo privado que sirve para almacenar el valor
        /// de la dependencia
        /// </summary>
        private T _value;

        /// <summary>
        /// Nombre de la propiedad de la clase sobre la que tenemos
        /// la dependencia
        /// </summary>
        private string _referenceName;

        /// <summary>
        /// Entidad que contiene la dependencia
        /// </summary>
        private Entity _entityRef;

        #endregion
        
        #region Propiedades
               
        /// <summary>
        /// Propiedad que permite la carga perezosa del valor de la dependecia
        /// </summary>
        public T Value
        {
            get
            {
                try
                {
                    if (_value == null)
                    {
                        IPersistence<T> servicioPers = new Persistence<T>();

                        T aux = Activator.CreateInstance<T>();

                        //Establecemos la relaci�n entre ambas entidades
                        PropertyInfo prop = aux.GetType().GetProperty(ReferenceName);

                        object refAux = Activator.CreateInstance(prop.PropertyType);
                        refAux.GetType().GetProperty("Value").SetValue(refAux, EntityRef, null);

                        prop.SetValue(aux, refAux, null);

                        //Realizamos la busqueda de los datos relacionados
                        IList<T> res = servicioPers.GetEntities(aux) as IList<T>;

                        if (res != null)
                        {
                            _value = res[0];
                        }
                        else
                        {
                            _value = Activator.CreateInstance<T>();
                        }
                    }
                }
                catch (Exception)
                {
                }

                return _value;
            }

            set { _value = value; }
        }

        /// <summary>
        /// Entidad que contiene la dependencia 
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

        #endregion

        #region Constructores

        /// <summary>
        /// Constructor donde se define la propiedad referenciada en la clase
        /// dependiente
        /// </summary>
        public Dependence()
        {
         
        }

        /// <summary>
        /// Constructor donde se define la propiedad referenciada en la clase
        /// dependiente
        /// </summary>
        /// <param name="refName">Nombre de la propiedad referenciada</param>
        /// <param name="entidad">Entidad que contiene la dependencia</param>
        public Dependence(string refName, Entity entidad)
        {
            _referenceName = refName;
            _entityRef = entidad;
        }

        #endregion
    }
}