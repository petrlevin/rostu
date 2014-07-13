using System;
using System.Reflection;

namespace Platform.BusinessLogic.Common.Exceptions
{
    /// <summary>
    /// 
    /// </summary>
    public class ValueConvertException:ValueException
    {
        /// <summary>
        /// Выкидывается когда запрашивеемое значение не может быть найдено
        /// по имени в контейнере ( нет метода или свойства или они неправильно определены)
        /// </summary>
        /// <param name="message"></param>
        /// <param name="innerException"></param>
        /// <param name="action"></param>
        /// <param name="valueContainerType"></param>
        /// <param name="valueName"></param>
        /// <param name="value"></param>
        public ValueConvertException(string message, Exception innerException, MemberInfo action,Type valueContainerType, string valueName , object value) : base(message, innerException, action,valueContainerType,valueName)
        {
            Value = value;
        }

        /// <summary>
        /// 
        /// </summary>
        public ValueConvertException(Exception innerException)
            : base(null, innerException, null, null, null)
        {}

        /// <summary>
        /// 
        /// </summary>
        public Object Value { get; set; }

    }
}
