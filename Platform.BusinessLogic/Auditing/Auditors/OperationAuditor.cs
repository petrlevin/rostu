using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;
using Platform.BusinessLogic.Auditing.Auditors.Abstract;
using Platform.BusinessLogic.Auditing.Interfaces;
using Platform.BusinessLogic.Common.Enums;
using Platform.BusinessLogic.Common.Interfaces;
using Platform.Common;
using Platform.PrimaryEntities.Common.Interfaces;

namespace Platform.BusinessLogic.Auditing.Auditors
{
    public class OperationAuditor: AuditorBase, IStartEndAuditor
    {
        private int transactionScopeHash;
        
        public int EntityId { get; set; }
        public int ElementId { get; set; }
        public ProcessOperationTypes OperationType { get; set; }
        public string OperationName { get; set; }
        public int? OperationId { get; set; }

        public void Start(DateTime startedAt)
        {
            if (!configuration.OperationsEnabled)
                return;

            transactionScopeHash = Transaction.Current != null ? Transaction.Current.GetHashCode() : -1;
            if (OperationType == ProcessOperationTypes.EntityOperation)
                logger.Log(EntityId, ElementId, transactionScopeHash, OperationType, OperationId, OperationName, null);
        }

        public void End(int elapsedMilliseconds)
        {
            if (!configuration.OperationsEnabled)
                return;

            logger.Log(EntityId, ElementId, transactionScopeHash, OperationType, OperationId, OperationName, elapsedMilliseconds);
        }
    }
}
