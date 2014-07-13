using System;
using System.Reflection;
using Platform.Common.Exceptions;

namespace Platform.BusinessLogic.Common.Exceptions
{
    /// <summary>
    /// 
    /// </summary>
    public abstract class ActivityException :PlatformException
    {
        protected ActivityException(string message, Exception innerException,MemberInfo action) :base(message,innerException)
        {

            Action = action;
        }

        protected ActivityException(string message, MemberInfo action)
            : this(message, null, action)
        {
        }

        /// <summary>
        /// Тип объекта где определен контрол или оперция
        /// </summary>
        public Type DeclaringType { get { return Action.DeclaringType; }

        }

        /// <summary>
        /// Метод контрола или операции или объект реализующий контрол  или операцию для общих контролов и операций
        /// </summary>
        public MemberInfo Action { get; set; }
        

    }
}
