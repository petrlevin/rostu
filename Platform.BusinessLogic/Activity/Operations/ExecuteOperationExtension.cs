using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Platform.BusinessLogic.EntityTypes;
using Platform.ClientInteraction;

namespace Platform.BusinessLogic.Activity.Operations
{
    /// <summary>
    /// Расширение позволяющие вызывать операции для интерфейсов
    /// </summary>
    public static class ExecuteOperationExtension
    {
        /// <summary>
        /// Выполнить операцию
        /// </summary>
        /// <param name="operationLambda"></param>
        /// <returns></returns>
        public static ClientActionList ExecuteOperation<TInterface>(this TInterface documentAsInterface,
                                                                             Expression<Action<TInterface>>
                                                                                 operationLambda)
        {
            return new OperationInvoker<TInterface>(documentAsInterface).InvokeOperation(operationLambda);

        }
    }
}
