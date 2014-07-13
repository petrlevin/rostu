using System;
using Platform.BusinessLogic.Common.Enums;

namespace Platform.BusinessLogic.Auditing.Interfaces
{
    public interface IAuditLogger
    {
        void Log(int entityId,int elementId, Operations operation , string xmlBefore, string xmlAfter);
        void Log(int entityId, int elementId, int transactionScope, ProcessOperationTypes operationType, int? operationId, string operationName, int? operationTime);
        void Log(int multilinkEntityId, int firstElementId, int secondElementId, MultilinkOperations operation);
        void Log(string methodName, int? methodTime, string jsonData);
        void Log(ISqlLogData data);

        bool IsEnabled { get; }
        bool IsOperationsEnabled { get; }
        bool IsRequestsEnabled { get; }
        void SessionStart(string sessionId, DateTime start);
        void SessionEnd(string sessionId, DateTime end);
        void Login(string sessionId, DateTime time);
    }
}