using System;
using System.Reflection;
using Platform.Common.Interfaces;
using Platform.PrimaryEntities.Common.Interfaces;

namespace Platform.BusinessLogic.Common.Exceptions
{
    /// <summary>
    /// Исключение при работе с контролом
    /// </summary>
    public abstract class ControlTargetException : ControlException, IComplete
    {
        protected ControlTargetException(string message, Exception innerException,MemberInfo control, IBaseEntity target ) :base(message,innerException,control)
        {
            Target = target;

        }

        protected ControlTargetException(string message,MemberInfo control, IBaseEntity target ) : this(message,null,control,target)
        {
        }

        /// <summary>
        /// Объект прикладной сущности для которой выполняется контрол
        /// </summary>
        public IBaseEntity Target { get; set; }


        public void Complete()
        {
            if (OnComplete != null)
                OnComplete();
        }

        public event Action OnComplete;
    }
}
