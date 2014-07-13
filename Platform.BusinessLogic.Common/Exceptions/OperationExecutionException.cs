using System;
using System.Reflection;
using Platform.BusinessLogic.Common.Interfaces;
using Platform.PrimaryEntities.Common.Interfaces;

namespace Platform.BusinessLogic.Common.Exceptions
{
    /// <summary>
    /// Исключение выкидывается если в коде операции произошла ошибка
    /// Прикладной код не должен выкидывать исключение этого типа
    /// </summary>
    public class OperationExecutionException :OperationException
    {
        /// <summary>
        /// конструктор
        /// </summary>
        /// <param name="message"></param>
        /// <param name="innerException"></param>
        /// <param name="memberInfo"></param>
        /// <param name="target"></param>
        public OperationExecutionException(string message, Exception innerException, MemberInfo memberInfo, IBaseEntity target,IOperation operation)
            : base(message, innerException, memberInfo, operation)
        {
            Target = target;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        /// <param name="memberInfo"></param>
        /// <param name="target"></param>
        public OperationExecutionException(string message, MemberInfo memberInfo, IBaseEntity target,IOperation operation)
            : this(message, null, memberInfo, target, operation)
        {
        }

        /// <summary>
        /// Объект прикладной сущности для которой выполняется операция
        /// </summary>
        public IBaseEntity Target { get; internal set; }

    }
}
