using BaseApp.Rights.Organizational;
using BaseApp.Service.Interfaces;
using Platform.BusinessLogic.DataAccess;
using Platform.ClientInteraction;
using Platform.ClientInteraction.Scopes;
using Platform.Utils.Collections;

namespace BaseApp.Service.Common
{
    public class OperationsManager : DataAccessService, IOperationsManager
    {
        public ToolsDataManager Manager { get; set; }

        public void BeginOperation(CommunicationContext communicationContext, int entityId, int docId, int OperationId)
        {
            using (new CommunicationContextScope(communicationContext))
            {
                ValidateExecuteFunctional(entityId, OperationId);
                OrganizationRights.ValidateWrite(entityId, docId, "выполнение операции");
                Manager = GetDataManager<ToolsDataManager>(entityId);
                Manager.BeginOperation(docId, OperationId);
            }
        }

        public ClientActionList CompleteOperation(CommunicationContext communicationContext, int entityId, int docId, IgnoreCaseDictionary<object> values)
        {
            using (new CommunicationContextScope(communicationContext))
            {
                ValidateWriteFunctional(entityId, null);
                ValidateExecuteFunctional(entityId, null);
                OrganizationRights.ValidateWrite(entityId, docId, values, "выполнение операции");
                Manager = GetDataManager<ToolsDataManager>(entityId);
                return Manager.CompleteOperation(docId, values);
            }
        }

        public void CancelOperation(CommunicationContext communicationContext, int entityId, int docId)
        {
            using (new CommunicationContextScope(communicationContext))
            {
                ValidateExecuteFunctional(entityId, null);
                OrganizationRights.ValidateWrite(entityId, docId, "отмену операции");
                Manager = GetDataManager<ToolsDataManager>(entityId);
                Manager.CancelOperation(docId);
            }
        }

        public ClientActionList Exec(CommunicationContext communicationContext, int entityId, int docId, int OperationId)
        {
            using (new CommunicationContextScope(communicationContext))
            {
                ValidateWriteFunctional(entityId, null);
                ValidateExecuteFunctional(entityId, OperationId);
                OrganizationRights.ValidateWrite(entityId, docId, "выполнение операции");
                Manager = GetDataManager<ToolsDataManager>(entityId);
                return Manager.ExecuteOperation(docId, OperationId);
            }
        }
    }
}