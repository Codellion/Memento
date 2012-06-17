using System;
using System.Collections.Generic;
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
        private List<T> _value;

        /// <summary>
        /// Nombre de la propiedad de la clase sobre la que tenemos
        /// la dependencia
        /// </summary>
        private string _referenceName;

        /// <summary>
        /// Entidad que contiene las dependencias
        /// </summary>
        private Entity _entityRef;

        #endregion

        #region Propiedades

        /// <summary>
        /// Propiedad que permite la carga perezosa de los valores de las dependecias
        /// </summary>
        public List<T> Value
        {
            get
            {
                try
                {
                    if (_value == null)
                    {
                        IPersistence<T> servicioPers = new Persistence<T>();

                        T aux = Activator.CreateInstance<T>();

                        //Establecemos la relación entre ambas entidades
                        PropertyInfo prop = aux.GetType().GetProperty(ReferenceName);
                        
                        object refAux = Activator.CreateInstance(prop.PropertyType);
                        refAux.GetType().GetProperty("Value").SetValue(refAux, EntityRef, null);

                        prop.SetValue(aux, refAux, null);

                        //Realizamos la busqueda de los datos relacionados
                        List<T> res = servicioPers.GetEntities(aux) as List<T>;

                        _value = res;
                    }
                }
                catch (Exception)
                {
                    //TODO Registrar error
                }
                

                return _value;
            }

            set { _value = value; }
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

        #endregion

        #region Constructores
               
        /// <summary>
        /// Constructor donde se define la propiedad referenciada en la clase
        /// dependiente
        /// </summary>
        public Dependences()
        {
         
        }

        /// <summary>
        /// Constructor donde se define la propiedad referenciada en la clase
        /// dependiente
        /// </summary>
        /// <param name="refName">Nombre de la propiedad referenciada</param>
        /// <param name="entidad">Entidad que contiene la dependencia</param>
        public Dependences(string refName, Entity entidad)
        {
            _referenceName = refName;
            _entityRef = entidad;
        }

        #endregion
    }
}
