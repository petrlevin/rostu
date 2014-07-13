using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Platform.BusinessLogic.EntityTypes;
using Platform.BusinessLogic.Interfaces;
using Platform.BusinessLogic.Reference;
using Platform.ClientInteraction;

namespace Platform.BusinessLogic.Activity.Operations
{
    /// <summary>
    /// 
    /// </summary>
    public static class OperationDispatcherExtension
    {
        /// <summary>
        /// Завершить начатаю наатомарную операцию для документа
        /// </summary>
        /// <param name="operationDispatcher"></param>
        /// <param name="document"></param>
        /// <returns></returns>
        public static ClientActionList CompleteOperation(this IOperationDispatcher operationDispatcher, ToolEntity document)
        {
            EntityOperation unused;
            return operationDispatcher.CompleteOperation(document, out unused);
        }
    }
}
