using System;
using System.Collections.Generic;
using System.Configuration;
using System.Reflection;

using Memento.Persistence.Commons.Annotations;

namespace Memento.Persistence.Commons.Config
{
    /// <summary>
    /// Cache de prototipos de mapeos de entidades
    /// </summary>
    public class MetadataCache
    {
        #region Atributos

        /// <summary>
        /// Cache de prototipos
        /// </summary>
        private readonly Dictionary<string, Prototype> _baseMetadata = new Dictionary<string, Prototype>();

        /// <summary>
        /// Singleton de la instancia del cache
        /// </summary>
        private static readonly MetadataCache Singleton = new MetadataCache();

        private MetadataCache() { }

        #endregion

        #region Propiedades

        /// <summary>
        /// Devuelve una instancia del cache de prototipos
        /// </summary>
        public static MetadataCache Instance
        {
            get 
            {
                return Singleton; 
            }
        }

        #endregion

        #region Métodos Públicos

        /// <summary>
        /// Devuelve el prototipo de una entidad de la cache o la agrega en caso de no existir
        /// </summary>
        /// <typeparam name="T">Tipo de la entidad</typeparam>
        /// <param name="entity">Entidad</param>
        /// <returns>Prototipo</returns>
        public Prototype GetMetadata<T>(T entity) where T : Entity
        {
            var type = entity.GetType();
            Prototype proto = null;

            if(!string.IsNullOrEmpty(type.FullName)
                && Singleton._baseMetadata.ContainsKey(type.FullName))
            {
                proto = _baseMetadata[type.FullName];
            }
            else if (!string.IsNullOrEmpty(type.FullName)) 
            {
                proto = LoadMetadataFromEntity(type);
                _baseMetadata[type.FullName] = proto;
          
            }

            return proto;
        }

        #endregion

        #region Métodos Privados

        /// <summary>
        /// Carga los datos de un nuevo prototipo a partir del tipo de una entidad
        /// </summary>
        /// <param name="typeEntity">Tipo de la entidad</param>
        /// <returns>Prototipo</returns>
        private Prototype LoadMetadataFromEntity(Type typeEntity)
        {
            var newProto = new Prototype();

            newProto.PrimaryKeyName = string.Empty;
            newProto.FieldsMap = new Dictionary<string, string>();

            newProto.TransientProps = new List<string>(6)
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


            foreach (object cAttribute in typeEntity.GetCustomAttributes(false))
            {
                if (cAttribute is Table)
                {
                    var tAnnotation = cAttribute as Table;

                    if (!string.IsNullOrEmpty(tAnnotation.Name))
                    {
                        newProto.Table = tAnnotation.Name;
                    }
                }
            }

            if (string.IsNullOrEmpty(newProto.Table))
            {
                var section = ConfigurationManager.GetSection("spock/memento") as MementoSection;

                string fullName = typeEntity.FullName;

                if (fullName != null && section != null && section.PersistenceEntities[fullName] != null)
                {
                    newProto.Table = section.PersistenceEntities[fullName].Table;
                }

                if (string.IsNullOrEmpty(newProto.Table)) newProto.Table = typeEntity.Name;
            }

            newProto.References = new List<string>();
            newProto.Dependences = new List<string>();

            foreach (PropertyInfo prop in typeEntity.GetProperties())
            {
                if (prop.PropertyType.BaseType == typeof(EaterEntity))
                {
                    newProto.References.Add(prop.Name);
                }
                else if (prop.PropertyType.BaseType == typeof(LazyEntity))
                {
                    newProto.Dependences.Add(prop.Name);
                }
            }

            newProto.PropValues = new Dictionary<string, object>(typeEntity.GetProperties().Length);
            InitializeCustomProps(typeEntity, ref newProto);

            return newProto;
        }

        /// <summary>
        /// Inicializa los atributos personalizados de una entidad
        /// </summary>
        /// <param name="typeEntity">Tipo de entidad</param>
        /// <param name="newProto">Prototipo</param>
        private void InitializeCustomProps(Type typeEntity, ref Prototype newProto)
        {
            foreach (var propertyInfo in typeEntity.GetProperties())
            {
                if (propertyInfo.GetCustomAttributes(false).Length > 0)
                {
                    foreach (object cAttribute in propertyInfo.GetCustomAttributes(false))
                    {
                        if (cAttribute is Relation)
                        {
                            var attRelation = cAttribute as Relation;

                            if (attRelation.Type != RelationType.Reference)
                            {
                                if (newProto.DependsConfig == null) newProto.DependsConfig = new Dictionary<string, string>();

                                newProto.DependsConfig.Add(propertyInfo.Name, attRelation.PropertyName);
                            }
                        }
                        else if (cAttribute is PrimaryKey)
                        {
                            var prk = cAttribute as PrimaryKey;

                            newProto.PrimaryKeyName = propertyInfo.Name;
                            newProto.KeyGenerator = prk.Generator;
                        }
                        else if (cAttribute is Field)
                        {
                            var propField = cAttribute as Field;

                            if (!string.IsNullOrEmpty(propField.Name))
                            {
                                newProto.FieldsMap[propertyInfo.Name] = propField.Name;
                            }
                        }
                        else if (cAttribute is Transient)
                        {
                            newProto.TransientProps.Add(propertyInfo.Name);
                        }
                    }
                }
            }
        }

        #endregion
    }
}
