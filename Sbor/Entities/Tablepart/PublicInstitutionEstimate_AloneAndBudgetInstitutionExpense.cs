using System;
using System.Linq;
using System.Linq.Dynamic;
using Sbor.Interfaces;
using Sbor.Logic;

// ReSharper disable CheckNamespace
namespace Sbor.Tablepart
// ReSharper restore CheckNamespace
{
// ReSharper disable InconsistentNaming
    partial class PublicInstitutionEstimate_AloneAndBudgetInstitutionExpense : ILineCost
// ReSharper restore InconsistentNaming
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="line"></param>
        public PublicInstitutionEstimate_AloneAndBudgetInstitutionExpense(ILineCost line)
        {
            foreach (var property in typeof(ILineCost).GetProperties())
            {
                var value = property.GetValue(line);
                property.SetValue(this, value);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="result"></param>
        /// <param name="line"></param>
        /// <param name="alias"></param>
        /// <returns></returns>
        public static IQueryable<PublicInstitutionEstimate_AloneAndBudgetInstitutionExpense> GetQueryByKBK(IQueryable<PublicInstitutionEstimate_AloneAndBudgetInstitutionExpense> result, ILineCost line, String alias = "")
        {
            if (!String.IsNullOrEmpty(alias))
                alias = alias + ".";
            else
                alias = String.Empty;

            foreach (var p in typeof(ILineCost).GetProperties())
            {
                var pValue = p.GetValue(line) as int? ?? p.GetValue(line) as byte?;
                if (pValue.HasValue)
                    result = result.Where(String.Format("{2}{0} == {1}", p.Name, pValue.Value, alias));
                else
                    result = result.Where(String.Format("!{1}{0}.HasValue", p.Name, alias));
            }

            return result;
        }
    }
}
