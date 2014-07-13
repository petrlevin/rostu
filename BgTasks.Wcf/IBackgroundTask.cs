using System.ServiceModel;

namespace BgTasks.Wcf
{
    [ServiceContract]
    public interface IBackgroundTask
    {
        [OperationContract]
        void ProcessOperation(OperationPhase phase, int entityId, int docId, int entityOperationId);
    }
}
