using System;
using System.Reflection;
using Platform.BusinessLogic.Common.Interfaces;

namespace Platform.BusinessLogic.Common.Exceptions
{
    /// <summary>
    /// Исключение при работе с операцией
    /// </summary>
    public abstract class OperationException : ActivityException 
    {
        protected OperationException(string message, Exception innerException,MemberInfo action,IOperation operation) :base(message,innerException,action)
        {

            Operation = operation;
        }

        protected OperationException(string message, MemberInfo action,IOperation operation)
            : this(message, null, action ,operation)
        {
        }

        public IOperation Operation { get; set; }


    }
}
