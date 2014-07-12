using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Platform.Common;

namespace Sbor.DbEnums
{
    /// <summary>
    /// Виды бюджетной деятельности
    /// </summary>
    public enum ActivityBudgetaryType
    {
        /// <summary>
        /// Доходы
        /// </summary>
        [EnumCaption("Доходы")]
        Incoming = 0,

        /// <summary>
        /// Расходы
        /// </summary>
        [EnumCaption("Расходы")]
        Costs = 1,

        /// <summary>
        /// Источники
        /// </summary>
        [EnumCaption("Источники")]
        Sources = 2,
    }
}
