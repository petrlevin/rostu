using System;
using System.Reflection;

namespace Platform.BusinessLogic.Common.Exceptions
{
    /// <summary>
    /// Исключение при работе с контролом
    /// </summary>
    public abstract class ControlException : ActivityException 
    {
        protected ControlException(string message, Exception innerException,MemberInfo control) :base(message,innerException,control)
        {


        }

        protected ControlException(string message,MemberInfo control) : this(message,null,control)
        {
        }


    }
}
