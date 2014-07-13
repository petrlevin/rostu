using System;
using System.Diagnostics;
using System.Transactions;
using NLog;
using Platform.BusinessLogic.Activity.Values;
using Platform.BusinessLogic.Auditing;
using Platform.BusinessLogic.Auditing.Auditors;
using Platform.BusinessLogic.Common.Exceptions;
using Platform.BusinessLogic.DataAccess.ClientFilters;
using Platform.BusinessLogic.NavigationPanel;
using Platform.Client.Filters.Extensions;
using Platform.Common;
using Platform.Common.Exceptions;
using Platform.Dal;
using Platform.Dal.Common.Interfaces;
using Platform.Dal.Common.Interfaces.QueryBuilders;
using Platform.Dal.Decorators;
using Platform.Dal.QueryBuilders;
using Platform.Dal.QueryBuilders.Multilink;
using Platform.Dal.QueryBuilders.QueryParts;
using Platform.Log;
using Platform.PrimaryEntities;
using Platform.PrimaryEntities.Common.DbEnums;
using Platform.PrimaryEntities.Common.Extensions;
using Platform.PrimaryEntities.Common.Interfaces;
using Platform.PrimaryEntities.Factoring;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using Platform.PrimaryEntities.Reference;
using Platform.Utils.Collections;
using Platform.Utils.Extensions;
using Options = Platform.Log.Options;


namespace Platform.BusinessLogic.DataAccess
{
    /// <summary>
    /// Менеджер работы с данными
    /// </summary>
    public abstract class DataManager : ManagerBase
    {
        /// <summary>
        /// Сущности элементов, с которыми будет работать менеджер
        /// </summary>
        public IEntity Source { get; private set; }

        /// <summary>
        /// Форма элемента
        /// </summary>
        public Form Form { get; private set; }

        private IFilterConditions _filter;

        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        #region Конструкторы

        protected DataManager(SqlConnection dbConnection, IEntity source)
        {
            if (source == null) 
                throw new ArgumentNullException("source");
            
            Source = source;
            DbConnection = dbConnection;
        }

        protected DataManager(SqlConnection dbConnection, IEntity source, Form form)
            : this(dbConnection, source)
        {
            Form = form;
        }

        #endregion

        #region CRUD
        
        /// <summary>
        /// Событие при успешном создание/изменении элемента
        /// </summary>
        public event CreateUpdateHandler CreateUpdate;
        private void OnCreateUpdate(CreateUpdateOperation operation, int itemid)
        {
            var handler = CreateUpdate;
            if (handler != null)
                handler(this, operation, itemid);
        }

        #region Create

        /// <summary>
        /// Создать экземпляр сущности в БД
        /// </summary>
        /// <param name="values">Параметры элемента</param>
        /// <param name="inTransaction">Выполнять в транзакции </param>
        /// <returns>Возвращает null в случае ошибки.</returns>
// ReSharper disable MethodOverloadWithOptionalParameter
        public int? CreateEntry(Dictionary<String, object> values, bool inTransaction = true)
// ReSharper restore MethodOverloadWithOptionalParameter
        {
            try
            {
                if (inTransaction)
                {
                    using (TransactionScope transaction = CreateTransaction())
                    {
// ReSharper disable AccessToDisposedClosure
                        return CreateEntry(values, transaction.Complete);
// ReSharper restore AccessToDisposedClosure
                    }
                }

                return CreateEntry(values, null);

            }
            catch (PlatformException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new PlatformException("Ошибка при создании экземпляра сущности .Обращайтесь к разработчику. ", ex);
            }
        }

        private int? CreateEntry(Dictionary<String, object> values, Action onCreate)
        {
            var result = CreateEntry(values);

            if (onCreate != null)
                onCreate();

            return result;
        }

        private int? CreateEntry(Dictionary<string, object> values)
        {
            var result = DoCreateEntry(values);

            if (result.HasValue)
                OnCreateUpdate(CreateUpdateOperation.Create, result.Value);

            if (result.HasValue)
                Audit<ReferenceAuditor>.Do(auditor => auditor.OnInsert(Source, result.Value), AuditTime.AfterTransaction);

            return result;
        }

        protected virtual int? DoCreateEntry(Dictionary<String, object> values)
        {
            FillRequiredFieldsWithDefaults(values);
            return null;
        }

        /// <summary>
        /// Создание новой версии элемента
        /// </summary>
        /// <param name="idItem">Идентификатор элемента, от которого создается версия</param>
        /// <returns></returns>
        public abstract int CreateNewVersionEntityItem(int idItem);

        #endregion

        #region Read

        
        /// <summary>
        /// Получить все элементы сущности. 
        /// При выборке применяются T-SQL декораторы зарегистрированные в <see cref="BaseApp.Environment.Storages.RequestStorage"/>
        /// </summary>
        /// <returns></returns>
        public AppResponse GetEntityEntries()
        {
            return GetEntityEntries(null);
        }

        /// <summary>
        /// Получить элемент сущности.
        /// При выборке применяются T-SQL декораторы зарегистрированные в <see cref="BaseApp.Environment.Storages.RequestStorage"/>
        /// </summary>
        /// <param name="itemId">Идентификатор элемента</param>
        /// <returns></returns>
        public virtual AppResponse GetEntityEntries(int itemId)
        {
            return GetEntityEntries(new FilterConditions
                {
                Field = "id",
                Value = itemId
            });
        }

        /// <summary>
        /// Получить все элементы сущности. 
        /// При выборке учитываются параметры запроса <see cref="param"/> и T-SQL декораторы зарегистрированные в <see cref="BaseApp.Environment.Storages.RequestStorage"/>
        /// </summary>
        /// <param name="param">Параметры запроса</param>
        /// <returns></returns>
        public AppResponse GetEntityEntries(GridParams param)
        {
            _filter = GetFilter(param);
            
            Paging paging = null;
            if (param.Page != 0 && param.Limit != 0)
                paging = new Paging { Start = (param.Page - 1) * param.Limit + 1, Count = param.Limit };
            var order = new Order();
            if (param.Sort != null && param.Sort.Length > 0)
            {
                foreach (GridParams.SortInfo sortInfo in param.Sort)
                {
                    order.Add(sortInfo.Property, sortInfo.Direction == "ASC");
                }
            }

            return GetEntityEntries(_filter, param.Search, paging, order);
        }

        /// <summary>
        /// Получить все элементы сущности. 
        /// При выборке учитываются строка поиска <paramref name="likeString"/>,
        ///     сортировки <paramref name="order"/>,
        ///     список выводимых полей <paramref name="fields"/>  
        ///     и T-SQL декораторы зарегистрированные в <see cref="BaseApp.Environment.Storages.RequestStorage"/>
        /// </summary>
        /// <param name="likeString">Строка поиска</param>
        /// <param name="order">Параметры сортировки в формате ключ-значение</param>
        /// <param name="fields">
        /// Имена полей, которые должны присутствовать в выборке. С помощью данного параметры можно ограничить набор отбираемых полей. 
        /// Если параметр ен указан, то будут отобраны все поля сущности.
        /// </param>
        /// <param name="replaceEntityName">
        /// Если <see cref="Source"/> ссылается на сущность Entity, то результат будет пропущен через 
        /// </param>
        /// <returns></returns>
        public virtual AppResponse GetEntityEntries(string likeString,
                                                    Dictionary<string, bool> order = null,
                                                    IEnumerable<string> fields = null, 
                                                    bool replaceEntityName = true)
        {
            Order normalOrder = null;
            if (order != null)
            {
                normalOrder = new Order();
                foreach (var o in order)
                {
                    normalOrder[o.Key] = o.Value;
                }
            }

            AppResponse result = GetEntityEntries(null, likeString, null, normalOrder, fields);

            if (Source == Objects.ByName<Entity>(typeof (Entity).Name))
            {
                CaptionsProvider.Replace(result.Result);
            }

            return result;
        }

        /// <summary>
        /// Получить все элементы сущности. 
        /// При выборке учитываются 
        ///     строка поиска <see cref="likeString"/>, 
        ///     параметры пагинации <see cref="paging"/>, 
        ///     сортировки <see cref="order"/>,
        ///     список выводимых полей <see cref="fields"/>  
        ///     и T-SQL декораторы зарегистрированные в <see cref="BaseApp.Environment.Storages.RequestStorage"/>
        /// </summary>
        /// <param name="likeString">Строка поиска</param>
        /// <param name="paging">Параметры пагинации</param>
        /// <param name="order">Параметры сортировки</param>
        /// <param name="fields">
        /// Имена полей, которые должны присутствовать в выборке. С помощью данного параметры можно ограничить набор отбираемых полей. 
        /// Если параметр ен указан, то будут отобраны все поля сущности.
        /// </param>
        /// <returns></returns>
        public virtual AppResponse GetEntityEntries(string likeString,
                                                  Paging paging = null, Order order = null,
                                                  IEnumerable<string> fields = null)
        {
            return GetEntityEntries(null, likeString, paging, order, fields);
        }

        /// <summary>
        /// Получить все элементы сущности. 
        /// При выборке учитываются 
        ///     параметры запроса <see cref="param"/>,
        ///     список выводимых полей <see cref="fields"/>  
        ///     и T-SQL декораторы зарегистрированные в <see cref="BaseApp.Environment.Storages.RequestStorage"/>
        /// </summary>
        /// <param name="param">Параметры запроса</param>
        /// <param name="fields">
        /// Имена полей, которые должны присутствовать в выборке. С помощью данного параметры можно ограничить набор отбираемых полей. 
        /// Если параметр ен указан, то будут отобраны все поля сущности.
        /// </param>
        /// <returns></returns>
        public virtual AppResponse GetEntityEntries(GridParams param, IEnumerable<string> fields)
        {
            return GetEntityEntries(null, param, fields);
        }

        /// <summary>
        /// Получить все элементы сущности. 
        /// При выборке учитываются фильтр <see cref="filter"/>, 
        ///     параметры запроса <see cref="param"/>,
        ///     список выводимых полей <see cref="fields"/>  
        ///     и T-SQL декораторы зарегистрированные в <see cref="BaseApp.Environment.Storages.RequestStorage"/>
        /// </summary>
        /// <param name="filter">Серверные фильтры</param>
        /// <param name="param">Параметры запроса</param>
        /// <param name="fields">
        /// Имена полей, которые должны присутствовать в выборке. С помощью данного параметры можно ограничить набор отбираемых полей. 
        /// Если параметр ен указан, то будут отобраны все поля сущности.
        /// </param>
        /// <returns></returns>
        public virtual AppResponse GetEntityEntries(IFilterConditions filter, GridParams param, IEnumerable<string> fields)
        {
            return GetEntityEntries(filter, param.Search, GetPaging(param), GetOrder(param), fields);
        }

        /// <summary>
        /// Получить все элементы сущности. 
        /// При выборке учитываются фильтр <see cref="filter"/>, 
        ///     строка поиска <see cref="likeString"/>, 
        ///     параметры пагинации <see cref="paging"/>, 
        ///     сортировки <see cref="order"/>,
        ///     список выводимых полей <see cref="fields"/>  
        ///     и T-SQL декораторы зарегистрированные в <see cref="BaseApp.Environment.Storages.RequestStorage"/>
        /// </summary>
        /// <param name="filter">Серверные фильтры</param>
        /// <param name="likeString">Строка поиска</param>
        /// <param name="paging">Параметры пагинации</param>
        /// <param name="order">Параметры сортировки</param>
        /// <param name="fields">
        /// Имена полей, которые должны присутствовать в выборке. С помощью данного параметры можно ограничить набор отбираемых полей. 
        /// Если параметр ен указан, то будут отобраны все поля сущности.
        /// </param>
        /// <returns></returns>
        public virtual AppResponse GetEntityEntries(IFilterConditions filter, string likeString = "", Paging paging = null, Order order = null, IEnumerable<string> fields = null)
        {
            return GetEntityEntries(filter, Source.CaptionField, Source.DescriptionField, likeString, paging, order, fields);
        }

        /// <summary>
        /// Получить все элементы сущности. 
        /// При выборке учитываются фильтр <see cref="filter"/>, 
        ///     строка поиска <see cref="likeString"/>, 
        ///     параметры пагинации <see cref="paging"/>, 
        ///     сортировки <see cref="order"/>,
        ///     список выводимых полей <see cref="fields"/>  
        ///     и T-SQL декораторы зарегистрированные в <see cref="BaseApp.Environment.Storages.RequestStorage"/>
        /// </summary>
        /// <param name="filter">Серверные фильтры</param>
        /// <param name="caption">Поле-наименование</param>
        /// <param name="description">Поле-описание</param>
        /// <param name="likeString">Строка поиска</param>
        /// <param name="paging">Параметры пагинации</param>
        /// <param name="order">Параметры сортировки</param>
        /// <param name="fields">
        /// Имена полей, которые должны присутствовать в выборке. С помощью данного параметры можно ограничить набор отбираемых полей. 
        /// Если параметр ен указан, то будут отобраны все поля сущности.
        /// </param>
        /// <returns></returns>
        private AppResponse GetEntityEntries(IFilterConditions filter, IEntityField caption, IEntityField description, string likeString = "", Paging paging = null, Order order = null, IEnumerable<string> fields = null)
        {
            var query = new IoCQueryFactory(Source).Select();

            if (fields != null && fields.Any())
            {
                // ToDo: Чтобы избежать ненужных join'ов при обрабате декораторами AddCaptions и AddDescriptions ссылочных полей, 
                // которые отсутствуют на форме и являются обязательными (idOwner, idMaster, ссылка на сущность общей ссылки), нужно
                // в IQueryBuilder передавать FormedEntity.

                List<string> required = GetRequiredFields().Select(ef => ef.Name).ToList();
                query.Fields = fields.Union(required).ToList();
            }

            query.Conditions = filter;
            query.Search = likeString;
            query.Paging = paging;
            query.Order = order;
            
            var captionFieldName = caption == null ? "name" : caption.Name.ToLowerInvariant();
            var descriptionFieldName = description == null ? "" : description.Name.ToLowerInvariant();

            var appResponse = CreateAppResponce();
                appResponse.EntityName = Source.Name;
                appResponse.CaptionField = captionFieldName;
                appResponse.DescriptionField = descriptionFieldName;

            GridResult gridResult = GetDataSet(query);
                appResponse.Result = gridResult.Rows;
                appResponse.Count = gridResult.Count;

            return appResponse;
        }

        #endregion

        #region Update

        /// <summary>
        /// Обновить элемент в БД
        /// </summary>
        /// <param name="itemId">Идентификатор элемента</param>
        /// <param name="values">Значения параметров</param>
        /// <returns></returns>
        public int UpdateEntry(int itemId, Dictionary<string, object> values)
        {
            try
            {
                using (TransactionScope transaction = CreateTransaction())
                {
                    var audit = Audit<ReferenceAuditor>.Start((startedAt, auditor) => auditor.OnUpdate(Source, itemId));

                    var result = DoUpdateEntry(itemId, values);

                    OnCreateUpdate(CreateUpdateOperation.Update, result);

                    audit.Complete(true);
                    transaction.Complete();

                    return result;
                }
            }
            catch (PlatformException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new PlatformException(String.Format("Ошибка при сохранении экземпляра сущности. Идентификатор экземпляра - {0}. Обращайтесь к разработчику. ", itemId), ex);
            }


        }

        protected abstract int DoUpdateEntry(int itemId, Dictionary<string, object> values);

        /// <summary>
        /// Изменить несколько элементов
        /// </summary>
        /// <param name="itemIds">Идентификаторы элементов</param>
        /// <param name="values">Значения параметров</param>
        /// <exception cref="PlatformException"></exception>
        public void UpdateEntries(int[] itemIds, Dictionary<string, object> values)
        {
            try
            {
                using (TransactionScope transaction = CreateTransaction())
                {
                    var audit = Audit<ReferenceAuditor>.Start((startedAt, auditor) => auditor.OnUpdate(Source, itemIds));

                    try
                    {
                        DoUpdateEntries(itemIds, values);
                    }
                    catch (ControlResponseException ex)
                    {
                        transaction.Complete();
                        throw;
                    }

                    audit.Complete(true);
                    transaction.Complete();
                }
            }
            catch (PlatformException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new PlatformException(String.Format("Ошибка при изменении набора сущностей. Идентификаторы экземпляров - {0}. Обращайтесь к разработчику. ", String.Join(",", itemIds)), ex);
            }
        }

        protected abstract void DoUpdateEntries(int[] itemIds, Dictionary<string, object> values);


        #endregion

        #region Delete

        /// <summary>
        /// Удалить элементы
        /// </summary>
        /// <param name="itemIds">Идентификаторы элементов</param>
        /// <returns></returns>
        public bool DeleteItem(int[] itemIds)
        {
            if (itemIds == null)
                throw new ArgumentNullException("itemIds");

            try
            {
                using (TransactionScope transaction = CreateTransaction())
                {
                    var audit = Audit<ReferenceAuditor>.Start((startedAt, auditor) => auditor.OnDelete(Source, itemIds));

                    var result = DoDeleteItem(itemIds);
                    if (result)
                        audit.Complete(true);
                    transaction.Complete();
                    return result;
                }
            }
            catch (PlatformException)
            {
                throw;
            }
            catch (Exception ex)
            {
                if (itemIds.Count() == 1)
                    throw new PlatformException(String.Format("Ошибка при удалении экземпляра сущности. Идентификатор экземпляра - {0}. Обращайтесь к разработчику. ", itemIds[0]), ex);
                else
                    throw new PlatformException("Ошибка при удалении экземпляров сущности. Обращайтесь к разработчику. ", ex);
            }
        }

        protected abstract bool DoDeleteItem(int[] itemId);



        #endregion

        #endregion

        #region DefaultValue

        /// <summary>
        /// Получить значения по-умолчанию
        /// </summary>
        /// <param name="clientDefaultsValues">Значения, расчитанные на клиенте</param>
        /// <returns></returns>
        public virtual InitialItemState GetInitialState(Dictionary<string, object> clientDefaultsValues = null)
        {
            var result = CreateInitialState();
            result.Defaults = new Dictionary<string, object>()
                                    .Union(clientDefaultsValues ?? new Dictionary<string, object>())
                                    .Union(GetDefaults())
                                    .Union(GetComputated())
                                    .ToDictionary(pair => pair.Key, pair => pair.Value);

            result.Defaults = result.Defaults.Union(GetSqlComputedDefaults(result.Defaults))
                                             .ToDictionary(pair => pair.Key, pair => pair.Value);

            return result;
        }

        protected virtual InitialItemState CreateInitialState()
        {
            return new InitialItemState();
        }

        /// <summary>
        /// Возвращает словарь заполненный значениями по умолчаниями для нового сущностного объекта 
        /// </summary>
        /// <returns></returns>
        /// <exception cref="PlatformException"></exception>
        public virtual Dictionary<string, object> GetDefaults()
        {
            var defValues = IoC.Resolve<Object>("DeafaultValues");
            var evaluator = new Evaluator();
            var defaults = new Dictionary<string, object>();

            var cmd = DbConnection.CreateCommand();
            foreach (IEntityField ef in Source.Fields.Where(f => !String.IsNullOrWhiteSpace(f.DefaultValue)).ToList())
            {
                try
                {
                    object value = GetDefaultValue(ef, defValues, evaluator, cmd);

                    if (value != null)
                    {
                        defaults.Add(ef.Name.ToLower(), value);
                        if (ef.IsLinkField())
                        {
                            defaults.Add(ef.Name.ToLower() + "_caption", _getCaption(ef, value));
                        }
                    }
                }
                catch (Exception ex)
                {
                    throw new PlatformException(String.Format("Ошибка при получении значения по умолчанию для поля {0} сущности '{1}' ", Source.Name, ef.Name), ex);
                }
            }
            return defaults;
        }

        private Dictionary<string, object> GetSqlComputedDefaults(Dictionary<string, object> defaults)
        {
            var results = new Dictionary<string, object>();
            var sqlEvaluator = new SqlEvaluator();

            var cmd = DbConnection.CreateCommand();

            foreach (IEntityField ef in Source.Fields.Where(f => !String.IsNullOrWhiteSpace(f.DefaultValue) && f.IdFieldDefaultValueType == (int)FieldDefaultValueType.SqlComputed).ToList())
            {
                try
                {
                    object value = GetDefaultSqlComputedValue(ef, defaults, sqlEvaluator, cmd);

                    results.Add(ef.Name.ToLower(), value);
                    if (ef.IsLinkField())
                    {
                        results.Add(ef.Name.ToLower() + "_caption", _getCaption(ef, value));
                    }
                }
                catch (Exception ex)
                {
                    throw new PlatformException(String.Format("Ошибка при вычислении sql-выражения для получения значения по умолчанию для поля {0} сущности '{1}' ", Source.Name, ef.Name), ex);
                }
            }


            return results;
        }

        /// <summary>
        /// Возвращает словарь заполненный вычисляемыми на стороне сервера значениями
        /// </summary>
        /// <returns></returns>
        /// <exception cref="PlatformException"></exception>
        public virtual Dictionary<string, object> GetComputated()
        {
            var defValues = IoC.Resolve<Object>("DeafaultValues");
            var evaluator = new Evaluator();
            var results = new Dictionary<string, object>();

            foreach (IEntityField ef in Source.Fields.Where(f => f.IdCalculatedFieldType.HasValue && !String.IsNullOrWhiteSpace(f.Expression)).ToList())
            {
                try
                {
                    object value = null;
                    if (ef.CalculatedFieldType == CalculatedFieldType.NumeratorExpression)
                    {
                        value = evaluator.Evaluate(ef.Expression, defValues);
                    }

                    if (value != null)
                        results.Add(ef.Name.ToLower(), value);
                }
                catch (Exception ex)
                {
                    throw new PlatformException(String.Format("Ошибка при получении вычисляемого значения для поля {0} сущности '{1}' ", Source.Name, ef.Name), ex);
                }

            }
            return results;
        }

        protected object GetDefaultSqlComputedValue(IEntityField ef, Dictionary<string, object> defaultValues, SqlEvaluator sqlEvaluator, SqlCommand cmd)
        {
            if (string.IsNullOrWhiteSpace(ef.DefaultValue))
                return null;

            object value = sqlEvaluator.Evaluate(ef.DefaultValue, defaultValues, cmd);

            return value;
        }

        /// <summary>
        /// Получение значения по умолчанию для поля по заданному в свойствах поля выражению
        /// </summary>
        /// <param name="ef"></param>
        /// <param name="defValues"></param>
        /// <param name="evaluator"></param>
        /// <param name="cmd"></param>
        /// <returns></returns>
        protected object GetDefaultValue(IEntityField ef, object defValues, Evaluator evaluator, SqlCommand cmd)
        {
            if (string.IsNullOrWhiteSpace(ef.DefaultValue))
                return null;

            object value = null;
            if (ef.FieldDefaultValueType == FieldDefaultValueType.Application)
            {
                value = evaluator.Evaluate(ef.DefaultValue, defValues);
            }
            else if (ef.FieldDefaultValueType == FieldDefaultValueType.Sql)
            {
                cmd.CommandText = String.Format("SELECT  {0}", ef.DefaultValue);
                value = cmd.ExecuteScalarLog(Options.ReplaceLogger, Logger);
            }

            return value;
        }

        /// <summary>
        /// Заполняет обязательные поля, не получившие значений, значениями по умолчанию. Только для полей, имеющих значения по умолчанию.
        /// </summary>
        /// <remarks>
        /// ЗАЧЕМ: 
        /// 1. в кастомную форму можно будет не помещать обязательные поля, имеющие значение по умолчанию.
        /// 2. обязательные поля, имеющее значение по умолчанию, можгно делать readonly
        /// </remarks>
        /// <param name="values"></param>
        private void FillRequiredFieldsWithDefaults(Dictionary<string, object> values)
        {
            Func<EntityField, bool> hasValue = field =>
            {
                if (!values.ContainsKey(field.Name))
                    return false;
                object value = values[field.Name];
                if (value == null || string.IsNullOrEmpty(Convert.ToString(value)))
                    return false;
                return true;
            };

            //TODO: Откат изменений, сделанных по http://jira.rostu-comp.ru/browse/CORE-158. Если что сломается -- смотреть сюда.
            var fields = Source.RealFields.Cast<EntityField>().Where(a =>
                !a.AllowNull
                && a.Name != "id"
                && !string.IsNullOrWhiteSpace(a.DefaultValue) && !hasValue(a));

            if (!fields.Any())
                return;

            var defValues = IoC.Resolve<Object>("DeafaultValues");
            var evaluator = new Evaluator();
            var cmd = DbConnection.CreateCommand();

            foreach (EntityField field in fields)
            {
                values[field.Name] = GetDefaultValue(field, defValues, evaluator, cmd);
            }
        }

        #endregion

        #region Aggregates

        /// <summary>
        /// Получить значения аггрегатных функций
        /// </summary>
        /// <param name="param">Параметры запроса</param>
        /// <param name="reInitFilter">Сбрасывать текущие фильтры</param>
        /// <returns></returns>
        public Dictionary<String, Object> GetAggregates(GridParams param, bool reInitFilter = true)
        {
            if (_filter == null || reInitFilter)
                _filter = GetFilter(param);

            return GetAggregates(_filter, param.Search, param.HierarchyFieldName);
        }

        /// <summary>
        /// Получить значения аггрегатных функций
        /// </summary>
        /// <param name="filter">Серверные фильтры</param>
        /// <param name="likeString">Строка поиска</param>
        /// <param name="hierarchyFieldName">Имя поля, по которому строится иерархия</param>
        /// <returns></returns>
        public Dictionary<String, Object> GetAggregates(IFilterConditions filter, string likeString = "", string hierarchyFieldName = null)
        {
            var query = new SummaryAggregates.AggregateQueryBuilder(Source, hierarchyFieldName);

            if (query.AggregateFields.Count == 0)
                return null;
            query.Conditions = filter;
            query.Search = likeString;
            SqlCommand cmd = query.GetSqlCommand(DbConnection);

            var result = new Dictionary<String, Object>();
            using (var reader = cmd.ExecuteReaderLog())
            {
                reader.Read();
                query.AggregateFields.ForEach(f => result.Add(f.ToLowerInvariant(), reader.GetValue(columnName: f)));
                reader.Close();
            }
            return result;
        }

        #endregion

        #region Multilink
        
        /// <summary>
        /// Получение данных для списка элементов мультиссылки
        /// </summary>
        /// <param name="param">Параметры списка</param>
        /// <returns></returns>
        public GridResult MultilinkSource(GridParams param)
        {
            if (param.DocId == null)
                return new GridResult();

            Entity entityMl = Objects.ById<Entity>(param.EntityId);

            if (entityMl.IdEntityType != (byte)EntityType.Multilink)
                throw new PlatformException("При запросе данных для мультиссылки в параметре EntityId пришел id сущности неверного типа. Ожидается id сущности типа Мультиссылка. Выполните таск check на БД.");

            IEntityField joinEntityField =
                entityMl.Fields.Single(f => f.EntityFieldType == EntityFieldType.Link && f.IdEntityLink != param.SrcEntityId);
            IEntity joinEntity = joinEntityField.EntityLink;
            var builder = new MultilinkSelectQueryBuilder()
            {
                Entity = joinEntity,
                EntityMl = entityMl,
                MultilinkOwnerId = param.SrcEntityId,
                FilterValue = (int)param.DocId,
                Search = param.Search,
                Conditions = ClientFilterConditionsFactory.Create(param.Filters.ForRealFields(joinEntity))
            };

            if (!string.IsNullOrEmpty(param.Search))
                builder.QueryDecorators.Add(new AddGridSearch(param.Search, param.VisibleColumns.ToList()));
            if (param.Page != 0 && param.Limit != 0)
                builder.Paging = new Paging { Start = (param.Page - 1) * param.Limit + 1, Count = param.Limit };

            return GetDataSet(builder);
        }

        /// <summary>
        /// Добавление записи в таблицу мультиссылки
        /// </summary>
        /// <param name="idEntity">id сущности открытого элемента</param>
        /// <param name="idEntityItem">id открытого элемента сущности, в которой находится поле мультиссылки</param>
        /// <param name="selectedItems">id выбранных для вставки элементов</param>
        /// <returns></returns>
        public bool CreateMultilink(int idEntity, int idEntityItem, int[] selectedItems)
        {
            if (Source.EntityType != EntityType.Multilink)
                throw new ArgumentException("Переданная сущность не является мультиссылкой");

            //if (selectedItems.Length > 1)
            //throw new NotImplementedException("Возможность добавления нескольких записей в мультиссылку пока не реализована.");

            string leftFieldName = MultilinkHelper.GetLeftMultilinkField(Source, idEntity).Name;
            IEntityField rightField = MultilinkHelper.GetRightMultilinkField(Source, idEntity);
            using (TransactionScope transaction = CreateTransaction())
            {
                System.Diagnostics.Debug.WriteLine("Начало транзакции {0}. {1}", transaction.GetHashCode(), transaction.Previous != null);

                Audit<MultilinkAuditor>.Do(auditor => auditor.OnInsert(Source, idEntity, idEntityItem, selectedItems), AuditTime.NowWithCompleteAfterTransaction);
                bool result = true;
                foreach (int selectedItem in selectedItems)
                {
                    IgnoreCaseDictionary<object> valuesToInsert = new IgnoreCaseDictionary<object>()
                                                                      {
                                                                          {leftFieldName, idEntityItem},
                                                                          {rightField.Name, selectedItem}
                                                                      };

                    InsertQueryBuilder builder = new InsertQueryBuilder(Source, valuesToInsert);
                    SqlCommand cmd = builder.GetSqlCommand(DbConnection);
                    try
                    {
                        result = result && cmd.ExecuteNonQueryLog() > 0;
                    }
                    catch (SqlException exception)
                    {
                        SqlExceptionHandler sqlExceptionHandler = new SqlExceptionHandler(exception, rightField,
                                                                                          selectedItem);
                        throw new DalSqlException("", "", sqlExceptionHandler.Message);
                    }
                }
                transaction.AfterComplete += () => Debug.WriteLine("AfterComplete транзакции {0}. {1}", transaction.GetHashCode(), transaction.Previous != null);
                transaction.AfterDispose += () => Debug.WriteLine("AfterDispose транзакции {0}. {1}", transaction.GetHashCode(), transaction.Previous != null);
                transaction.Complete();
                Debug.WriteLine("Конец using транзакции {0}. {1}", transaction.GetHashCode(), transaction.Previous != null);
                return result;
            }
        }

        /// <summary>
        /// Удалить строку из таблицы мультиссылки
        /// </summary>
        /// <param name="idEntity">id сущности открытого элемента</param>
        /// <param name="idEntityItem">id открытого элемента сущности, в которой находится поле мультиссылки</param>
        /// <param name="selectedItems">id выделенных в момент удаления элементов сущности, отображаемого в гриде мультиссылки</param>
        /// <returns></returns>
        public bool DeleteMultilink(int idEntity, int idEntityItem, int[] selectedItems)
        {
            if (Source.EntityType != EntityType.Multilink)
                throw new ArgumentException("Переданная сущность не является мультиссылкой");

            if (selectedItems.Length > 1)
                throw new NotImplementedException("Возможность удаления нескольких записей из мультиссылки пока не реализована.");

            var builder = new DeleteQueryBuilder(Source);

            builder.Conditions = new FilterConditions()
            {
                Type = LogicOperator.And,
                Operands = new List<IFilterConditions>
                                                            {
                                                                new FilterConditions
                                                                    {
                                                                        Field =
                                                                            MultilinkHelper.GetLeftMultilinkField(
                                                                                Source, idEntity).Name,
                                                                        Operator = ComparisionOperator.Equal,
                                                                        Value = idEntityItem
                                                                    },
                                                                new FilterConditions
                                                                    {
                                                                        Field =
                                                                            MultilinkHelper.GetRightMultilinkField(
                                                                                Source, idEntity).Name,
                                                                        Operator = ComparisionOperator.Equal,
                                                                        Value = selectedItems[0]
                                                                    }
                                                            }
            };
            using (TransactionScope transaction = CreateTransaction())
            {
                Audit<MultilinkAuditor>.Do(auditor => auditor.OnDelete(Source, idEntity, idEntityItem, selectedItems), AuditTime.NowWithCompleteAfterTransaction);

                SqlCommand cmd = builder.GetSqlCommand(DbConnection);

                var result = cmd.ExecuteNonQueryLog() > 0;
                transaction.Complete();
                return result;
            }

        }

        #endregion

        #region private
        protected TransactionScope CreateTransaction()
        {
            var result = new TransactionScope(
                TransactionScopeOption.Required,
                new TransactionOptions
                {
                    IsolationLevel = System.Transactions.IsolationLevel.ReadCommitted
                });

            return result;
        }
        
        protected abstract int? DoCloneInternalPartEntry(object sourceItem, object targetItem);

        private IFilterConditions GetFilter(GridParams param)
        {
            IFilterConditions filter = null;
            if (param.OwnerFieldName != null)
                filter = GetFilterByOwner(param);

            filter = ClientFilterConditionsFactory.Create(filter, param.Filters.ForRealFields(Objects.ById<Entity>(param.EntityId)));
            return filter;
        }

        /// <summary>
        /// Поля, которые обязательно должны присутствовать при отборе данных сущности <see cref="Source"/>.
        /// Примеры таких полей: id, idOwner, idMaster, idRefStatus, ValidityFrom, ValidityTo.
        /// </summary>
        /// <returns></returns>
        private IEnumerable<IEntityField> GetRequiredFields()
        {
            // Поля idOwner, idMaster невозможно отпределить точно не зная в каком контексте читаются данные для сущности (как ТЧ или как справочник)
            // А о контексте чтения данных мы здесь знать не должны.
            // Следовательно предлагается упрощенный и слегка избыточный вариант: получать ВСЕ ссылочные поля,

            return Source.RealFields.Where(ef =>
                ef.IsId()
                || (ef.IsSystem.HasValue && ef.IsSystem.Value)
                || ef.IdEntityFieldType == (byte)EntityFieldType.Link);
        }

        protected static void CheckValues(Dictionary<string, object> values)
        {
            if (!values.Any())
                throw new PlatformException(
                    "Список значений для сохранения пустой. Если ничего не требуется сохранять, не следует посылать запрос на сервер.");
        }

        protected Paging GetPaging(GridParams param)
        {
            Paging paging = null;
            if (param.Page != 0 && param.Limit != 0)
                paging = new Paging { Start = (param.Page - 1) * param.Limit + 1, Count = param.Limit };
            return paging;
        }

        protected Order GetOrder(GridParams param)
        {
            Order order = new Order();
            if (param.Sort != null && param.Sort.Length > 0)
            {
                foreach (GridParams.SortInfo sortInfo in param.Sort)
                {
                    order.Add(sortInfo.Property, sortInfo.Direction == "ASC");
                }
            }
            return order;
        }

        /// <summary>
        /// Получение значения для ссылочного поля
        /// </summary>
        /// <param name="entityField">Ссылочное поле</param>
        /// <param name="value">Значение идентифкатора</param>
        /// <returns></returns>
        private string _getCaption(IEntityField entityField, object value)
        {
            ISelectQueryBuilder queryBuilder = new SelectQueryBuilder(entityField.EntityLink);
            queryBuilder.Fields = new List<string>() { entityField.EntityLink.CaptionField.Name };
            queryBuilder.Conditions = new FilterConditions() { Field = "id", Operator = ComparisionOperator.Equal, Value = value };
            string result = string.Empty;
            using (SqlCommand command = queryBuilder.GetSqlCommand(DbConnection))
            {
                using (SqlDataReader reader = command.ExecuteReaderLog())
                {
                    if (reader.HasRows && reader.Read())
                    {
                        if (entityField.EntityLink.CaptionField.IsLinkField())
                        {
                            result = Convert.ToString(reader.GetValue(1));
                        }
                        else
                        {
                            result = Convert.ToString(reader.GetValue(0));
                        }
                    }
                    reader.Close();
                }
            }
            return result;
        }

        /// <summary>
        /// Получает фильтр по владельцу
        /// </summary>
        /// <param name="param"></param>
        /// <returns></returns>
        private IFilterConditions GetFilterByOwner(GridParams param)
        {
            if (param.OwnerFieldName == null || param.DocId == null)
                return null;
            return new FilterConditions()
            {
                Field = param.OwnerFieldName,
                Operator = ComparisionOperator.Equal,
                Value = (int)param.DocId
            };
        }

        protected virtual AppResponse CreateAppResponce()
        {
            return new AppResponse();
        }

        #endregion


    }

}