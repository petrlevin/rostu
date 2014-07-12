using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Platform.BusinessLogic.Common.Interfaces;
using Platform.Common.Extensions;
using Sbor.Interfaces;

namespace Sbor.Logic
{
    /// <summary>
    /// Работа с наборами КБК
    /// </summary>
    public static class KBKHelper
    {
        /// <summary>
        /// Проверка date попадает период действия версионных КБК
        /// </summary>
        /// <param name="expense"></param>
        /// <param name="date"></param>
        /// <returns></returns>
        public static string CheckVersioningKbk(this ILineCostWithRelations expense, DateTime date)
        {
            var result = new List<string> { expense.ExpenseObligationType.Caption() };
            bool hasErrors = false;

            foreach (var p in typeof(ILineCost).GetProperties())
            {
                //Имя без Id
                var pName = p.Name.Substring(2);

                var expensePropertie = typeof(ILineCostWithRelations).GetProperty(pName);
                var expenseValue = expensePropertie.GetValue(expense);

                var versionedEntity = expenseValue as IVersioning;
                if (versionedEntity != null)
                {
                    if ((versionedEntity.ValidityFrom.HasValue && versionedEntity.ValidityFrom.Value > date) ||
                        (versionedEntity.ValidityTo.HasValue && versionedEntity.ValidityTo.Value <= date))
                    {
                        hasErrors = true;
                        result.Add("<b>" + expenseValue.ToString() + "</b>");
                    }
                    else
                        result.Add(expenseValue.ToString());
                }
                else
                {
                    if(expenseValue != null)
                    result.Add(expenseValue.ToString());
                }
            }

            return hasErrors ? String.Join(", ", result) : null;
        }

        /// <summary>
        /// Русские наименование кодов КБК
        /// </summary>
        public static readonly Dictionary<string, string> RussianCaptions = new Dictionary<string, string>
            {
                {"ExpenseObligationType", "Тип РО"},
                {"FinanceSource", "ИФ"},
                {"KFO", "КФО"},
                {"KVSR", "КВСР"},
                {"RZPR", "РЗПР"},
                {"KCSR", "КЦСР"},
                {"KVR", "КВР"},
                {"KOSGU", "КОСГУ"},
                {"DFK", "ДФК"},
                {"DKR", "ДКР"},
                {"DEK", "ДЭК"},
                {"CodeSubsidy", "Код субсидии"},
                {"BranchCode", "Отраслевой код"}

            };

        /// <summary>
        /// Свойство-наименование КБК
        /// </summary>
        public static readonly Dictionary<string, string> CaptionFields = new Dictionary<string, string>
            {
                {"ExpenseObligationType", "Caption"},
                {"FinanceSource", "Code"},
                {"KFO", "Code"},
                {"KVSR", "Caption"},
                {"RZPR", "Code"},
                {"KCSR", "Code"},
                {"KVR", "Code"},
                {"KOSGU", "Code"},
                {"DFK", "Code"},
                {"DKR", "Code"},
                {"DEK", "Code"},
                {"CodeSubsidy", "Code"},
                {"BranchCode", "Code"}
            };
    }
}
