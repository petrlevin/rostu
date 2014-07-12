using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sbor.Logic;

namespace Sbor.Interfaces
{
    /// <summary>
    /// Строка с КБК и денежными значениями за года \in [Бюджет.Год -- Бюджет.Год + 2]
    /// </summary>
    public interface IDenormilizedExpense : ILineCost
    {
        /// <summary>
        /// `'Сумма ' + ({BudgetYear} + 1) + ', руб.'`
        /// </summary>
        decimal? PFG1 { get; set; }

        /// <summary>
        /// `'Сумма ' + ({BudgetYear}) + ', руб.'`
        /// </summary>
         decimal? OFG { get; set; }

        /// <summary>
        /// `'Сумма ' + ({BudgetYear} + 2) + ', руб.'`
        /// </summary>
         decimal? PFG2 { get; set; }
    }
}
