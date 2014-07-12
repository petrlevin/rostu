using System;
using System.Text;
using Sbor.Document;
using Sbor.Tablepart;
using Platform.Common.Extensions;

namespace Sbor.Logic
{
    /// <summary>
    /// Методы расширения для строк данных в документе "Предельные объемы бюджетных ассигнований"
    /// </summary>
    public static class LimitBudgetAllocations_LimitAllocationsHelper
    {
        /// <summary>
        /// Проверка заполнения полей действующими элементами
        /// </summary>
        /// <param name="data"></param>
        /// <param name="limitBudget"></param>
        /// <returns></returns>
        public static string CheckActual(this LimitBudgetAllocations_LimitAllocations data, LimitBudgetAllocations limitBudget)
        {
            return data.CheckActual(limitBudget.Date, limitBudget.IdPublicLegalFormation);
        }

        /// <summary>
        /// Проверка заполнения полей действующими элементами
        /// </summary>
        /// <param name="data"></param>
        /// <param name="date"></param>
        /// <param name="idPublicLegalFormation"></param>
        /// <returns></returns>
        public static string CheckActual(this LimitBudgetAllocations_LimitAllocations data, DateTime date, int idPublicLegalFormation)
        {
            var msg = new StringBuilder();
            var hasErrors = false;

            if (data.ExpenseObligationType != null)
                msg.AppendFormat("Тип РО  {0}", data.ExpenseObligationType.Caption());

            if (data.FinanceSource != null)
                msg.Append(data.FinanceSource.ToString() );

            if (data.KFO != null)
            {
                var tempMsg = data.KFO.ToString();
                if (data.KFO.ValidityFrom > date || data.KFO.ValidityTo < date)
                {
                    hasErrors = true;
                    tempMsg = "<b>" + tempMsg + "</b>";
                }
                msg.Append(", " + tempMsg);
            }

            if (data.KVSR != null)
            {
                var tempMsg = data.KVSR.ToString();
                if (data.KVSR.ValidityFrom > date || data.KVSR.ValidityTo < date || data.KVSR.IdPublicLegalFormation != idPublicLegalFormation)
                {
                    hasErrors = true;
                    tempMsg = "<b>" + tempMsg + "</b>";
                }
                msg.Append(", " + tempMsg);
            }

            if (data.RZPR != null)
            {
                var tempMsg = data.RZPR.ToString();
                if (data.RZPR.ValidityFrom > date || data.RZPR.ValidityTo < date)
                {
                    hasErrors = true;
                    tempMsg = "<b>" + tempMsg + "</b>";
                }
                msg.Append(", " + tempMsg);
            }

            if (data.KCSR != null)
            {
                var tempMsg = data.KCSR.ToString();
                if (data.KCSR.ValidityFrom > date || data.KCSR.ValidityTo < date || data.KCSR.IdPublicLegalFormation != idPublicLegalFormation)
                {
                    hasErrors = true;
                    tempMsg = "<b>" + tempMsg + "</b>";
                }
                msg.Append(", " + tempMsg);
            }

            if (data.KVR != null)
            {
                var tempMsg = data.KVR.ToString();
                if (data.KVR.ValidityFrom > date || data.KVR.ValidityTo < date || data.KVR.IdPublicLegalFormation != idPublicLegalFormation)
                {
                    hasErrors = true;
                    tempMsg = "<b>" + tempMsg + "</b>";
                }
                msg.Append(", " + tempMsg);
            }

            if (data.KOSGU != null)
            {
                var tempMsg = data.KOSGU.ToString();
                if (data.KOSGU.ValidityFrom > date || data.KOSGU.ValidityTo < date)
                {
                    hasErrors = true;
                    tempMsg = "<b>" + tempMsg + "</b>";
                }
                msg.Append(", " + tempMsg);
            }

            if (data.DFK != null)
            {
                var tempMsg = data.DFK.ToString();
                if (data.DFK.ValidityFrom > date || data.DFK.ValidityTo < date || data.DFK.IdPublicLegalFormation != idPublicLegalFormation)
                {
                    hasErrors = true;
                    tempMsg = "<b>" + tempMsg + "</b>";
                }
                msg.Append(", " + tempMsg);
            }

            if (data.DKR != null)
            {
                var tempMsg = data.DKR.ToString();
                if (data.DKR.ValidityFrom > date || data.DKR.ValidityTo < date || data.DKR.IdPublicLegalFormation != idPublicLegalFormation)
                {
                    hasErrors = true;
                    tempMsg = "<b>" + tempMsg + "</b>";
                }
                msg.Append(", " + tempMsg);
            }

            if (data.DEK != null)
            {
                var tempMsg = data.DEK.ToString();
                if (data.DEK.ValidityFrom > date || data.DEK.ValidityTo < date || data.DEK.IdPublicLegalFormation != idPublicLegalFormation)
                {
                    hasErrors = true;
                    tempMsg = "<b>" + tempMsg + "</b>";
                }
                msg.Append(", " + tempMsg);
            }

            if (data.CodeSubsidy != null)
            {
                var tempMsg = data.CodeSubsidy.ToString();
                if (data.CodeSubsidy.ValidityFrom > date || data.CodeSubsidy.ValidityTo < date || data.CodeSubsidy.IdPublicLegalFormation != idPublicLegalFormation)
                {
                    hasErrors = true;
                    tempMsg = "<b>" + tempMsg + "</b>";
                }
                msg.Append(", " + tempMsg);
            }

            if (data.BranchCode  != null)
            {
                var tempMsg = data.BranchCode.ToString();
                if (data.BranchCode.ValidityFrom > date || data.BranchCode.ValidityTo < date || data.BranchCode.IdPublicLegalFormation != idPublicLegalFormation)
                {
                    hasErrors = true;
                    tempMsg = "<b>" + tempMsg + "</b>";
                }
                msg.Append(", " + tempMsg);
            }

            return hasErrors ? msg.ToString().TrimStart(',').Trim() : String.Empty;
        }

    }
}
