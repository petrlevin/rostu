using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Platform.BusinessLogic.Common.Enums;

namespace Platform.BusinessLogic.Common.Interfaces
{
    /// <summary>
    /// Свободный общий контроль.
    /// </summary>
    /// <typeparam name="TTarget"></typeparam>
    /// <typeparam name="TDataContext"></typeparam>
    public interface IFreeCommonControl<in TTarget, in TDataContext> 
    {
        void Execute(TDataContext dataContext, TTarget element );
    }
}
