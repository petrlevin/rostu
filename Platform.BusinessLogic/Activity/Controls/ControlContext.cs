using System;
using System.Reflection;
using Platform.BusinessLogic.Common.Exceptions;
using Platform.BusinessLogic.Common.Interfaces;
using Platform.BusinessLogic.Interfaces;
using Platform.Common;
using Platform.PrimaryEntities.Common.Interfaces;
using Platform.Utils;

namespace Platform.BusinessLogic.Activity.Controls
{
    /// <summary>
    /// 
    /// </summary>
    public class ControlContext : Stacked<ControlContext>
    {
        internal ControlContext(IControlDispatcher controlDispatcher, MemberInfo action, IBaseEntity target, Action onComplete , IControlInfo initialControlInfo)
        {
            if (controlDispatcher == null) throw new ArgumentNullException("controlDispatcher");
            InitialControlInfo = initialControlInfo;
            _controlDispatcher = controlDispatcher;
            Action = action;
            Target = target;
            _onComplete = onComplete;

        }


        /// <summary>
        /// Информация о правилах выполнения автоконтроля
        /// </summary>
        public IControlInfo ControlInfo { get; internal set; }

        /// <summary>
        /// Информация о контроле (из атрибутов метода)
        /// </summary>
        public IControlInfo InitialControlInfo { get; internal set; }

        private readonly IControlDispatcher _controlDispatcher;

        /// <summary>
        /// Информация о методе контроля
        /// </summary>
        public MemberInfo Action { get; internal set; }
       
        /// <summary>
        /// Сущность в которой отрабатывает контроль
        /// </summary>
        public IBaseEntity Target { get; internal set; }
        
        private readonly Action _onComplete;



        internal void Throw(string message)
        {
            var ex = new ControlResponseException(message, Action, Target);
            if (ControlInfo != null)
            {
                if (!String.IsNullOrWhiteSpace(ControlInfo.UNK))
                    ex.Caption = String.Format("Сообщение контроля УНК {0} ({1})", ControlInfo.UNK, ControlInfo.Caption);
                else
                    ex.Caption = String.Format("Сообщение контроля {0} ", ControlInfo.Caption);
                ex.UNK = ControlInfo.UNK;
            }
            else
                ex.Caption = String.Format("Сообщение контроля {0} ", Action.Name);
            if (_onComplete != null)
                ex.OnComplete += () => _onComplete();


            if (!_controlDispatcher.MaySkip(ControlInfo))
                throw ex;
            if (!_controlDispatcher.MaySkipSkippable(ex))
                throw ex;
        }

        /// <summary>
        /// Проверка на существование контекста выполнения контроля
        /// </summary>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        public static ControlContext GetRequiredCurrent()
        {
            if (Current != null)
                return Current;
            
            throw new InvalidOperationException("Не определен текущий контест исполнения контроля");
        }
    }
}
