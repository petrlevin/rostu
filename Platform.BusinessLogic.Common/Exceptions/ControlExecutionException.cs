using System;
using System.Reflection;
using Platform.PrimaryEntities.Common.Interfaces;

namespace Platform.BusinessLogic.Common.Exceptions
{
    /// <summary>
    /// Исключение выкидывается если в коде контрола произошла ошибка
    /// Прикладной код не должен выкидывать исключение этого типа
    /// </summary>
    public class ControlExecutionException :ControlTargetException
    {
        /// <summary>
        /// конструктор
        /// </summary>
        /// <param name="message"></param>
        /// <param name="innerException"></param>
        /// <param name="control"></param>
        /// <param name="target"></param>
        public ControlExecutionException(string message, Exception innerException, MemberInfo control, IBaseEntity target) : base(message, innerException, control, target)
        {
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        /// <param name="control"></param>
        /// <param name="target"></param>
        public ControlExecutionException(string message, MemberInfo control, IBaseEntity target) : base(message, control, target)
        {
        }


    }
}
