using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Memento.Persistence.Commons.Annotations;

namespace Memento.Persistence.Commons
{
    /// <summary>
    /// Clase que contiene los datos de configuración del mapeo de una entidad
    /// </summary>
    public class Prototype
    {
        #region Atributos

        /// <summary>
        /// Propiedades privadas de la entidad
        /// </summary>
        internal IDictionary<string, object> PropValues;

        /// <summary>
        /// Lista de propiedades que contienen una dependecia de la entidad
        /// </summary>
        internal List<string> Dependences;

        /// <summary>
        /// Lista de propiedades que contienen una referencia hacía la entidad
        /// </summary>
        internal List<string> References;

        /// <summary>
        /// Nombre de la tabla en BBDD
        /// </summary>
        internal string Table;

        /// <summary>
        /// Lista de propiedades que contienen no son persistentes
        /// </summary>
        internal List<string> TransientProps;

        /// <summary>
        /// Atributos de las relaciones de la entidad
        /// </summary>
        internal IDictionary<string, string> DependsConfig;

        /// <summary>
        /// Nombre del campo que contiene la clave de la entidad
        /// </summary>
        internal string PrimaryKeyName;

        /// <summary>
        /// Estrategia usada para generar la clave de la entidad
        /// </summary>
        internal KeyGenerationType KeyGenerator = KeyGenerationType.Memento;

        /// <summary>
        /// Mapeo de propiedades
        /// </summary>
        internal IDictionary<string, string> FieldsMap;

        #endregion
    }
}
