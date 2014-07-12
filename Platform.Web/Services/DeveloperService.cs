using BaseApp.Service.Common;
using Platform.BusinessLogic.Activity.Controls;
using Platform.BusinessLogic.Activity.Controls.DispatcherStrategies;
using Platform.BusinessLogic.DataAccess;
using Platform.BusinessLogic.ReportProfiles;
using Platform.ClientInteraction;

namespace Platform.Web.Services
{
    /// <summary>
    /// Сервис для разработчиков. Методы, выполняющие всяческие maintenance-манипуляции, тестирование и диагностику.
    /// </summary>
    public class DeveloperService : DataAccessService
    {
        /// <summary>
        /// Удалить все временные экземпляры всех сущностей типа "Отчет"
        /// </summary>
        /// <param name="userId">Идентификатор пользователя</param>
        /// <returns>количество удаленных элементов</returns>
        public int RemoveTemporaryProfiles(int userId)
        {
            return TemporaryItemsRemover.RemoveAllTemporaryProfiles(userId);
        }

        /// <summary>
        /// Выполнить операцию, минуя все мягкие контроли и проверки прав
        /// </summary>
        /// <param name="communicationContext">Контекст взаимодействия с клиентом</param>
        /// <param name="entityId"></param>
        /// <param name="docId"></param>
        /// <param name="operationId"></param>
        /// <returns></returns>
        public virtual AppResponse ExecWithoutControls(CommunicationContext communicationContext, int entityId, int docId, int operationId)
        {
            AppResponse result;
            using (new ControlScope<SkipSkippableStrategy>(() => true, ScopeOptions.ApplyOnlyDispatching))
            {
                var manager = GetDataManager<ToolsDataManager>(entityId);
                var actions = manager.ExecuteOperation(docId, operationId);
                result = manager.GetEntityEntries(docId, false);
                result.Actions = actions;
            }
            return result;
        }
    }
}