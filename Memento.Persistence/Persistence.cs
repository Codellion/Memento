using System;
using System.Collections.Generic;
using System.Data;
using System.Reflection;
using Memento.DataAccess;
using Memento.DataAccess.Interfaces;
using Memento.Persistence.Commons;
using Memento.Persistence.Interfaces;

namespace Memento.Persistence
{
    /// <summary>
    /// Clase que actua de fachada del módulo de persistencia
    /// </summary>
    /// <typeparam name="T">Tipo de dato sobre el que se creará la factoría</typeparam>
    public class Persistence<T> : IPersistence<T> where T : Entity
    {
        #region Constantes

        /// <summary>
        /// Prefijo que llevan las propiedades que queremos filtrar con LIKE en lugar de =
        /// </summary>
        private const string PrefixLike = "#like#";

        #endregion

        #region Atributos

        private readonly DataContext _context;

        /// <summary>
        /// Atributo privado del servicio de datos de persistencia
        /// </summary>
        private IDataPersistence _persistenceService;

        #endregion

        #region Propiedades

        /// <summary>
        /// Propiedad del servicio de datos de persistencia donde
        /// se llama al Proveedor de servicio de persistencia para que 
        /// devuelva una implementación de dicha interfaz
        /// </summary>
        private IDataPersistence PersistenceService
        {
            get
            {
                if (_persistenceService != null) return _persistenceService;
                _persistenceService = DataFactoryProvider.GetProvider<T>(_context != null ? _context.Transaction : null);

                return _persistenceService;
            }
            set { _persistenceService = value; }
        }

        #endregion

        #region Constructores

        /// <summary>
        /// Crea una instancia del módulo de persistencia con los valores por defecto
        /// </summary>
        public Persistence()
        {
        }

        /// <summary>
        /// Crea una instancia del módulo de persistencia dentro del contexto pasado
        /// como parámetro
        /// </summary>
        /// <param name="contexto">Contexto transaccional</param>
        public Persistence(DataContext contexto)
        {
            _context = contexto;
        }

        #endregion

        #region Implementación de la interfaz

        /// <summary>
        /// Da de alta una entidad en BBDD
        /// </summary>
        /// <param name="entity">Entidad que se dará de alta</param>
        /// <returns>Entidad con el identificador de BBDD informado</returns>
        public T PersistEntity(T entity)
        {
            if (entity.GetEntityId() == null)
            {
                if (entity.Dependences.Count > 0)
                {
                    object id;
                    if (_context == null)
                    {
                        using (var dtContext = new DataContext())
                        {
                            try
                            {
                                IDataPersistence pserServAux =
                                    DataFactoryProvider.GetProvider<T>(dtContext.Transaction);

                                id = pserServAux.InsertEntity(entity);

                                ManageDependence(entity, id, dtContext);
                                dtContext.SaveChanges();
                            }
                            catch (Exception)
                            {
                                dtContext.Rollback();
                                throw;
                            }
                        }
                    }
                    else
                    {
                        id = PersistenceService.InsertEntity(entity);

                        ManageDependence(entity, id, _context);
                    }

                    Type tipoEntidad = entity.GetType();

                    PropertyInfo propId = tipoEntidad.GetProperty(entity.GetEntityIdName());
                    Type nullType = Nullable.GetUnderlyingType(propId.PropertyType);

                    object nullValue = nullType != null ? Convert.ChangeType(id, nullType) : id;

                    propId.SetValue(entity, nullValue, null);
                }
                else
                {
                    entity = DoInsertEntity(entity);
                }
            }
            else
            {
                UpdateEntity(entity);
            }

            return entity;
        }

        /// <summary>
        /// Realiza las modificaciones sobre una entidad en BBDD
        /// </summary>
        /// <param name="entity">Entidad modificada</param>
        public void UpdateEntity(T entity)
        {
            if (entity.Dependences.Count > 0)
            {
                if (_context == null)
                {
                    using (var dtContext = new DataContext())
                    {
                        try
                        {
                            IDataPersistence pserServAux =
                                DataFactoryProvider.GetProvider<T>(dtContext.Transaction);

                            pserServAux.UpdateEntity(entity);

                            ManageDependence(entity, entity.GetEntityId(), dtContext);
                            dtContext.SaveChanges();
                        }
                        catch (Exception)
                        {
                            dtContext.Rollback();
                            throw;
                        }
                    }
                }
                else
                {
                    PersistenceService.UpdateEntity(entity);

                    ManageDependence(entity, entity.GetEntityId(), _context);
                }
            }
            else
            {
                PersistenceService.UpdateEntity(entity);
            }
        }

        /// <summary>
        /// Realiza una borrado lógico de la entidad cuyo identificador
        /// se pasa como parámetro
        /// </summary>
        /// <param name="entitydId">Identificador de la entidad que se quiere eliminar</param>
        public void DeleteEntity(object entitydId)
        {
            T entity;

            if (entitydId is T)
            {
                entity = (T) entitydId;
            }
            else
            {
                entity = GetEntity(entitydId);
            }


            if (entity != null && entity.Dependences.Count > 0)
            {
                if (_context == null)
                {
                    using (var dtContext = new DataContext())
                    {
                        try
                        {
                            IDataPersistence pserServAux =
                                DataFactoryProvider.GetProvider<T>(dtContext.Transaction);

                            pserServAux.DeleteEntity(entitydId);
                            entity.Activo = false;

                            ManageDependence(entity, entity.GetEntityId(), dtContext);
                            dtContext.SaveChanges();
                        }
                        catch (Exception)
                        {
                            dtContext.Rollback();
                            throw;
                        }
                    }
                }
                else
                {
                    PersistenceService.DeleteEntity(entitydId);
                    entity.Activo = false;

                    ManageDependence(entity, entity.GetEntityId(), _context);
                }
            }
            else
            {
                PersistenceService.DeleteEntity(entitydId);
            }
        }

        /// <summary>
        /// Devuelve la entidad cuyo identificador se pasa como parámetro
        /// de entrada
        /// </summary>
        /// <param name="entitydId">Identificador de la entidad que se quiere obtener</param>
        /// <returns>Entidad</returns>
        public T GetEntity(object entitydId)
        {
            IDataReader dr = PersistenceService.GetEntity(entitydId);
            dr.Read();

            //Realizamos el mapeo de la entidad desde los datos de la fila
            T res = ParseRowToEntidad(dr);

            dr.Close();

            return res;
        }

        /// <summary>
        /// Devuelve una lista de todas las entidades activas existentes en BBDD
        /// </summary>
        /// <returns>Lista da entidades</returns>
        public IList<T> GetEntities()
        {
            IList<T> entidades = new List<T>();

            IDataReader dr = PersistenceService.GetEntities();

            //Realizamos el mapeo de las entidades desde los datos de cada fila
            while (dr.Read())
            {
                entidades.Add(ParseRowToEntidad(dr));
            }

            dr.Close();

            return entidades;
        }

        /// <summary>
        /// Devuelve una lista de todas las entidades obtenidas con los 
        /// filtros establecidos en la variable entidadFiltro
        /// </summary>
        /// <param name="filterEntity">Entidad que actua como filtro</param>
        /// <returns>Lista de entidades</returns>
        public IList<T> GetEntities(T filterEntity)
        {
            IList<T> entidades = new List<T>();

            IDataReader dr = PersistenceService.GetEntities(filterEntity);

            //Realizamos el mapeo de las entidades desde los datos de cada fila
            while (dr.Read())
            {
                entidades.Add(ParseRowToEntidad(dr));
            }

            dr.Close();

            return entidades;
        }

        /// <summary>
        /// Devuelve un dataset con todas las filas activas en BBDD de la entidad
        /// </summary>
        /// <returns>Dataset con el resultado</returns>
        public DataSet GetEntitiesDs()
        {
            return PersistenceService.GetEntitiesDs();
        }

        /// <summary>
        /// Devuelve un dataset con todas las filas obtenidas con los 
        /// filtros establecidos en la variable entidadFiltro
        /// </summary>
        /// <param name="filterEntity">Entidad que actua como filtro</param>
        /// <returns>Dataset con el resultado del filtro</returns>
        public DataSet GetEntitiesDs(T filterEntity)
        {
            return PersistenceService.GetEntitiesDs(filterEntity);
        }

        /// <summary>
        /// Método que devuelve un dataset con los datos devueltos por el procedimiento indicado
        /// con los parametros pasados
        /// </summary>
        /// <param name="storeProcedure">Nombre del procedimiento</param>
        /// <param name="procParams">Parametros necesitados por el procedimiento</param>
        /// <returns>Dataset con los resultados</returns>
        public DataSet GetEntitiesDs(string storeProcedure, IDictionary<string, object> procParams)
        {
            return PersistenceService.GetEntitiesDs(storeProcedure, procParams);
        }

        #endregion

        #region Métodos privados

        /// <summary>
        /// Establece el valor de una propiedad dentro de un objeto, el nombre de la propiedad
        /// puede contener elementos de varios niveles (Ej: Elemento.Propiedad1.Atributo2
        /// Dichos objetos intermedios se instancia automáticamente
        /// </summary>
        /// <param name="targetObject">Objeto donde queremos introducir el valor</param>
        /// <param name="propName">Nombre de la propiedad</param>
        /// <param name="value">Valor de la propiedad</param>
        private static void SetValueInObject(T targetObject, string propName, Object value)
        {
            Type tipoEntidad = targetObject.GetType();

            PropertyInfo refsInfo = tipoEntidad.GetProperty("References");

            var referencias = (IList<String>) refsInfo.GetValue(targetObject, null);

            //Comprobamos si la propiedad contiene niveles de profundidad
            if (propName.Contains("."))
            {
                string[] subProps = propName.Split('.');
                Object aux = targetObject;

                //Recorremos todos los niveles informados
                foreach (string subProp in subProps)
                {
                    PropertyInfo sprop = tipoEntidad.GetProperty(subProp);

                    PropertyInfo subRefsInfo = tipoEntidad.GetProperty("References");

                    //Comprobamos si la propiedad es una referencia del objeto para 
                    //Saber si tenemos que instanciarla
                    if (subRefsInfo != null)
                    {
                        var subReferencias = (IList<String>) subRefsInfo.GetValue(aux, null);

                        //Comprobamos si es necesario establecer el valor
                        if (sprop != null)
                        {
                            Object svalue = sprop.GetValue(aux, null);

                            //Comprobamos si se necesita instanciar una subpropiedad
                            if (subReferencias.Contains(sprop.Name))
                            {
                                if (svalue == null)
                                {
                                    if (sprop.PropertyType.BaseType == typeof (EaterEntity))
                                    {
                                        object refValue =
                                            Activator.CreateInstance(sprop.PropertyType.GetGenericArguments()[0]);

                                        var param = new object[1];
                                        param[0] = refValue;

                                        svalue = Activator.CreateInstance(sprop.PropertyType, param);

                                        sprop.SetValue(aux, svalue, null);
                                        tipoEntidad = sprop.PropertyType.GetGenericArguments()[0];
                                        aux = refValue;
                                    }
                                    else
                                    {
                                        svalue = Activator.CreateInstance(sprop.PropertyType);
                                        sprop.SetValue(aux, svalue, null);
                                        tipoEntidad = sprop.PropertyType;
                                        aux = svalue;
                                    }
                                }
                                else
                                {
                                    if (sprop.PropertyType.BaseType == typeof (EaterEntity))
                                    {
                                        tipoEntidad = sprop.PropertyType.GetGenericArguments()[0];
                                        aux = svalue.GetType().GetProperty("Value").GetValue(svalue, null);
                                    }
                                    else
                                    {
                                        tipoEntidad = sprop.PropertyType;
                                        aux = svalue;
                                    }
                                }
                            }
                            else
                            {
                                value = TryParserNullBoolean(sprop, value);
                                Object nullValue = GetNullableValueFromProp(sprop, value);

                                sprop.SetValue(aux, nullValue, null);
                            }
                        }
                    }
                    else if (sprop != null)
                    {
                        value = TryParserNullBoolean(sprop, value);
                        Object nullValue = GetNullableValueFromProp(sprop, value);

                        sprop.SetValue(aux, nullValue, null);
                    }
                }
            }
            else
            {
                //Bifurcación de las propiedades de primer nivel
                PropertyInfo prop = tipoEntidad.GetProperty(propName);

                if (!referencias.Contains(propName))
                {
                    //Comprobamos que el valor a introducir no se nulo
                    if (typeof (DBNull) != value.GetType())
                    {
                        value = TryParserNullBoolean(prop, value);
                        Object nullValue = GetNullableValueFromProp(prop, value);

                        prop.SetValue(targetObject, nullValue, null);
                    }
                }
            }
        }

        /// <summary>
        /// Parsea un valor boolean que puede ser nulo
        /// </summary>
        /// <param name="prop">Propiedad que queremos establecer</param>
        /// <param name="valor">Booleano que queremos establecer</param>
        /// <returns>Booleano nullable</returns>
        private static object TryParserNullBoolean(PropertyInfo prop, object valor)
        {
            if (prop.PropertyType == typeof (bool)
                || prop.PropertyType == typeof (bool?))
            {
                bool res;

                if (!bool.TryParse(valor.ToString(), out res))
                {
                    valor = int.Parse(valor.ToString());
                }
                else
                {
                    valor = res;
                }
            }

            return valor;
        }

        /// <summary>
        /// Devuelve un valor que puede ser nulo
        /// </summary>
        /// <param name="prop">Propiedad del valor</param>
        /// <param name="valor">Valor</param>
        /// <returns>Valor nullable</returns>
        private static object GetNullableValueFromProp(PropertyInfo prop, object valor)
        {
            //Establecemos el valor
            Type nullType = Nullable.GetUnderlyingType(prop.PropertyType);
            Object nullValue = nullType != null ? Convert.ChangeType(valor, nullType) : valor;

            return nullValue;
        }

        /// <summary>
        /// Realiza una insercción del resgistro en BBDD y establece su identificador
        /// </summary>
        /// <param name="entity">Entidad a insertar</param>
        /// <returns>Entidad insertada</returns>
        private T DoInsertEntity(T entity)
        {
            object id = PersistenceService.InsertEntity(entity);

            if(entity.GetEntityId() == null)
            {
                entity.SetEntityId(id);    
            }

            return entity;
        }

        /// <summary>
        /// Gestiona la persistencia de las entidades dependientes N-M con la entidad que se esta persistiendo
        /// </summary>
        /// <param name="father">Entidad a la que pertenece la dependencia</param>
        /// <param name="relation">Relación de la dependencia</param>
        /// <param name="dtContext">Contexto de persistencia</param>
        private void ManageRelationsNmEntity(Entity father, NmEntity relation, DataContext dtContext = null)
        {
            foreach (string propRefName in relation.References)
            {
                PropertyInfo refProp = relation.GetType().GetProperty(propRefName);

                //Obtenemos la dependencia
                Entity value = null;
                object refValue = null;

                if (refProp.GetValue(relation, null) != null)
                {
                    refValue = refProp.GetValue(relation, null);
                    value = (Entity) refValue.GetType().GetProperty("Value").GetValue(refValue, null);
                }

                //Para evitar referencias circulares a la hora de persistir de nuevo la entidad padre
                //que se está procesando comprobamos que sea distinta
                if (value != null
                    && value.GetType() == father.GetType()
                    && value.GetEntityId().Equals(father.GetEntityId()))
                {
                    continue;
                }

                if (value != null && value.IsDirty)
                {
                    Type tPersServ = typeof (Persistence<>);
                    Type genericType = value.GetType();

                    tPersServ = tPersServ.MakeGenericType(genericType);
                    object pService = GetServicePersistence(genericType, dtContext);

                    if (pService != null)
                    {
                        object valueAux = tPersServ.GetMethod("PersistEntity").Invoke(pService, new object[] {value});

                        if (valueAux != null && valueAux is Entity)
                        {
                            value = (Entity) valueAux;
                            value.IsDirty = false;

                            refValue.GetType().GetProperty("Value").SetValue(refValue, value, null);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Devuelve un servición de persistencia para un tipo de entidad
        /// </summary>
        /// <param name="type">Tipo de entidad</param>
        /// <param name="dtContext">Contexto de persistencia</param>
        /// <returns>Servicio de persistencia</returns>
        private object GetServicePersistence(Type type, DataContext dtContext = null)
        {
            //Obtenemos el servicio de persistencia oportuno para el tipo de la dependencia
            Type tPersServ = typeof (Persistence<>);

            tPersServ = tPersServ.MakeGenericType(type);

            ConstructorInfo constructor;
            object pService = null;

            if (dtContext == null)
            {
                constructor = tPersServ.GetConstructor(new Type[] {});
                if (constructor != null)
                {
                    pService = constructor.Invoke(new object[] {});
                }
            }
            else
            {
                constructor = tPersServ.GetConstructor(new[] {typeof (DataContext)});
                if (constructor != null)
                {
                    pService = constructor.Invoke(new object[] {dtContext});
                }
            }

            return pService;
        }

        /// <summary>
        /// Se encarga de persistir u modificar las dependencias relacionadas con una entidad
        /// </summary>
        /// <param name="entity">Entidad que contiene las dependencias</param>
        /// /// <param name="entityId">Identificador de la entidad</param>
        /// /// <param name="dtContext">Contexto de persistencia</param>
        private void ManageDependence(T entity, object entityId, DataContext dtContext)
        {
            Type tipoEntidad = entity.GetType();

            foreach (string propDepName in entity.Dependences)
            {
                PropertyInfo depProp = tipoEntidad.GetProperty(propDepName);

                //Obtenemos la dependencia
                object depValue = depProp.GetValue(entity, null);

                //Obtenemos el servicio de persistencia oportuno para el tipo de la dependencia
                Type tPersServ = typeof (Persistence<>);
                Type genericType = depProp.PropertyType.GetGenericArguments()[0];

                tPersServ = tPersServ.MakeGenericType(genericType);
                object pService = GetServicePersistence(genericType, dtContext);

                if (pService != null)
                {
                    //Obtenemos el valor de la dependencia
                    object depValueImplict = depValue.GetType().GetProperty("Value").GetValue(depValue, null);

                    //Comprobamos si es única o múltiple
                    if (!depValueImplict.GetType().Name.StartsWith("BindingList"))
                    {
                        var isDirty = (bool) depValue.GetType()
                                                 .GetProperty("IsDirty").GetValue(depValue, null);

                        //Si la dependencia no ha sido modificada no hacemos nada
                        if (!isDirty && entity.Activo)
                        {
                            continue;
                        }

                        //Actualizamos la referencia de la entidad padre recien creada
                        PropertyInfo refEntityProp = depValueImplict.GetType().GetProperty(tipoEntidad.Name);
                        object refEntity = refEntityProp.GetValue(depValueImplict, null);

                        if (refEntity == null)
                        {
                            Type tReference = typeof (Reference<>);
                            tReference =
                                tReference.MakeGenericType(tipoEntidad);

                            refEntity = Activator.CreateInstance(tReference,
                                                                 new object[] {Activator.CreateInstance<T>()});
                        }

                        object refEntityValue = refEntity.GetType().GetProperty("Value").GetValue(refEntity, null);

                        refEntityValue.GetType().GetProperty(entity.GetEntityIdName())
                            .SetValue(refEntityValue, entityId, null);

                        refEntityProp.SetValue(depValueImplict, refEntity, null);

                        MethodInfo operationEntity = null;

                        var statusEnt = (StatusDependence)
                                        depValue.GetType().GetProperty("Status").GetValue(depValue, null);

                        switch (statusEnt)
                        {
                            case StatusDependence.Created:
                                operationEntity = tPersServ.GetMethod("PersistEntity");
                                break;
                            case StatusDependence.Modified:
                                operationEntity = tPersServ.GetMethod("UpdateEntity");
                                break;
                            case StatusDependence.Deleted:
                                operationEntity = tPersServ.GetMethod("DeleteEntity");
                                break;
                        }

                        if (!entity.Activo)
                        {
                            operationEntity = tPersServ.GetMethod("DeleteEntity");
                        }

                        if (operationEntity != null)
                        {
                            object depValueImplAux = operationEntity.Invoke(pService, new[] {depValueImplict});

                            if (depValueImplAux != null)
                            {
                                depValueImplict = depValueImplAux;
                            }

                            depValueImplict.GetType().GetProperty("IsDirty").SetValue(depValueImplict, false, null);

                            depValue.GetType().GetProperty("Value").SetValue(depValue, depValueImplict, null);
                            depValue.GetType().GetProperty("Status").SetValue(depValue, StatusDependence.Synchronized,
                                                                              null);
                            depValue.GetType().GetProperty("IsDirty").SetValue(depValue, false, null);
                        }
                    }
                    else
                    {
                        //Obtenemos el iterador de las dependencias e insertamos cada una de ellas si procede
                        var isDirty = (bool) depValue.GetType()
                                                 .GetProperty("IsDirty").GetValue(depValue, null);

                        //Si la colección no ha sido modificada no hacemos nada
                        if (!isDirty)
                        {
                            continue;
                        }

                        ManageSubList("PersistEntity", "Inserts", depValue, entity, pService, dtContext);
                        ManageSubList("UpdateEntity", "Updates", depValue, entity, pService, dtContext);
                        ManageSubList("DeleteEntity", "Deletes", depValue, entity, pService, dtContext);

                        depValue.GetType().GetMethod("Initialize",
                                                     BindingFlags.NonPublic | BindingFlags.Instance).Invoke(depValue,
                                                                                                            null);
                    }

                    //Establecemos el valor insertado
                    depProp.SetValue(entity, depValue, null);
                }
            }
        }

        /// <summary>
        /// Gestiona la persistencia de las entidades dependientes 1-N de una entidad
        /// </summary>
        /// <param name="persistMethod">Operación de persistencia</param>
        /// <param name="subList">Tipo de lista (Inserts, Updates, Deletes...)</param>
        /// <param name="value">Lista</param>
        /// <param name="entity">Entidad a la que pertenecen las dependencias</param>
        /// <param name="pService">Servicio de persistencia</param>
        /// <param name="dtContext">Contexto de persistencia</param>
        private void ManageSubList(string persistMethod, string subList, object value, Entity entity, object pService,
                                   DataContext dtContext = null)
        {
            object entityId = entity.GetEntityId();
            Type tPersServ = pService.GetType();

            MethodInfo persistEntity = tPersServ.GetMethod(persistMethod);

            object depValueImplict = value.GetType().
                GetProperty(subList, BindingFlags.NonPublic | BindingFlags.Instance).GetValue(value, null);

            //Obtenemos el iterador de las dependencias e insertamos cada una de ellas si procede
            MethodInfo getEnumerator = depValueImplict.GetType().GetMethod("GetEnumerator");

            object enumerator = getEnumerator.Invoke(depValueImplict, null);

            MethodInfo moveNext = enumerator.GetType().GetMethod("MoveNext");

            while ((bool) moveNext.Invoke(enumerator, null))
            {
                PropertyInfo currentDepProp = enumerator.GetType().GetProperty("Current");

                if (currentDepProp != null)
                {
                    object currentDepValue = currentDepProp.GetValue(enumerator, null);

                    //Si la dependencia es una relación N-M es necesario asegurarnos de que todas
                    //las entidades de la relación existen para poder insertar las FKs
                    var depValue = currentDepValue as NmEntity;
                    if (depValue != null)
                    {
                        ManageRelationsNmEntity(entity, depValue, dtContext);
                    }

                    //Actualizamos la referencia de la entidad padre recien creada
                    var refName = (string) value.GetType().GetProperty("ReferenceName").GetValue(value, null);
                    PropertyInfo refEntityProp = currentDepValue.GetType().GetProperty(refName);

                    object refEntity = refEntityProp.GetValue(currentDepValue, null);

                    if (refEntity == null)
                    {
                        Type tReference = typeof (Reference<>);
                        tReference =
                            tReference.MakeGenericType(refEntityProp.PropertyType.GetGenericArguments());

                        refEntity = Activator.CreateInstance(tReference,
                                                             new object[] {Activator.CreateInstance<T>()});
                    }

                    object refEntityValue = refEntity.GetType().GetProperty("Value").GetValue(refEntity, null);

                    PropertyInfo propId = refEntityValue.GetType().GetProperty(entity.GetEntityIdName());
                    Type nullType = Nullable.GetUnderlyingType(propId.PropertyType);

                    object nullValue = nullType != null ? Convert.ChangeType(entityId, nullType) : entityId;

                    propId.SetValue(refEntityValue, nullValue, null);

                    refEntityProp.SetValue(currentDepValue, refEntity, null);

                    persistEntity.Invoke(pService, new[] {currentDepValue});
                }
            }
        }

        /// <summary>
        /// Convierte una fila de datos obtenida de una consulta
        /// en una entidad del módelo de dominio
        /// </summary>
        /// <param name="dr">Lector posicionado en la fila</param>
        /// <returns>Entidad que representa los datos de la fila</returns>
        private T ParseRowToEntidad(IDataReader dr)
        {
            int numCols = dr.FieldCount;

            var aux = Activator.CreateInstance<T>();

            //Por cada columna establecemos el valor
            //dentro de la entidad que vamos a devolver
            for (int i = 0; i < numCols; i++)
            {
                //Establecemos el valor de la columna
                SetValueInObject(aux, dr.GetName(i), dr.GetValue(i));
            }

            return aux;
        }

        /// <summary>
        /// Convierte una fila de datos obtenida de una consulta
        /// en una entidad del módelo de dominio
        /// </summary>
        /// <param name="dr">Lector posicionado en la fila</param>
        /// <returns>Entidad que representa los datos de la fila</returns>
        public static T ParseRowToEntity(DataRow dr)
        {
            int numCols = dr.Table.Columns.Count;

            var aux = Activator.CreateInstance<T>();

            //Por cada columna establecemos el valor
            //dentro de la entidad que vamos a devolver
            for (int i = 0; i < numCols; i++)
            {
                string colName = dr.Table.Columns[i].ColumnName;
                //Establecemos el valor de la columna
                SetValueInObject(aux, colName, dr[colName]);
            }

            return aux;
        }

        #endregion

        #region Métodos públicos

        /// <summary>
        /// Devuelve un valor utilizable en una búsqueda parcial con LIKE
        /// </summary>
        /// <param name="value">Valor a procesar</param>
        /// <returns>Valor procesado</returns>
        public static string GetPartialSearchName(string value)
        {
            return string.Format("{0}%{1}%", PrefixLike, value.Replace(" ", "%"));
        }

        #endregion
    }
}