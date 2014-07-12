using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sbor.Interfaces;

namespace Sbor.Logic
{
    public static class NonDenormalizaedHelper
    {
        /// <summary>
        /// Проверка наличия строк с нулевыми суммами
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static string CheckNotNullSum(this IDenormilizedExpense data, DataContext context)
        {
            return (!data.OFG.HasValue || data.OFG == 0) && (!data.PFG1.HasValue || data.PFG1 == 0) && (!data.PFG2.HasValue || data.PFG2 == 0)
                        ? data.GetEstimatedLine(context).ToString()
                        : String.Empty;
        }

        /// <summary>
        /// Проверка сумм на отрицательность
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static string CheckPositiveSum(this IDenormilizedExpense data, DataContext context)
        {
            return (data.OFG < 0 || data.PFG1 < 0 || data.PFG2 < 0) ? data.GetEstimatedLine(context).ToString() : String.Empty;
        }

        /// <summary>
        /// Проверка сумм на отрицательность
        /// </summary>
        /// <param name="data"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public static bool HasNonPositiveSum(this IDenormilizedExpense data, DataContext context)
        {
            return (data.OFG < 0 || data.PFG1 < 0 || data.PFG2 < 0);
        }
    }
}
