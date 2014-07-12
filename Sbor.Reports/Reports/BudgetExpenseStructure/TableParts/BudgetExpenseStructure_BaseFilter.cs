using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sbor.Reports.Report;

namespace Sbor.Reports.Tablepart
{
    public partial class BudgetExpenseStructure_BaseFilter : IBudgetExpenseStructureFilter
    {
        public int[] ExpenseObligationTypeIds
        {
            get
            {
                return
                    ExpenseObligationTypes.Where(e => e.IdExpenseObligationType.HasValue)
                                          .Select(e => (int)e.IdExpenseObligationType.Value)
                                          .ToArray();
            }
        }
    }
}
