using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sbor.Reports.Report;

namespace Sbor.Reports.Tablepart
{
// ReSharper disable InconsistentNaming
    public partial class BudgetExpenseStructure_CustomFilter : IBudgetExpenseStructureFilter
// ReSharper restore InconsistentNaming
    {
        public int[] ExpenseObligationTypeIds {
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
