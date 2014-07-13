using System;
using System.Reflection;
using Platform.BusinessLogic.Common.Interfaces;

namespace Platform.BusinessLogic.Common.Exceptions
{
    /// <summary>
    /// Операция неправильно определена (не правильная сигнатура)  или вообще не определена
    /// Прикладной код не должен выкидывать исключение этого типа
    /// </summary>
    public class  OperationDefinitionException :OperationException
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        /// <param name="innerException"></param>
        /// <param name="control"></param>
        /// <param name="operation"></param>
        public OperationDefinitionException(string message, Exception innerException, MemberInfo control,IOperation operation) : base(message, innerException, control,operation)
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        /// <param name="action"></param>
        /// <param name="operation"></param>
        public OperationDefinitionException(string message, MemberInfo action,IOperation operation)
            : base(message, action,operation)
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        /// <param name="operation"></param>
        public OperationDefinitionException(string message, IOperation operation)
            : base(message, null, operation)
        {
        }

    }
}
