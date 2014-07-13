using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Platform.BusinessLogic.Common.Enums;

namespace Platform.BusinessLogic.Common.Interfaces
{
    /// <summary>
    /// Общий контроль.
    /// Контроль, срабатывающий для нескольких сущностей, реализующийх интерфейс TTarget.
    /// </summary>
    /// <typeparam name="TTarget"></typeparam>
    /// <typeparam name="TDataContext"></typeparam>
    public interface ICommonControl<in TTarget, in TDataContext> 
    {
        void Execute(TDataContext dataContext,ControlType controlType, Sequence sequence , TTarget element , TTarget oldElement);
    }
}
