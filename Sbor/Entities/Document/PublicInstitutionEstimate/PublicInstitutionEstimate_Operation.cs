using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using BaseApp;
using Platform.BusinessLogic;
using Platform.BusinessLogic.Activity.Controls;
using Platform.Common.Exceptions;
using Sbor.CommonControls;
using Platform.BusinessLogic.Reference;
using Sbor.DbEnums;
using Sbor.Logic;
using Sbor.Registry;
using Sbor.Tablepart;
using ValueType = Sbor.DbEnums.ValueType;

// ReSharper disable CheckNamespace
namespace Sbor.Document
// ReSharper restore CheckNamespace
{
    public partial class PublicInstitutionEstimate
    {
        #region Вспомогательные методы

        private bool SetBlankActual(DataContext context)
        {
            var newBlanks =
                context.SBP_BlankHistory.Where(r =>
                                               r.IdBudget == this.IdBudget && r.IdOwner == this.SBP.IdParent &&
                                               r.IdBlankType == (byte) DbEnums.BlankType.FormationKU)
                       .OrderByDescending(o => o.DateCreate);

            SBP_BlankHistory oldBlankActual = this.SBP_BlankActual;

            this.SBP_BlankActual = newBlanks.FirstOrDefault();

            if (this.SBP.SBPType == DbEnums.SBPType.TreasuryEstablishment && this.SBP.IsFounder)
            {
                newBlanks =
                    context.SBP_BlankHistory.Where(r =>
                                                   r.IdBudget == this.IdBudget && r.IdOwner == this.SBP.Id &&
                                                   r.IdBlankType == (byte) DbEnums.BlankType.FormationAUBU)
                           .OrderByDescending(o => o.DateCreate);

                this.SBP_BlankActualAuBu = newBlanks.FirstOrDefault();
            }

            return this.SBP_BlankActual != null && (oldBlankActual == null || !SBP_BlankHelper.IsEqualBlank(oldBlankActual, this.SBP_BlankActual));
        }


        private void TrimKbkByNewActualBlank(DataContext context)
        {
            var gkbktrim_Expense =
                (from kbk in context.PublicInstitutionEstimate_Expense.
                                     Where(r => r.IdOwner == this.Id).
                                     ToList().
                                     Select(s =>
                                            new
                                                {
                                                    s.IdOwner,
                                                    s.IdMaster,
                                                    s.IdAuthorityOfExpenseObligation,
                                                    s.IdOKATO,
                                                    s.IsIndirectCosts,
                                                    s.IdIndirectCostsDistributionMethod,
                                                    IdBranchCode = SBP_BlankHelper.IsBvtEmpty(SBP_BlankActual.IdBlankValueType_BranchCode) ? null : s.IdBranchCode,
                                                    IdCodeSubsidy = SBP_BlankHelper.IsBvtEmpty(SBP_BlankActual.IdBlankValueType_CodeSubsidy) ? null : s.IdCodeSubsidy,
                                                    IdDEK = SBP_BlankHelper.IsBvtEmpty(SBP_BlankActual.IdBlankValueType_DEK) ? null : s.IdDEK,
                                                    IdDFK = SBP_BlankHelper.IsBvtEmpty(SBP_BlankActual.IdBlankValueType_DFK) ? null : s.IdDFK,
                                                    IdDKR = SBP_BlankHelper.IsBvtEmpty(SBP_BlankActual.IdBlankValueType_DKR) ? null : s.IdDKR,
                                                    IdExpenseObligationType = SBP_BlankHelper.IsBvtEmpty(SBP_BlankActual.IdBlankValueType_ExpenseObligationType) ? null : s.IdExpenseObligationType,
                                                    IdFinanceSource = SBP_BlankHelper.IsBvtEmpty(SBP_BlankActual.IdBlankValueType_FinanceSource) ? null : s.IdFinanceSource,
                                                    IdKCSR = SBP_BlankHelper.IsBvtEmpty(SBP_BlankActual.IdBlankValueType_KCSR) ? null : s.IdKCSR,
                                                    IdKOSGU = SBP_BlankHelper.IsBvtEmpty(SBP_BlankActual.IdBlankValueType_KOSGU) ? null : s.IdKOSGU,
                                                    IdKFO = SBP_BlankHelper.IsBvtEmpty(SBP_BlankActual.IdBlankValueType_KFO) ? null : s.IdKFO,
                                                    IdKVR = SBP_BlankHelper.IsBvtEmpty(SBP_BlankActual.IdBlankValueType_KVR) ? null : s.IdKVR,
                                                    IdKVSR = SBP_BlankHelper.IsBvtEmpty(SBP_BlankActual.IdBlankValueType_KVSR) ? null : s.IdKVSR,
                                                    IdRZPR = SBP_BlankHelper.IsBvtEmpty(SBP_BlankActual.IdBlankValueType_RZPR) ? null : s.IdRZPR,
                                                    s.OFG,
                                                    s.PFG1,
                                                    s.PFG2,
                                                    s.AdditionalOFG,
                                                    s.AdditionalPFG1,
                                                    s.AdditionalPFG2
                                                })
                 select new {kbk}).
                    GroupBy(g =>
                            new
                                {
                                    g.kbk.IdOwner,
                                    g.kbk.IdMaster,
                                    g.kbk.IdAuthorityOfExpenseObligation,
                                    g.kbk.IdOKATO,
                                    g.kbk.IsIndirectCosts,
                                    g.kbk.IdIndirectCostsDistributionMethod,
                                    g.kbk.IdBranchCode,
                                    g.kbk.IdCodeSubsidy,
                                    g.kbk.IdDEK,
                                    g.kbk.IdDFK,
                                    g.kbk.IdDKR,
                                    g.kbk.IdExpenseObligationType,
                                    g.kbk.IdFinanceSource,
                                    g.kbk.IdKCSR,
                                    g.kbk.IdKOSGU,
                                    g.kbk.IdKFO,
                                    g.kbk.IdKVR,
                                    g.kbk.IdKVSR,
                                    g.kbk.IdRZPR
                                }).
                    Select(s =>
                           new
                               {
                                   kbk = s.Key,
                                   OFG = s.Sum(ss => ss.kbk.OFG),
                                   PFG1 = s.Sum(ss => ss.kbk.PFG1),
                                   PFG2 = s.Sum(ss => ss.kbk.PFG2),
                                   AdditionalOFG = s.Sum(ss => ss.kbk.AdditionalOFG),
                                   AdditionalPFG1 = s.Sum(ss => ss.kbk.AdditionalPFG1),
                                   AdditionalPFG2 = s.Sum(ss => ss.kbk.AdditionalPFG2)
                               }).
                    Select(kbk =>
                           new PublicInstitutionEstimate_Expense()
                               {
                                   IdOwner = this.Id,
                                   IdMaster = kbk.kbk.IdMaster,
                                   IdAuthorityOfExpenseObligation = kbk.kbk.IdAuthorityOfExpenseObligation,
                                   IdOKATO = kbk.kbk.IdOKATO,
                                   IsIndirectCosts = kbk.kbk.IsIndirectCosts,
                                   IdIndirectCostsDistributionMethod =
                                       kbk.kbk.IdIndirectCostsDistributionMethod,
                                   IdBranchCode = kbk.kbk.IdBranchCode,
                                   IdCodeSubsidy = kbk.kbk.IdCodeSubsidy,
                                   IdDEK = kbk.kbk.IdDEK,
                                   IdDFK = kbk.kbk.IdDFK,
                                   IdDKR = kbk.kbk.IdDKR,
                                   IdExpenseObligationType =
                                       kbk.kbk.IdExpenseObligationType,
                                   IdFinanceSource = kbk.kbk.IdFinanceSource,
                                   IdKCSR = kbk.kbk.IdKCSR,
                                   IdKOSGU = kbk.kbk.IdKOSGU,
                                   IdKFO = kbk.kbk.IdKFO,
                                   IdKVR = kbk.kbk.IdKVR,
                                   IdKVSR = kbk.kbk.IdKVSR,
                                   IdRZPR = kbk.kbk.IdRZPR,
                                   OFG = kbk.OFG,
                                   PFG1 = kbk.PFG1,
                                   PFG2 = kbk.PFG2,
                                   AdditionalOFG = kbk.AdditionalOFG,
                                   AdditionalPFG1 = kbk.AdditionalPFG1,
                                   AdditionalPFG2 = kbk.AdditionalPFG2
                               }).
                    ToList();

            context.PublicInstitutionEstimate_Expense.RemoveAll(context.PublicInstitutionEstimate_Expense.Where(r => r.IdOwner == this.Id));

            context.PublicInstitutionEstimate_Expense.InsertAsTableValue(gkbktrim_Expense, context);

            var gkbktrim_IndirectExpenses =
                (from kbk in context.PublicInstitutionEstimate_IndirectExpenses.
                                     Where(r => r.IdOwner == this.Id).
                                     ToList().
                                     Select(s =>
                                            new
                                                {
                                                    s.IdOwner,
                                                    s.IdMaster,
                                                    IdBranchCode = SBP_BlankHelper.IsBvtEmpty(SBP_BlankActual.IdBlankValueType_BranchCode) ? null : s.IdBranchCode,
                                                    IdCodeSubsidy = SBP_BlankHelper.IsBvtEmpty(SBP_BlankActual.IdBlankValueType_CodeSubsidy) ? null : s.IdCodeSubsidy,
                                                    IdDEK = SBP_BlankHelper.IsBvtEmpty(SBP_BlankActual.IdBlankValueType_DEK) ? null : s.IdDEK,
                                                    IdDFK = SBP_BlankHelper.IsBvtEmpty(SBP_BlankActual.IdBlankValueType_DFK) ? null : s.IdDFK,
                                                    IdDKR = SBP_BlankHelper.IsBvtEmpty(SBP_BlankActual.IdBlankValueType_DKR) ? null : s.IdDKR,
                                                    IdExpenseObligationType = SBP_BlankHelper.IsBvtEmpty(SBP_BlankActual.IdBlankValueType_ExpenseObligationType) ? null : s.IdExpenseObligationType,
                                                    IdFinanceSource = SBP_BlankHelper.IsBvtEmpty(SBP_BlankActual.IdBlankValueType_FinanceSource) ? null : s.IdFinanceSource,
                                                    IdKCSR = SBP_BlankHelper.IsBvtEmpty(SBP_BlankActual.IdBlankValueType_KCSR) ? null : s.IdKCSR,
                                                    IdKOSGU = SBP_BlankHelper.IsBvtEmpty(SBP_BlankActual.IdBlankValueType_KOSGU) ? null : s.IdKOSGU,
                                                    IdKFO = SBP_BlankHelper.IsBvtEmpty(SBP_BlankActual.IdBlankValueType_KFO) ? null : s.IdKFO,
                                                    IdKVR = SBP_BlankHelper.IsBvtEmpty(SBP_BlankActual.IdBlankValueType_KVR) ? null : s.IdKVR,
                                                    IdKVSR = SBP_BlankHelper.IsBvtEmpty(SBP_BlankActual.IdBlankValueType_KVSR) ? null : s.IdKVSR,
                                                    IdRZPR = SBP_BlankHelper.IsBvtEmpty(SBP_BlankActual.IdBlankValueType_RZPR) ? null : s.IdRZPR,
                                                    s.OFG,
                                                    s.PFG1,
                                                    s.PFG2
                                                })
                 select new {kbk}).GroupBy(g =>
                                           new
                                               {
                                                   g.kbk.IdOwner,
                                                   g.kbk.IdMaster,
                                                   g.kbk.IdBranchCode,
                                                   g.kbk.IdCodeSubsidy,
                                                   g.kbk.IdDEK,
                                                   g.kbk.IdDFK,
                                                   g.kbk.IdDKR,
                                                   g.kbk.IdExpenseObligationType,
                                                   g.kbk.IdFinanceSource,
                                                   g.kbk.IdKCSR,
                                                   g.kbk.IdKOSGU,
                                                   g.kbk.IdKFO,
                                                   g.kbk.IdKVR,
                                                   g.kbk.IdKVSR,
                                                   g.kbk.IdRZPR
                                               }).
                                   Select(s =>
                                          new
                                              {
                                                  kbk = s.Key,
                                                  OFG = s.Sum(ss => ss.kbk.OFG),
                                                  PFG1 = s.Sum(ss => ss.kbk.PFG1),
                                                  PFG2 = s.Sum(ss => ss.kbk.PFG2)
                                              }).
                                   Select(kbk =>
                                          new PublicInstitutionEstimate_IndirectExpenses()
                                              {
                                                  IdOwner = this.Id,
                                                  IdMaster = kbk.kbk.IdMaster,
                                                  IdBranchCode = kbk.kbk.IdBranchCode,
                                                  IdCodeSubsidy = kbk.kbk.IdCodeSubsidy,
                                                  IdDEK = kbk.kbk.IdDEK,
                                                  IdDFK = kbk.kbk.IdDFK,
                                                  IdDKR = kbk.kbk.IdDKR,
                                                  IdExpenseObligationType = kbk.kbk.IdExpenseObligationType,
                                                  IdFinanceSource = kbk.kbk.IdFinanceSource,
                                                  IdKCSR = kbk.kbk.IdKCSR,
                                                  IdKOSGU = kbk.kbk.IdKOSGU,
                                                  IdKFO = kbk.kbk.IdKFO,
                                                  IdKVR = kbk.kbk.IdKVR,
                                                  IdKVSR = kbk.kbk.IdKVSR,
                                                  IdRZPR = kbk.kbk.IdRZPR,
                                                  OFG = kbk.OFG,
                                                  PFG1 = kbk.PFG1,
                                                  PFG2 = kbk.PFG2
                                              }).ToList();

            context.PublicInstitutionEstimate_IndirectExpenses.RemoveAll(context.PublicInstitutionEstimate_IndirectExpenses.Where(r => r.IdOwner == this.Id));

            context.PublicInstitutionEstimate_IndirectExpenses.InsertAsTableValue(gkbktrim_IndirectExpenses, context);

            if (SBP_BlankActualAuBu == null)
            {
                return;
            }

            var gkbktrim_FounderExpense =
                (from kbk in context.PublicInstitutionEstimate_FounderAUBUExpense.
                                     Where(r => r.IdOwner == this.Id).
                                     ToList().
                                     Select(s =>
                                            new
                                                {
                                                    s.IdOwner,
                                                    s.IdMaster,
                                                    IdBranchCode = SBP_BlankHelper.IsBvtEmpty(SBP_BlankActual.IdBlankValueType_BranchCode) ? null : s.IdBranchCode,
                                                    IdCodeSubsidy = SBP_BlankHelper.IsBvtEmpty(SBP_BlankActual.IdBlankValueType_CodeSubsidy) ? null : s.IdCodeSubsidy,
                                                    IdDEK = SBP_BlankHelper.IsBvtEmpty(SBP_BlankActual.IdBlankValueType_DEK) ? null : s.IdDEK,
                                                    IdDFK = SBP_BlankHelper.IsBvtEmpty(SBP_BlankActual.IdBlankValueType_DFK) ? null : s.IdDFK,
                                                    IdDKR = SBP_BlankHelper.IsBvtEmpty(SBP_BlankActual.IdBlankValueType_DKR) ? null : s.IdDKR,
                                                    IdExpenseObligationType = SBP_BlankHelper.IsBvtEmpty(SBP_BlankActual.IdBlankValueType_ExpenseObligationType) ? null : s.IdExpenseObligationType,
                                                    IdFinanceSource = SBP_BlankHelper.IsBvtEmpty(SBP_BlankActual.IdBlankValueType_FinanceSource) ? null : s.IdFinanceSource,
                                                    IdKCSR = SBP_BlankHelper.IsBvtEmpty(SBP_BlankActual.IdBlankValueType_KCSR) ? null : s.IdKCSR,
                                                    IdKOSGU = SBP_BlankHelper.IsBvtEmpty(SBP_BlankActual.IdBlankValueType_KOSGU) ? null : s.IdKOSGU,
                                                    IdKFO = SBP_BlankHelper.IsBvtEmpty(SBP_BlankActual.IdBlankValueType_KFO) ? null : s.IdKFO,
                                                    IdKVR = SBP_BlankHelper.IsBvtEmpty(SBP_BlankActual.IdBlankValueType_KVR) ? null : s.IdKVR,
                                                    IdKVSR = SBP_BlankHelper.IsBvtEmpty(SBP_BlankActual.IdBlankValueType_KVSR) ? null : s.IdKVSR,
                                                    IdRZPR = SBP_BlankHelper.IsBvtEmpty(SBP_BlankActual.IdBlankValueType_RZPR) ? null : s.IdRZPR,
                                                    s.OFG,
                                                    s.PFG1,
                                                    s.PFG2,
                                                    s.AdditionalOFG,
                                                    s.AdditionalPFG1,
                                                    s.AdditionalPFG2
                                                })
                 select new {kbk}).GroupBy(g =>
                                           new
                                               {
                                                   g.kbk.IdOwner,
                                                   g.kbk.IdMaster,
                                                   g.kbk.IdBranchCode,
                                                   g.kbk.IdCodeSubsidy,
                                                   g.kbk.IdDEK,
                                                   g.kbk.IdDFK,
                                                   g.kbk.IdDKR,
                                                   g.kbk.IdExpenseObligationType,
                                                   g.kbk.IdFinanceSource,
                                                   g.kbk.IdKCSR,
                                                   g.kbk.IdKOSGU,
                                                   g.kbk.IdKFO,
                                                   g.kbk.IdKVR,
                                                   g.kbk.IdKVSR,
                                                   g.kbk.IdRZPR,
                                               }).
                                   Select(s =>
                                          new
                                              {
                                                  kbk = s.Key,
                                                  OFG = s.Sum(ss => ss.kbk.OFG),
                                                  PFG1 = s.Sum(ss => ss.kbk.PFG1),
                                                  PFG2 = s.Sum(ss => ss.kbk.PFG2),
                                                  AdditionalOFG = s.Sum(ss => ss.kbk.AdditionalOFG),
                                                  AdditionalPFG1 = s.Sum(ss => ss.kbk.AdditionalPFG1),
                                                  AdditionalPFG2 = s.Sum(ss => ss.kbk.AdditionalPFG2)
                                              }).
                                   Select(kbk =>
                                          new PublicInstitutionEstimate_FounderAUBUExpense()
                                              {
                                                  IdOwner = this.Id,
                                                  IdMaster = kbk.kbk.IdMaster,
                                                  IdBranchCode = kbk.kbk.IdBranchCode,
                                                  IdCodeSubsidy = kbk.kbk.IdCodeSubsidy,
                                                  IdDEK = kbk.kbk.IdDEK,
                                                  IdDFK = kbk.kbk.IdDFK,
                                                  IdDKR = kbk.kbk.IdDKR,
                                                  IdExpenseObligationType = kbk.kbk.IdExpenseObligationType,
                                                  IdFinanceSource = kbk.kbk.IdFinanceSource,
                                                  IdKCSR = kbk.kbk.IdKCSR,
                                                  IdKOSGU = kbk.kbk.IdKOSGU,
                                                  IdKFO = kbk.kbk.IdKFO,
                                                  IdKVR = kbk.kbk.IdKVR,
                                                  IdKVSR = kbk.kbk.IdKVSR,
                                                  IdRZPR = kbk.kbk.IdRZPR,
                                                  OFG = kbk.OFG,
                                                  PFG1 = kbk.PFG1,
                                                  PFG2 = kbk.PFG2,
                                                  AdditionalOFG = kbk.AdditionalOFG,
                                                  AdditionalPFG1 = kbk.AdditionalPFG1,
                                                  AdditionalPFG2 = kbk.AdditionalPFG2
                                              }).ToList();

            context.PublicInstitutionEstimate_FounderAUBUExpense.RemoveAll(context.PublicInstitutionEstimate_FounderAUBUExpense.Where(r => r.IdOwner == this.Id));

            context.PublicInstitutionEstimate_FounderAUBUExpense.InsertAsTableValue(gkbktrim_FounderExpense, context);


            var gkbktrim_AloneAndBudgetInstitutionExpense =
                (from kbk in context.PublicInstitutionEstimate_AloneAndBudgetInstitutionExpense
                     .Where(r => r.IdOwner == this.Id)
                     .ToList()
                     .Select(s =>
                                            new
                                                {
                                                    s.IdOwner,
                                                    s.IdMaster,
                                                    IdBranchCode = SBP_BlankHelper.IsBvtEmpty(SBP_BlankActualAuBu.IdBlankValueType_BranchCode) ? null : s.IdBranchCode,
                                                    IdCodeSubsidy = SBP_BlankHelper.IsBvtEmpty(SBP_BlankActualAuBu.IdBlankValueType_CodeSubsidy) ? null : s.IdCodeSubsidy,
                                                    IdDEK = SBP_BlankHelper.IsBvtEmpty(SBP_BlankActualAuBu.IdBlankValueType_DEK) ? null : s.IdDEK,
                                                    IdDFK = SBP_BlankHelper.IsBvtEmpty(SBP_BlankActualAuBu.IdBlankValueType_DFK) ? null : s.IdDFK,
                                                    IdDKR = SBP_BlankHelper.IsBvtEmpty(SBP_BlankActualAuBu.IdBlankValueType_DKR) ? null : s.IdDKR,
                                                    IdExpenseObligationType = SBP_BlankHelper.IsBvtEmpty(SBP_BlankActualAuBu.IdBlankValueType_ExpenseObligationType) ? null : s.IdExpenseObligationType,
                                                    IdFinanceSource = SBP_BlankHelper.IsBvtEmpty(SBP_BlankActualAuBu.IdBlankValueType_FinanceSource) ? null : s.IdFinanceSource,
                                                    IdKCSR = SBP_BlankHelper.IsBvtEmpty(SBP_BlankActualAuBu.IdBlankValueType_KCSR) ? null : s.IdKCSR,
                                                    IdKOSGU = SBP_BlankHelper.IsBvtEmpty(SBP_BlankActualAuBu.IdBlankValueType_KOSGU) ? null : s.IdKOSGU,
                                                    IdKFO = SBP_BlankHelper.IsBvtEmpty(SBP_BlankActualAuBu.IdBlankValueType_KFO) ? null : s.IdKFO,
                                                    IdKVR = SBP_BlankHelper.IsBvtEmpty(SBP_BlankActualAuBu.IdBlankValueType_KVR) ? null : s.IdKVR,
                                                    IdKVSR = SBP_BlankHelper.IsBvtEmpty(SBP_BlankActualAuBu.IdBlankValueType_KVSR) ? null : s.IdKVSR,
                                                    IdRZPR = SBP_BlankHelper.IsBvtEmpty(SBP_BlankActualAuBu.IdBlankValueType_RZPR) ? null : s.IdRZPR,
                                                    s.OFG,
                                                    s.PFG1,
                                                    s.PFG2,
                                                    s.AdditionalOFG,
                                                    s.AdditionalPFG1,
                                                    s.AdditionalPFG2
                                                })
                 select new {kbk}).
                    GroupBy(g =>
                            new
                                {
                                    g.kbk.IdOwner,
                                    g.kbk.IdMaster,
                                    g.kbk.IdBranchCode,
                                    g.kbk.IdCodeSubsidy,
                                    g.kbk.IdDEK,
                                    g.kbk.IdDFK,
                                    g.kbk.IdDKR,
                                    g.kbk.IdExpenseObligationType,
                                    g.kbk.IdFinanceSource,
                                    g.kbk.IdKCSR,
                                    g.kbk.IdKOSGU,
                                    g.kbk.IdKFO,
                                    g.kbk.IdKVR,
                                    g.kbk.IdKVSR,
                                    g.kbk.IdRZPR,
                                }).
                    Select(s =>
                           new
                               {
                                   kbk = s.Key,
                                   OFG = s.Sum(ss => ss.kbk.OFG),
                                   PFG1 = s.Sum(ss => ss.kbk.PFG1),
                                   PFG2 = s.Sum(ss => ss.kbk.PFG2),
                                   AdditionalOFG = s.Sum(ss => ss.kbk.AdditionalOFG),
                                   AdditionalPFG1 = s.Sum(ss => ss.kbk.AdditionalPFG1),
                                   AdditionalPFG2 = s.Sum(ss => ss.kbk.AdditionalPFG2)
                               }).
                    Select(kbk =>
                           new PublicInstitutionEstimate_AloneAndBudgetInstitutionExpense()
                               {
                                   IdOwner = this.Id,
                                   IdMaster = kbk.kbk.IdMaster,
                                   IdBranchCode = kbk.kbk.IdBranchCode,
                                   IdCodeSubsidy = kbk.kbk.IdCodeSubsidy,
                                   IdDEK = kbk.kbk.IdDEK,
                                   IdDFK = kbk.kbk.IdDFK,
                                   IdDKR = kbk.kbk.IdDKR,
                                   IdExpenseObligationType = kbk.kbk.IdExpenseObligationType,
                                   IdFinanceSource = kbk.kbk.IdFinanceSource,
                                   IdKCSR = kbk.kbk.IdKCSR,
                                   IdKOSGU = kbk.kbk.IdKOSGU,
                                   IdKFO = kbk.kbk.IdKFO,
                                   IdKVR = kbk.kbk.IdKVR,
                                   IdKVSR = kbk.kbk.IdKVSR,
                                   IdRZPR = kbk.kbk.IdRZPR,
                                   OFG = kbk.OFG,
                                   PFG1 = kbk.PFG1,
                                   PFG2 = kbk.PFG2,
                                   AdditionalOFG = kbk.AdditionalOFG,
                                   AdditionalPFG1 = kbk.AdditionalPFG1,
                                   AdditionalPFG2 = kbk.AdditionalPFG2
                               }).
                    ToList();

            context.PublicInstitutionEstimate_AloneAndBudgetInstitutionExpense.RemoveAll(context.PublicInstitutionEstimate_AloneAndBudgetInstitutionExpense.Where(r => r.IdOwner == this.Id));

            context.PublicInstitutionEstimate_AloneAndBudgetInstitutionExpense.InsertAsTableValue(gkbktrim_AloneAndBudgetInstitutionExpense, context);

            context.SaveChanges();
        }


        /// <summary>
        /// Очистить доп. расходы
        /// </summary>
        /// <param name="context"></param>
        public void ClearAdditionalExpenses(DataContext context)
        {
            foreach (var expense in Expenses.ToList())
            {
                expense.AdditionalOFG = null;
                expense.AdditionalPFG1 = null;
                expense.AdditionalPFG2 = null;
            }

            foreach (var expenseAuBu in FounderAUBUExpenses.ToList())
            {
                expenseAuBu.AdditionalOFG = null;
                expenseAuBu.AdditionalPFG1 = null;
                expenseAuBu.AdditionalPFG2 = null;
            }
        }

        /// <summary>
        /// Добавляем доп. расходы к обычным расходам
        /// </summary>
        /// <param name="context"></param>
        public void ReWriteAdditionalsExpenses(DataContext context)
        {
            foreach (var expense in Expenses.ToList())
            {
                expense.OFG += expense.AdditionalOFG;
                expense.AdditionalPFG1 += expense.AdditionalPFG1;
                expense.AdditionalPFG2 += expense.AdditionalPFG2;

                expense.AdditionalOFG = null;
                expense.AdditionalPFG1 = null;
                expense.AdditionalPFG2 = null;
            }

            foreach (var expenseAuBu in FounderAUBUExpenses.ToList())
            {
                expenseAuBu.OFG += expenseAuBu.AdditionalOFG;
                expenseAuBu.AdditionalPFG1 += expenseAuBu.AdditionalPFG1;
                expenseAuBu.AdditionalPFG2 += expenseAuBu.AdditionalPFG2;

                expenseAuBu.AdditionalOFG = null;
                expenseAuBu.AdditionalPFG1 = null;
                expenseAuBu.AdditionalPFG2 = null;
            }
        }

        private void WriteToRegistry(DataContext context)
        {
            WriteToRegistryExpenses(context);
            WriteToRegistryFounderExpenses(context);
        }

        private void WriteToRegistryExpenses(DataContext context)
        {
            var findParamEstimatedLine = new FindParamEstimatedLine
            {
                IdBudget = IdBudget,
                IdPublicLegalFormation = IdPublicLegalFormation,
                IdSbp = IdSBP,
                IsCreate = true,
                IsKosgu000 = false,
                IsRequired = false,
                TypeLine = ActivityBudgetaryType.Costs
            };
            var blank = context.SBP_Blank.FirstOrDefault(b => b.IdOwner == SBP.IdParent && b.IdBudget == IdBudget && b.IdBlankType == (byte)BlankType.FormationKU);

            var documentAllocations = new List<RegistryAllocations>();

            var year = Budget.Year;
          
            var estimatedLines = new Dictionary<int, int>();
            var taskCollections = new Dictionary<int, int>();

            foreach (var expense in Expenses.ToList())
            {
                estimatedLines[expense.Id] = expense.GetLineId(context, Id, EntityId, blank, findParamEstimatedLine).Value;
                taskCollections[expense.Id] = context.TaskCollection.Where(t => t.IdActivity == expense.Master.IdActivity &&
                                                                       t.IdContingent == expense.Master.IdContingent)
                                                             .Select(t => t.Id).FirstOrDefault();
            }

            foreach (var expense in Expenses.ToList())
            {
                var estimatedLineId = estimatedLines[expense.Id];
                var idTaskCollection = taskCollections[expense.Id];

                var ofgYearId = year.GetIdHierarchyPeriodYear(context);
                var pfg1YearId = (year + 1).GetIdHierarchyPeriodYear(context);
                var pfg2YearId = (year + 2).GetIdHierarchyPeriodYear(context);

                var proto = new RegistryAllocations
                {
                    IdEstimatedLine = estimatedLineId,
                    IsIndirectCosts = expense.IsIndirectCosts.HasValue && expense.IsIndirectCosts.Value,
                    IdAuthorityOfExpenseObligation = expense.IdAuthorityOfExpenseObligation,
                    IdTaskCollection = idTaskCollection,
                    IdOKATO = expense.IdOKATO
                };

                documentAllocations.Add(proto.Clone(ofgYearId, expense.OFG, false));
                documentAllocations.Add(proto.Clone(pfg1YearId, expense.PFG1, false));
                documentAllocations.Add(proto.Clone(pfg2YearId, expense.PFG2, false));

                documentAllocations.Add(proto.Clone(ofgYearId, expense.AdditionalOFG, true));
                documentAllocations.Add(proto.Clone(pfg1YearId, expense.AdditionalPFG1, true));
                documentAllocations.Add(proto.Clone(pfg2YearId, expense.AdditionalPFG2, true));
            }


            //Если нужно посчитаем сторнирующие проводки
            if (IdParent.HasValue)
            {
                var prevVersions = PrevVersionIds;
                documentAllocations = documentAllocations.Union(context.LimitVolumeAppropriations.Where(l =>
                                                                                                        prevVersions.Contains(l.IdRegistrator) &&
                                                                                                        l.IdRegistratorEntity == EntityId &&
                                                                                                        l.IdValueType == (byte)ValueType.Justified &&
                                                                                                        !l.IsMeansAUBU &&
                                                                                                        l.EstimatedLine.IdSBP == IdSBP)
                                                                       .Select(l => new RegistryAllocations
                                                                       {
                                                                           IdEstimatedLine = l.IdEstimatedLine,
                                                                           IdHierarchyPeriod = l.IdHierarchyPeriod,
                                                                           Value = -l.Value,
                                                                           IsIndirectCosts = l.IsIndirectCosts,
                                                                           IdAuthorityOfExpenseObligation = l.IdAuthorityOfExpenseObligation,
                                                                           HasAdditionalNeed = l.HasAdditionalNeed,
                                                                           IdTaskCollection = l.IdTaskCollection,
                                                                           IdOKATO = l.IdOKATO
                                                                       }).ToList()).ToList();
            }


	        List<LimitVolumeAppropriations> items = new List<LimitVolumeAppropriations>();
			foreach (var documentAllocation in documentAllocations.GroupBy(l => new
            {
                idEstimatedLine = l.IdEstimatedLine,
                idHierarchyPeriod = l.IdHierarchyPeriod,
                isIndirectCosts = l.IsIndirectCosts,
                idAuthorityOfExpenseObligation = l.IdAuthorityOfExpenseObligation,
                hasAdditionalNeed = l.HasAdditionalNeed,
                idTaskCollection = l.IdTaskCollection,
                idOKATO = l.IdOKATO
            })
                                                                   .Select(g => new
                                                                   {
                                                                       g.Key.idEstimatedLine,
                                                                       g.Key.idHierarchyPeriod,
                                                                       g.Key.isIndirectCosts,
                                                                       g.Key.idAuthorityOfExpenseObligation,
                                                                       g.Key.hasAdditionalNeed,
                                                                       g.Key.idTaskCollection,
                                                                       g.Key.idOKATO,
                                                                       value = g.Sum(c => c.Value)
                                                                   })
                                                                   .ToList())
            {
                if (documentAllocation.value.HasValue && documentAllocation.value != 0)
                {
                    items.Add(new LimitVolumeAppropriations
                    {
                        IdPublicLegalFormation = IdPublicLegalFormation,
                        IdVersion = IdVersion,
                        IdBudget = IdBudget,
                        IdEstimatedLine = documentAllocation.idEstimatedLine.Value,
                        IdAuthorityOfExpenseObligation = documentAllocation.idAuthorityOfExpenseObligation,
                        IdTaskCollection = documentAllocation.idTaskCollection,
                        IsIndirectCosts = documentAllocation.isIndirectCosts,
                        IdHierarchyPeriod = documentAllocation.idHierarchyPeriod,
                        IdValueType = (byte)ValueType.Justified,
                        Value = documentAllocation.value.Value,
                        IdOKATO = documentAllocation.idOKATO,
                        IsMeansAUBU = false,
                        IdRegistrator = Id,
                        IdRegistratorEntity = EntityId,
                        DateCommit = null,
                        DateCreate = DateTime.Now,
                        IdApproved = null,
                        IdApprovedEntity = null,
                        HasAdditionalNeed = documentAllocation.hasAdditionalNeed
                    });
                }

            }
	        context.LimitVolumeAppropriations.InsertAsTableValue(items, context);

            context.SaveChanges();
        }

        private void WriteToRegistryFounderExpenses(DataContext context)
        {
            var findParamEstimatedLine = new FindParamEstimatedLine
            {
                IdBudget = IdBudget,
                IdPublicLegalFormation = IdPublicLegalFormation,
                IdSbp = IdSBP,
                IsCreate = true,
                IsKosgu000 = false,
                IsRequired = false,
                TypeLine = ActivityBudgetaryType.Costs
            };
            var blank = context.SBP_Blank.FirstOrDefault(b => b.IdOwner == SBP.IdParent && b.IdBudget == IdBudget && b.IdBlankType == (byte)BlankType.FormationKU);

            var documentAllocations = new List<RegistryAllocations>();

            var year = Budget.Year;
           
            var estimatedLines = new Dictionary<int, int>();
            var taskCollections = new Dictionary<int, int>();

            foreach (var expense in FounderAUBUExpenses.ToList())
            {
                estimatedLines[expense.Id] = expense.GetLineId(context, Id, EntityId, blank, findParamEstimatedLine).Value;
                taskCollections[expense.Id] = context.TaskCollection.Where(t => t.IdActivity == expense.Master.IdActivity &&
                                                                       t.IdContingent == expense.Master.IdContingent)
                                                             .Select(t => t.Id).FirstOrDefault();
            }

            foreach (var expense in FounderAUBUExpenses.ToList())
            {
                var estimatedLineId = estimatedLines[expense.Id];
                var idTaskCollection = taskCollections[expense.Id];

                var ofgYearId = year.GetIdHierarchyPeriodYear(context);
                var pfg1YearId = (year + 1).GetIdHierarchyPeriodYear(context);
                var pfg2YearId = (year + 2).GetIdHierarchyPeriodYear(context);

                var proto = new RegistryAllocations
                {
                    IdEstimatedLine = estimatedLineId,
                    IsIndirectCosts = false,
                    IdAuthorityOfExpenseObligation = expense.IdAuthorityOfExpenseObligation,
                    IdTaskCollection = idTaskCollection,
                    IdOKATO = null
                };

                documentAllocations.Add(proto.Clone(ofgYearId, expense.OFG, false));
                documentAllocations.Add(proto.Clone(pfg1YearId, expense.PFG1, false));
                documentAllocations.Add(proto.Clone(pfg2YearId, expense.PFG2, false));

                documentAllocations.Add(proto.Clone(ofgYearId, expense.AdditionalOFG, true));
                documentAllocations.Add(proto.Clone(pfg1YearId, expense.AdditionalPFG1, true));
                documentAllocations.Add(proto.Clone(pfg2YearId, expense.AdditionalPFG2, true));
            }


            //Если нужно посчитаем сторнирующие проводки
            if (IdParent.HasValue)
            {
                var prevVersions = PrevVersionIds;
            
                documentAllocations = documentAllocations.Union(context.LimitVolumeAppropriations.Where(l =>
                                                                                                        prevVersions.Contains(l.IdRegistrator) &&
                                                                                                        l.IdRegistratorEntity == EntityId &&
                                                                                                        l.IdValueType == (byte)ValueType.Justified &&
                                                                                                        l.IsMeansAUBU &&
                                                                                                        l.EstimatedLine.IdSBP == IdSBP)
                                                                       .Select(l => new RegistryAllocations
                                                                       {
                                                                           IdEstimatedLine = l.IdEstimatedLine,
                                                                           IdHierarchyPeriod = l.IdHierarchyPeriod,
                                                                           Value = -l.Value,
                                                                           IsIndirectCosts = l.IsIndirectCosts,
                                                                           IdAuthorityOfExpenseObligation = l.IdAuthorityOfExpenseObligation,
                                                                           HasAdditionalNeed = l.HasAdditionalNeed,
                                                                           IdTaskCollection = l.IdTaskCollection,
                                                                           IdOKATO = l.IdOKATO
                                                                       }).ToList()).ToList();
            }

	        List<LimitVolumeAppropriations> items = new List<LimitVolumeAppropriations>();
			foreach (var documentAllocation in documentAllocations.GroupBy(l => new
            {
                idEstimatedLine = l.IdEstimatedLine,
                idHierarchyPeriod = l.IdHierarchyPeriod,
                isIndirectCosts = l.IsIndirectCosts,
                idAuthorityOfExpenseObligation = l.IdAuthorityOfExpenseObligation,
                hasAdditionalNeed = l.HasAdditionalNeed,
                idTaskCollection = l.IdTaskCollection,
                idOKATO = l.IdOKATO
            })
                                                                   .Select(g => new
                                                                   {
                                                                       g.Key.idEstimatedLine,
                                                                       g.Key.idHierarchyPeriod,
                                                                       g.Key.isIndirectCosts,
                                                                       g.Key.idAuthorityOfExpenseObligation,
                                                                       g.Key.hasAdditionalNeed,
                                                                       g.Key.idTaskCollection,
                                                                       g.Key.idOKATO,
                                                                       value = g.Sum(c => c.Value)
                                                                   })
                                                                    .ToList())
            {
                if (documentAllocation.value.HasValue && documentAllocation.value != 0)
                {
                    items.Add(new LimitVolumeAppropriations
                    {
                        IdPublicLegalFormation = IdPublicLegalFormation,
                        IdVersion = IdVersion,
                        IdBudget = IdBudget,
                        IdEstimatedLine = documentAllocation.idEstimatedLine.Value,
                        IdAuthorityOfExpenseObligation = documentAllocation.idAuthorityOfExpenseObligation,
                        IdTaskCollection = documentAllocation.idTaskCollection,
                        IsIndirectCosts = documentAllocation.isIndirectCosts,
                        IdHierarchyPeriod = documentAllocation.idHierarchyPeriod,
                        IdValueType = (byte)ValueType.Justified,
                        Value = documentAllocation.value.Value,
                        IdOKATO = documentAllocation.idOKATO,
                        IsMeansAUBU = true,
                        IdRegistrator = Id,
                        IdRegistratorEntity = EntityId,
                        DateCommit = null,
                        DateCreate = DateTime.Now,
                        IdApproved = null,
                        IdApprovedEntity = null,
                        HasAdditionalNeed = documentAllocation.hasAdditionalNeed
                    });
                }

            }
			context.LimitVolumeAppropriations.InsertAsTableValue(items, context);
            context.SaveChanges();
        }

        private IEnumerable<PublicInstitutionEstimate_Activity> GetActivitiesFromRegister(DataContext context)
        {
            var budgetYear = Budget.Year;

            var hierarchyPeriodIds = new[] { budgetYear, budgetYear + 1, budgetYear + 2 };
            hierarchyPeriodIds = hierarchyPeriodIds.Select(y => y.GetIdHierarchyPeriodYear(context)).ToArray();

            return context.TaskVolume.Where(tv => tv.IdPublicLegalFormation == IdPublicLegalFormation
                                                                    && tv.IdBudget == IdBudget
                                                                    && tv.IdVersion == IdVersion
                                                                    && tv.IdSBP == IdSBP
                                                                    && hierarchyPeriodIds.Contains(tv.IdHierarchyPeriod)
                                                                    && tv.IdValueType == (int)ValueType.Plan
                                                                    && (!tv.ActivityAUBU.HasValue || !tv.ActivityAUBU.Value)
                                                                    && !tv.IdTerminator.HasValue)
                                            .Select(tv => new { tv.TaskCollection.IdActivity, tv.TaskCollection.Activity.IdActivityType, tv.TaskCollection.IdContingent, IdIndicatorActivity = tv.IdIndicatorActivity_Volume, tv.IndicatorActivity_Volume.IdUnitDimension })
                                            .Distinct().ToList()
                                            .Select(tv => new PublicInstitutionEstimate_Activity
                                            {
                                                IdActivity = tv.IdActivity,
                                                IdContingent = tv.IdContingent,
                                                IdIndicatorActivity = tv.IdIndicatorActivity,
                                                IdUnitDimension = tv.IdUnitDimension,
                                            }).ToList();
        }

        private IEnumerable<PublicInstitutionEstimate_ActivityAUBU> GetActivitiesAUBUFromRegister(DataContext context)
        {
            var budgetYear = Budget.Year;

            var hierarchyPeriodIds = new[] { budgetYear, budgetYear + 1, budgetYear + 2 };
            hierarchyPeriodIds = hierarchyPeriodIds.Select(y => y.GetIdHierarchyPeriodYear(context)).ToArray();

            return context.TaskVolume.Where(tv => tv.IdPublicLegalFormation == IdPublicLegalFormation
                                                                    && tv.IdBudget == IdBudget
                                                                    && tv.IdVersion == IdVersion
                                                                    && tv.IdSBP == IdSBP
                                                                    && hierarchyPeriodIds.Contains(tv.IdHierarchyPeriod)
                                                                    && tv.IdValueType == (int)ValueType.Plan
                                                                    && (tv.ActivityAUBU.HasValue && tv.ActivityAUBU.Value)
                                                                    && !tv.IdTerminator.HasValue)
                                            .Select(tv => new { tv.TaskCollection.IdActivity, tv.TaskCollection.IdContingent, IdIndicatorActivity = tv.IdIndicatorActivity_Volume, tv.IndicatorActivity_Volume.IdUnitDimension })
                                            .Distinct().ToList()
                                            .Select(tv => new PublicInstitutionEstimate_ActivityAUBU { IdActivity = tv.IdActivity, IdContingent = tv.IdContingent, IdIndicatorActivity = tv.IdIndicatorActivity, IdUnitDimension = tv.IdUnitDimension }).ToList();
        }

        private IEnumerable<ActivityValues> GetExpensesForAloneSubjects(DataContext context)
        {
            var mandatoryKBK = SBP_BlankHelper.GetBlanksCostMandatoryProperties(new List<SBP_Blank> { BlankFormationAUBU, BlankFormationKU }).ToList();

            var kbkString = mandatoryKBK.GetQueryString("EL");
            var sbpIds = context.SBP.Where(s => s.IdParent == IdSBP).Select(s => s.Id).ToList().GetQueryString();
            if (String.IsNullOrEmpty(sbpIds))
                sbpIds = "NULL";

            var query = String.Format("Select {0}, " +
                                        "Sum( CASE WHEN L.HasAdditionalNeed = 0 THEN L.Value ELSE 0 END ) as regValue, " +
                                        "Sum(CASE WHEN L.HasAdditionalNeed = 1 THEN L.Value ELSE 0 END) as additionalRegValue, " +
                                        "TC.idActivity, " +
                                        "TC.idContingent," +
                                        "L.idHierarchyPeriod " +
                                      "From reg.LimitVolumeAppropriations L " +
                                      "Inner join reg.EstimatedLine EL on (EL.id = L.idEstimatedLine) " +
                                      "Inner join ref.KFO K on (K.id = EL.idKFO) " +
                                      "Inner join reg.TaskCollection TC on (TC.id = L.idTaskCollection) " +
                                      "Inner join ref.FinanceSource FS on (FS.id = EL.idFinanceSource) " +
                                      "Where " +
                                        "L.idPublicLegalFormation = {1} AND " +
                                        "L.idVersion = {2} AND " +
                                        "L.idBudget = {6} AND " +
                                        "K.IsIncludedInBudget = 1 AND " +
                                        "EL.idSBP IN ({3}) AND " +
                                        "L.idValueType = {4} AND " +
                                        "FS.idFinanceSourceType <> {5} " +
                                      "Group by L.idHierarchyPeriod, {0}, TC.idActivity, TC.idContingent",
                                            kbkString,
                                            IdPublicLegalFormation,
                                            IdVersion,
                                            sbpIds,
                                            (int)ValueType.JustifiedFBA,
                                            (byte)FinanceSourceType.Remains,
                                            IdBudget);

            return context.Database.SqlQuery<ActivityValues>(query).ToList();
        }

        /// <summary>
        /// Заполнить ТЧ Мероприятие (вкладка Расходы по видам деятельности)
        /// </summary>
        /// <param name="context"></param>
        private void FillActivityOnCreate(DataContext context)
        {
            var reg = GetActivitiesFromRegister(context);

            foreach (var r in reg)
            {
                var activity = r;
                r.IdOwner = Id;
                context.PublicInstitutionEstimate_Activity.Add(activity);
            }
        }

        /// <summary>
        /// Заполнить ТЧ «Мероприятие АУ/БУ» (вкладка «Расходы по мероприятиям АУ/БУ)
        /// </summary>
        /// <param name="context"></param>
        private void FillActivityAUBUOnCreate(DataContext context)
        {
            var reg = GetActivitiesAUBUFromRegister(context);

            foreach (var r in reg)
            {
                var activity = new PublicInstitutionEstimate_ActivityAUBU
                {
                    IdActivity = r.IdActivity,
                    IdContingent = r.IdContingent,
                    IdIndicatorActivity = r.IdIndicatorActivity,
                    IdUnitDimension = r.IdUnitDimension,
                    IdOwner = Id
                };
                context.PublicInstitutionEstimate_ActivityAUBU.Add(activity);
            }
            context.SaveChanges();
        }

        /// <summary>
        /// Заполнить ТЧ «Расходы автономных и бюджетных учреждений
        /// </summary>
        /// <param name="context"></param>
        private void FillAloneExpenses(DataContext context)
        {
            var regValues = GetExpensesForAloneSubjects(context);

            var year = Budget.Year;
            var docTaskCollections = ActivitiesAUBU.Select(a => new { a.IdActivity, a.IdContingent, a.Id }).ToList();
            foreach (var value in regValues.Where(r => docTaskCollections.Any(c => c.IdActivity == r.IdActivity && c.IdContingent == r.IdContingent)))
            {
                var masterActivity = docTaskCollections.Where(c => c.IdActivity == value.IdActivity && c.IdContingent == value.IdContingent)
                                                                                 .Select(c => c.Id)
                                                                                 .FirstOrDefault();
                var expense = AloneAndBudgetInstitutionExpenses.FirstOrDefault(e =>
                    e.IdMaster == masterActivity &&
                    e.IdOwner == Id) ?? new PublicInstitutionEstimate_AloneAndBudgetInstitutionExpense(value)
                {
                    IdMaster = masterActivity,
                    IdOwner = Id
                };

                expense.OFG = year.GetIdHierarchyPeriodYear(context) == value.IdHierarchyPeriod ? value.RegValue : expense.OFG;
                expense.PFG1 = (year + 1).GetIdHierarchyPeriodYear(context) == value.IdHierarchyPeriod ? value.RegValue : expense.PFG1;
                expense.PFG2 = (year + 2).GetIdHierarchyPeriodYear(context) == value.IdHierarchyPeriod ? value.RegValue : expense.PFG2;
                expense.AdditionalOFG = year.GetIdHierarchyPeriodYear(context) == value.IdHierarchyPeriod ? value.AdditionalRegValue : expense.AdditionalOFG;
                expense.AdditionalPFG1 = (year + 1).GetIdHierarchyPeriodYear(context) == value.IdHierarchyPeriod ? value.AdditionalRegValue : expense.AdditionalPFG1;
                expense.AdditionalPFG2 = (year + 2).GetIdHierarchyPeriodYear(context) == value.IdHierarchyPeriod ? value.AdditionalRegValue : expense.AdditionalPFG2;

                AloneAndBudgetInstitutionExpenses.Add(expense);
            }

            context.SaveChanges();
        }

        #endregion

        /// <summary>   
        /// Операция «Создать»   
        /// </summary>  
        public void Create(DataContext context)
        {
            ExecuteControl(e => e.Control_0701(context));
            ExecuteControl(e => e.Control_0702(context));

            SetBlankActual(context);

            FillActivityOnCreate(context);

            if (SBP.IsFounder)
            {
                FillActivityAUBUOnCreate(context);
                FillAloneExpenses(context);
                ExecuteControl(e => e.Control_0715(context, true));
            }

            var error = true;
            do
            {
                try
                {
                    var sc = context.PublicInstitutionEstimate
                        .Where(w => w.IdPublicLegalFormation == IdPublicLegalFormation && !w.IdParent.HasValue)
                        .Select(s => s.Number).Distinct().ToList();
                    Number = sc.GetNextCode();

                    context.SaveChanges();
                    error = false;
                }
                catch (Exception e)
                {
                    if (!e.Message.Contains("Номер"))
                    {
                        throw;
                    }
                }
            } while (error);

        }

        /// <summary>   
        /// Операция «Редактировать»   
        /// </summary>  
        public void BeforeEdit(DataContext context)
        {
            ExecuteControl(e => e.Control_0732(context));

            if (SetBlankActual(context))
            {
                TrimKbkByNewActualBlank(context);
                context.SaveChanges();
            }
        }
        public void Edit(DataContext context)
        {
            ExecuteControl(e => e.Control_0703(context));
            //ExecuteControl(e => e.Control_0710(context, null));
            //ExecuteControl(e => e.Control_0716(context, TODO));
            ExecuteControl(e => e.Control_0711(context));
            ExecuteControl(e => e.Control_0712(context));
            //ExecuteControl(e => e.Control_0727(context, null));
            ExecuteControl(e => e.Control_0704(context));
            ExecuteControl(e => e.Control_0705(context));
            ExecuteControl(e => e.Control_0723(context));

            DateLastEdit = DateTime.Now;
        }

        /// <summary>   
        /// Операция «Прекратить»
        /// </summary>  
        public void Terminate(DataContext context)
        {
            ExecuteControl<Control_7001>();
        }

        /// <summary>   
        /// Операция «Отменить прекращение»   
        /// </summary>  
        public void UndoTerminate(DataContext context)
        {
            ReasonTerminate = null;
            DateTerminate = null;
        }

        /// <summary>
        /// Операция «Обработать»
        /// </summary>
        /// <param name="context"></param>
        public void Process(DataContext context)
        {
            //ok
            ExecuteControl(e => e.Control_0708(context));
            //ok
            ExecuteControl(e => e.Control_0713(context));
            //so-so
            ExecuteControl(e => e.Control_0706(context));
            //so-so
            ExecuteControl(e => e.Control_0707(context));
            //?
            ExecuteControl(e => e.Control_0727(context, null));
            //bad
            ExecuteControl(e => e.Control_0718(context));
            //bad
            ExecuteControl(e => e.Control_0709(context));
            ExecuteControl(e => e.Control_0710(context, null));
            ExecuteControl(e => e.Control_0716(context, null));
            ExecuteControl(e => e.Control_0714(context));
            ExecuteControl(e => e.Control_0717(context));
            ExecuteControl(e => e.Control_0730(context));
            ExecuteControl(e => e.Control_0731(context));
            ExecuteControl(e => e.Control_0729(context));
            ExecuteControl(e => e.Control_0737(context));

            ReasonClarification = null;
            IsRequireClarification = false;

            if (IdParent.HasValue)
            {
                var prevDoc = (PublicInstitutionEstimate)CommonMethods.GetPrevVersionDoc(context, this, EntityId);
                if (prevDoc != null)
                {
                    prevDoc.ExecuteOperation(e => e.Archive(context));
                }
            }

            WriteToRegistry(context);
        }

        /// <summary>
        /// Операция «Отменить обработку»
        /// </summary>  
        public void UndoProcess(DataContext context)
        {
            RegisterMethods.RemoveFromRegistersByRegistrator(context, Id, EntityId, _arrRegisters);
            using (new ControlScope())
            {
                context.SaveChanges();
            }

            ReasonClarification = null;
            IsRequireClarification = false;

            var prevDoc = (PublicInstitutionEstimate)CommonMethods.GetPrevVersionDoc(context, this, EntityId);
            if (prevDoc != null)
            {
                prevDoc.ExecuteOperation(e => e.UndoArchive(context));
            }
        }

        /// <summary>
        /// Отменить принятие изменений (скрытая)
        /// </summary>
        /// <param name="context"></param>
        public void UndoArchive(DataContext context)
        {
            IdDocStatus = DocStatus.Changed;
        }

        /// <summary>
        /// Операция «Отказать»
        /// </summary>  
        public void Deny(DataContext context)
        {
            if (string.IsNullOrEmpty(ReasonCancel))
            {
                Controls.Throw("Не заполнено поле 'Причина отказа'");
            }
        }


        /// <summary>   
        /// Операция «Вернуть в проект»   
        /// </summary>  
        public void ReturnToProject(DataContext context)
        {
            IdDocStatus = DocStatus.Project;
            ReasonCancel = String.Empty;
            
            context.SaveChanges();
        }

        /// <summary>
        /// Вернуть на черновик 
        /// </summary>
        /// <param name="context"></param>
        public void BackToDraft(DataContext context)
        {
            UndoProcess(context);
        }

        /// <summary>   
        /// Операция «Утвердить»   
        /// </summary>  
        public void Confirm(DataContext context)
        {
            ExecuteControl(e => e.Control_0724(context));
            ExecuteControl(e => e.Control_0719(context));
            ExecuteControl(e => e.Control_0720(context));
            ExecuteControl(e => e.Control_0721(context));
            ExecuteControl(e => e.Control_0722(context));

            DateCommit = DateTime.Now.Date;

            #region Есть доп. потребности
            if (HasAdditionalNeed.HasValue && HasAdditionalNeed.Value)
            {
                var clone = new Clone(this);
                var newDoc = (PublicInstitutionEstimate)clone.GetResult();

                IdDocStatus = DocStatus.Archive;

                newDoc.HasAdditionalNeed = false;
                newDoc.IdParent = Id;
                newDoc.IsApproved = true;

                var sc = context.PublicInstitutionEstimate
                        .Where(w => w.IdPublicLegalFormation == IdPublicLegalFormation && !w.IdParent.HasValue)
                        .Select(s => s.Number).Distinct().ToList();
                newDoc.Number = sc.GetNextCode();

                newDoc.ClearAdditionalExpenses(context);
                newDoc.IdDocStatus = DocStatus.Approved;
                context.Entry(newDoc).State = EntityState.Added;

                context.SaveChanges();

                newDoc.Caption = newDoc.ToString();
                context.SaveChanges();

                IsApproved = true;
                IdDocStatus = DocStatus.Approved;

                ApproveInReg(context, new[] { newDoc.Id, Id }, this);

                foreach (var reg in context.LimitVolumeAppropriations.Where(l => l.IdRegistratorEntity == EntityId &&
                                                                                 l.HasAdditionalNeed.HasValue && l.HasAdditionalNeed.Value &&
                                                                                 !l.DateCommit.HasValue &&
                                                                                 new[] { newDoc.Id, Id }.Contains(l.IdRegistrator)))
                {
                    var sreg = reg.Clone();
                    sreg.Value = -sreg.Value;
                    sreg.IdRegistrator = newDoc.Id;
                    sreg.IdApproved = newDoc.Id;
                    sreg.IdApprovedEntity = newDoc.Id;
                    sreg.DateCommit = newDoc.Date;

                    reg.IdApproved = newDoc.Id;
                    reg.IdApprovedEntity = newDoc.Id;
                    reg.DateCommit = newDoc.Date;

                    context.LimitVolumeAppropriations.Add(sreg);
                }

            }
            #endregion
            //Вести доп. потребности ложь
            else
            {
                IsApproved = true;
                IdDocStatus = DocStatus.Approved;

                ApproveInReg(context, IdParent.HasValue ? new[] { Id, IdParent.Value } : new[] { Id }, this);
            }
        }

        /// <summary>
        /// Операция «Утвердить с доп. потребностями»   
        /// </summary>
        /// <param name="context"></param>
        public void ConfirmWithAddNeed(DataContext context)
        {
            ExecuteControl(e => e.Control_0725(context));
            ExecuteControl(e => e.Control_0726(context));
            ExecuteControl(e => e.Control_0719(context));
            ExecuteControl(e => e.Control_0720(context));
            ExecuteControl(e => e.Control_0721(context));
            ExecuteControl(e => e.Control_0722(context));


            IdDocStatus = DocStatus.Archive;

            var cloner = new Clone(this);
            var newDoc = (PublicInstitutionEstimate)cloner.GetResult();

            IdDocStatus = DocStatus.Archive;

            newDoc.HasAdditionalNeed = false;
            newDoc.IdParent = Id;
            newDoc.IsApproved = true;

            newDoc.IdDocStatus = DocStatus.Approved;
            newDoc.Number = this.GetDocNextNumber(context);

            context.Entry(newDoc).State = EntityState.Added;
            context.SaveChanges();

            newDoc.Caption = newDoc.ToString();
            //суммировать поля «Доп. Потребность…» в ТЧ «Расходы», «Расходы учредителя по мероприятиям АУ/БУ» 
            //с полями «Сумма…» тех же строк за соответствующие периоды, затем произвести очистку значений в полях «Доп. потребность».
            newDoc.ReWriteAdditionalsExpenses(context);
            context.SaveChanges();

            newDoc.Process(context);

            //прописать в регистрах даты утверждения
            IsApproved = true;
            IdDocStatus = DocStatus.Approved;

            ApproveInReg(context, new[] { Id, newDoc.Id }, newDoc);

            context.SaveChanges();
        }

        /// <summary>   
        /// Операция «Отменить утверждение»   
        /// </summary>  
        public void UndoConfirm(DataContext context)
        {
            DateCommit = null;
            IsApproved = false;

            foreach (var reg in context.LimitVolumeAppropriations.Where(l => l.IdApproved == Id && l.IdApprovedEntity == EntityId))
            {
                reg.DateCommit = null;
                reg.IdApproved = null;
                reg.IdApprovedEntity = null;
            }
        }

        /// <summary>   
        /// Операция «Изменить»   
        /// </summary>  
        public void Change(DataContext context)
        {
            PublicInstitutionEstimate newDoc = Clone(context);
            newDoc.IdDocStatus = DocStatus.Draft;
            newDoc.Date = DateTime.Now.Date;
            newDoc.IdParent = Id;
            newDoc.IsRequireClarification = false;
            newDoc.IsApproved = false;
            newDoc.DateCommit = null;
            //newDoc.ReasonClarification = null;
            newDoc.ReasonTerminate = null;
            //newDoc.ReasonCancel = null;
            newDoc.DateTerminate = null;

            newDoc.Number = this.GetDocNextNumber(context);

            context.Entry(newDoc).State = EntityState.Added;
            context.SaveChanges();

            newDoc.Caption = newDoc.ToString();
            context.SaveChanges();
        }



        /// <summary>   
        /// Операция «Отменить изменение»   
        /// </summary>  
        public void UndoChange(DataContext context)
        {
            var d = context.PublicInstitutionEstimate.Where(s => s.IdParent == Id).ToList();
            foreach (var doc in d)
            {
                context.PublicInstitutionEstimate.Remove(doc);
            }
            IdDocStatus = DateCommit.HasValue ? DocStatus.Approved : 
                                ( !String.IsNullOrEmpty(ReasonCancel) ? DocStatus.Denied : DocStatus.Project);
            context.SaveChanges();
        }

        /// <summary>   
        /// Операция «В архив»   
        /// </summary>  
        public void Archive(DataContext context)
        {
            IdDocStatus = DocStatus.Archive;
            context.SaveChanges();
        }


        /// <summary>   
        /// Операция «В архив» скрытая
        /// </summary>  
        public void ArchiveHidden(DataContext context)
        {
            IdDocStatus = DocStatus.Archive;
            context.SaveChanges();
        }

        /// <summary>   
        /// Операция «Согласование»
        /// </summary>  
        public void Check(DataContext context)
        {
            ExecuteControl(e => e.Control_0733(context));
            ExecuteControl(e => e.Control_0734(context));
            ExecuteControl(e => e.Control_0735(context));
            ExecuteControl(e => e.Control_0736(context));

            IdDocStatus = DocStatus.Checking;
            context.SaveChanges();
        }

        /// <summary>   
        /// Операция «Отменить согласование»
        /// </summary>  
        public void UndoCheck(DataContext context)
        {
            IdDocStatus = DocStatus.Project;
            context.SaveChanges();
        }

        /// <summary>   
        /// Операция «Согласование МРГ»   
        /// </summary>  
        public void CheckMRG(DataContext context)
        {
            this.IsRequireCheck = false;
        }

    }
}
