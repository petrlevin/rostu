using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Platform.BusinessLogic.Activity.Operations
{
    /// <summary>
    /// Статический фасад для подписки на операции
    /// </summary>
    public static class Operations
    {

        /// <summary>
        /// Событие перед отменой или завершением неатомарной опперации  (только в том же потоке)
        /// </summary>
        public static event OperationsHandler BeforeCancelComplete
        {
            add
            {
                if (OperationDispatcherBase.BeforeCancelCompleteEvents == null)
                    OperationDispatcherBase.BeforeCancelCompleteEvents = new List<OperationsHandler>();
                OperationDispatcherBase.BeforeCancelCompleteEvents.Add(value);

            }
            remove
            {
                if (OperationDispatcherBase.BeforeCancelCompleteEvents == null)
                    return;
                OperationDispatcherBase.BeforeCancelCompleteEvents.Remove(value);

            }

        }
    }

}
