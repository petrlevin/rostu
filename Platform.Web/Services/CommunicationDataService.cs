using System;
using BaseApp.Rights.Functional;
using BaseApp.Rights.Organizational;
using BaseApp.Service;
using Platform.BusinessLogic.AppServices;
using Platform.BusinessLogic.DataAccess;
using Platform.BusinessLogic.Denormalizer.Crud;
using Platform.ClientInteraction;
using Platform.ClientInteraction.Scopes;
using Platform.Common.Exceptions;
using Platform.Utils.Collections;

namespace Platform.Web.Services
{
    /// <summary>
    /// Методы для работы с данными, имеющие возможность взаимодействия с пользователем 
    /// </summary>
    [AppService]
    public class CommunicationDataService : DataService
    {
        /// <summary>
        /// Добавление элементов в мультиссылку
        /// </summary>
        /// <param name="communicationContext">Контекст взаимодействия с клиентом</param>
        /// <param name="entityId">id сущности мультиссылки</param>
        /// <param name="ownerEntityId">id сущности открытого элемента</param>
        /// <param name="ownerId">id открытого элемента сущности, в которой находится поле мультиссылки</param>
        /// <param name="selectedItems">id выбранных для вставки элементов</param>
        /// <returns>Истина в случае успешного сохранения</returns>
        public virtual bool CreateMultilink(CommunicationContext communicationContext, int entityId, int ownerEntityId, int? ownerId, int[] selectedItems)
        {
            if (ownerId == null)
                throw new Exception("Добавление элемента в мультиссылку: Не передан обязательный идентификатор владельца. Возможно вы пытаетесь добавить элемент в мультиссылку для несохраненного владельца.");
            ValidateWriteFunctional(entityId, ownerEntityId, ownerId);
            
            OrganizationRights.ValidateMultilinkWrite(entityId, ownerEntityId, selectedItems, "добавление");
            
            return this.GetDataManager(entityId).CreateMultilink(ownerEntityId, (int)ownerId, selectedItems);
        }

        /// <summary>
        /// Удалить элементы из списка мультиссылки
        /// </summary>
        /// <param name="communicationContext"></param>
        /// <param name="entityId">Идентификатор сущности мультиссылки</param>
        /// <param name="ownerEntityId">Идентификатор сущности элемента, в котором находится поле мультиссылки</param>
        /// <param name="ownerId">Идентификатор элемента, в которой находится поле мультиссылки</param>
        /// <param name="itemIds">Идентификаторы элементов списка мультиссылки</param>
        /// <returns>Истина в случае успешного удаления элементов</returns>
        public virtual bool DeleteMultilink(CommunicationContext communicationContext, int entityId, int ownerEntityId, int ownerId, int[] itemIds)
        {
                ValidateWriteFunctional(entityId, ownerEntityId, ownerId);
                OrganizationRights.ValidateMultilinkWrite(entityId, ownerEntityId, itemIds, "удаление");
                return this.GetDataManager(entityId).DeleteMultilink(ownerEntityId, ownerId, itemIds);
        }

        /// <summary>
        /// Создать экземпляр отчета
        /// </summary>
        /// <remarks>
        /// При выборе в панели навигации отчета создается его экземпляр и он открывается на редактирование. 
        /// Это позволяет сразу же заполнять табличные части отчета.
        /// </remarks>
        /// <param name="communicationContext">Контекст взаимодействия с клиентом</param>
        /// <param name="entityId">Идентификатор сущности отчета</param>
        /// <returns>Идентификатор созданного экземпляра отчета</returns>
        public int? CreateReportItem(CommunicationContext communicationContext, int entityId)
        {
            var dataManager = GetDataManager(entityId);
            
            //Сразу пробросим значения по-умолчанию
            var values = new IgnoreCaseDictionary<object>( dataManager.GetInitialState().Defaults );
            
            int? newId = SaveElement(communicationContext, null, entityId, values);

            if (!newId.HasValue)
                throw new PlatformException("Не удалось создать экземпляр отчета");

            return newId;
        }

        /// <summary>
        /// Сохранить элемент
        /// </summary>
        /// <param name="communicationContext">Контекст взаимодействия с клиентом</param>
        /// <param name="itemId">Идентификатор элемента</param>
        /// <param name="entityId">Идентификатор сущности</param>
        /// <param name="values">Значения, пришедшие с клиента</param>
        /// <param name="ownerEntityId">Идентификатор сущности владельца</param>
        /// <returns></returns>
        public int? SaveElement(CommunicationContext communicationContext, int? itemId, int entityId, IgnoreCaseDictionary<object> values, int? ownerEntityId = null)
        {
            using (new CommunicationContextScope(communicationContext))
            {
                var dataManager = GetDataManager(entityId);
                ValidateWriteFunctional(entityId, ownerEntityId, itemId, dataManager);
                OrganizationRights.ValidateWrite(entityId, itemId, values, "сохранение");
                var denormalizedSave = new DenormalizedSaver(entityId);
                if (denormalizedSave.IsMasterTablepart)
                {
                    return denormalizedSave.SaveElement(itemId, values);
                }

                if (itemId == null)
                {
                    return dataManager.CreateEntry(values);
                }
                return dataManager.UpdateEntry((int)itemId, values);
            }
        }

        /// <summary>
        /// Удалить элементы
        /// </summary>
        /// <param name="communicationContext">Контекст взаимодействия с клиентом</param>
        /// <param name="entityId">Идентификатор сущности</param>
        /// <param name="itemIds">Идентификаторы удаляемых элементов</param>
        /// <param name="ownerEntityId">Идентификатор сущности владельца</param>
        /// <returns>Истина в случае успешного удаления элементов</returns>
        public bool DeleteItem(CommunicationContext communicationContext, int entityId, int[] itemIds, int? ownerEntityId)
        {
            using (new CommunicationContextScope(communicationContext))
            {
                logger.Info("Удаление элементов.EntityId - {0}", entityId);
                GetRightsManager(entityId).ValidateWrite(RightsHolder.Define(entityId, ownerEntityId), itemIds);
                OrganizationRights.ValidateWrite(entityId, itemIds, "удаление");

                var denormalizedDelete = new DenormalizedDeleter(entityId);
                if (denormalizedDelete.IsMasterTablepart)
                {
                    return denormalizedDelete.DeleteItem(itemIds);
                }

                try
                {
                    return this.GetDataManager(entityId).DeleteItem(itemIds);
                }
                catch (Exception ex)
                {
                    logger.ErrorException(String.Format("Ошибка при при удаление элементов.EntityId - {0} ", entityId),
                                          ex);
                    throw;
                }
            }
        }

        /// <summary>
        /// Создать новую версию элемента и вернуть данные элемента. 
        /// Результат аналогичен методу <see cref="DataService.GetItem"/>.
        /// </summary>
        /// <param name="communicationContext">Контекст взаимодействия с клиентом</param>
        /// <param name="entityId">Идентифкатор сущности</param>
        /// <param name="itemId">Идентификатор копируемого элемента</param>
        /// <param name="ownerEntityId">Идентификатор сущности владельца</param>
        /// <returns></returns>
        public AppResponse CreateVersion(CommunicationContext communicationContext, int entityId, int itemId, int? ownerEntityId)
        {
            using (new CommunicationContextScope(communicationContext))
            {
                ValidateWriteFunctional(entityId, ownerEntityId, itemId);
                OrganizationRights.ValidateWrite(entityId, itemId, "создание новой версии");
                int newIdItem = GetDataManager(entityId).CreateNewVersionEntityItem(itemId);
                return GetItem(entityId, newIdItem);
            }
        }

        /// <summary>
        /// Групповое изменение элементов
        /// </summary>
        /// <param name="communicationContext">Контекст взаимодействия с клиентом</param>
        /// <param name="entityId">Идентификатор сущности</param>
        /// <param name="itemIds">Идентификаторы элементов</param>
        /// <param name="values">Значения, пришедшие с клиента</param>
        public void UpdateElements(CommunicationContext communicationContext, int entityId, int[] itemIds, IgnoreCaseDictionary<object> values)
        {
            var dataManager = GetDataManager(entityId);
            
            ValidateWriteFunctional(entityId, null, itemIds, dataManager);
            OrganizationRights.ValidateWrite(entityId, itemIds, "сохранение");

            dataManager.UpdateEntries(itemIds, values);
        }
    }
}