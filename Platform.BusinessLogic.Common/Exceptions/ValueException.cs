using System;
using System.Reflection;

namespace Platform.BusinessLogic.Common.Exceptions
{
    /// <summary>
    /// 
    /// </summary>
    public abstract class ValueException:ActivityException
    {
        /// <summary>
        /// Выкидывается когда запрашивеемое значение не может быть найдено
        /// по имени в контейнере ( нет метода или свойства они неправильно определены)
        /// </summary>
        /// <param name="message"></param>
        /// <param name="innerException"></param>
        /// <param name="action"></param>
        /// <param name="valueContainerType"></param>
        /// <param name="valueName"></param>
        public ValueException(string message, Exception innerException, MemberInfo action,Type valueContainerType, string valueName) : base(message, innerException, action)
        {
            ValueContainerType = valueContainerType;
            ValueName = valueName;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        /// <param name="action"></param>
        /// <param name="valueContainerType"></param>
        /// <param name="valueName"></param>
        public ValueException(string message, MemberInfo action, Type valueContainerType, string valueName)
            : this(message, null, action,valueContainerType,valueName)
        {
        }

        /// <summary>
        /// Тип объект содершащий значения (методы или свойства)
        /// </summary>
        public Type ValueContainerType { get; set; }
        /// <summary>
        /// Имя запрашиваемого значения
        /// </summary>
        public string ValueName { get; set; } 
    }
}
