using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Platform.Common;

namespace Sbor.DbEnums
{
    /// <summary>
    /// Тип формируемого инструмента
    /// </summary>
    public enum BalancingIFDBType
    {
        /// <summary>
        /// Балансировка ПОБА
        /// </summary>
        [EnumCaption("Балансировка ПОБА")]
        LimitBudgetAllocations = 1,

        /// <summary>
        /// Балансировка ПОБА и Деятельности ведомства
        /// </summary>
        [EnumCaption("Балансировка ПОБА и Деятельности ведомства")]
        LimitBudgetAllocationsAndActivityOfSBP = 2
    }
}
