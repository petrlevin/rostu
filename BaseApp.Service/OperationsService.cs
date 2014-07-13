using BaseApp.Rights.Organizational;
using BaseApp.Service.Common;
using Platform.BusinessLogic.AppServices;
using Platform.BusinessLogic.DataAccess;
using Platform.ClientInteraction;
using Platform.ClientInteraction.Scopes;
using Platform.Utils.Collections;

namespace BaseApp.Service
{
    /// <summary>
    /// Сервис выполнения операций.
    /// Возможно взаимодействие с клиентом.
    /// </summary>
    [AppService]
    public class OperationsService : DataAccessService
    {
        private readonly OperationsManager _service = new OperationsManager();

        /// <summary>
        /// Начать неатомарную операцию
        /// </summary>
        /// <param name="communicationContext">Контекст взаимодействия с клиентом</param>
        /// <param name="entityId">Идентификатор сущности</param>
        /// <param name="docId">Идентификатор элемента</param>
        /// <param name="operationId">Идентификатор операции</param>
        /// <returns></returns>
        public AppResponse BeginOperation(CommunicationContext communicationContext, int entityId, int docId, int operationId)
        {
            _service.BeginOperation(communicationContext, entityId, docId, operationId);
            return _service.Manager.GetEntityEntries(docId);
        }

        /// <summary>
        /// Завершить неатомарную операцию
        /// </summary>
        /// <param name="communicationContext">Контекст взаимодействия с клиентом</param>
        /// <param name="entityId">Идентификатор сущности</param>
        /// <param name="docId">Идентификатор элемента</param>
        /// <param name="values">Изменившиеся значения с клиента</param>
        /// <returns></returns>
        public AppResponse CompleteOperation(CommunicationContext communicationContext, int entityId, int docId, IgnoreCaseDictionary<object> values)
        {
            var actions = _service.CompleteOperation(communicationContext, entityId, docId, values);
            var result = _service.Manager.GetEntityEntries(docId, false);
            result.Actions = actions;
            return result;
        }
        
        /// <summary>
        /// Отменить начатую неатомарную операцию
        /// </summary>
        /// <param name="communicationContext">Контекст взаимодействия с клиентом</param>
        /// <param name="entityId">Идентификатор сущности</param>
        /// <param name="docId">Идентификатор элемента</param>
        /// <returns></returns>
        public AppResponse CancelOperation(CommunicationContext communicationContext, int entityId, int docId)
        {
            _service.CancelOperation(communicationContext, entityId, docId);
            return _service.Manager.GetEntityEntries(docId, false);
        }

        /// <summary>
        /// Выполнить атомарную операцию
        /// </summary>
        /// <param name="communicationContext">Контекст взаимодействия с клиентом</param>
        /// <param name="entityId">Идентификатор сущности</param>
        /// <param name="docId">Идентификатор элемента</param>
        /// <param name="operationId">Идентификатор операции</param>
        /// <returns></returns>
        public AppResponse Exec(CommunicationContext communicationContext, int entityId, int docId, int operationId)
        {
            var actions = _service.Exec(communicationContext, entityId, docId, operationId);
            var result = _service.Manager.GetEntityEntries(docId, false);
            result.Actions = actions;
            return result;
        }

        /// <summary>
        /// Выполнить атомарную операцию над группой элементов
        /// </summary>
        /// <param name="communicationContext">Контекст взаимодействия с клиентом</param>
        /// <param name="entityId">Идентификатор сущности</param>
        /// <param name="docIds">Идентификатор элемента</param>
        /// <param name="operationId">Идентификатор операции</param>
        public void ExecGroup(CommunicationContext communicationContext, int entityId, int[] docIds, int operationId)
        {
            using (new CommunicationContextScope(communicationContext))
            {
                var manager = GetDataManager<ToolsDataManager>(entityId);

                ValidateWriteFunctional(entityId, null, docIds);
                ValidateExecuteFunctional(entityId, operationId);
                OrganizationRights.ValidateWrite(entityId, docIds, "выполнение операции");

                manager.ExecuteGroupOperation(docIds, operationId);
            }
        }
    }
}