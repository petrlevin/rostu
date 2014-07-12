using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Platform.BusinessLogic;
using Platform.Common;
using Platform.PrimaryEntities.Common.Interfaces;
using Sbor.DbEnums;
using Sbor.Reports.Tablepart;

namespace Sbor.Reports.Report
{
    partial class BudgetExpenseStructure
    {
        private string GetCustomFilter(BudgetExpenseStructure_CustomColumn column, string prefix = "R")
        {
            if (column.BudgetExpenseStructure_CustomFilter.Any())
            {
                var filterParts = BuildCustomFilters(column, prefix).ToList();
                
                if (filterParts.Any())
                {
                    var query = " Where " + String.Join(" OR ", filterParts) + " ";
                    return query;
                }
            }

            return String.Empty;
        }

        private IEnumerable<string> BuildCustomFilters(BudgetExpenseStructure_CustomColumn column, string prefix)
        {
            var result = new List<String>();
            foreach (var filter in column.BudgetExpenseStructure_CustomFilter.ToList() )
            {
                var temp = BuildBaseFilter(filter, prefix);

                if (!String.IsNullOrEmpty(temp))
                    result.Add(temp);
            }

            return result;
        }

       

    }
}
