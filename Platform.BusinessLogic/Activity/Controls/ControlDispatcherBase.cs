using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Practices.Unity;
using Platform.BusinessLogic.Common.Exceptions;
using Platform.BusinessLogic.Common.Interfaces;
using Platform.BusinessLogic.Interfaces;
using Platform.Common;
using Platform.Common.Exceptions;
using Platform.PrimaryEntities.Common.Interfaces;

namespace Platform.BusinessLogic.Activity.Controls
{
    
    /// <summary>
    /// Диспетчер выполнения контролей по-умолчанию. 
    /// Запрещено пропускать контроли
    /// </summary>
    public class ControlDispatcherBase: IControlDispatcher
    {
        /// <summary>
        /// Выполнить метод контроля
        /// </summary>
        /// <param name="controlAction"></param>
        /// <param name="controlName"></param>
        /// <param name="target"></param>
        public virtual void InvokeControl(Action controlAction,string controlName, IBaseEntity target)
        {
            controlAction();
        }

        /// <summary>
        /// Возможность пропуска контролей
        /// Запрещено
        /// </summary>
        /// <param name="exception"></param>
        /// <returns></returns>
        public virtual bool MaySkipSkippable(ControlResponseException exception)
        {
            return false;
        }

        /// <summary>
        /// Возможность пропуска конкретного контроля
        /// Запрещено
        /// </summary>
        /// <param name="info"></param>
        /// <returns></returns>
        public virtual bool MaySkip(IControlInfo info)
        {
            return false;
        }
    }
}
