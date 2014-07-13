using System.ServiceModel;
using Platform.ClientInteraction;
using Platform.Utils.Collections;

namespace BaseApp.Service.Interfaces
{
    [ServiceContract]
    public interface IOperationsManager
    {
        [OperationContract]
        void BeginOperation(CommunicationContext communicationContext, int entityId, int docId, int OperationId);
        
        [OperationContract]
        ClientActionList CompleteOperation(CommunicationContext communicationContext, int entityId, int docId, IgnoreCaseDictionary<object> values);
        
        [OperationContract]
        void CancelOperation(CommunicationContext communicationContext, int entityId, int docId);
        
        [OperationContract]
        ClientActionList Exec(CommunicationContext communicationContext, int entityId, int docId, int OperationId);
    }
}
