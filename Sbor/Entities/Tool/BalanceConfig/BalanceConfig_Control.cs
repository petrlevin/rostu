using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Platform.BusinessLogic;
using Platform.BusinessLogic.Activity.Controls;
using Platform.BusinessLogic.Common.Enums;
using Platform.BusinessLogic.EntityTypes;
using Platform.BusinessLogic.Common.Interfaces;
using Platform.BusinessLogic.Interfaces;
using Platform.Common.Extensions;
using Platform.PrimaryEntities.Reference;
using System.Xml.Linq;
using System.ComponentModel.DataAnnotations.Schema;
using Platform.Application.Common;
using Platform.Utils.Common;
using BaseApp.Interfaces;
using Sbor.Tablepart;


namespace Sbor.Tool
{
	/// <summary>
	/// Настройка балансировки
	/// </summary>
	public partial class BalanceConfig
	{
        //public string filterKBK_ToString(BalanceConfig_FilterKBK str)
        //{
        //    var texts = new List<string>();
        //
        //    //texts.Add("Вид бюджетной деятельности " + str.ActivityBudgetaryType.Caption());
        //    if (str.ExpenseObligationType.HasValue)
        //        texts.Add("Тип РО " + str.ExpenseObligationType.Value.Caption());
        //    if (str.IdFinanceSource.HasValue)
        //        texts.Add(str.FinanceSource.ToString());
        //    if (str.IdKFO.HasValue)
        //        texts.Add(str.KFO.ToString());
        //    if (str.IdKVSR.HasValue)
        //        texts.Add(str.KVSR.ToString());
        //    if (str.IdRZPR.HasValue)
        //        texts.Add(str.RZPR.ToString());
        //    if (str.IdKCSR.HasValue)
        //        texts.Add(str.KCSR.ToString());
        //    if (str.IdKVR.HasValue)
        //        texts.Add(str.KVR.ToString());
        //    if (str.IdKOSGU.HasValue)
        //        texts.Add(str.KOSGU.ToString());
        //    if (str.IdDFK.HasValue)
        //        texts.Add(str.DFK.ToString());
        //    if (str.IdDKR.HasValue)
        //        texts.Add(str.DKR.ToString());
        //    if (str.IdDEK.HasValue)
        //        texts.Add(str.DEK.ToString());
        //    if (str.IdCodeSubsidy.HasValue)
        //        texts.Add(str.CodeSubsidy.ToString());
        //    if (str.IdBranchCode.HasValue)
        //        texts.Add(str.BranchCode.ToString());
        //
        //    return string.Join(", ", texts);
        //}

        /// <summary>   
        /// Контроль "Контроль на непересечение правил"
        /// </summary>         
        [ControlInitial(InitialUNK = "600101", InitialCaption = "Контроль на непересечение правил", InitialSkippable = true)]
        public void Control_600101(DataContext context)
        {
            //var q = context.BalanceConfig_FilterKBK.Where(w =>
            //    w.IdOwner == Id
            //    && context.BalanceConfig_FilterKBK.Any(a =>
            //        a.IdOwner == Id
            //        && a.IdMaster != w.IdMaster
            //        && (a.IdExpenseObligationType == w.IdExpenseObligationType || !a.IdExpenseObligationType.HasValue || !w.IdExpenseObligationType.HasValue )
            //        && (a.IdFinanceSource == w.IdFinanceSource || !a.IdFinanceSource.HasValue || !w.IdFinanceSource.HasValue )
            //        && (a.IdKFO == w.IdKFO || !a.IdKFO.HasValue || !w.IdKFO.HasValue )
            //        && (a.IdKVSR == w.IdKVSR || !a.IdKVSR.HasValue || !w.IdKVSR.HasValue )
            //        && (a.IdRZPR == w.IdRZPR || !a.IdRZPR.HasValue || !w.IdRZPR.HasValue )
            //        && (a.IdKCSR == w.IdKCSR || !a.IdKCSR.HasValue || !w.IdKCSR.HasValue )
            //        && (a.IdKVR == w.IdKVR   || !a.IdKVR.HasValue  || !w.IdKVR.HasValue )
            //        && (a.IdKOSGU == w.IdKOSGU || !a.IdKOSGU.HasValue || !w.IdKOSGU.HasValue )
            //        && (a.IdDFK == w.IdDFK || !a.IdDFK.HasValue || !w.IdDFK.HasValue )
            //        && (a.IdDKR == w.IdDKR || !a.IdDKR.HasValue || !w.IdDKR.HasValue )
            //        && (a.IdDEK == w.IdDEK || !a.IdDEK.HasValue || !w.IdDEK.HasValue )
            //        && (a.IdCodeSubsidy == w.IdCodeSubsidy || !a.IdCodeSubsidy.HasValue || !w.IdCodeSubsidy.HasValue )
            //        && (a.IdBranchCode  == w.IdBranchCode  || !a.IdBranchCode.HasValue  || !w.IdBranchCode.HasValue  )
            //    )
            //);
            //
            //List<string> list = new List<string>();
            //foreach (var rule in q.GroupBy(g => g.Master))
            //{
            //    list.Add(rule.Key.Caption + " :");
            //    foreach (var str in rule)
            //    {
            //        list.Add(" - " + filterKBK_ToString(str));
            //    }
            //}
            //
            //Controls.Check(list, "Произошло пересечение правил по строкам:<br>{0}");
        }
	}
}