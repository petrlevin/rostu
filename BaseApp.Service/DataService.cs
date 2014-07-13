using System;
using System.Data.Entity;
using System.Linq;
using BaseApp.Common.Interfaces;
using BaseApp.DataAccess;
using BaseApp.DbEnums;
using BaseApp.Rights.Functional;
using BaseApp.Service.Common;
using NLog;
using Platform.BusinessLogic;
using Platform.BusinessLogic.AppServices;
using Platform.BusinessLogic.DataAccess;
using Platform.BusinessLogic.Denormalizer.Crud;
using Platform.BusinessLogic.Reference;
using Platform.Client;
using Platform.Common;
using Platform.PrimaryEntities.Common.DbEnums;
using Platform.PrimaryEntities.DbEnums;
using Platform.Common.Extensions;
using Platform.PrimaryEntities.Factoring;
using Platform.PrimaryEntities.Reference;
using System.Collections.Generic;
using ToolsDataManager = Platform.BusinessLogic.DataAccess.ToolsDataManager;

namespace BaseApp.Service
{
    /// <summary>
    /// Методы для получения данных
    /// </summary>
    [AppService]
    public class DataService : DataAccessService
    {
        #region Public 
        /// <summary>
        /// Используется для получения списка сущностей для панели навигации
        /// </summary>
        /// <param name="entityId">Идентификатор сущности</param>
        /// <returns></returns>
        public Object GetEntitiesTree(int entityId)
        {
            return getEntityEntries(Objects.ById<Entity>(entityId), getOrderer("Order"));
        }

        /// <summary>
        /// Получение списка сущностей для панели навигации
        /// </summary>
        /// <returns></returns>
        public virtual AppResponse GetEntities()
        {
            Entity entity = Objects.ByName<Entity>(typeof(Entity).Name);
            return getEntityEntries(entity, getOrderer("Order"));
        }

        /// <summary>
        /// Получение данных для списка элементов мультиссылки
        /// </summary>
        /// <param name="param">Параметры списка</param>
        /// <returns></returns>
        public GridResult MultilinkSource(GridParams param)
        {
			GetRightsManager(param.EntityId).ValidateRead(RightsHolder.Define(param));

            var dataManager = GetDataManager(param.EntityId);
            DecoratorsManager.RegisterMultilinkDecorators(param);

            return dataManager.MultilinkSource(param);
        }

        /// <summary>
        /// Получение данных для линейного списка
        /// </summary>
        /// <param name="param">Параметры списка</param>
        /// <returns></returns>
        public GridResult GridSource(GridParams param)
        {
            return getGridSource(param);
        }

        /// <summary>
        /// Получение данных для списка элементов регистра
        /// </summary>
        /// <param name="param">Параметры списка с пользовательскими фильтрами</param>
        /// <returns></returns>
        public GridResult RegInfoGridSource(CustomFilterGridParams param)
        {
            DecoratorsManager.RegisterRegInfoDecorators(param);
            return getGridSource(param);
        }

        /// <summary>
        /// Получение данных для иерархического списка
        /// </summary>
        /// <param name="param">Параметры списка</param>
        /// <returns></returns>
        public GridResult GridSourceHierarchy(GridParams param)
		{
			return getGridSource(param);
		}

        /// <summary>
        /// Получить элемент
        /// </summary>
        /// <param name="entityId">Идентификатор сущности</param>
        /// <param name="itemId">Идентификатор элемента</param>
        /// <returns></returns>
		public AppResponse GetItem(int entityId, int itemId)
		{
			logger.Info("Получение элемента .EntityId - {0} .ItemId - {1}", entityId, itemId);
			try
			{
                var dataManager = GetDataManager(entityId);
                var result = dataManager.GetEntityEntries(itemId);

			    if (result.Result.Any())
			        result.Result[0] = result.Result.First().Union(dataManager.GetComputated()).ToDictionary(p=>p.Key, p=>p.Value);

			    result.ReadOnly = isItemReadonly(entityId, result.Result[0]);

                return result;
			}
			catch (Exception ex)
			{
				logger.ErrorException(String.Format("Ошибка при при получение элемента.EntityId - {0} .ItemId - {1}", entityId, itemId), ex);
				throw;
			}
		}

        /// <summary>
        /// Получение элемента табличной части
        /// </summary>
        /// <param name="entityId">Идентификатор сущности ТЧ</param>
        /// <param name="itemId">Идентификатор элемента табличной части</param>
        /// <param name="ownerEntityId">Идентификатор сущности-владельца</param>
        /// <param name="ownerItemId">Идентификатор владельца (элемент сущности, в которой находится ТЧ)</param>
        /// <returns></returns>
        public AppResponse GetTpItem(int entityId, int itemId, int? ownerEntityId, int? ownerItemId)
        {
            DecoratorsManager.RegisterDenormalizerDecorators(entityId, ownerEntityId, ownerItemId);

	        return GetItem(entityId, itemId);
        }


        /// <summary>
        /// Получение значений по умолчанию для сущности.
        /// Возвращает только поля, имеющие не нулевые значения
        /// </summary>
        /// <param name="entityId">Идентифкатор сущности</param>
        /// <param name="clientDefaultValues">Значения по-умолчанию, рассчитывающиеся на клиенте</param>
        /// <returns></returns>
        public InitialItemState GetDefaults(int entityId, Dictionary<string, object> clientDefaultValues = null )
        {
            var denormalizedDefaults = new DenormalizedDefaults(entityId);
            if (denormalizedDefaults.IsMasterTablepart)
            {
                return denormalizedDefaults.GetDefaults(clientDefaultValues);
            }

            var dm = GetDataManager(entityId);
            var result = dm.GetInitialState(clientDefaultValues);
            return result;
        }

        /// <summary>
        /// Получение статусов справочников
        /// </summary>
        /// <returns></returns>
        public List<ExtMenuItem> GetRefStatuses()
	    {
			return (from RefStatus value in Enum.GetValues(typeof(RefStatus))
		            select new ExtMenuItem
			            {
				            id = (byte) value, 
							name = value.ToString(),
							text = value.Caption()
			            }).ToList();
	    }

        /// <summary>
        /// Получить список доступных атомарных операций
        /// </summary>
        /// <param name="itemIds">Идентификаторы элементов</param>
        /// <param name="entityId">Идентификатор сущности</param>
        /// <returns></returns>
        public List<ResponseOperation> GetAvaliableAtomaricOperations(int[] itemIds, int entityId)
        {
            var dataManager = GetDataManager(entityId);

            var toolsDataManager = dataManager as ToolsDataManager;
            if (toolsDataManager != null)
                return toolsDataManager.GetAvaliableAtomaricOperations(itemIds);

            return new List<ResponseOperation>();
        }

	    #endregion

        #region Private

        /// <summary>
        /// Получение элементов сущности для панели навигации
        /// </summary>
        /// <returns></returns>
        private AppResponse getEntityEntries(Entity entity, Dictionary<string,bool> order)
        {
            IUser currentUser = IoC.Resolve<IUser>("CurrentUser");
            if (!currentUser.IsSuperUser())
                DecoratorsManager.RegisterUserDecorators();

            return this.GetDataManager(entity).GetEntityEntries("", order);
        }

        private Dictionary<string, bool> getOrderer(string fieldName, bool asc = true)
        {
            return new Dictionary<string, bool>
                {
                    { fieldName, asc }
                };
        }

        protected static Logger logger = LogManager.GetCurrentClassLogger();

        private bool isItemReadonly(int EntityId, IDictionary<string, object> item)
        {
            Entity entity = Objects.ById<Entity>(EntityId);
            IUser currentUser = IoC.Resolve<IUser>("CurrentUser");
            if (entity.EntityType == EntityType.Report && !(bool)item["isTemporary".ToLower()])
            {
                return
                    !currentUser.IsSuperUser()
                    && (byte)item["idReportProfileType".ToLower()] != (byte)ReportProfileType.Public
                    && (int)item["idReportProfileUser".ToLower()] != currentUser.Id;
            }
            return false;
        }


        private GridResult getGridSource(GridParams param)
		{
            ValidateReadFunctional(param);

            var dataManager = this.GetDataManager(param.EntityId, TryGetForm(param.EntityId, param.IsSelectionFormRequest ? FormType.Selection : FormType.List));

            DecoratorsManager.RegisterDecorators(param);

            if (param.OwnerFieldName != null && param.DocId == null) // элемент еще не сохранен
                    return new GridResult();
            
            var result=new GridResult();
            AppResponse entityEntries = dataManager.GetEntityEntries(param);
			
            result.Rows = entityEntries.Result;
			result.Count = entityEntries.Count;

            result.Aggregates = dataManager.GetAggregates(param);

            if ( !String.IsNullOrEmpty(param.HierarchyFieldName) /* иерархический грид */
                 && !param.HierarchyFieldValue.HasValue && (param.HierarchyFieldValues == null || !param.HierarchyFieldValues.Any()) /* запрос на корневые элементы */ )
                result.RootCount = result.Count;
		    
			return result;
		}

        /// <summary>
        /// Получает список имен полей, которые определены в форме для грида, данные которого мы запрашиваем с параметрами <paramref name="param"/>
        /// </summary>
        /// <param name="param"></param>
        /// <returns></returns>
        protected IEnumerable<string> getFormFields(GridParams param)
        {
            var db = IoC.Resolve<DbContext>().Cast<BaseApp.DataContext>();

            Func<Form, IEnumerable<FormElement>> getFormElements = form => db.FormElement.Where(formElement =>
                formElement.IdOwner == form.Id
                && formElement.IdEntityField.HasValue
                );

            IEnumerable<FormElement> formElements = new List<FormElement>();
            if (!param.FieldId.HasValue)
            {
                // данные запрошены формой списка
                Form form = TryGetForm(param.EntityId, FormType.List);
                if (form != null)
                    formElements = getFormElements(form).Where(formElement => formElement.EntityField.IdEntity == param.EntityId);
            }
            else
            {
                // данные запрошены полем (форма выбора ссылочного поля, ТЧ, мультиссылка)
                EntityField field = db.EntityField.Single(ef => ef.Id == param.FieldId);
                Form form = TryGetForm(field.Entity.Id, FormType.Item); // форма сущности-владельца

                if (form != null)
                    formElements = getFormElements(form).Where(formElement =>
                        formElement.IdParent.HasValue
                        && formElement.Parent.IdEntityField == param.FieldId
                        );
            }

            return formElements.Select(formElement => formElement.EntityField)
                .ToList() // выгружаем список в память для дальнейшего использования IsRealField, которая не может быть сопоставлена с ф-цией sql
                //.Where(ef => ef.IsRealField())
                .Select(ef => ef.Name);
        }

        private Form TryGetForm(int entityId, FormType formType)
        {
            Form form = null;
            var db = IoC.Resolve<DbContext>().Cast<BaseApp.DataContext>();
            IQueryable<EntitySetting> settingsList = db.EntitySetting.Where(es => es.IdEntity == entityId);

            switch (formType)
            {
                case FormType.Item:
                    form = settingsList.Where(es => es.IdItemForm.HasValue).Select(es => es.ItemForm).SingleOrDefault();
                    break;
                case FormType.List:
                    form = settingsList.Where(es => es.IdListForm.HasValue).Select(es => es.ListForm).SingleOrDefault();
                    break;
                case FormType.Selection:
                    form = settingsList.Where(es => es.IdSelectionForm.HasValue).Select(es => es.SelectionForm).SingleOrDefault();
                    break;
            }

            return form;
        }

        #endregion

    }
}