using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Platform.BusinessLogic.Activity.Values
{
    /// <summary>
    /// опции для  получения значений из объекта
    /// </summary>
    [Flags]
    public enum Options
    {
        IgnoreCase = 1,
        UseProperty = 2,
        UseMethod = 4,
        Default = UseProperty | UseMethod | IgnoreCase
    }
}
