using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Platform.BusinessLogic.DbEnums
{
    /// <summary>
    /// Агрегатные функции
    /// </summary>
    public enum AggregateFunction
    {
        /// <summary>
        /// Сумма
        /// </summary>
        Sum = 1,

        /// <summary>
        /// Сумма листовых элементов
        /// </summary>
        LeafSum = 2
    }
}
