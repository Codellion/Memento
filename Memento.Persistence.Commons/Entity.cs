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
        private string _tabla;

        /// <summary>
        /// Lista de propiedades que contienen una referencia hacía la entidad
        /// </summary>
        private List<string> _referencias;

        /// <summary>
        /// Booleano que informa si la entidad esta activa o no
        /// </summary>
        private bool _activo = true;

        #endregion

        #region Propiedades
      
        /// <summary>
        /// Lista de propiedades que contienen una referencia hacía la entidad
        /// </summary>
        public List<string> Referencias
        {
            get { return _referencias ?? (_referencias = new List<string>()); }
            set { _referencias = value; }
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
        public string Tabla
        {
            get { return _tabla; }
            set { _tabla = value; }
        }

        #endregion

        /// <summary>
        /// Constructor de la clase donde se invoca el método de configurar entidad
        /// para las clases que heredan de Entidad establezcan sus configuraciones
        /// </summary>
        public Entity()
        {
            NameValueCollection section = ConfigurationManager.GetSection("PersistenceEntities") as NameValueCollection;

            Tabla = section[this.GetType().FullName];

            Referencias = new List<string>();

            foreach (PropertyInfo prop in GetType().GetProperties())
            {
                if(prop.PropertyType.Name.Equals("Reference`1"))
                {
                    Referencias.Add(prop.Name);
                }
            }
        }

        #region Métodos a implementar

        #endregion
    }
}
