using System;
using System.Reflection;

namespace Platform.BusinessLogic.Common.Exceptions
{
    /// <summary>
    /// Контрол неправильно определен (не правильная сигнатура)
    /// Прикладной код не должен выкидывать исключение этого типа
    /// </summary>
    public class  ControlDefinitionException :ControlException
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        /// <param name="innerException"></param>
        /// <param name="control"></param>

        public ControlDefinitionException(string message, Exception innerException, MemberInfo control) : base(message, innerException, control)
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        /// <param name="control"></param>

        public ControlDefinitionException(string message, MemberInfo control) : base(message, control)
        {
        }
    }
}
