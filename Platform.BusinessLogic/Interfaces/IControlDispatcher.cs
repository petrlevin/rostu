using System;
using Platform.BusinessLogic.Common.Exceptions;
using Platform.BusinessLogic.Common.Interfaces;
using Platform.PrimaryEntities.Common.Interfaces;

namespace Platform.BusinessLogic.Interfaces
{

    /// <summary>
    /// Диспетчер выполнения контролей
    /// </summary>
    public interface IControlDispatcher
    {
        /// <summary>
        /// Выполнить метод контроля
        /// </summary>
        /// <param name="controlAction"></param>
        /// <param name="controlName"></param>
        /// <param name="target"></param>
        void InvokeControl(Action controlAction, string controlName, IBaseEntity target);
       
        /// <summary>
        /// Возможность пропуска контролей
        /// </summary>
        /// <param name="exception"></param>
        /// <returns></returns>
        bool MaySkipSkippable(ControlResponseException exception);
        
        /// <summary>
        /// Возможность пропуска конкретного контроля
        /// </summary>
        /// <param name="info"></param>
        /// <returns></returns>
        bool MaySkip(IControlInfo info);
    }
}
