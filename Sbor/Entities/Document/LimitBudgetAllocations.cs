using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Dynamic;
using System.Text;
using BaseApp;
using EntityFramework.Extensions;
using Microsoft.Practices.Unity.Utility;
using Platform.BusinessLogic;
using Platform.BusinessLogic.Activity.Controls;
using Platform.BusinessLogic.Common.Enums;
using Platform.BusinessLogic.Common.Exceptions;
using Platform.BusinessLogic.Reference;
using Platform.Common.Exceptions;
using Platform.Common.Extensions;
using BaseApp.Activity.Operations;
using Sbor.DbEnums;
using Sbor.Interfaces;
using Sbor.Logic;
using Sbor.Registry;
using Sbor.Tablepart;
using ValueType = Sbor.DbEnums.ValueType;

namespace Sbor.Document
{
    partial class LimitBudgetAllocations : IDocOfSbpBudget
    {
        private class RegistryAllocations
        {
            public int? idEstimatedLine { get; set; }

            public int idHierarchyPeriod { get; set; }

            public bool HasAdditionalNeed { get; set; }
            
            public decimal? docValue { get; set; }
            
            public decimal? regValue { get; set; }

        }

        public class GrpKey
        {
            public byte? ExpenseObligationType;
            public int? FinanceSource;
            public int? KFO;
            public int? KVSR;
            public int? RZPR;
            public int? KCSR;
            public int? KVR;
            public int? KOSGU;
            public int? DFK;
            public int? DKR;
            public int? DEK;
            public int? CodeSubsidy;
            public int? BranchCode;
            public int? Year;
        }

        public class GrpRec
        {
            public GrpKey Key;
            public decimal? SumPlan;
            public decimal? SumDO;
            public decimal? SumDoc;
            public decimal? SumPBC;
        }

        private int[] PrevVersionIds(DataContext context)
        {
            return _prevVersionIds ?? (_prevVersionIds = this.GetIdAllVersion(context).ToArray());
        }

        private int[] _prevVersionIds;

        #region ������ ��������

        #region ��������������� ������

        /// <summary>   
        /// ��� ������� ��������������� ���������� ���� ������ ����� ���������
        /// </summary>         
        private int[] _ids;

        /// <summary>   
        /// ��������� ������� ��������������� ���������� ���� ������ ����� ���������, ������� ��� ������
        /// </summary>         
        public int[] GetIdAllVersionDoc(DataContext context, bool isClearCache = false)
        {
            if (isClearCache || _ids == null)
            {
                var curdoc = this;
                List<int> tmp = new List<int>();
                while (curdoc != null)
                {
                    tmp.Add(curdoc.Id);
                    curdoc = GetPrevVersionDoc(context, curdoc);
                }
                _ids = tmp.ToArray();
            }

            return _ids;
        }

        /// <summary>   
        /// ��������� ���������� ������ ���������
        /// </summary>         
        public LimitBudgetAllocations GetPrevVersionDoc(DataContext context, LimitBudgetAllocations curdoc)
        {
            if (curdoc.IdParent.HasValue)
            {
                return
                    context.LimitBudgetAllocations.Where(w => w.Id == curdoc.IdParent).FirstOrDefault();
            }
            else
            {
                return null;
            }
        }

        public bool SetBlankActual(DataContext context)
        {
            IQueryable<SBP_BlankHistory> newBlanks;

            byte idchekblanktype;
            if (this.SBP.SBPType == DbEnums.SBPType.GeneralManager)
            {
                idchekblanktype = (byte)DbEnums.BlankType.BringingGRBS;
                newBlanks = context.SBP_BlankHistory.Where(r =>
                                                           r.IdBudget == this.IdBudget && r.IdOwner == this.IdSBP &&
                                                           r.IdBlankType == idchekblanktype)
                                   .OrderByDescending(o => o.DateCreate);
            }
            else
            {
                idchekblanktype = (byte)DbEnums.BlankType.BringingRBS;
                newBlanks = context.SBP_BlankHistory.Where(r =>
                                                           r.IdBudget == this.IdBudget && r.IdOwner == this.SBP.IdParent &&
                                                           r.IdBlankType == idchekblanktype)
                                   .OrderByDescending(o => o.DateCreate);
            }

            if (!newBlanks.Any()) return false;

            SBP_BlankHistory oldBlankActual = this.SBP_BlankActual;

            this.SBP_BlankActual = newBlanks.FirstOrDefault();

            return this.SBP_BlankActual != null && (oldBlankActual == null || !SBP_BlankHelper.IsEqualBlank(oldBlankActual, this.SBP_BlankActual));
        }


        private void TrimKbkByNewActualBlank(DataContext context)
        {
            var gkbktrim =
                (from kbk in context.LimitBudgetAllocations_LimitAllocations.
                                     Where(r => r.IdOwner == this.Id).
                                     ToList().
                                     Select(s =>
                                            new
                                                {
                                                    s.IdOwner,
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
                                                    s.AdditionalNeedOFG,
                                                    s.AdditionalNeedPFG1,
                                                    s.AdditionalNeedPFG2
                                                })
                 select new {kbk}).
                    GroupBy(g =>
                            new
                                {
                                    g.kbk.IdOwner,
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
                                   AdditionalNeedOFG = s.Sum(ss => ss.kbk.AdditionalNeedOFG),
                                   AdditionalNeedPFG1 = s.Sum(ss => ss.kbk.AdditionalNeedPFG1),
                                   AdditionalNeedPFG2 = s.Sum(ss => ss.kbk.AdditionalNeedPFG2),
                               }).
                    Select(kbk =>
                           new LimitBudgetAllocations_LimitAllocations()
                               {
                                   IdOwner = this.Id,
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
                                   AdditionalNeedOFG = kbk.AdditionalNeedOFG,
                                   AdditionalNeedPFG1 = kbk.AdditionalNeedPFG1,
                                   AdditionalNeedPFG2 = kbk.AdditionalNeedPFG2
                               }).
                    ToList();

            context.LimitBudgetAllocations_LimitAllocations.RemoveAll(context.LimitBudgetAllocations_LimitAllocations.Where(r => r.IdOwner == this.Id));

            context.LimitBudgetAllocations_LimitAllocations.InsertAsTableValue(gkbktrim, context);
        }

        private void WriteToRegistry(DataContext context)
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
            var blank = SBPBlank;

            var documentAllocations = new List<RegistryAllocations>();

            var year = Budget.Year;
            foreach (var allocation in LimitAllocations)
            {
                var estimatedLineId = allocation.GetLineId(context, Id, EntityId, blank, findParamEstimatedLine);
                
                documentAllocations.Add(new RegistryAllocations()
                    {
                        idEstimatedLine = estimatedLineId, idHierarchyPeriod = year.GetIdHierarchyPeriodYear(context), docValue = allocation.OFG, regValue = null
                    });

                documentAllocations.Add(new RegistryAllocations()
                {
                    idEstimatedLine = estimatedLineId,
                    idHierarchyPeriod = (year+1).GetIdHierarchyPeriodYear(context),
                    docValue = allocation.PFG1,
                    regValue = null,
                    HasAdditionalNeed = false
                });

                documentAllocations.Add(new RegistryAllocations()
                {
                    idEstimatedLine = estimatedLineId,
                    idHierarchyPeriod = (year+2).GetIdHierarchyPeriodYear(context),
                    docValue = allocation.PFG2,
                    regValue = null,
                    HasAdditionalNeed = false
                });

               documentAllocations.Add(new RegistryAllocations()
                {
                    idEstimatedLine = estimatedLineId,
                    idHierarchyPeriod = (year).GetIdHierarchyPeriodYear(context),
                    docValue = allocation.AdditionalNeedOFG,
                    regValue = null,
                    HasAdditionalNeed = true
                });

               documentAllocations.Add(new RegistryAllocations()
                {
                    idEstimatedLine = estimatedLineId,
                    idHierarchyPeriod = (year+1).GetIdHierarchyPeriodYear(context),
                    docValue = allocation.AdditionalNeedPFG1,
                    regValue = null,
                    HasAdditionalNeed = true
                });

               documentAllocations.Add(new RegistryAllocations()
                {
                    idEstimatedLine = estimatedLineId,
                    idHierarchyPeriod = (year+2).GetIdHierarchyPeriodYear(context),
                    docValue = allocation.AdditionalNeedPFG2,
                    regValue = null,
                    HasAdditionalNeed = true
                });

            }

            var prevVersionIds = PrevVersionIds(context);

            if (IdParent.HasValue)
                documentAllocations = documentAllocations.Union(context.LimitVolumeAppropriations.Where(l =>
                                                                                                        prevVersionIds.Contains(l.IdRegistrator) &&
                                                                                                        l.IdRegistratorEntity == EntityId &&
                                                                                                        l.IdValueType == (byte) ValueType.Plan &&
                                                                                                        l.EstimatedLine.IdSBP == IdSBP)
                                                                       .Select(l => new RegistryAllocations
                                                                           {
                                                                               idEstimatedLine = l.IdEstimatedLine,
                                                                               idHierarchyPeriod = l.IdHierarchyPeriod,
                                                                               HasAdditionalNeed = l.HasAdditionalNeed ?? false,
                                                                               docValue = null,
                                                                               regValue = l.Value
                                                                           }).ToList()).ToList();

            foreach (var documentAllocation in documentAllocations.GroupBy(l => new { l.idEstimatedLine, l.idHierarchyPeriod, l.HasAdditionalNeed }).Select(g => new { g.Key.idEstimatedLine, g.Key.idHierarchyPeriod, g.Key.HasAdditionalNeed, value = g.Sum(c => c.docValue) - g.Sum(c => c.regValue) }).ToList())
            {
                if (documentAllocation.value.HasValue && documentAllocation.value != 0)
                {
                    var dataLine = new LimitVolumeAppropriations()
                    {
                        IdPublicLegalFormation = IdPublicLegalFormation,
                        IdVersion = IdVersion,
                        IdBudget = IdBudget,
                        IdEstimatedLine = documentAllocation.idEstimatedLine.Value,
                        IdAuthorityOfExpenseObligation = null,
                        IdTaskCollection = null,
                        IsIndirectCosts = false,
                        IdHierarchyPeriod = documentAllocation.idHierarchyPeriod,
                        IdValueType = (byte)ValueType.Plan,
                        Value = documentAllocation.value.Value,
                        IdOKATO = null,
                        IsMeansAUBU = false,
                        IdRegistrator = Id,
                        //DateCommit = Date.Date,
                        //IdApproved = Id,
                        //IdApprovedEntity = EntityId,
                        DateCreate = DateTime.Now.Date,
                        IdRegistratorEntity = EntityId,
                        HasAdditionalNeed = documentAllocation.HasAdditionalNeed
                    };

                    context.LimitVolumeAppropriations.Add(dataLine);
                }
                
            }

            context.SaveChanges();
            
           if (SBP.Parent != null)
                WriteToRegistryParentSbp(context);
        }

        private void WriteToRegistryParentSbp(DataContext context)
        {
            var blank = SBP.Parent.SBP_Blank.FirstOrDefault(b => b.IdBudget == IdBudget && b.IdBlankType == (int)BlankType.BringingGRBS);
            if (blank == null)
                throw new PlatformException("� ������������� ���������� ����������� ����� ���������");

            var idSBPParent = SBP.Parent.Id;

            var findParamEstimatedLine = new FindParamEstimatedLine
            {
                IdBudget = IdBudget,
                IdPublicLegalFormation = IdPublicLegalFormation,
                IdSbp = idSBPParent,
                IsCreate = true,
                IsKosgu000 = blank.IdBlankValueType_KOSGU == (int)BlankValueType.Mandatory,
                IsRequired = true,
                TypeLine = ActivityBudgetaryType.Costs
            };
  
            var documentAllocations = new List<RegistryAllocations>();

            var year = Budget.Year;
            foreach (var allocation in LimitAllocations)
            {
                var estimatedLineId = allocation.GetLineId(context, Id, EntityId, blank, findParamEstimatedLine);

                documentAllocations.Add(new RegistryAllocations()
                {
                    idEstimatedLine = estimatedLineId,
                    idHierarchyPeriod = year.GetIdHierarchyPeriodYear(context),
                    docValue = allocation.OFG,
                    regValue = null,
                    HasAdditionalNeed = false
                });

                documentAllocations.Add(new RegistryAllocations()
                {
                    idEstimatedLine = estimatedLineId,
                    idHierarchyPeriod = (year + 1).GetIdHierarchyPeriodYear(context),
                    docValue = allocation.PFG1,
                    regValue = null,
                    HasAdditionalNeed = false
                });

                documentAllocations.Add(new RegistryAllocations()
                {
                    idEstimatedLine = estimatedLineId,
                    idHierarchyPeriod = (year + 2).GetIdHierarchyPeriodYear(context),
                    docValue = allocation.PFG2,
                    regValue = null,
                    HasAdditionalNeed = false
                });

                documentAllocations.Add(new RegistryAllocations()
                {
                    idEstimatedLine = estimatedLineId,
                    idHierarchyPeriod = year.GetIdHierarchyPeriodYear(context),
                    docValue = allocation.AdditionalNeedOFG,
                    regValue = null,
                    HasAdditionalNeed = true
                });

                documentAllocations.Add(new RegistryAllocations()
                {
                    idEstimatedLine = estimatedLineId,
                    idHierarchyPeriod = (year + 1).GetIdHierarchyPeriodYear(context),
                    docValue = allocation.AdditionalNeedPFG1,
                    regValue = null,
                    HasAdditionalNeed = true
                });

                documentAllocations.Add(new RegistryAllocations()
                {
                    idEstimatedLine = estimatedLineId,
                    idHierarchyPeriod = (year + 2).GetIdHierarchyPeriodYear(context),
                    docValue = allocation.AdditionalNeedPFG2,
                    regValue = null,
                    HasAdditionalNeed = true
                });

            }

            var prevVersionIds = PrevVersionIds(context);

            if (IdParent.HasValue)
                documentAllocations = documentAllocations.Union(context.LimitVolumeAppropriations.Where(l =>
                                                                                                        prevVersionIds.Contains(l.IdRegistrator) &&
                                                                                                        l.IdRegistratorEntity == EntityId &&
                                                                                                        l.IdValueType == (byte)ValueType.Bring &&
                                                                                                        l.EstimatedLine.IdSBP == idSBPParent)
                                                                       .Select(l => new RegistryAllocations()
                                                                       {
                                                                           idEstimatedLine = l.IdEstimatedLine,
                                                                           idHierarchyPeriod = l.IdHierarchyPeriod,
                                                                           HasAdditionalNeed = l.HasAdditionalNeed??false,
                                                                           docValue = null,
                                                                           regValue = l.Value
                                                                       }).ToList()).ToList();

            foreach (var documentAllocation in documentAllocations.GroupBy(l => new { l.idEstimatedLine, l.idHierarchyPeriod, l.HasAdditionalNeed }).Select(g => new { g.Key.idEstimatedLine, g.Key.idHierarchyPeriod, g.Key.HasAdditionalNeed, value = g.Sum(c => c.docValue) - g.Sum(c => c.regValue) }).ToList())
            {
                if (documentAllocation.value.HasValue && documentAllocation.value != 0)
                {
                    var dataLine = new LimitVolumeAppropriations()
                    {
                        IdPublicLegalFormation = IdPublicLegalFormation,
                        IdVersion = IdVersion,
                        IdBudget = IdBudget,
                        IdEstimatedLine = documentAllocation.idEstimatedLine.Value,
                        IdAuthorityOfExpenseObligation = null,
                        IdTaskCollection = null,
                        IsIndirectCosts = false,
                        IdHierarchyPeriod = documentAllocation.idHierarchyPeriod,
                        IdValueType = (byte)ValueType.Bring,
                        Value = documentAllocation.value.Value,
                        IdOKATO = null,
                        IsMeansAUBU = false,
                        IdRegistrator = Id,
                        //DateCommit = DateCommit,
                        //IdApproved = Id,
                        //IdApprovedEntity = EntityId,
                        DateCreate = DateTime.Now,
                        IdRegistratorEntity = EntityId,
                        HasAdditionalNeed = documentAllocation.HasAdditionalNeed
                    };

                    context.LimitVolumeAppropriations.Add(dataLine);
                }

            }

            context.SaveChanges();
        }

        public void ControlRelation_FillData(DataContext context)
        {
            //var SBP = context.SBP.SingleOrDefault(s => s.Id == IdSBP);

            var blank = new SBP_Blank();
            bool isGRBS = SBP.SBPType == DbEnums.SBPType.GeneralManager;

            if (SBP.SBPType == DbEnums.SBPType.GeneralManager)
            {
                blank = context.SBP_Blank.SingleOrDefault(s => s.IdOwner == IdSBP && s.IdBlankType == (int)BlankType.BringingGRBS);
            }
            else
            {
                blank = context.SBP_Blank.SingleOrDefault(s => s.IdOwner == SBP.IdParent && s.IdBlankType == (int)BlankType.BringingGRBS);
            }

            byte req = (byte)BlankValueType.Mandatory;
            var Data = new List<GrpRec>();

            Data.AddRange(context.LimitVolumeAppropriations.Where(r =>
                r.IdBudget == this.IdBudget
                && r.IdVersion == this.IdVersion
                && r.IdPublicLegalFormation == this.IdPublicLegalFormation
                && (r.IdValueType == (int)ValueType.Plan || r.IdValueType == (int)ValueType.Bring)
                && (r.EstimatedLine.IdSBP == SBP.IdParent || r.EstimatedLine.IdSBP == IdSBP)
                ).GroupBy(g => new GrpKey
                {
                    ExpenseObligationType = (blank.IdBlankValueType_ExpenseObligationType != req || g.EstimatedLine.IdExpenseObligationType == null ? null : g.EstimatedLine.IdExpenseObligationType),
                    FinanceSource = (blank.IdBlankValueType_FinanceSource != req || g.EstimatedLine.IdFinanceSource == null ? null : g.EstimatedLine.IdFinanceSource),
                    KFO = (blank.IdBlankValueType_KFO != req || g.EstimatedLine.IdKFO == null ? null : g.EstimatedLine.IdKFO),
                    KVSR = (blank.IdBlankValueType_KVSR != req || g.EstimatedLine.IdKVSR == null ? null : g.EstimatedLine.IdKVSR),
                    RZPR = (blank.IdBlankValueType_RZPR != req || g.EstimatedLine.IdRZPR == null ? null : g.EstimatedLine.IdRZPR),
                    KCSR = (blank.IdBlankValueType_KCSR != req || g.EstimatedLine.IdKCSR == null ? null : g.EstimatedLine.IdKCSR),
                    KVR = (blank.IdBlankValueType_KVR != req || g.EstimatedLine.IdKVR == null ? null : g.EstimatedLine.IdKVR),
                    KOSGU = (blank.IdBlankValueType_KOSGU != req || g.EstimatedLine.IdKOSGU == null ? null : g.EstimatedLine.IdKOSGU),
                    DFK = (blank.IdBlankValueType_DFK != req || g.EstimatedLine.IdDFK == null ? null : g.EstimatedLine.IdDFK),
                    DKR = (blank.IdBlankValueType_DKR != req || g.EstimatedLine.IdDKR == null ? null : g.EstimatedLine.IdDKR),
                    DEK = (blank.IdBlankValueType_DEK != req || g.EstimatedLine.IdDEK == null ? null : g.EstimatedLine.IdDEK),
                    CodeSubsidy = (blank.IdBlankValueType_CodeSubsidy != req || g.EstimatedLine.IdCodeSubsidy == null ? null : g.EstimatedLine.IdCodeSubsidy),
                    BranchCode = (blank.IdBlankValueType_BranchCode != req || g.EstimatedLine.IdBranchCode == null ? null : g.EstimatedLine.IdBranchCode),
                    Year = g.HierarchyPeriod.Year
                }).Select(s => new GrpRec
                {
                    Key = s.Key
                    ,
                    SumPlan = s.Sum(c => (
                       c.EstimatedLine.IdSBP == (isGRBS ? IdSBP : SBP.IdParent) && c.IdValueType == (int)ValueType.Plan
                       ? (decimal?)c.Value : (decimal?)0)
                    )
                    ,
                    SumDO = s.Sum(c => (
                       c.EstimatedLine.IdSBP == (isGRBS ? IdSBP : SBP.IdParent) && c.IdValueType == (int)ValueType.Bring
                       ? (decimal?)c.Value : (decimal?)0)
                    )
                    ,
                    SumPBC = s.Sum(c => (
                       c.EstimatedLine.IdSBP == (isGRBS ? IdSBP : SBP.IdParent) && c.IdValueType == (int)ValueType.Plan
                       ? (decimal?)c.Value : (decimal?)0)
                    ),
                    SumDoc = 0
                }).ToList());

            

            for (var i = 0; i < 3; i++)
            {
                Data.AddRange(
                    this.LimitAllocations.GroupBy(g => new GrpKey
                {
                    ExpenseObligationType = (blank.IdBlankValueType_ExpenseObligationType != req || g.IdExpenseObligationType == null ? null : g.IdExpenseObligationType),
                    FinanceSource = (blank.IdBlankValueType_FinanceSource != req || g.IdFinanceSource == null ? null : g.IdFinanceSource),
                    KFO = (blank.IdBlankValueType_KFO != req || g.IdKFO == null ? null : g.IdKFO),
                    KVSR = (blank.IdBlankValueType_KVSR != req || g.IdKVSR == null ? null : g.IdKVSR),
                    RZPR = (blank.IdBlankValueType_RZPR != req || g.IdRZPR == null ? null : g.IdRZPR),
                    KCSR = (blank.IdBlankValueType_KCSR != req || g.IdKCSR == null ? null : g.IdKCSR),
                    KVR = (blank.IdBlankValueType_KVR != req || g.IdKVR == null ? null : g.IdKVR),
                    KOSGU = (blank.IdBlankValueType_KOSGU != req || g.IdKOSGU == null ? null : g.IdKOSGU),
                    DFK = (blank.IdBlankValueType_DFK != req || g.IdDFK == null ? null : g.IdDFK),
                    DKR = (blank.IdBlankValueType_DKR != req || g.IdDKR == null ? null : g.IdDKR),
                    DEK = (blank.IdBlankValueType_DEK != req || g.IdDEK == null ? null : g.IdDEK),
                    CodeSubsidy = (blank.IdBlankValueType_CodeSubsidy != req || g.IdCodeSubsidy == null ? null : g.IdCodeSubsidy),
                    BranchCode = (blank.IdBlankValueType_BranchCode != req || g.IdBranchCode == null ? null : g.IdBranchCode),
                    Year   = Budget.Year+i
                }).Select(s => new GrpRec
                 {
                        Key = s.Key,
                        SumPlan = 0,
                        SumDO = 0,
                        SumPBC = 0,
                        SumDoc = s.Sum(c => (i == 0 ? (decimal?)c.OFG : (i == 1 ? (decimal?)c.PFG1 : (decimal?)c.PFG2)))
                 }).ToList()
                );
            }
            
            var list = Data.GroupBy(g => new
            {
                g.Key.ExpenseObligationType,
                g.Key.FinanceSource,
                g.Key.KFO,
                g.Key.KVSR,
                g.Key.RZPR,
                g.Key.KCSR,
                g.Key.KVR,
                g.Key.KOSGU,
                g.Key.DFK,
                g.Key.DKR,
                g.Key.DEK,
                g.Key.CodeSubsidy,
                g.Key.BranchCode,
                g.Key.Year
            }).Select(s => new LimitBudgetAllocations_ControlRelation ()
            {
                IdOwner = this.Id,
                IdExpenseObligationType = s.Key.ExpenseObligationType,
                IdFinanceSource = s.Key.FinanceSource,
                IdKFO = s.Key.KFO,
                IdKVSR = s.Key.KVSR,
                IdRZPR = s.Key.RZPR,
                IdKCSR = s.Key.KCSR,
                IdKVR = s.Key.KVR,
                IdKOSGU = s.Key.KOSGU,
                IdDFK = s.Key.DFK,
                IdDKR = s.Key.DKR,
                IdDEK = s.Key.DEK,
                IdCodeSubsidy = s.Key.CodeSubsidy,
                IdBranchCode = s.Key.BranchCode,
                Year = s.Key.Year,
                DiffAllocations = (isGRBS ? (decimal?)null : s.Sum(c => (c.SumPlan ?? 0) - (c.SumDO ?? 0) - (c.SumDoc ?? 0))),
                UnallocatedAllocations = s.Sum(c => (c.SumPlan ?? 0) - (c.SumDO ?? 0)),
                TotalDocumentAllocations = s.Sum(c => c.SumDoc),
                AllocatedAllocations = s.Sum(c => c.SumDO),
                WithCompanyAllocations = (isGRBS ? (decimal?)null : s.Sum(c => c.SumPBC)),
                PlanGRBSAllocations = s.Sum(c => c.SumPlan)
            }).Where(w =>
                    (w.DiffAllocations ?? 0) != 0
                 || (w.UnallocatedAllocations ?? 0) != 0
                 || (w.TotalDocumentAllocations ?? 0) != 0
                 || (w.AllocatedAllocations ?? 0) != 0
                 || (w.WithCompanyAllocations ?? 0) != 0
                 || (w.PlanGRBSAllocations ?? 0) != 0
             );

            context.LimitBudgetAllocations_ControlRelation.Delete(d => d.IdOwner == this.Id);
            context.LimitBudgetAllocations_ControlRelation.InsertAsTableValue(list, context);
            context.SaveChanges();
        }

        public void ShowChanges_FillData(DataContext context)
        {

            context.LimitBudgetAllocations_ShowChanges.Delete(d => d.IdOwner == this.Id);

            if (IdCompareWithDocument == null)
            {
                return;
            }


            var CompareDocument = context.LimitBudgetAllocations.SingleOrDefault(s => s.Id == IdCompareWithDocument);

            var blank = new SBP_Blank();
            bool isGRBS = SBP.SBPType == DbEnums.SBPType.Manager;

            if (SBP.SBPType == DbEnums.SBPType.GeneralManager)
            {
                blank = context.SBP_Blank.SingleOrDefault(s => s.IdOwner == IdSBP && s.IdBlankType == (int)BlankType.BringingGRBS);
            }
            else
            {
                blank = context.SBP_Blank.SingleOrDefault(s => s.IdOwner == SBP.IdParent && s.IdBlankType == (int)BlankType.BringingGRBS);
            }

            byte req = (byte)BlankValueType.Mandatory;

            var DataReg = context.LimitVolumeAppropriations.Where(r =>
                r.IdBudget == this.IdBudget
                && r.IdVersion == this.IdVersion
                && r.IdPublicLegalFormation == this.IdPublicLegalFormation
                && r.IdValueType == (int)ValueType.Plan
                && r.EstimatedLine.IdSBP == IdSBP
                && r.DateCommit <= CompareDocument.DateCommit
                ).GroupBy(g => new GrpKey
                {
                    ExpenseObligationType = (blank.IdBlankValueType_ExpenseObligationType != req || g.EstimatedLine.IdExpenseObligationType == null ? null : g.EstimatedLine.IdExpenseObligationType),
                    FinanceSource = (blank.IdBlankValueType_FinanceSource != req || g.EstimatedLine.IdFinanceSource == null ? null : g.EstimatedLine.IdFinanceSource),
                    KFO = (blank.IdBlankValueType_KFO != req || g.EstimatedLine.IdKFO == null ? null : g.EstimatedLine.IdKFO),
                    KVSR = (blank.IdBlankValueType_KVSR != req || g.EstimatedLine.IdKVSR == null ? null : g.EstimatedLine.IdKVSR),
                    RZPR = (blank.IdBlankValueType_RZPR != req || g.EstimatedLine.IdRZPR == null ? null : g.EstimatedLine.IdRZPR),
                    KCSR = (blank.IdBlankValueType_KCSR != req || g.EstimatedLine.IdKCSR == null ? null : g.EstimatedLine.IdKCSR),
                    KVR = (blank.IdBlankValueType_KVR != req || g.EstimatedLine.IdKVR == null ? null : g.EstimatedLine.IdKVR),
                    KOSGU = (blank.IdBlankValueType_KOSGU != req || g.EstimatedLine.IdKOSGU == null ? null : g.EstimatedLine.IdKOSGU),
                    DFK = (blank.IdBlankValueType_DFK != req || g.EstimatedLine.IdDFK == null ? null : g.EstimatedLine.IdDFK),
                    DKR = (blank.IdBlankValueType_DKR != req || g.EstimatedLine.IdDKR == null ? null : g.EstimatedLine.IdDKR),
                    DEK = (blank.IdBlankValueType_DEK != req || g.EstimatedLine.IdDEK == null ? null : g.EstimatedLine.IdDEK),
                    CodeSubsidy = (blank.IdBlankValueType_CodeSubsidy != req || g.EstimatedLine.IdCodeSubsidy == null ? null : g.EstimatedLine.IdCodeSubsidy),
                    BranchCode = (blank.IdBlankValueType_BranchCode != req || g.EstimatedLine.IdBranchCode == null ? null : g.EstimatedLine.IdBranchCode),
                    Year = g.HierarchyPeriod.Year
                }).Select(s => new 
                {
                    Key = s.Key,
                    Sum = s.Sum(c =>  (decimal?)c.Value)*-1
                }).ToList();


            for (var i = 0; i < 3; i++)
            {
                DataReg.AddRange(
                    this.LimitAllocations.GroupBy(g => new GrpKey
                    {
                        ExpenseObligationType = (blank.IdBlankValueType_ExpenseObligationType != req || g.IdExpenseObligationType == null ? null : g.IdExpenseObligationType),
                        FinanceSource = (blank.IdBlankValueType_FinanceSource != req || g.IdFinanceSource == null ? null : g.IdFinanceSource),
                        KFO = (blank.IdBlankValueType_KFO != req || g.IdKFO == null ? null : g.IdKFO),
                        KVSR = (blank.IdBlankValueType_KVSR != req || g.IdKVSR == null ? null : g.IdKVSR),
                        RZPR = (blank.IdBlankValueType_RZPR != req || g.IdRZPR == null ? null : g.IdRZPR),
                        KCSR = (blank.IdBlankValueType_KCSR != req || g.IdKCSR == null ? null : g.IdKCSR),
                        KVR = (blank.IdBlankValueType_KVR != req || g.IdKVR == null ? null : g.IdKVR),
                        KOSGU = (blank.IdBlankValueType_KOSGU != req || g.IdKOSGU == null ? null : g.IdKOSGU),
                        DFK = (blank.IdBlankValueType_DFK != req || g.IdDFK == null ? null : g.IdDFK),
                        DKR = (blank.IdBlankValueType_DKR != req || g.IdDKR == null ? null : g.IdDKR),
                        DEK = (blank.IdBlankValueType_DEK != req || g.IdDEK == null ? null : g.IdDEK),
                        CodeSubsidy = (blank.IdBlankValueType_CodeSubsidy != req || g.IdCodeSubsidy == null ? null : g.IdCodeSubsidy),
                        BranchCode = (blank.IdBlankValueType_BranchCode != req || g.IdBranchCode == null ? null : g.IdBranchCode),
                        Year = Budget.Year + i
                    }).Select(s => new 
                    {
                        Key = s.Key,
                        Sum = s.Sum(c => (i == 0 ? (decimal?)c.OFG : (i == 1 ? c.PFG1 : c.PFG2)))
                    }).ToList()
                );
            }

            var list = DataReg.GroupBy(g => new
                                                {
                                                    g.Key.ExpenseObligationType,
                                                    g.Key.FinanceSource,
                                                    g.Key.KFO,
                                                    g.Key.KVSR,
                                                    g.Key.RZPR,
                                                    g.Key.KCSR,
                                                    g.Key.KVR,
                                                    g.Key.KOSGU,
                                                    g.Key.DFK,
                                                    g.Key.DKR,
                                                    g.Key.DEK,
                                                    g.Key.CodeSubsidy,
                                                    g.Key.BranchCode,
                                                    g.Key.Year
                                                }).Select(s => new
                                                                   {
                                                                       IdExpenseObligationType = s.Key.ExpenseObligationType,
                                                                       IdFinanceSource = s.Key.FinanceSource,
                                                                       IdKFO = s.Key.KFO,
                                                                       IdKVSR = s.Key.KVSR,
                                                                       IdRZPR = s.Key.RZPR,
                                                                       IdKCSR = s.Key.KCSR,
                                                                       IdKVR = s.Key.KVR,
                                                                       IdKOSGU = s.Key.KOSGU,
                                                                       IdDFK = s.Key.DFK,
                                                                       IdDKR = s.Key.DKR,
                                                                       IdDEK = s.Key.DEK,
                                                                       IdCodeSubsidy = s.Key.CodeSubsidy,
                                                                       IdBranchCode = s.Key.BranchCode,
                                                                       Year = s.Key.Year,
                                                                       ChangedAllocations = s.Sum(c => c.Sum)
                                                                   }).Where(w =>
                                                                            (w.ChangedAllocations ?? 0) != 0);

 
            foreach (var str in list)
            {
                context.LimitBudgetAllocations_ShowChanges.Add(new LimitBudgetAllocations_ShowChanges
                                                                   {
                                                                       IdOwner = this.Id,
                                                                       IdExpenseObligationType = str.IdExpenseObligationType,
                                                                       IdFinanceSource = str.IdFinanceSource,
                                                                       IdKFO = str.IdKFO,
                                                                       IdKVSR = str.IdKVSR,
                                                                       IdRZPR = str.IdRZPR,
                                                                       IdKCSR = str.IdKCSR,
                                                                       IdKVR = str.IdKVR,
                                                                       IdKOSGU = str.IdKOSGU,
                                                                       IdDFK = str.IdDFK,
                                                                       IdDKR = str.IdDKR,
                                                                       IdDEK = str.IdDEK,
                                                                       IdCodeSubsidy = str.IdCodeSubsidy,
                                                                       IdBranchCode = str.IdBranchCode,
                                                                       Year = str.Year,
                                                                       ChangedAllocations = str.ChangedAllocations
                                                                   });
            }
            context.SaveChanges();
        }

        /// <summary>   
        /// ��������� ���������� ������ ���������
        /// </summary>         
        private LimitBudgetAllocations GetPrevVersionDoc(DataContext context, int? idParent)
        {
            return idParent.HasValue ? context.LimitBudgetAllocations.FirstOrDefault(w => w.Id == idParent) : null;
        }

        private void RemoveFromRegistry(DataContext context)
        {
            context.LimitVolumeAppropriations.Delete(l => l.IdRegistrator == Id && l.IdRegistratorEntity == EntityId);
            context.SaveChanges();
        }

        private void afterControlSkip_0512(DataContext context, IEnumerable<LimitVolumeAppropriationResult> errors)
        {
            var blank = SBP.SBP_Blank.FirstOrDefault(b => b.IdBlankType == (int)BlankType.BringingGRBS);
            if (blank != null)
            {
                // ������������ ��������� �� ���
                foreach (var sbpId in context.SBP.Where(s => s.IdParent == IdSBP).Select(s => s.Id).ToList())
                {
                    SetDocumentStatusForSBP(context, sbpId, errors, blank);
                }


                // ������������ ������� ��������

                bool hasErrorsDoc = false;
                var doc = _tpLimitAllocations.ToList();
                var errMsgDoc = new StringBuilder();

                var ErrorLineDoc = doc.Join(errors,
                     a => new
                     {
                         IdExpenseObligationType = (blank.BlankValueType_ExpenseObligationType == BlankValueType.Mandatory ? a.IdExpenseObligationType : 0),
                         IdBranchCode = (blank.BlankValueType_BranchCode == BlankValueType.Mandatory ? a.IdBranchCode : 0),
                         IdCodeSubsidy = (blank.BlankValueType_CodeSubsidy == BlankValueType.Mandatory ? a.IdCodeSubsidy : 0),
                         IdDEK = (blank.BlankValueType_DEK == BlankValueType.Mandatory ? a.IdDEK : 0),
                         IdDFK = (blank.BlankValueType_DFK == BlankValueType.Mandatory ? a.IdDFK : 0),
                         IdDKR = (blank.BlankValueType_DKR == BlankValueType.Mandatory ? a.IdDKR : 0),
                         IdFinanceSource = (blank.BlankValueType_FinanceSource == BlankValueType.Mandatory ? a.IdFinanceSource : 0),
                         IdKCSR = (blank.BlankValueType_KCSR == BlankValueType.Mandatory ? a.IdKCSR : 0),
                         IdKFO = (blank.BlankValueType_KFO == BlankValueType.Mandatory ? a.IdKFO : 0),
                         IdKOSGU = (blank.BlankValueType_KOSGU == BlankValueType.Mandatory ? a.IdKOSGU : 0),
                         IdKVR = (blank.BlankValueType_KVR == BlankValueType.Mandatory ? a.IdKVR : 0),
                         IdKVSR = (blank.BlankValueType_KVSR == BlankValueType.Mandatory ? a.IdKVSR : 0),
                         IdRZPR = (blank.BlankValueType_RZPR == BlankValueType.Mandatory ? a.IdRZPR : 0)
                     },
                     b => new
                     {
                         IdExpenseObligationType = (blank.BlankValueType_ExpenseObligationType == BlankValueType.Mandatory ? b.IdExpenseObligationType : 0),
                         IdBranchCode = (blank.BlankValueType_BranchCode == BlankValueType.Mandatory ? b.IdBranchCode : 0),
                         IdCodeSubsidy = (blank.BlankValueType_CodeSubsidy == BlankValueType.Mandatory ? b.IdCodeSubsidy : 0),
                         IdDEK = (blank.BlankValueType_DEK == BlankValueType.Mandatory ? b.IdDEK : 0),
                         IdDFK = (blank.BlankValueType_DFK == BlankValueType.Mandatory ? b.IdDFK : 0),
                         IdDKR = (blank.BlankValueType_DKR == BlankValueType.Mandatory ? b.IdDKR : 0),
                         IdFinanceSource = (blank.BlankValueType_FinanceSource == BlankValueType.Mandatory ? b.IdFinanceSource : 0),
                         IdKCSR = (blank.BlankValueType_KCSR == BlankValueType.Mandatory ? b.IdKCSR : 0),
                         IdKFO = (blank.BlankValueType_KFO == BlankValueType.Mandatory ? b.IdKFO : 0),
                         IdKOSGU = (blank.BlankValueType_KOSGU == BlankValueType.Mandatory ? b.IdKOSGU : 0),
                         IdKVR = (blank.BlankValueType_KVR == BlankValueType.Mandatory ? b.IdKVR : 0),
                         IdKVSR = (blank.BlankValueType_KVSR == BlankValueType.Mandatory ? b.IdKVSR : 0),
                         IdRZPR = (blank.BlankValueType_RZPR == BlankValueType.Mandatory ? b.IdRZPR : 0)
                     }, (a, b) => a).ToList();

                foreach (var error in ErrorLineDoc)
                {
                    hasErrorsDoc = true;
                    errMsgDoc.Append("\n"
                        + (blank.BlankValueType_ExpenseObligationType == BlankValueType.Mandatory ? "��� �� - " + error.ExpenseObligationType.Caption() + "; " : "")
                        + (blank.BlankValueType_FinanceSource == BlankValueType.Mandatory ? "�������� �������������� - " + error.FinanceSource.Code + "; " : "")
                        + (blank.BlankValueType_KCSR == BlankValueType.Mandatory ? "��� - " + error.KFO.Code + "; " : "")
                        + (blank.BlankValueType_KVSR == BlankValueType.Mandatory ? "����/����/���� - " + error.KVSR.Caption + "; " : "")
                        + (blank.BlankValueType_RZPR == BlankValueType.Mandatory ? "���� - " + error.RZPR.Code + "; " : "")
                        + (blank.BlankValueType_KCSR == BlankValueType.Mandatory ? "���� - " + error.KCSR.Code + "; " : "")
                        + (blank.BlankValueType_KVR == BlankValueType.Mandatory ? "��� - " + error.KVR.Code + "; " : "")
                        + (blank.BlankValueType_KOSGU == BlankValueType.Mandatory ? "����� - " + error.KOSGU.Code + "; " : "")
                        + (blank.BlankValueType_DFK == BlankValueType.Mandatory ? "��� - " + error.DFK.Code + "; " : "")
                        + (blank.BlankValueType_DKR == BlankValueType.Mandatory ? "��� - " + error.DKR.Code + "; " : "")
                        + (blank.BlankValueType_DEK == BlankValueType.Mandatory ? "��� - " + error.DEK.Code + "; " : "")
                        + (blank.BlankValueType_CodeSubsidy == BlankValueType.Mandatory ? "��� �������� - " + error.CodeSubsidy.Code + "; " : "")
                        + (blank.BlankValueType_BranchCode == BlankValueType.Mandatory ? "���������� ��� - " + error.BranchCode.Code + "; " : "")
                        );
                }

                if (hasErrorsDoc)
                {
                    this.IsRequireClarification = true;
                    this.ReasonClarification = String.Format(
                            "�{0}.  � ������� ��������� �� ��������� ������� ������� ��� ������� �������� ����� ��������� ������������:" +
                            "{1}", DateTime.Now.ToShortDateString(), errMsgDoc); ;
                }
            }
            context.SaveChanges();
        }

        private void afterControlSkip_0511(DataContext context, IEnumerable<LimitVolumeAppropriationResult> errors)
        {
            var SBPParent = context.SBP.SingleOrDefault(s => s.Id == SBP.IdParent);
            var blank = SBPParent.SBP_Blank.FirstOrDefault(b => b.IdBlankType == (int)BlankType.BringingGRBS);
            if (blank != null)
            {
                // ������������ �� ����������� ������ ��������� ������������ 

                foreach (var sbpId in context.SBP.Where(s => s.IdParent == SBP.IdParent).Select(s => s.Id).Union(context.SBP.Where(w => w.Id == SBP.IdParent).Select(s => s.Id)).ToList())
                {
                    
                    foreach (var docLimitBudgetAllocations in context.LimitBudgetAllocations.Where(l => l.IdSBP == sbpId &&
                                                                                l.IdPublicLegalFormation == IdPublicLegalFormation &&
                                                                                l.IdBudget == IdBudget &&
                                                                                l.IdVersion == IdVersion).ToList())
                    {
                        bool hasErrors = false;
                        var docLimitAllocations = docLimitBudgetAllocations.LimitAllocations.ToList();
                        var errMsg = new StringBuilder();

                        var ErrorLine = docLimitAllocations.Join(errors,
                             a => new
                             {
                                 IdExpenseObligationType = (blank.BlankValueType_ExpenseObligationType == BlankValueType.Mandatory ? a.IdExpenseObligationType : 0),
                                 IdBranchCode = (blank.BlankValueType_BranchCode == BlankValueType.Mandatory ? a.IdBranchCode : 0),
                                 IdCodeSubsidy = (blank.BlankValueType_CodeSubsidy == BlankValueType.Mandatory ? a.IdCodeSubsidy : 0),
                                 IdDEK = (blank.BlankValueType_DEK == BlankValueType.Mandatory ? a.IdDEK : 0),
                                 IdDFK = (blank.BlankValueType_DFK == BlankValueType.Mandatory ? a.IdDFK : 0),
                                 IdDKR = (blank.BlankValueType_DKR == BlankValueType.Mandatory ? a.IdDKR : 0),
                                 IdFinanceSource = (blank.BlankValueType_FinanceSource == BlankValueType.Mandatory ? a.IdFinanceSource : 0),
                                 IdKCSR = (blank.BlankValueType_KCSR == BlankValueType.Mandatory ? a.IdKCSR : 0),
                                 IdKFO = (blank.BlankValueType_KFO == BlankValueType.Mandatory ? a.IdKFO : 0),
                                 IdKOSGU = (blank.BlankValueType_KOSGU == BlankValueType.Mandatory ? a.IdKOSGU : 0),
                                 IdKVR = (blank.BlankValueType_KVR == BlankValueType.Mandatory ? a.IdKVR : 0),
                                 IdKVSR = (blank.BlankValueType_KVSR == BlankValueType.Mandatory ? a.IdKVSR : 0),
                                 IdRZPR = (blank.BlankValueType_RZPR == BlankValueType.Mandatory ? a.IdRZPR : 0)
                             },
                             b => new
                             {
                                 IdExpenseObligationType = (blank.BlankValueType_ExpenseObligationType == BlankValueType.Mandatory ? b.IdExpenseObligationType : 0),
                                 IdBranchCode = (blank.BlankValueType_BranchCode == BlankValueType.Mandatory ? b.IdBranchCode : 0),
                                 IdCodeSubsidy = (blank.BlankValueType_CodeSubsidy == BlankValueType.Mandatory ? b.IdCodeSubsidy : 0),
                                 IdDEK = (blank.BlankValueType_DEK == BlankValueType.Mandatory ? b.IdDEK : 0),
                                 IdDFK = (blank.BlankValueType_DFK == BlankValueType.Mandatory ? b.IdDFK : 0),
                                 IdDKR = (blank.BlankValueType_DKR == BlankValueType.Mandatory ? b.IdDKR : 0),
                                 IdFinanceSource = (blank.BlankValueType_FinanceSource == BlankValueType.Mandatory ? b.IdFinanceSource : 0),
                                 IdKCSR = (blank.BlankValueType_KCSR == BlankValueType.Mandatory ? b.IdKCSR : 0),
                                 IdKFO = (blank.BlankValueType_KFO == BlankValueType.Mandatory ? b.IdKFO : 0),
                                 IdKOSGU = (blank.BlankValueType_KOSGU == BlankValueType.Mandatory ? b.IdKOSGU : 0),
                                 IdKVR = (blank.BlankValueType_KVR == BlankValueType.Mandatory ? b.IdKVR : 0),
                                 IdKVSR = (blank.BlankValueType_KVSR == BlankValueType.Mandatory ? b.IdKVSR : 0),
                                 IdRZPR = (blank.BlankValueType_RZPR == BlankValueType.Mandatory ? b.IdRZPR : 0)
                             }, (a, b) => a).ToList();

                        foreach (var error in ErrorLine)
                        {
                            hasErrors = true;
                            errMsg.Append("\n"
                                + (blank.BlankValueType_ExpenseObligationType == BlankValueType.Mandatory ? "��� �� - " + error.ExpenseObligationType.Caption() + "; " : "")
                                + (blank.BlankValueType_FinanceSource == BlankValueType.Mandatory ? "�������� �������������� - " + error.FinanceSource.Code + "; " : "")
                                + (blank.BlankValueType_KCSR == BlankValueType.Mandatory ? "��� - " + error.KFO.Code + "; " : "")
                                + (blank.BlankValueType_KVSR == BlankValueType.Mandatory ? "����/����/���� - " + error.KVSR.Caption + "; " : "")
                                + (blank.BlankValueType_RZPR == BlankValueType.Mandatory ? "���� - " + error.RZPR.Code + "; " : "")
                                + (blank.BlankValueType_KCSR == BlankValueType.Mandatory ? "���� - " + error.KCSR.Code + "; " : "")
                                + (blank.BlankValueType_KVR == BlankValueType.Mandatory ? "��� - " + error.KVR.Code + "; " : "")
                                + (blank.BlankValueType_KOSGU == BlankValueType.Mandatory ? "����� - " + error.KOSGU.Code + "; " : "")
                                + (blank.BlankValueType_DFK == BlankValueType.Mandatory ? "��� - " + error.DFK.Code + "; " : "")
                                + (blank.BlankValueType_DKR == BlankValueType.Mandatory ? "��� - " + error.DKR.Code + "; " : "")
                                + (blank.BlankValueType_DEK == BlankValueType.Mandatory ? "��� - " + error.DEK.Code + "; " : "")
                                + (blank.BlankValueType_CodeSubsidy == BlankValueType.Mandatory ? "��� �������� - " + error.CodeSubsidy.Code + "; " : "")
                                + (blank.BlankValueType_BranchCode == BlankValueType.Mandatory ? "���������� ��� - " + error.BranchCode.Code + "; " : "")
                                );
                        }

                        if (hasErrors)
                        {
                            docLimitBudgetAllocations.IsRequireClarification = true;
                            docLimitBudgetAllocations.ReasonClarification = String.Format(
                                    "�{0}. ����� ��������� ������������, ��������� �� ���������������� ����������, ��������� ������� ���������������� ������� ������������ ��� �{1}�:" +
                                    "{2}", DateTime.Now.ToShortDateString(), SBP.Parent.Caption, errMsg);
                        }
                    }                 
                }

                // ������������ �� ����� ������������  

                foreach (var sbpId in context.SBP.Where(s => s.IdParent == SBP.IdParent).Select(s => s.Id).ToList())
                {

                    foreach (var docPlanActivity in context.PlanActivity.Where(l => l.IdSBP == sbpId &&
                                                                                l.IdPublicLegalFormation == IdPublicLegalFormation &&
                                                                                l.IdBudget == IdBudget &&
                                                                                l.IdVersion == IdVersion
                                                                                ).ToList())
                    {
                        bool hasErrors = false;
                        var docKBKOfFinancialProvisions = docPlanActivity.KBKOfFinancialProvisions.ToList();
                        var errMsg = new StringBuilder();

                        var ErrorLine = docKBKOfFinancialProvisions.Join(errors,
                            a => new
                            {
                                IdExpenseObligationType = (blank.BlankValueType_ExpenseObligationType == BlankValueType.Mandatory ? a.IdExpenseObligationType : 0),
                                IdBranchCode = (blank.BlankValueType_BranchCode == BlankValueType.Mandatory ? a.IdBranchCode : 0),
                                IdCodeSubsidy = (blank.BlankValueType_CodeSubsidy == BlankValueType.Mandatory ? a.IdCodeSubsidy : 0),
                                IdDEK = (blank.BlankValueType_DEK == BlankValueType.Mandatory ? a.IdDEK : 0),
                                IdDFK = (blank.BlankValueType_DFK == BlankValueType.Mandatory ? a.IdDFK : 0),
                                IdDKR = (blank.BlankValueType_DKR == BlankValueType.Mandatory ? a.IdDKR : 0),
                                IdFinanceSource = (blank.BlankValueType_FinanceSource == BlankValueType.Mandatory ? a.IdFinanceSource : 0),
                                IdKCSR = (blank.BlankValueType_KCSR == BlankValueType.Mandatory ? a.IdKCSR : 0),
                                IdKFO = (blank.BlankValueType_KFO == BlankValueType.Mandatory ? a.IdKFO : 0),
                                IdKOSGU = (blank.BlankValueType_KOSGU == BlankValueType.Mandatory ? a.IdKOSGU : 0),
                                IdKVR = (blank.BlankValueType_KVR == BlankValueType.Mandatory ? a.IdKVR : 0),
                                IdKVSR = (blank.BlankValueType_KVSR == BlankValueType.Mandatory ? a.IdKVSR : 0),
                                IdRZPR = (blank.BlankValueType_RZPR == BlankValueType.Mandatory ? a.IdRZPR : 0)
                            },
                            b => new
                            {
                                IdExpenseObligationType = (blank.BlankValueType_ExpenseObligationType == BlankValueType.Mandatory ? b.IdExpenseObligationType : 0),
                                IdBranchCode = (blank.BlankValueType_BranchCode == BlankValueType.Mandatory ? b.IdBranchCode : 0),
                                IdCodeSubsidy = (blank.BlankValueType_CodeSubsidy == BlankValueType.Mandatory ? b.IdCodeSubsidy : 0),
                                IdDEK = (blank.BlankValueType_DEK == BlankValueType.Mandatory ? b.IdDEK : 0),
                                IdDFK = (blank.BlankValueType_DFK == BlankValueType.Mandatory ? b.IdDFK : 0),
                                IdDKR = (blank.BlankValueType_DKR == BlankValueType.Mandatory ? b.IdDKR : 0),
                                IdFinanceSource = (blank.BlankValueType_FinanceSource == BlankValueType.Mandatory ? b.IdFinanceSource : 0),
                                IdKCSR = (blank.BlankValueType_KCSR == BlankValueType.Mandatory ? b.IdKCSR : 0),
                                IdKFO = (blank.BlankValueType_KFO == BlankValueType.Mandatory ? b.IdKFO : 0),
                                IdKOSGU = (blank.BlankValueType_KOSGU == BlankValueType.Mandatory ? b.IdKOSGU : 0),
                                IdKVR = (blank.BlankValueType_KVR == BlankValueType.Mandatory ? b.IdKVR : 0),
                                IdKVSR = (blank.BlankValueType_KVSR == BlankValueType.Mandatory ? b.IdKVSR : 0),
                                IdRZPR = (blank.BlankValueType_RZPR == BlankValueType.Mandatory ? b.IdRZPR : 0)
                            }, (a, b) => a).ToList();

                        foreach (var error in ErrorLine)
                        {
                            hasErrors = true;
                            errMsg.Append("\n"
                                + (blank.BlankValueType_ExpenseObligationType == BlankValueType.Mandatory ? "��� �� - " + error.ExpenseObligationType.Caption() + "; " : "")
                                + (blank.BlankValueType_FinanceSource == BlankValueType.Mandatory ? "�������� �������������� - " + error.FinanceSource.Code + "; " : "")
                                + (blank.BlankValueType_KCSR == BlankValueType.Mandatory ? "��� - " + error.KFO.Code + "; " : "")
                                + (blank.BlankValueType_KVSR == BlankValueType.Mandatory ? "����/����/���� - " + error.KVSR.Caption + "; " : "")
                                + (blank.BlankValueType_RZPR == BlankValueType.Mandatory ? "���� - " + error.RZPR.Code + "; " : "")
                                + (blank.BlankValueType_KCSR == BlankValueType.Mandatory ? "���� - " + error.KCSR.Code + "; " : "")
                                + (blank.BlankValueType_KVR == BlankValueType.Mandatory ? "��� - " + error.KVR.Code + "; " : "")
                                + (blank.BlankValueType_KOSGU == BlankValueType.Mandatory ? "����� - " + error.KOSGU.Code + "; " : "")
                                + (blank.BlankValueType_DFK == BlankValueType.Mandatory ? "��� - " + error.DFK.Code + "; " : "")
                                + (blank.BlankValueType_DKR == BlankValueType.Mandatory ? "��� - " + error.DKR.Code + "; " : "")
                                + (blank.BlankValueType_DEK == BlankValueType.Mandatory ? "��� - " + error.DEK.Code + "; " : "")
                                + (blank.BlankValueType_CodeSubsidy == BlankValueType.Mandatory ? "��� �������� - " + error.CodeSubsidy.Code + "; " : "")
                                + (blank.BlankValueType_BranchCode == BlankValueType.Mandatory ? "���������� ��� - " + error.BranchCode.Code + "; " : "")
                                );
                        }

                        if (hasErrors)
                        {
                            docPlanActivity.IsRequireClarification = true;
                            docPlanActivity.ReasonClarification = String.Format(
                                    "�{0}. ����� ��������� ������������, ��������� �� ���������������� ����������, ��������� ������� ���������������� ������� ������������ ��� �{1}�:" +
                                    "{2}", DateTime.Now.ToShortDateString(), SBP.Parent.Caption, errMsg);
                        }

                    }
                }

                // ������������ ������� ��������

                bool hasErrorsDoc = false;
                var doc = _tpLimitAllocations.ToList();
                var errMsgDoc = new StringBuilder();

                var ErrorLineDoc = doc.Join(errors,
                     a => new
                     {
                         IdExpenseObligationType = (blank.BlankValueType_ExpenseObligationType == BlankValueType.Mandatory ? a.IdExpenseObligationType : 0),
                         IdBranchCode = (blank.BlankValueType_BranchCode == BlankValueType.Mandatory ? a.IdBranchCode : 0),
                         IdCodeSubsidy = (blank.BlankValueType_CodeSubsidy == BlankValueType.Mandatory ? a.IdCodeSubsidy : 0),
                         IdDEK = (blank.BlankValueType_DEK == BlankValueType.Mandatory ? a.IdDEK : 0),
                         IdDFK = (blank.BlankValueType_DFK == BlankValueType.Mandatory ? a.IdDFK : 0),
                         IdDKR = (blank.BlankValueType_DKR == BlankValueType.Mandatory ? a.IdDKR : 0),
                         IdFinanceSource = (blank.BlankValueType_FinanceSource == BlankValueType.Mandatory ? a.IdFinanceSource : 0),
                         IdKCSR = (blank.BlankValueType_KCSR == BlankValueType.Mandatory ? a.IdKCSR : 0),
                         IdKFO = (blank.BlankValueType_KFO == BlankValueType.Mandatory ? a.IdKFO : 0),
                         IdKOSGU = (blank.BlankValueType_KOSGU == BlankValueType.Mandatory ? a.IdKOSGU : 0),
                         IdKVR = (blank.BlankValueType_KVR == BlankValueType.Mandatory ? a.IdKVR : 0),
                         IdKVSR = (blank.BlankValueType_KVSR == BlankValueType.Mandatory ? a.IdKVSR : 0),
                         IdRZPR = (blank.BlankValueType_RZPR == BlankValueType.Mandatory ? a.IdRZPR : 0)
                     },
                     b => new
                     {
                         IdExpenseObligationType = (blank.BlankValueType_ExpenseObligationType == BlankValueType.Mandatory ? b.IdExpenseObligationType : 0),
                         IdBranchCode = (blank.BlankValueType_BranchCode == BlankValueType.Mandatory ? b.IdBranchCode : 0),
                         IdCodeSubsidy = (blank.BlankValueType_CodeSubsidy == BlankValueType.Mandatory ? b.IdCodeSubsidy : 0),
                         IdDEK = (blank.BlankValueType_DEK == BlankValueType.Mandatory ? b.IdDEK : 0),
                         IdDFK = (blank.BlankValueType_DFK == BlankValueType.Mandatory ? b.IdDFK : 0),
                         IdDKR = (blank.BlankValueType_DKR == BlankValueType.Mandatory ? b.IdDKR : 0),
                         IdFinanceSource = (blank.BlankValueType_FinanceSource == BlankValueType.Mandatory ? b.IdFinanceSource : 0),
                         IdKCSR = (blank.BlankValueType_KCSR == BlankValueType.Mandatory ? b.IdKCSR : 0),
                         IdKFO = (blank.BlankValueType_KFO == BlankValueType.Mandatory ? b.IdKFO : 0),
                         IdKOSGU = (blank.BlankValueType_KOSGU == BlankValueType.Mandatory ? b.IdKOSGU : 0),
                         IdKVR = (blank.BlankValueType_KVR == BlankValueType.Mandatory ? b.IdKVR : 0),
                         IdKVSR = (blank.BlankValueType_KVSR == BlankValueType.Mandatory ? b.IdKVSR : 0),
                         IdRZPR = (blank.BlankValueType_RZPR == BlankValueType.Mandatory ? b.IdRZPR : 0)
                     }, (a, b) => a).ToList();

                foreach (var error in ErrorLineDoc)
                {
                    hasErrorsDoc = true;
                    errMsgDoc.Append("\n"
                        + (blank.BlankValueType_ExpenseObligationType == BlankValueType.Mandatory ? "��� �� - " + error.ExpenseObligationType.Caption() + "; " : "")
                        + (blank.BlankValueType_FinanceSource == BlankValueType.Mandatory ? "�������� �������������� - " + error.FinanceSource.Code + "; " : "")
                        + (blank.BlankValueType_KCSR == BlankValueType.Mandatory ? "��� - " + error.KFO.Code + "; " : "")
                        + (blank.BlankValueType_KVSR == BlankValueType.Mandatory ? "����/����/���� - " + error.KVSR.Caption + "; " : "")
                        + (blank.BlankValueType_RZPR == BlankValueType.Mandatory ? "���� - " + error.RZPR.Code + "; " : "")
                        + (blank.BlankValueType_KCSR == BlankValueType.Mandatory ? "���� - " + error.KCSR.Code + "; " : "")
                        + (blank.BlankValueType_KVR == BlankValueType.Mandatory ? "��� - " + error.KVR.Code + "; " : "")
                        + (blank.BlankValueType_KOSGU == BlankValueType.Mandatory ? "����� - " + error.KOSGU.Code + "; " : "")
                        + (blank.BlankValueType_DFK == BlankValueType.Mandatory ? "��� - " + error.DFK.Code + "; " : "")
                        + (blank.BlankValueType_DKR == BlankValueType.Mandatory ? "��� - " + error.DKR.Code + "; " : "")
                        + (blank.BlankValueType_DEK == BlankValueType.Mandatory ? "��� - " + error.DEK.Code + "; " : "")
                        + (blank.BlankValueType_CodeSubsidy == BlankValueType.Mandatory ? "��� �������� - " + error.CodeSubsidy.Code + "; " : "")
                        + (blank.BlankValueType_BranchCode == BlankValueType.Mandatory ? "���������� ��� - " + error.BranchCode.Code + "; " : "")
                        );
                }

                

                if (hasErrorsDoc)
                {
                    this.IsRequireClarification = true;
                    this.ReasonClarification = String.Format(
                            "�{0}.  � ������� ��������� �� ��������� ������� ������� ��� ������� �������� ����� ��������� ������������:" +
                            "{1}", DateTime.Now.ToShortDateString(), errMsgDoc); ;
                }
            }
            context.SaveChanges();
        }

        private void afterControlSkip_0514(DataContext context, IEnumerable<LimitVolumeAppropriationResult> errors)
        {
            var sbpBlank = SBP.SBP_Blank.FirstOrDefault(b => b.IdBlankType == (int)BlankType.BringingGRBS);
            if (sbpBlank != null)
                foreach (var sbpId in context.SBP.Where(s => s.Id == IdSBP).Select(s => s.Id).ToList())
                {
                    SetDocumentStatusForSBP_0514(context, sbpId, errors, sbpBlank);
                }

            context.SaveChanges();
        }

        /// <summary>
        /// ��� ��������� ������������� ��������� ��������� � ��������� "������� ���������"
        /// </summary>
        /// <param name="context"></param>
        /// <param name="idSubSBP"></param>
        /// <param name="errors"></param>
        /// <param name="blank"></param>
        private void SetDocumentStatusForSBP_0514(DataContext context, int idSubSBP, IEnumerable<LimitVolumeAppropriationResult> errors, SBP_Blank blank)
        {
            var DOCs = context.ActivityOfSBP.Where(l =>
                                                                        l.IdPublicLegalFormation == IdPublicLegalFormation &&
                                                                        l.IdVersion == IdVersion &&
                                                                        !context.ActivityOfSBP.Any(a => a.IdParent == l.Id) &&
                                                                        context.ActivityOfSBP_Activity.Any(w => w.IdOwner == l.Id && w.IdSBP == idSubSBP)
                                                                        ).ToList();

            foreach (var docActivityOfSBP in DOCs)
            {
                bool hasErrors = false;
                var errMsg = new StringBuilder();

                var tpActivity = docActivityOfSBP.Activity.Where(w => w.IdSBP == idSubSBP);
                foreach (var Activity in tpActivity)
                {
                    var tpActivityResourceMaintenance = docActivityOfSBP.ActivityResourceMaintenance.Where(w => w.IdMaster == Activity.Id).ToList(); var ErrorLine = tpActivityResourceMaintenance.Join(errors,
                         a => new
                         {
                             IdExpenseObligationType = (blank.BlankValueType_ExpenseObligationType == BlankValueType.Mandatory ? a.IdExpenseObligationType : 0),
                             IdBranchCode = (blank.BlankValueType_BranchCode == BlankValueType.Mandatory ? a.IdBranchCode : 0),
                             IdCodeSubsidy = (blank.BlankValueType_CodeSubsidy == BlankValueType.Mandatory ? a.IdCodeSubsidy : 0),
                             IdDEK = (blank.BlankValueType_DEK == BlankValueType.Mandatory ? a.IdDEK : 0),
                             IdDFK = (blank.BlankValueType_DFK == BlankValueType.Mandatory ? a.IdDFK : 0),
                             IdDKR = (blank.BlankValueType_DKR == BlankValueType.Mandatory ? a.IdDKR : 0),
                             IdFinanceSource = (blank.BlankValueType_FinanceSource == BlankValueType.Mandatory ? a.IdFinanceSource : 0),
                             IdKCSR = (blank.BlankValueType_KCSR == BlankValueType.Mandatory ? a.IdKCSR : 0),
                             IdKFO = (blank.BlankValueType_KFO == BlankValueType.Mandatory ? a.IdKFO : 0),
                             IdKOSGU = (blank.BlankValueType_KOSGU == BlankValueType.Mandatory ? a.IdKOSGU : 0),
                             IdKVR = (blank.BlankValueType_KVR == BlankValueType.Mandatory ? a.IdKVR : 0),
                             IdKVSR = (blank.BlankValueType_KVSR == BlankValueType.Mandatory ? a.IdKVSR : 0),
                             IdRZPR = (blank.BlankValueType_RZPR == BlankValueType.Mandatory ? a.IdRZPR : 0)
                         },
                         b => new
                         {
                             IdExpenseObligationType = (blank.BlankValueType_ExpenseObligationType == BlankValueType.Mandatory ? b.IdExpenseObligationType : 0),
                             IdBranchCode = (blank.BlankValueType_BranchCode == BlankValueType.Mandatory ? b.IdBranchCode : 0),
                             IdCodeSubsidy = (blank.BlankValueType_CodeSubsidy == BlankValueType.Mandatory ? b.IdCodeSubsidy : 0),
                             IdDEK = (blank.BlankValueType_DEK == BlankValueType.Mandatory ? b.IdDEK : 0),
                             IdDFK = (blank.BlankValueType_DFK == BlankValueType.Mandatory ? b.IdDFK : 0),
                             IdDKR = (blank.BlankValueType_DKR == BlankValueType.Mandatory ? b.IdDKR : 0),
                             IdFinanceSource = (blank.BlankValueType_FinanceSource == BlankValueType.Mandatory ? b.IdFinanceSource : 0),
                             IdKCSR = (blank.BlankValueType_KCSR == BlankValueType.Mandatory ? b.IdKCSR : 0),
                             IdKFO = (blank.BlankValueType_KFO == BlankValueType.Mandatory ? b.IdKFO : 0),
                             IdKOSGU = (blank.BlankValueType_KOSGU == BlankValueType.Mandatory ? b.IdKOSGU : 0),
                             IdKVR = (blank.BlankValueType_KVR == BlankValueType.Mandatory ? b.IdKVR : 0),
                             IdKVSR = (blank.BlankValueType_KVSR == BlankValueType.Mandatory ? b.IdKVSR : 0),
                             IdRZPR = (blank.BlankValueType_RZPR == BlankValueType.Mandatory ? b.IdRZPR : 0)
                         }, (a, b) => a).ToList();

                    foreach (var error in ErrorLine)
                    {
                        hasErrors = true;
                        errMsg.Append("\n"
                            + (blank.BlankValueType_ExpenseObligationType == BlankValueType.Mandatory ? "��� �� - " + error.ExpenseObligationType.Caption() + "; " : "")
                            + (blank.BlankValueType_FinanceSource == BlankValueType.Mandatory ? "�������� �������������� - " + error.FinanceSource.Code + "; " : "")
                            + (blank.BlankValueType_KCSR == BlankValueType.Mandatory ? "��� - " + error.KFO.Code + "; " : "")
                            + (blank.BlankValueType_KVSR == BlankValueType.Mandatory ? "����/����/���� - " + error.KVSR.Caption + "; " : "")
                            + (blank.BlankValueType_RZPR == BlankValueType.Mandatory ? "���� - " + error.RZPR.Code + "; " : "")
                            + (blank.BlankValueType_KCSR == BlankValueType.Mandatory ? "���� - " + error.KCSR.Code + "; " : "")
                            + (blank.BlankValueType_KVR == BlankValueType.Mandatory ? "��� - " + error.KVR.Code + "; " : "")
                            + (blank.BlankValueType_KOSGU == BlankValueType.Mandatory ? "����� - " + error.KOSGU.Code + "; " : "")
                            + (blank.BlankValueType_DFK == BlankValueType.Mandatory ? "��� - " + error.DFK.Code + "; " : "")
                            + (blank.BlankValueType_DKR == BlankValueType.Mandatory ? "��� - " + error.DKR.Code + "; " : "")
                            + (blank.BlankValueType_DEK == BlankValueType.Mandatory ? "��� - " + error.DEK.Code + "; " : "")
                            + (blank.BlankValueType_CodeSubsidy == BlankValueType.Mandatory ? "��� �������� - " + error.CodeSubsidy.Code + "; " : "")
                            + (blank.BlankValueType_BranchCode == BlankValueType.Mandatory ? "���������� ��� - " + error.BranchCode.Code + "; " : "")
                            );
                    }

                }


                if (hasErrors && !context.ActivityOfSBP.Any(l => l.IdParent == docActivityOfSBP.Id))
                {
                    docActivityOfSBP.IsRequireClarification = true;
                    docActivityOfSBP.ReasonClarification = String.Format(
                            "�{0}.  ���������� �{1}� �� ��������� ������� ������ ��� ������� �������� ����� ��������� ������������ ��� �{2}�:" +
                            "{3}", DateTime.Now.ToShortDateString(), Caption, SBP.Caption, errMsg); ;
                }
            }

        }

        /// <summary>
        /// ��� ��������� ������ � ����� ������������ ��� ��������� ��� ��������� � ��������� "������� ���������"
        /// </summary>
        /// <param name="context"></param>
        /// <param name="idSubSBP"></param>
        /// <param name="errors"></param>
        /// <param name="blank"></param>
        private void SetDocumentStatusForSBP(DataContext context, int idSubSBP, IEnumerable<LimitVolumeAppropriationResult> errors, SBP_Blank blank)
        {

            // ������������ �� ����������� ������ ��������� ������������ 
            foreach (var docLimitBudgetAllocations in context.LimitBudgetAllocations.Where(l => l.IdSBP == idSubSBP &&
                                                                        l.IdPublicLegalFormation == IdPublicLegalFormation &&
                                                                        l.IdBudget == IdBudget && 
                                                                        l.IdVersion == IdVersion &&
                                                                        (l.IdDocStatus == DocStatus.Approved || l.IdDocStatus == DocStatus.Project)).ToList())
            {
                bool hasErrors = false;
                var docLimitAllocations = docLimitBudgetAllocations.LimitAllocations.ToList();
                var errMsg = new StringBuilder();

                var ErrorLine = docLimitAllocations.Join(errors,
                     a => new
                     {
                         IdExpenseObligationType = (blank.BlankValueType_ExpenseObligationType == BlankValueType.Mandatory ? a.IdExpenseObligationType : 0),
                         IdBranchCode = (blank.BlankValueType_BranchCode == BlankValueType.Mandatory ? a.IdBranchCode : 0),
                         IdCodeSubsidy = (blank.BlankValueType_CodeSubsidy == BlankValueType.Mandatory ? a.IdCodeSubsidy : 0),
                         IdDEK = (blank.BlankValueType_DEK == BlankValueType.Mandatory ? a.IdDEK : 0),
                         IdDFK = (blank.BlankValueType_DFK == BlankValueType.Mandatory ? a.IdDFK : 0),
                         IdDKR = (blank.BlankValueType_DKR == BlankValueType.Mandatory ? a.IdDKR : 0),
                         IdFinanceSource = (blank.BlankValueType_FinanceSource == BlankValueType.Mandatory ? a.IdFinanceSource : 0),
                         IdKCSR = (blank.BlankValueType_KCSR == BlankValueType.Mandatory ? a.IdKCSR : 0),
                         IdKFO = (blank.BlankValueType_KFO == BlankValueType.Mandatory ? a.IdKFO : 0),
                         IdKOSGU = (blank.BlankValueType_KOSGU == BlankValueType.Mandatory ? a.IdKOSGU : 0),
                         IdKVR = (blank.BlankValueType_KVR == BlankValueType.Mandatory ? a.IdKVR : 0),
                         IdKVSR = (blank.BlankValueType_KVSR == BlankValueType.Mandatory ? a.IdKVSR : 0),
                         IdRZPR = (blank.BlankValueType_RZPR == BlankValueType.Mandatory ? a.IdRZPR : 0)
                     },
                     b => new
                     {
                         IdExpenseObligationType = (blank.BlankValueType_ExpenseObligationType == BlankValueType.Mandatory ? b.IdExpenseObligationType : 0),
                         IdBranchCode = (blank.BlankValueType_BranchCode == BlankValueType.Mandatory ? b.IdBranchCode : 0),
                         IdCodeSubsidy = (blank.BlankValueType_CodeSubsidy == BlankValueType.Mandatory ? b.IdCodeSubsidy : 0),
                         IdDEK = (blank.BlankValueType_DEK == BlankValueType.Mandatory ? b.IdDEK : 0),
                         IdDFK = (blank.BlankValueType_DFK == BlankValueType.Mandatory ? b.IdDFK : 0),
                         IdDKR = (blank.BlankValueType_DKR == BlankValueType.Mandatory ? b.IdDKR : 0),
                         IdFinanceSource = (blank.BlankValueType_FinanceSource == BlankValueType.Mandatory ? b.IdFinanceSource : 0),
                         IdKCSR = (blank.BlankValueType_KCSR == BlankValueType.Mandatory ? b.IdKCSR : 0),
                         IdKFO = (blank.BlankValueType_KFO == BlankValueType.Mandatory ? b.IdKFO : 0),
                         IdKOSGU = (blank.BlankValueType_KOSGU == BlankValueType.Mandatory ? b.IdKOSGU : 0),
                         IdKVR = (blank.BlankValueType_KVR == BlankValueType.Mandatory ? b.IdKVR : 0),
                         IdKVSR = (blank.BlankValueType_KVSR == BlankValueType.Mandatory ? b.IdKVSR : 0),
                         IdRZPR = (blank.BlankValueType_RZPR == BlankValueType.Mandatory ? b.IdRZPR : 0)
                     }, (a, b) => a).ToList();

                foreach (var error in ErrorLine)
                {
                    hasErrors = true; 
                    errMsg.Append("\n"
                        + (blank.BlankValueType_ExpenseObligationType == BlankValueType.Mandatory ? "��� �� - " + error.ExpenseObligationType.Caption() + "; " : "")
                        + (blank.BlankValueType_FinanceSource == BlankValueType.Mandatory ? "�������� �������������� - "+error.FinanceSource.Code+"; " : "")
                        + (blank.BlankValueType_KCSR == BlankValueType.Mandatory ? "��� - " + error.KFO.Code + "; " : "")
                        + (blank.BlankValueType_KVSR == BlankValueType.Mandatory ? "����/����/���� - " + error.KVSR.Caption + "; " : "")
                        + (blank.BlankValueType_RZPR == BlankValueType.Mandatory ? "���� - " + error.RZPR.Code + "; " : "")
                        + (blank.BlankValueType_KCSR == BlankValueType.Mandatory ? "���� - " + error.KCSR.Code + "; " : "")
                        + (blank.BlankValueType_KVR == BlankValueType.Mandatory ? "��� - " + error.KVR.Code + "; " : "")
                        + (blank.BlankValueType_KOSGU == BlankValueType.Mandatory ? "����� - " + error.KOSGU.Code + "; " : "")
                        + (blank.BlankValueType_DFK == BlankValueType.Mandatory ? "��� - " + error.DFK.Code + "; " : "")
                        + (blank.BlankValueType_DKR == BlankValueType.Mandatory ? "��� - " + error.DKR.Code + "; " : "")
                        + (blank.BlankValueType_DEK == BlankValueType.Mandatory ? "��� - " + error.DEK.Code + "; " : "")
                        + (blank.BlankValueType_CodeSubsidy == BlankValueType.Mandatory ? "��� �������� - " + error.CodeSubsidy.Code + "; " : "")
                        + (blank.BlankValueType_BranchCode == BlankValueType.Mandatory ? "���������� ��� - " + error.BranchCode.Code + "; " : "")
                        );
                }

                if (hasErrors)
                {
                    docLimitBudgetAllocations.IsRequireClarification = true;
                    docLimitBudgetAllocations.ReasonClarification = String.Format(
                            "�{0}.  ���������� �{1}� �� ��������� ������� ������ ��� ������� �������� ����� ��������� ������������ ������������ ��� �{2}�:" +
                            "{3}", DateTime.Now.ToShortDateString(), Caption, SBP.Caption, errMsg); ;
                }
            }

            // ������������ �� ����� ������������  
            foreach (var docPlanActivity in context.PlanActivity.Where(l => l.IdSBP == idSubSBP &&
                                                                        l.IdPublicLegalFormation == IdPublicLegalFormation &&
                                                                        l.IdBudget == IdBudget &&
                                                                        l.IdVersion == IdVersion &&
                                                                        (l.IdDocStatus == DocStatus.Approved || l.IdDocStatus == DocStatus.Project)).ToList())
            {
                bool hasErrors = false;
                var docKBKOfFinancialProvisions = docPlanActivity.KBKOfFinancialProvisions.ToList();
                var errMsg = new StringBuilder();

                var ErrorLine = docKBKOfFinancialProvisions.Join(errors,
                    a => new
                             {
                                 IdExpenseObligationType = (blank.BlankValueType_ExpenseObligationType == BlankValueType.Mandatory ? a.IdExpenseObligationType : 0),
                                 IdBranchCode = (blank.BlankValueType_BranchCode == BlankValueType.Mandatory ? a.IdBranchCode : 0),
                                 IdCodeSubsidy = (blank.BlankValueType_CodeSubsidy == BlankValueType.Mandatory ? a.IdCodeSubsidy : 0),
                                 IdDEK = (blank.BlankValueType_DEK == BlankValueType.Mandatory ? a.IdDEK : 0),
                                 IdDFK = (blank.BlankValueType_DFK == BlankValueType.Mandatory ? a.IdDFK : 0),
                                 IdDKR = (blank.BlankValueType_DKR == BlankValueType.Mandatory ? a.IdDKR : 0),
                                 IdFinanceSource = (blank.BlankValueType_FinanceSource == BlankValueType.Mandatory ? a.IdFinanceSource : 0),
                                 IdKCSR = (blank.BlankValueType_KCSR == BlankValueType.Mandatory ? a.IdKCSR : 0),
                                 IdKFO = (blank.BlankValueType_KFO == BlankValueType.Mandatory ? a.IdKFO : 0),
                                 IdKOSGU = (blank.BlankValueType_KOSGU == BlankValueType.Mandatory ? a.IdKOSGU : 0),
                                 IdKVR = (blank.BlankValueType_KVR == BlankValueType.Mandatory ? a.IdKVR : 0),
                                 IdKVSR = (blank.BlankValueType_KVSR == BlankValueType.Mandatory ? a.IdKVSR : 0),
                                 IdRZPR = (blank.BlankValueType_RZPR == BlankValueType.Mandatory ? a.IdRZPR : 0)
                             },
                    b => new
                             {
                                 IdExpenseObligationType = (blank.BlankValueType_ExpenseObligationType == BlankValueType.Mandatory ? b.IdExpenseObligationType : 0),
                                 IdBranchCode = (blank.BlankValueType_BranchCode == BlankValueType.Mandatory ? b.IdBranchCode : 0),
                                 IdCodeSubsidy = (blank.BlankValueType_CodeSubsidy == BlankValueType.Mandatory ? b.IdCodeSubsidy : 0),
                                 IdDEK = (blank.BlankValueType_DEK == BlankValueType.Mandatory ? b.IdDEK : 0),
                                 IdDFK = (blank.BlankValueType_DFK == BlankValueType.Mandatory ? b.IdDFK : 0),
                                 IdDKR = (blank.BlankValueType_DKR == BlankValueType.Mandatory ? b.IdDKR : 0),
                                 IdFinanceSource = (blank.BlankValueType_FinanceSource == BlankValueType.Mandatory ? b.IdFinanceSource : 0),
                                 IdKCSR = (blank.BlankValueType_KCSR == BlankValueType.Mandatory ? b.IdKCSR : 0),
                                 IdKFO = (blank.BlankValueType_KFO == BlankValueType.Mandatory ? b.IdKFO : 0),
                                 IdKOSGU = (blank.BlankValueType_KOSGU == BlankValueType.Mandatory ? b.IdKOSGU : 0),
                                 IdKVR = (blank.BlankValueType_KVR == BlankValueType.Mandatory ? b.IdKVR : 0),
                                 IdKVSR = (blank.BlankValueType_KVSR == BlankValueType.Mandatory ? b.IdKVSR : 0),
                                 IdRZPR = (blank.BlankValueType_RZPR == BlankValueType.Mandatory ? b.IdRZPR : 0)
                             }, (a, b) => a).ToList();

                foreach (var error in ErrorLine)
                {
                    hasErrors = true;
                    errMsg.Append("\n"
                        + (blank.BlankValueType_ExpenseObligationType == BlankValueType.Mandatory ? "��� �� - " + error.ExpenseObligationType.Caption() + "; " : "")
                        + (blank.BlankValueType_FinanceSource == BlankValueType.Mandatory ? "�������� �������������� - " + error.FinanceSource.Code + "; " : "")
                        + (blank.BlankValueType_KCSR == BlankValueType.Mandatory ? "��� - " + error.KFO.Code + "; " : "")
                        + (blank.BlankValueType_KVSR == BlankValueType.Mandatory ? "����/����/���� - " + error.KVSR.Caption + "; " : "")
                        + (blank.BlankValueType_RZPR == BlankValueType.Mandatory ? "���� - " + error.RZPR.Code + "; " : "")
                        + (blank.BlankValueType_KCSR == BlankValueType.Mandatory ? "���� - " + error.KCSR.Code + "; " : "")
                        + (blank.BlankValueType_KVR == BlankValueType.Mandatory ? "��� - " + error.KVR.Code + "; " : "")
                        + (blank.BlankValueType_KOSGU == BlankValueType.Mandatory ? "����� - " + error.KOSGU.Code + "; " : "")
                        + (blank.BlankValueType_DFK == BlankValueType.Mandatory ? "��� - " + error.DFK.Code + "; " : "")
                        + (blank.BlankValueType_DKR == BlankValueType.Mandatory ? "��� - " + error.DKR.Code + "; " : "")
                        + (blank.BlankValueType_DEK == BlankValueType.Mandatory ? "��� - " + error.DEK.Code + "; " : "")
                        + (blank.BlankValueType_CodeSubsidy == BlankValueType.Mandatory ? "��� �������� - " + error.CodeSubsidy.Code + "; " : "")
                        + (blank.BlankValueType_BranchCode == BlankValueType.Mandatory ? "���������� ��� - " + error.BranchCode.Code + "; " : "")
                        );
                }

                if (hasErrors)
                {
                    docPlanActivity.IsRequireClarification = true;
                    docPlanActivity.ReasonClarification = String.Format(
                            "�{0}.  ���������� �{1}� �� ��������� ������� ������ ��� ������� �������� ����� ��������� ������������ ������������ ��� �{2}�:" +
                            "{3}", DateTime.Now.ToShortDateString(), Caption, SBP.Caption, errMsg); ;
                }

            }
        }

        #endregion

        /// <summary>   
        /// �������� ���������   
        /// </summary>  
        public void Create(DataContext context)
        {
            ExecuteControl(e => e.Control_0501(context));
            ExecuteControl(e => e.Control_0502(context));

            SetBlankActual(context);

            var error = true;
            do
            {
                try
                {
                    var sc = context.LimitBudgetAllocations
                        .Where(w => w.IdPublicLegalFormation == IdPublicLegalFormation && !w.IdParent.HasValue)
                        .Select(s => s.Number).Distinct().ToList();
                    Number = sc.GetNextCode();

                    context.SaveChanges();
                    error = false;
                }
                catch (Exception e)
                {
                    if (!e.Message.Contains("�����"))
                    {
                        throw;
                    }
                }
            } while (error);
        }

        /// <summary>   
        /// �������� ���������������   
        /// </summary>  
        public void BeforeEdit(DataContext context)
        {
            ExecuteControl(e => e.Control_0523(context));

            if (SetBlankActual(context))
            {
                TrimKbkByNewActualBlank(context);
                context.SaveChanges();
            }

        }
        public void Edit(DataContext context)
        {
            ExecuteControl(e => e.Control_0503(context));
            ExecuteControl(e => e.Control_0515(context));

            DateLastEdit = DateTime.Now;

            ControlRelation_FillData(context);
        }

        /// <summary>   
        /// �������� ������������   
        /// </summary>  
        public void Process(DataContext context)
        {
            ExecuteControl(e => e.Control_0504(context));
            ExecuteControl(e => e.Control_0505(context));
            ExecuteControl(e => e.Control_0507(context));
            ExecuteControl(e => e.Control_0508(context));
            ExecuteControl(e => e.Control_0509(context));
            ExecuteControl(e => e.Control_0510(context));
            ExecuteControl(e => e.Control_0511(context));
            ExecuteControl(e => e.Control_0512(context));
            ExecuteControl(e => e.Control_0514(context));
            ExecuteControl(e => e.Control_0520(context));
            ExecuteControl(e => e.Control_0521(context));
            ExecuteControl(e => e.Control_0522(context));

            DateCommit = DateTime.Now.Date;

            WriteToRegistry(context);

            if (IdParent.HasValue)
            {
                var prevDoc = GetPrevVersionDoc(context, IdParent);
                if (prevDoc != null)
                {
                    prevDoc.ExecuteOperation(e => e.Archive(context));
                }
            }
        }

        /// <summary>
        /// �������� ��������� ���������
        /// </summary>  
        public void UndoProcess(DataContext context)
        {
            ExecuteControl(e => e.Control_0513(context));

            //��� ����������, ��������� � ���� ����������� ���������, ��������� �������� ��������� �������� ��������� (�������)�.
            if (IdParent.HasValue && Parent != null)
                Parent.UndoProcessChange(context);

            RemoveFromRegistry(context);

            //���������� ����� �������� ���������� = ����, ���������� = ����, �������� ���� �������� ����������.
            IsRequireClarification = false;
            DateCommit = null;
            ReasonClarification = null;
        }

        /// <summary>   
        /// �������� �����������   
        /// </summary>  
        public void Confirm(DataContext context)
        {
            ExecuteControl(e => e.Control_0516(context));
            ExecuteControl(e => e.Control_0519(context));

            if (IsAdditionalNeed??false)
            {
                //	�������� ������ �������� ��������� �� ������.
                IdDocStatus = DocStatus.Archive;

                //������� ����� �������� ��������� (�����������) � ��� �� ����� ��������� � ���������� �����������, �� �����������:
                //- ���� ������ ���. ����������� ���������� = ����;
                //- ���� ����������� ��������� - ���������� ������ �� ������� ��������;
                //- ������� ���������� = ������;
                //- �������� ���� ����. ������������ � �� ����������� ������ ��������� ������������
                // - ������ ��������� = ���������.

                var cloner = new Clone(this);
                var newDoc = (LimitBudgetAllocations)cloner.GetResult();

                newDoc.IsAdditionalNeed = false;
                newDoc.IdParent = Id;
                newDoc.IsApproved = true;
                newDoc.IdDocStatus = DocStatus.Approved;
                newDoc.Number = this.GetDocNextNumber(context);
                context.Entry(newDoc).State = EntityState.Added;
                newDoc.Caption = newDoc.ToString();

                context.SaveChanges();

                context.LimitBudgetAllocations_LimitAllocations.Update(u => u.IdOwner == newDoc.Id,
                    u => new LimitBudgetAllocations_LimitAllocations
                    {
                        AdditionalNeedOFG = 0,
                        AdditionalNeedPFG1 = 0,
                        AdditionalNeedPFG2 = 0
                    });
                context.SaveChanges();

                var ids = GetIdAllVersionDoc(context);

                //�������� ��������� �� ����� ������������ ���������.
                //����� � ��������� ������� ���������� ������� ��� ������, � �������
                //�	����������� � ������� �������� ��� ��������-������ (�� ������� ����������)
                //�	���. ����������� = ����
                //�	� ���� ����������� = �����.
                //�� ���� ��������� ������� ���������� ���� ����������� = ��������������.����, ������������ �������� = ������� ��������.


                context.LimitVolumeAppropriations.Update(w => ids.Contains(w.IdRegistrator) && w.IdRegistratorEntity == EntityId  && w.DateCommit == null && w.HasAdditionalNeed == false,
                u => new LimitVolumeAppropriations
                {
                    DateCommit = newDoc.Date,
                    IdApprovedEntity = EntityId,
                    IdApproved = newDoc.Id
                });

                //�������� ��������� �� ����� ������������ ���������.
                //4.	����� � ��������� ������� ���������� ������� ��� ������, � �������
                //�	����������� � ������� �������� ��� ��������-������ (�� ������� ����������)
                //�	���. ����������� = ������
                //�	� ���� ����������� = �����.
                //������������ ��������� ������  ����� �� ���� ������������ � ������������ �������, � ������� ����������� ������� �������� ��� �������� ������ 
                //(�� ������� ����������), ���. ����������� = ������ � ���� ����������� = �����  ���������� ���� ����������� = ��������������.����, 
                //������������ �������� = ������� ��������

                // ������������ ��������
                var LVAs =
                    context.LimitVolumeAppropriations.Where(
                        w =>
                        ids.Contains(w.IdRegistrator) && w.IdRegistratorEntity == EntityId && w.HasAdditionalNeed == true &&
                        w.DateCommit == null).ToList();
                foreach (var LVA in LVAs)
                {
                    LVA.IdRegistrator = newDoc.Id;
                    LVA.IdRegistratorEntity = EntityId;
                    LVA.IdApproved = newDoc.Id;
                    LVA.IdApprovedEntity = EntityId;
                    LVA.DateCommit = newDoc.Date;
                    LVA.Value = -1 * LVA.Value;
                    context.LimitVolumeAppropriations.Add(LVA);                    
                }

                // ��������� ������������ ��������
                context.LimitVolumeAppropriations.Update(
                        w =>
                        ids.Contains(w.IdRegistrator) && w.IdRegistratorEntity == EntityId && w.HasAdditionalNeed == true &&
                        w.DateCommit == null,
                    u => new LimitVolumeAppropriations
                    {
                        DateCommit = newDoc.Date,
                        IdApprovedEntity = EntityId,
                        IdApproved = newDoc.Id
                    });

            }
            else
            {
                //	���������� ������ ���������� = ������
                IsApproved = true;

                //	���������� ������ �������� ��������� ����������
                IdDocStatus = DocStatus.Approved;

                //����� � �������� ������� ���������� ������� ��� ������, � �������
                //    ����������� � ������� �������� ��� ��������-������ (�� ������� ����������) � ���� ����������� = �����.
                //    �� ���� ��������� ������� ���������� ���� ����������� = �����.���� ���������, ������������ �������� = ������� ��������.


                context.LimitVolumeAppropriations.Update(w => w.IdRegistrator == Id && w.IdRegistratorEntity == EntityId && w.DateCommit == null,
                u => new LimitVolumeAppropriations
                    {
                        DateCommit = Date,
                        IdApprovedEntity = EntityId,
                        IdApproved = Id
                    });

                if (IdParent.HasValue)
                {
                    var prevDoc = GetPrevVersionDoc(context, IdParent);

                    context.LimitVolumeAppropriations.Update(w => w.IdRegistrator == prevDoc.Id && w.IdRegistratorEntity == EntityId && w.DateCommit == null,
                    u => new LimitVolumeAppropriations
                        {
                            DateCommit = Date,
                            IdApprovedEntity = EntityId,
                            IdApproved = Id
                        });
                }



            }
        }

        /// <summary>
        /// �������� ��������� �����������
        /// </summary>  
        public void UndoConfirm(DataContext context)
        {
            //�� ���� ������� �������� ������� ���������� �������,  � ������� ������������ �������� = ������� ��������, 
            //�������� ���� ����� ������������ � ������������� ��������.

            context.LimitVolumeAppropriations.Update(w => w.IdApproved == Id,
            u => new LimitVolumeAppropriations
            {
                DateCommit = null,
                IdApprovedEntity = null,
                IdApproved = null
            });

            IsApproved = false;
        }

        /// <summary>   
        /// �������� ���������� � ���. �������������   
        /// </summary>  
        public void ConfirmWithAddNeed(DataContext context)
        {
            ExecuteControl(e => e.Control_0517(context));
            ExecuteControl(e => e.Control_0518(context));
            ExecuteControl(e => e.Control_0519(context));

            IdDocStatus = DocStatus.Archive;

            //������� ����� �������� ��������� (�����������) � ��� �� ����� ��������� � ���������� �����������, �� �����������:
            //- ���� ������ ���. ����������� ���������� = ����;
            //- ���� ����������� ��������� - ���������� ������ �� ������� ��������;
            //- ������� ���������� = ������;
            //- ����������� ���� ����. ������������� � �� ����������� ������ ��������� ������������ � ������ �������� ��� �� ����� �� ��������������� �������, ����� ���������� ������� �������� � ����� ����. ������������.
            //- ������ ��������� = ���������.


            var cloner = new Clone(this);
            var newDoc = (LimitBudgetAllocations)cloner.GetResult();

            newDoc.IsAdditionalNeed = false;
            newDoc.IdParent = Id;
            newDoc.IsApproved = true;
            newDoc.IdDocStatus = DocStatus.Approved;
            newDoc.Number = this.GetDocNextNumber(context);
            newDoc.Caption = newDoc.ToString();
            context.Entry(newDoc).State = EntityState.Added;

            context.SaveChanges();



            foreach (var allocation in newDoc.LimitAllocations)
            {
                allocation.OFG = (allocation.OFG ?? 0) + (allocation.AdditionalNeedOFG ?? 0);
                allocation.PFG1 = (allocation.PFG1 ?? 0) + (allocation.AdditionalNeedPFG1 ?? 0);
                allocation.PFG2 = (allocation.PFG2 ?? 0) + (allocation.AdditionalNeedPFG2 ?? 0);
                allocation.AdditionalNeedOFG = 0;
                allocation.AdditionalNeedPFG1 = 0;
                allocation.AdditionalNeedPFG2 = 0;
            };
 
            context.SaveChanges();

            newDoc.WriteToRegistry(context);

            var ids = GetIdAllVersionDoc(context);

            //����� � ��������� ������� ���������� ������� ��� ������, � �������
            //�	����������� � ����������� �������� ��� ��������-������ (�� ������� ����������)
            //�	� ���� ����������� = �����.
            //�� ���� ��������� ������� ���������� ���� ����������� = ��������������.����, ������������ �������� = ����������� ��������.

            context.LimitVolumeAppropriations.Update(w => ids.Contains(w.IdRegistrator) && w.IdRegistratorEntity == EntityId && w.DateCommit == null,
             u => new LimitVolumeAppropriations
             {
                 DateCommit = newDoc.Date,
                 IdApprovedEntity = EntityId,
                 IdApproved = newDoc.Id
             });

        }

        /// <summary>   
        /// �������� ����������   
        /// </summary>  
        public void Change(DataContext context)
        {
            var cloner = new Clone(this);
            var newDoc = (LimitBudgetAllocations)cloner.GetResult();

            newDoc.IdDocStatus = DocStatus.Draft;
            newDoc.IdParent = Id;
            newDoc.IdCompareWithDocument = Id;
            newDoc.IsRequireClarification = false;
            newDoc.DateCommit = null;
            newDoc.ReasonClarification = null;
            newDoc.DateLastEdit = null;
            context.Entry(newDoc).State = EntityState.Added;
            newDoc.Number = this.GetDocNextNumber(context);
            newDoc.Caption = newDoc.ToString();
            context.SaveChanges();
        }

        /// <summary>   
        /// �������� ��������� ���������   
        /// </summary>
        public void UndoChange(DataContext context)
        {;
            //���� � �������� �������� ���������� ������ ����������, �� ������������ � ������ ����������, �����  � ������ �������.
            IdDocStatus = (IsApproved ?? false) ? DocStatus.Approved : DocStatus.Project;

            //������� �������� ��������
            var q = context.LimitBudgetAllocations.Where(w => w.IdParent == Id);
            foreach (var doc in q)
            {
                context.LimitBudgetAllocations.Remove(doc);
            }

            IdDocStatus = DocStatus.Approved;

            context.SaveChanges();
        }

        /// <summary>   
        /// �������� ��������� �������� ��������� (�������)�   
        /// </summary>
        public void UndoProcessChange(DataContext context)
        {
            IdDocStatus = DocStatus.Changed;
        }

        /// <summary>   
        /// �������� �� �����   
        /// </summary>  
        public void Archive(DataContext context)
        {
            IdDocStatus = DocStatus.Archive;
        }

        #endregion

        #region ��������


        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        [Control(ControlType.Insert | ControlType.Update, Sequence.After, ExecutionOrder = -1000)]
        public void AutoSet(DataContext context)
        {
            Caption = ToString();
        }

        /// <summary>
        /// �������� ������������ ���������
        /// </summary>
        /// <param name="context"></param>
        [ControlInitial(InitialUNK = "0501", InitialCaption = "�������� ������������ ���������")]
        public void Control_0501(DataContext context)
        {
            if (IdParent.HasValue)
                return;

            var obj =
                context.LimitBudgetAllocations.
                                Where(a =>
                                            a.Id != Id
                                            && a.IdPublicLegalFormation == IdPublicLegalFormation
                                            && a.IdBudget == IdBudget
                                            && a.IdVersion == IdVersion
                                            && a.IdSBP == IdSBP)
                                .GroupJoin(context.LimitBudgetAllocations, lp => lp.Id,
                                                                  lo => lo.IdParent, (lp, templba) => new { lp, templba })
                                .SelectMany(@t => @t.templba.DefaultIfEmpty(), (@t, l) => new { @t, l })
                                .Where(@t => @t.l == null)
                                .Select(@t => @t.@t.lp)
                                .FirstOrDefault();

            if (obj != null)
                Controls.Throw(string.Format(
                    "� ������� ��� ���������� �������� �{0}� � ������� �{1}� ��� ��� �{2}�<br>{0} �{3} �� {4}",
                    obj.EntityCaption,
                    obj.Version.Caption,
                    obj.SBP.Caption,
                    obj.Number,
                    obj.Date.ToShortDateString()
                ));
        }

        /// <summary>
        /// �������� ������� ������ ���������
        /// </summary>
        /// <param name="context"></param>
        /// [Control("0502")]
        [ControlInitial(InitialUNK = "0502", InitialCaption = "�������� ������� ������ ���������")]
        public void Control_0502(DataContext context)
        {
            if (SBP.SBPType == DbEnums.SBPType.GeneralManager)
            {
                var obj = SBP.SBP_Blank.FirstOrDefault(b => b.BlankType == BlankType.BringingGRBS && b.IdBudget == IdBudget);

                if (obj == null)
                    Controls.Throw(string.Format(
                        "� ����������� ��������� ���������� ������������� � �������� �{0}� ����������� ����� ���������� ���ѻ.",
                        SBP.Caption));
            }
            else if (SBP.SBPType == DbEnums.SBPType.Manager)
            {
                var parent = SBP.Parent;

                if (parent != null)
                {
                    var obj = parent.SBP_Blank.FirstOrDefault(b => b.BlankType == BlankType.BringingRBS && b.IdBudget == IdBudget);

                    if (obj == null)
                        Controls.Throw(string.Format(
                            "� ����������� ��������� ���������� ������������� � �������� �{0}� ����������� ����� ���������� ��ѻ.",
                            parent.Caption));
                }
            }


        }

        /// <summary>
        /// �������� ���� ���������-����� 
        /// </summary>
        /// <param name="context"></param>
        [ControlInitial(InitialUNK = "0503", InitialCaption = "�������� ���� ���������-����� ")]
        public void Control_0503(DataContext context)
        {
            if (Parent == null)
                return;

            if (Parent.Date > Date)
                Controls.Throw(string.Format(
                    "���� ��������� �� ����� ���� ������ ���� ���������� ��������. " +
                    "���� �������� ���������: {0}" +
                    "���� ���������� ��������: {1}.",
                    Date.ToShortDateString(),
                    Parent.Date.ToShortDateString()));
        }

        /// <summary>
        /// �������� ���� ��������� ������������ ��� � ����� ��������� ������������ ���
        /// </summary>
        /// <param name="context"></param>
        [ControlInitial(InitialUNK = "0504", InitialCaption = "�������� ���� ��������� ������������ ��� � ����� ��������� ������������ ���")]
        public void Control_0504(DataContext context)
        {
            //���� � ���, ���������� � ����� ���������, ���� ����������� ���, 
            if (SBP == null || !SBP.IdParent.HasValue)
                return;
            //����� �������� ������, ��������� �� ������������ ���, � �������: ���������� ��� ��������
            var document = context.LimitBudgetAllocations.FirstOrDefault(l =>
                                    l.IdSBP == SBP.IdParent &&
                                    (l.IdDocStatus == DocStatus.Approved || l.IdDocStatus == DocStatus.Changed));
            if (document == null)
                return;

            //���� �������� ������������ ���.���� > ������� ��������.����, �� �������� �� ��������� � ������ ���������:
            if (document.Date > Date)
                Controls.Throw(string.Format(
                    "���������� ��������������� ���� �������� ���������. <br/>" +
                    "�������� ����������� ������ ��������� ������������ ������������ ���������� ��� ��������� ����� ������� �����:<br/>" +
                    "�{0}�. <br/>" +
                    "���� �������� ���������: {1}",
                    document.Caption,
                    Date.ToShortDateString()));

        }

        /// <summary>
        /// �������� ������� ������� ������
        /// </summary>
        /// <param name="context"></param>
        [ControlInitial(InitialUNK = "0505", InitialCaption = "�������� ������� ������� ������")]
        public void Control_0505(DataContext context)
        {
            //���������, ���� �� ���� �� ���� ������ � �� ����������� ������ ��������� ������������.
            //���� ��� �� ����� ������, �� �������� �� ��������� � ������ ���������

            if (!context.LimitBudgetAllocations_LimitAllocations.Any(a => a.IdOwner == Id))
                Controls.Throw("� ������� ����������� ������ ��������� ������������ �� ������� �� ���� ������.");
        }


        /// <summary>
        /// �������� ���������� ����� � ������������ � ������� ���������
        /// </summary>
        /// <param name="context"></param>
        [ControlInitial(InitialUNK = "0507", InitialCaption = "�������� ���������� ����� � ������������ � ������� ���������")]
        public void Control_0507(DataContext context)
        {
            //��������� ������� ������ � �� ����������� ������ ��������� ������������ 
            // �� ������������ ���������� ����� �� � ������������ � ������� ���������� (��. ����������).
            //���� ���������� ��������������,  �� �������� �� ��������� � ������ ���������: 
            var blank = SBPBlank;

            //* ����������� � Control_0502 *//
            if (blank == null) return;

            var limits = context.LimitBudgetAllocations_LimitAllocations.Where(l => l.IdOwner == Id).ToList();

            if (limits.Any(limit => !blank.CheckByBlank(limit)))
            {
                Controls.Throw(
                    String.Format("� ������� ����������� ������ ��������� ������������ ������� ������, �� ��������������� ������ �{0}�.",
                    blank.BlankType.Caption()
                    ));
            }
        }

        /// <summary>
        /// �������� ���������� ����� ������������ ����������
        /// </summary>
        /// <param name="context"></param>
        [ControlInitial(InitialUNK = "0508", InitialCaption = "�������� ���������� ����� ������������ ����������")]
        public void Control_0508(DataContext context)
        {
            /*��������� ���������� ���� ����� ��������� �������� ���������� ������������� � ��� �������� ����� ���, 
             * ����� ��������� ��������������� � ���� �λ, ����� �� ����������� ������ ��������� ������������ �� ������������. 
            
             * �������:
                ���� ������ �������� �������� < = ����� ���������.���� ���� �����.
                ���� ��������� �������� �������� ����������� > ����� ���������.����, ���� �����.
                ��� �������� ����������� = ��� ���������.
            
             * ���� ������� �� �����������, �������� �� ���������, ������ ��������� */

            if (SBP.ValidityFrom > Date || SBP.ValidityTo < Date || SBP.IdPublicLegalFormation != IdPublicLegalFormation)
                Controls.Throw("� ���� �������� ���������� ������������� ������ ������������� �������.");

            var limitErrors = context.LimitBudgetAllocations_LimitAllocations
                                     .Where(l => l.IdOwner == Id).ToList()
                                     .Select(l => l.CheckActual(this))
                                     .Where(l => !String.IsNullOrEmpty(l)).ToList();

            if (limitErrors.Any())
            {
                var msg = new StringBuilder();
                foreach (var limitError in limitErrors)
                    msg.AppendFormat(" - {0}<br/>", limitError);

                Controls.Throw(
                    String.Format("� ������� ������� ����������� ������ ��������� ������������ ������� ������������� ��� (�������� ������ �������): <br/> {0}", msg));
            }
        }


        /// <summary>
        /// �������� ������� ����� � �������� �������
        /// </summary>
        /// <param name="context"></param>
        [ControlInitial(InitialUNK = "0509", InitialCaption = "�������� ������� ����� � �������� �������")]
        public void Control_0509(DataContext context)
        {
            /*���������, ����� ��� ������ ������ �� ����������� ������ ��������� ������������ ���� ������� ����� > 0 ���� �� �� ���� ��� �������. 
              ���� ������� �� ���������, �� �������� �� ��������� � ������ ��������� */

            var limitErrors = context.LimitBudgetAllocations_LimitAllocations
                                   .Where(l => l.IdOwner == Id).ToList()
                                   .Select(l => l.CheckNotNullSum(context))
                                   .Where(l => !String.IsNullOrEmpty(l)).ToList();

            if (limitErrors.Any())
            {
                var msg = new StringBuilder();
                foreach (var limitError in limitErrors)
                    msg.AppendFormat(" - {0}<br/>", limitError);

                Controls.Throw(
                    String.Format("� ������� ����������� ������ ��������� ������������ ������� ������ � �������� �������:<br/>{0}", msg));
            }
        }


        /// <summary>
        /// �������� ���� �� ���������������
        /// </summary>
        /// <param name="context"></param>
        [ControlInitial(InitialUNK = "0510", InitialCaption = "�������� ���� �� ���������������")]
        public void Control_0510(DataContext context)
        {
            //���������, ����� �������� ���� ������ ������ �� ����������� ������ ��������� ������������ ���� >= 0.

            var limitErrors = context.LimitBudgetAllocations_LimitAllocations
                                 .Where(l => l.IdOwner == Id).ToList()
                                 .Select(l => l.CheckPositiveSum(context))
                                 .Where(l => !String.IsNullOrEmpty(l)).ToList();

            if (limitErrors.Any())
            {
                var msg = new StringBuilder();
                foreach (var limitError in limitErrors)
                    msg.AppendFormat(" - {0}<br/>", limitError);

                Controls.Throw(
                    String.Format("� ������� ����������� ������ ��������� ������������ ������� ������ � �������������� �������:<br/>{0}", msg));
            }
        }

        /// <summary>
        /// �������� ���������� ������ ������������, ���������� �� ���������������� ����������, ��� �������� ���������������� ������������ ������������ ���
        /// </summary>
        /// <param name="context"></param>
        [ControlInitial(InitialUNK = "0511", InitialCaption = "�������� ���������� ������ ������������, ���������� �� ���������������� ����������, ��� �������� ���������������� ������������ ������������ ���")]
        public void Control_0511(DataContext context)
        {
            //�������� ��������� � ��� ������, ���� � ���, ���������� � ����� ���������, ���� ����������� ���
            if (SBP == null || !SBP.IdParent.HasValue)
                return;

            var parentSBP = SBP.Parent;
            if (parentSBP == null)
                return;

            //���� � ������ ��������� ������������ ��� ����� ������ ��� ������������, 
            //�� �������� �� �������� ������� ���������� ������� ����� �� ����� � ����� �������� ����� 
            //(� ������� ����� ������������ � ������ ���, ������, ������) � ���= ����������� ��� ��� ��� ���������.
            var parentSBPBlank = parentSBP.SBP_Blank.FirstOrDefault(b => b.IdBudget == IdBudget && b.BlankType == BlankType.BringingGRBS);
            if (parentSBPBlank == null)
                return;

            var idKosgu000 = parentSBPBlank.BlankValueType_KOSGU == BlankValueType.Mandatory ? (int?)context.KOSGU.Where(k=>k.Code == "000").Select(k=>k.Id).FirstOrDefault() : null;
            if (idKosgu000.HasValue && !context.LimitVolumeAppropriations.Any(l=>l.IdPublicLegalFormation == IdPublicLegalFormation && l.IdBudget == IdBudget && l.IdVersion == IdVersion))
                idKosgu000 = null;

            var limitErrors = CheckLimitAllocations0511(context, parentSBPBlank, idKosgu000).ToList();

            if (limitErrors.Any())
            {
                var msg = new StringBuilder();
                foreach (var limitError in limitErrors)
                    msg.AppendFormat(" - {0}<br/>", limitError.Value);

                var errorEstimatedLineResults = limitErrors.Select(l => l.Key).ToList();

                Controls.Throw(
                    String.Format(
                        "����� ��������� ������������, ��������� �� ���������������� ����������, ��������� ������� ���������������� ������� ������������ ��� �{0}�:<br/>{1}",
                        parentSBP.Caption, msg));

                afterControlSkip_0511(context, errorEstimatedLineResults);
            }
            else
            {
                //    ���� �������� ��� 0511 ���������� ������� (�.�. �� ���������� ����������), �� ��������� ��������� ��������:
                //    �	����� ��������� ����������� ������ ��������� ������������ � ����� ������������, � ������� ���ϻ ��������� �� ����� ������ � ��� �� ����� ���������, �.�. � ��� ���� � ��� �� ��������, � ����� �������� ������ �� ���-��������, ��� ��������������.��� (� ������ ���, ������, ������)
                //    �	� ��������� ���������� ������ � ����� ������������, � ����� � ������� ��������� ������ ������ �������� ���������� � �������� ���� �������� ����������

                // ������������ �� ����������� ������ ��������� ������������ 
                foreach (var sbpId in context.SBP.Where(s => s.IdParent == SBP.IdParent).Select(s => s.Id).Union(context.SBP.Where(w => w.Id == SBP.IdParent).Select(s => s.Id)).ToList())
                {

                    foreach (var docLimitBudgetAllocations in context.LimitBudgetAllocations.Where(l => l.IdSBP == sbpId &&
                                                                                l.IdPublicLegalFormation == IdPublicLegalFormation &&
                                                                                l.IdBudget == IdBudget &&
                                                                                l.IdVersion == IdVersion).ToList())
                    {
                        docLimitBudgetAllocations.IsRequireClarification = false;
                        docLimitBudgetAllocations.ReasonClarification = "";
                    }
                    
                }

                // ������������ �� ����� ������������  

                foreach (var sbpId in context.SBP.Where(s => s.IdParent == SBP.IdParent).Select(s => s.Id).ToList())
                {

                    foreach (var docPlanActivity in context.PlanActivity.Where(l => l.IdSBP == sbpId &&
                                                                                l.IdPublicLegalFormation == IdPublicLegalFormation &&
                                                                                l.IdBudget == IdBudget &&
                                                                                l.IdVersion == IdVersion
                                                                                ).ToList())
                    {
                            docPlanActivity.IsRequireClarification = false;
                        docPlanActivity.ReasonClarification = "";
                    }
                }

                // ������������ ������� ��������
                IsRequireClarification = false;
                ReasonClarification = "";

            }
        }

        /// <summary>
        /// �������� ���������� ������ ������������, ����������� �� ���������������� ����������, ��� �������� ������� ������������ ������������ ���
        /// </summary>
        /// <param name="context"></param>
        [ControlInitial(InitialUNK = "0512", InitialSkippable = false, InitialCaption = "�������� ���������� ������ ������������, ����������� �� ���������������� ����������, ��� �������� ������� ������������ ������������ ���")]
        public void Control_0512(DataContext context)
        {
            var blank = SBPBlank;
            if (blank == null)
                return;

            var limitErrors = CheckLimitAllocations0512(context, blank).ToList();

            if (limitErrors.Any())
            {
                var msg = new StringBuilder();
                foreach (var limitError in limitErrors)
                    msg.AppendFormat(" - {0}<br/>", limitError.Value);

                var errorEstimatedLine0512Results = limitErrors.Select(l => l.Key).ToList();

                Controls.Throw(
                    String.Format("����� ��������� ������������, ���������� �� ���������������� ����������, ��������� �������� ����� ��������� ������������ ������������ ���  �{0}�:<br/>{1}", SBP.Caption, msg));

                afterControlSkip_0512(context, errorEstimatedLine0512Results);
            }
        }

        /// <summary>
        /// �������� ���������� ������ ������������, ����������� �� ���������������� ����������, ��� �������� ������� ������������ 
        /// </summary>
        /// <param name="context"></param>
        [ControlInitial(InitialUNK = "0513", InitialCaption = "�������� ���������� ������ ������������, ����������� �� ���������������� ����������, ��� �������� ������� ������������ ������������ ���")]
        public void Control_0513(DataContext context)
        {
            var blank = SBPBlank;
            if (blank == null)
                return;

            if (context.LimitVolumeAppropriations.Any(l => l.IdValueType == (byte)ValueType.Bring &&
                                                                 l.IdPublicLegalFormation == IdPublicLegalFormation &&
                                                                 l.IdBudget == IdBudget &&
                                                                 l.IdVersion == IdVersion &&
                                                                 l.EstimatedLine.IdSBP == IdSBP))
                Controls.Throw("�������� �� ���������. <br/> ����� ������ ��������� ������������ �� ������� ��������� ��� �������� �� ���������������� ����������.");
        }

        /// <summary>
        /// �������� ���������� ������ ������������ ��������� ������������, ��� ������� ���������� ��������� ������������ �������� ��� 
        /// </summary>
        /// <param name="context"></param>
        [ControlInitial(InitialUNK = "0514", InitialCaption = "�������� ���������� ������ ������������ ��������� ������������, ��� ������� ���������� ��������� ������������ �������� ���")]
        public void Control_0514(DataContext context)
        {
            var errorBlank = "";
            var sbpBlankBringing = new SBP_Blank();
            var sbpBlankFormation = new SBP_Blank();

            if (SBP.SBPType == DbEnums.SBPType.GeneralManager)
            {
                sbpBlankBringing = SBP.SBP_Blank.FirstOrDefault(b => b.BlankType == BlankType.BringingGRBS && b.IdBudget == IdBudget);
                //if (sbpBlankBringing == null)
                //{
                //    errorBlank = errorBlank +
                //                 String.Format("��� ���: �{0}� �� ����� ����� ��������� ���� <br/>",
                //                               SBP.Caption);
                //}
                //sbpBlankFormation = SBP.SBP_Blank.FirstOrDefault(b => b.BlankType == BlankType.FormationGRBS && b.IdBudget == IdBudget);
                //if (sbpBlankFormation == null)
                //{
                //    errorBlank = errorBlank +
                //                 String.Format("��� ���: �{0}� �� ����� ����� ������������ ���� <br/>",
                //                               SBP.Caption);
                //}
            }
            if (SBP.SBPType == DbEnums.SBPType.Manager)
            {
                var parent = SBP.Parent;

                if (parent != null)
                {
                    sbpBlankBringing = parent.SBP_Blank.FirstOrDefault(b => b.BlankType == BlankType.BringingRBS && b.IdBudget == IdBudget);
                    //if (sbpBlankBringing == null)
                    //{
                    //    errorBlank = errorBlank +
                    //                 String.Format("��� ������������ ���: �{0}� �� ����� ����� ��������� ��� <br/>",
                    //                               parent.Caption);
                    //}
                    sbpBlankFormation = parent.SBP_Blank.FirstOrDefault(b => b.BlankType == BlankType.FormationGRBS && b.IdBudget == IdBudget);
                    //if (sbpBlankFormation == null)
                    //{
                    //    errorBlank = errorBlank +
                    //                 String.Format("��� ������������ ���: �{0}� �� ����� ����� ������������ ���� <br/>",
                    //                               parent.Caption);
                    //}

                }
            }


            if (errorBlank.Any())
            {
                Controls.Throw(errorBlank);
                return;
            }

            //�������� ����� ������������ ����� �������
            var costLineProperties = new List<string>();
            var costLinePropertiesBringing = sbpBlankBringing.GetBlankCostMandatoryProperties().ToList();
            var costLinePropertiesFormation = sbpBlankFormation.GetBlankCostMandatoryProperties().ToList();
            foreach (var kod in costLinePropertiesBringing)
            {
                if (costLinePropertiesFormation.Count != 0 && costLinePropertiesFormation.Contains(kod))
                    costLineProperties.Add(kod);
                else
                    costLineProperties.Add(kod);
            }

            var query = GetQueryForAllocations_Control_0514(costLineProperties, IdSBP, context, false);
            var limitErrors = new Dictionary<LimitVolumeAppropriationResult, string>();
            var year = Budget.Year;

            for (int i = 0; i < 3; i++)
            {
                var idHierarchy = year.GetIdHierarchyPeriodYear(context);
                var yearQuery = String.Format(query, (i == 0) ? "OFG" : (i == 1 ? "PFG1" : "PFG2"), idHierarchy);

                var result = context.Database.SqlQuery<LimitVolumeAppropriationResult>(yearQuery).ToList();

                foreach (var r in result)
                {
                    r.Value = r.Value ?? 0;
                    r.JustifiedValue = r.JustifiedValue ?? 0;
                    r.BringValue = r.BringValue ?? 0;

                    if (r.BringValue + r.JustifiedValue > r.Value)
                    {

                        // �������� ����������� �� ����� ��������� ���������� ��� � ������ 
                        List<LimitVolumeAppropriationResult> ChecResult = CheckParents(context, r, result, sbpBlankBringing);
                        if (ChecResult.Any() && !(ChecResult[0].BringValue + ChecResult[0].JustifiedValue > ChecResult[0].Value)) continue;



                        var estimatedLine = 
                            (r.IdExpenseObligationType.HasValue ? ", ��� ��: " + (((ExpenseObligationType)r.IdExpenseObligationType).Caption()) : "") +
                            (r.IdFinanceSource.HasValue ? ", ��: " + (context.FinanceSource.SingleOrDefault(s=> s.Id == r.IdFinanceSource).Code) : "") +
                            (r.IdKFO.HasValue ? ", ���: " + (context.KFO.SingleOrDefault(s => s.Id == r.IdKFO).Code) : "") +
                            (r.IdKVSR.HasValue ? ", ����: " + (context.KVSR.SingleOrDefault(s => s.Id == r.IdKVSR).Caption) : "") +
                            (r.IdRZPR.HasValue ? ", ����: " + (context.RZPR.SingleOrDefault(s => s.Id == r.IdRZPR).Code) : "") +
                            (r.IdKCSR.HasValue ? ", ����: " + (context.KCSR.SingleOrDefault(s => s.Id == r.IdKCSR).Code) : "") +
                            (r.IdKVR.HasValue ? ", ���: " + (context.KVR.SingleOrDefault(s => s.Id == r.IdKVR).Code) : "") +
                            (r.IdKOSGU.HasValue ? ", �����: " + (context.KOSGU.SingleOrDefault(s => s.Id == r.IdKOSGU).Code) : "") +
                            (r.IdDKR.HasValue ? ", ���: " + (context.DKR.SingleOrDefault(s => s.Id == r.IdDKR).Code) : "") +
                            (r.IdDEK.HasValue ? ", ���: " + (context.DEK.SingleOrDefault(s => s.Id == r.IdDEK).Code) : "") +
                            (r.IdDFK.HasValue ? ", ���: " + (context.DFK.SingleOrDefault(s => s.Id == r.IdDFK).Code) : "") +
                            (r.IdCodeSubsidy.HasValue ? ", ��� ��������: " + (context.CodeSubsidy.SingleOrDefault(s => s.Id == r.IdCodeSubsidy).Code) : "") +
                            (r.IdBranchCode.HasValue ? ", ���������� ���: " + (context.BranchCode.SingleOrDefault(s => s.Id == r.IdBranchCode).Code) : "");
                        r.Year = year;

                        if (SBP.SBPType == DbEnums.SBPType.GeneralManager)
                            limitErrors.Add(r,
                                String.Format("{5}, {0} - ����� ������� �� ��������� = {1}, �������������� ����� = {2}, ���������� = {3}, �������� = {4}",
                                    estimatedLine,
                                    r.Value,
                                    r.BringValue,
                                    r.JustifiedValue,
                                    r.Value - r.JustifiedValue - r.BringValue,
                                    year));
                        if (SBP.SBPType == DbEnums.SBPType.Manager)
                             limitErrors.Add(r,
                                 String.Format("{4}, {0} - ����� ������� �� ��������� = {1}, ���������� = {2}, �������� = {3}",
                                     estimatedLine,
                                     r.Value,
                                     r.JustifiedValue,
                                     r.Value - r.JustifiedValue,
                                     year));
                    }
                }
                year++;
            }

            if (limitErrors.Any())
            {
                var msg = new StringBuilder();
                foreach (var limitError in limitErrors)
                    msg.AppendFormat(" - {0}<br/>", limitError.Value);

                var errorEstimatedLine0514Results = limitErrors.Select(l => l.Key).ToList();

                Controls.Throw(
                    String.Format("����� ������������ ��������� ������������, ��������� �������� ����� ��������� ������������ ��� �{0}�.<br/>���������� ���������� �� �������:<br/>{1}", SBP.Caption, msg));

               afterControlSkip_0514(context, errorEstimatedLine0514Results);
            }
        }

        /// <summary>
        /// ������� ���. �����������
        /// </summary>
        /// <param name="context"></param>
        [ControlInitial(InitialCaption = "������� ���. �����������", InitialUNK = "0515", InitialSkippable = true)]
        public void Control_0515(DataContext context)
        {
            if (!IsAdditionalNeed.HasValue)
                return;

            if (IsAdditionalNeed.Value)
                return;

            var oldDocument = OperationContext.Current.OriginalTarget as LimitBudgetAllocations;

            if (oldDocument == null)
                throw new PlatformException("�� ������� �������� ������������ �������� ��������� ����� OperationContext.Current.OriginalTarget");

            if (IsAdditionalNeed.Value != oldDocument.IsAdditionalNeed.Value)
            {
                if (!context.LimitBudgetAllocations_LimitAllocations.Any(r => r.IdOwner == this.Id &&
                                                                              (r.AdditionalNeedOFG.HasValue ||
                                                                               r.AdditionalNeedPFG1.HasValue ||
                                                                               r.AdditionalNeedPFG2.HasValue)))
                {
                    return;
                }

                Controls.Throw(
                    "������� ������ ���. ����������� ��������. ��� ���. ����������� � ��������� ����� �������. ����������?");

                //LimitAllocations.AsQueryable()     ������ ����� (� ���� �� ������, ��� �� �������?), �� ��� �� ��������
                //        .Update(e => e.AdditionalNeedOFG.HasValue || e.AdditionalNeedPFG1.HasValue || e.AdditionalNeedPFG2.HasValue,
                context.LimitBudgetAllocations_LimitAllocations
                       .Update(
                           e =>
                           e.IdOwner == Id &&
                           (e.AdditionalNeedOFG.HasValue || e.AdditionalNeedPFG1.HasValue ||
                            e.AdditionalNeedPFG2.HasValue),
                           u => new LimitBudgetAllocations_LimitAllocations
                               {
                                   AdditionalNeedOFG = null,
                                   AdditionalNeedPFG1 = null,
                                   AdditionalNeedPFG2 = null
                               });
            }
        }

        /// <summary>
        /// �������� �������� ������ ���.�����������
        /// </summary>
        /// <param name="context"></param>
        [ControlInitial(InitialCaption = ". �������� �������� ������ ���.�����������", InitialUNK = "0516", InitialSkippable = true)]
        public void Control_0516(DataContext context)
        {
            if (IsAdditionalNeed.HasValue && IsAdditionalNeed.Value)
                Controls.Throw("�������� ������� � ���. �������������. " +
                               "�� ��������� �������� ����������� ������� ��������. " +
                               "����� ������� � ���������� ����� �������� ��������� � ���������� ������� �� ���. ������������. " +
                               "����������?");
        }

        /// <summary>
        ///�������� �������� ������ ���. �����������
        /// </summary>
        /// <param name="context"></param>
        [ControlInitial(InitialCaption = "�������� �������� ������ ���. �����������", InitialUNK = "0517")]
        public void Control_0517(DataContext context)
        {
            if (!IsAdditionalNeed.HasValue || !IsAdditionalNeed.Value)
                Controls.Throw("� ��������� ����������� �������� �� ���. ������������. �������������� ��������� �����������.");
        }

        /// <summary>
        ///�������� �������� ������ ���. �����������
        /// </summary>
        /// <param name="context"></param>
        [ControlInitial(InitialCaption = "�������� �������� ������ ���. �����������", InitialUNK = "0518", InitialSkippable = true)]
        public void Control_0518(DataContext context)
        {
            if (IsAdditionalNeed.HasValue && IsAdditionalNeed.Value)
                Controls.Throw("����� ������� � ���������� ����� �������� ��������� � ������ �� ���. ������������ ����� ����������� � �������� ����������. " +
                               "����������?");
        }

        /// <summary>
        /// �������� ����������� ���������� ������� ������������
        /// </summary>
        /// <param name="context"></param>
        [ControlInitial(InitialUNK = "0519", InitialCaption = "�������� ����������� ���������� ������� ������������")]
        public void Control_0519(DataContext context)
        {
            //�������� ��������� � ��� ������, ���� � ���, ���������� � ����� ���������, ���� ����������� ���
            if (SBP == null || !SBP.IdParent.HasValue)
                return;

            var parentSBP = SBP.Parent;
            if (parentSBP == null)
                return;

            //���� � ������ ��������� ������������ ��� ����� ������ ��� ������������, 
            //�� �������� �� �������� ������� ���������� ������� ����� �� ����� � ����� �������� ����� 
            //(� ������� ����� ������������ � ������ ���, ������, ������) � ���= ����������� ��� ��� ��� ���������.
            var parentSBPBlank = parentSBP.SBP_Blank.FirstOrDefault(b => b.IdBudget == IdBudget && b.BlankType == BlankType.BringingGRBS);
            if (parentSBPBlank == null)
                return;

            var idKosgu000 = parentSBPBlank.BlankValueType_KOSGU == BlankValueType.Mandatory ? (int?)context.KOSGU.Where(k => k.Code == "000").Select(k => k.Id).FirstOrDefault() : null;
            if (idKosgu000.HasValue && !context.LimitVolumeAppropriations.Any(l => l.IdPublicLegalFormation == IdPublicLegalFormation && l.IdBudget == IdBudget && l.IdVersion == IdVersion))
                idKosgu000 = null;

            var limitErrors = CheckLimitAllocations0519(context, parentSBPBlank, idKosgu000).ToList();

            if (limitErrors.Any())
            {
                var msg = new StringBuilder();
                foreach (var limitError in limitErrors)
                    msg.AppendFormat(" - {0}<br/>", limitError);

                Controls.Throw(
                    String.Format("����� ��������� ������������, ��������� �� ���������������� ����������, ��������� ������� ���������������� ������� ������������ ��� �{0}�:<br/>{1}",
                        parentSBP.Caption, msg));
            }
        }

        /// <summary>
        /// �������� ���������� ������ ������������ �� ���. ������������, ���������� �� ���������������� ����������, ��� �������� ���������������� ������������ �� ���. ������������ ������������ ���
        /// </summary>
        /// <param name="context"></param>
        [ControlInitial(InitialUNK = "0520", InitialCaption = "�������� ���������� ������ ������������ �� ���. ������������, ���������� �� ���������������� ����������, ��� �������� ���������������� ������������ �� ���. ������������ ������������ ���")]
        public void Control_0520(DataContext context)
        {
            //�������� ��������� � ��� ������, ���� � ���, ���������� � ����� ���������, ���� ����������� ���
            if (SBP == null || !SBP.IdParent.HasValue)
                return;

            var parentSBP = SBP.Parent;
            if (parentSBP == null)
                return;

            var sbpBlankBringing = new SBP_Blank();
            var sbpBlankFormation = new SBP_Blank();

            sbpBlankBringing = parentSBP.SBP_Blank.FirstOrDefault(b => b.BlankType == BlankType.BringingGRBS && b.IdBudget == IdBudget);
            sbpBlankFormation = parentSBP.SBP_Blank.FirstOrDefault(b => b.BlankType == BlankType.FormationGRBS && b.IdBudget == IdBudget);

            //�������� ����� ������������ ����� �������
            var costLineProperties = new List<string>();
            var costLinePropertiesBringing = sbpBlankBringing.GetBlankCostMandatoryProperties().ToList();
            var costLinePropertiesFormation = sbpBlankFormation.GetBlankCostMandatoryProperties().ToList();

            foreach (var kod in costLinePropertiesBringing)
            {
                if (costLinePropertiesFormation.Contains(kod))
                    costLineProperties.Add(kod);
            }

            var query = GetQueryForAllocations_Control_0520(costLineProperties, parentSBP.Id, context, false);
            var limitErrors = new Dictionary<LimitVolumeAppropriationResult, string>();
            var year = Budget.Year;

            for (int i = 0; i < 3; i++)
            {
                var idHierarchy = year.GetIdHierarchyPeriodYear(context);
                var yearQuery = String.Format(query, (i == 0) ? "AdditionalNeedOFG" : (i == 1 ? "AdditionalNeedPFG1" : "AdditionalNeedPFG2"), idHierarchy);

                var result = context.Database.SqlQuery<LimitVolumeAppropriationResult>(yearQuery).ToList();

                foreach (var r in result)
                {

                    r.Value = r.Value ?? 0;
                    r.PlanValue = r.PlanValue ?? 0;
                    r.BringValue = r.BringValue ?? 0;
                    r.JustifiedValue = r.JustifiedValue ?? 0;
                    

                    if ( r.Value > r.PlanValue - r.BringValue - r.JustifiedValue)
                    {
                        // �������� ����������� �� ����� ��������� ���������� ��� � ������ 
                        List<LimitVolumeAppropriationResult> ChecResult = CheckParents(context, r, result, sbpBlankBringing);
                        if (ChecResult.Any() && !(ChecResult[0].Value > ChecResult[0].PlanValue - ChecResult[0].BringValue - ChecResult[0].JustifiedValue)) continue;


                        var estimatedLine = r.GetEstimatedLine(context);
                        r.Year = year;

                            limitErrors.Add(r,
                                String.Format("{4}, {0} - ���������������� �������  = {2}, ����� ������� �� ��������� = {1}, �������� = {3}",
                                    estimatedLine,
                                    r.Value,
                                    r.PlanValue - r.BringValue - r.JustifiedValue,
                                    r.PlanValue - r.BringValue - r.Value,
                                    year));
                    }
                }
                year++;
            }

            if (limitErrors.Any())
            {
                var msg = new StringBuilder();
                foreach (var limitError in limitErrors)
                    msg.AppendFormat(" - {0}<br/>", limitError.Value);

                Controls.Throw(
                    String.Format("����� ��������� ������������ �� �������������� ������������, ��������� �� ���������������� ����������, ��������� ������� ���������������� ������� �� �������������� ������������ ������������ ��� �{0}�:<br/>{1}",
                        parentSBP.Caption, msg));
            }
        }

        /// <summary>
        /// �������� ���������� ������ ������������ �� ���. ������������, ����������� �� ���������������� ����������, ��� �������� ������� ������������ �� ���. ������������ ������������ ���
        /// </summary>
        /// <param name="context"></param>
        [ControlInitial(InitialUNK = "0521", InitialCaption = "�������� ���������� ������ ������������ �� ���. ������������, ����������� �� ���������������� ����������, ��� �������� ������� ������������ �� ���. ������������ ������������ ���", InitialSkippable = true, InitialManaged = true)]
        public void Control_0521(DataContext context)
        {
            
            var Blank = new SBP_Blank();

            if (SBP.SBPType == DbEnums.SBPType.GeneralManager)
                Blank = SBP.SBP_Blank.FirstOrDefault(b => b.BlankType == BlankType.BringingGRBS && b.IdBudget == IdBudget);

            if (SBP.SBPType == DbEnums.SBPType.Manager)
            {
                var parent = SBP.Parent;
                if (parent != null)
                    Blank = parent.SBP_Blank.FirstOrDefault(b => b.BlankType == BlankType.BringingRBS && b.IdBudget == IdBudget);
            }


            var query = GetQueryForAllocations_Control_0521(Blank, IdSBP, context, false);
            var limitErrors = new Dictionary<LimitVolumeAppropriationResult, string>();
            var year = Budget.Year;

            for (int i = 0; i < 3; i++)
            {
                var idHierarchy = year.GetIdHierarchyPeriodYear(context);
                var yearQuery = String.Format(query, (i == 0) ? "AdditionalNeedOFG" : (i == 1 ? "AdditionalNeedPFG1" : "AdditionalNeedPFG2"), idHierarchy);

                var result = context.Database.SqlQuery<LimitVolumeAppropriationResult>(yearQuery).ToList();

                foreach (var r in result)
                {
                    r.Value = r.Value ?? 0;
                    r.PlanValue = r.PlanValue ?? 0;
                    r.BringValue = r.BringValue ?? 0;
                    r.JustifiedValue = r.JustifiedValue ?? 0;


                    if (r.Value + r.PlanValue > r.BringValue)
                    {
                        var estimatedLine = r.GetEstimatedLine(context);
                        r.Year = year;

                        limitErrors.Add(r,
                            String.Format("{4}, {0} -����� ������� �� ��������� = {1}, �������������� ����� = {2}, �������� = {3}",
                                estimatedLine,
                                r.Value,
                                r.BringValue,
                                r.PlanValue + r.Value - r.BringValue,
                                year));
                    }
                }
                year++;
            }

            if (limitErrors.Any())
            {
                var msg = new StringBuilder();
                foreach (var limitError in limitErrors)
                    msg.AppendFormat(" - {0}<br/>", limitError.Value);

                Controls.Throw(
                    String.Format("����� ��������� ������������ �� �������������� ������������, ���������� �� ���������������� ����������, ��������� �������� ����� ��������� ������������ �� �������������� ������������ ������������ ��� �{0}�:<br/>{1}",
                        SBP.Caption, msg));
            }
        }


        /// <summary>
        /// �������� ���������� ������ ������������ ��������� ������������ �� ���. ������������, ��� ������� ���������� ��������� ������������ �� ���. ������������ �������� ��� 
        /// </summary>
        /// <param name="context"></param>
        [ControlInitial(InitialUNK = "0522", InitialCaption = "�������� ���������� ������ ������������ ��������� ������������ �� ���. ������������, ��� ������� ���������� ��������� ������������ �� ���. ������������ �������� ���", InitialSkippable = true, InitialManaged = true)]
        public void Control_0522(DataContext context)
        {
            var errorBlank = "";
            var sbpBlankBringing = new SBP_Blank();
            var sbpBlankFormation = new SBP_Blank();

            if (SBP.SBPType == DbEnums.SBPType.GeneralManager)
            {
                sbpBlankBringing = SBP.SBP_Blank.FirstOrDefault(b => b.BlankType == BlankType.BringingGRBS && b.IdBudget == IdBudget);
                //sbpBlankFormation = SBP.SBP_Blank.FirstOrDefault(b => b.BlankType == BlankType.FormationGRBS && b.IdBudget == IdBudget);
            }
            if (SBP.SBPType == DbEnums.SBPType.Manager)
            {
                var parent = SBP.Parent;

                if (parent != null)
                {
                    sbpBlankBringing = parent.SBP_Blank.FirstOrDefault(b => b.BlankType == BlankType.BringingRBS && b.IdBudget == IdBudget);
                    sbpBlankFormation = parent.SBP_Blank.FirstOrDefault(b => b.BlankType == BlankType.FormationGRBS && b.IdBudget == IdBudget);

                }
            }


            if (errorBlank.Any())
            {
                Controls.Throw(errorBlank);
                return;
            }

            //�������� ����� ������������ ����� �������
            var costLineProperties = new List<string>();
            var costLinePropertiesBringing = sbpBlankBringing.GetBlankCostMandatoryProperties().ToList();
            var costLinePropertiesFormation = sbpBlankFormation.GetBlankCostMandatoryProperties().ToList();
            foreach (var kod in costLinePropertiesBringing)
            {
                if (costLinePropertiesFormation.Count != 0 && costLinePropertiesFormation.Contains(kod))
                    costLineProperties.Add(kod);
                else
                    costLineProperties.Add(kod);
            }

            var query = GetQueryForAllocations_Control_0522(costLineProperties, IdSBP, context, false);
            var limitErrors = new Dictionary<LimitVolumeAppropriationResult, string>();
            var year = Budget.Year;

            for (int i = 0; i < 3; i++)
            {
                var idHierarchy = year.GetIdHierarchyPeriodYear(context);
                var yearQuery = String.Format(query, (i == 0) ? "AdditionalNeedOFG" : (i == 1 ? "AdditionalNeedPFG1" : "AdditionalNeedPFG2"), idHierarchy);

                var result = context.Database.SqlQuery<LimitVolumeAppropriationResult>(yearQuery).ToList();

                foreach (var r in result)
                {
                    r.Value = r.Value ?? 0;
                    r.JustifiedValue = r.JustifiedValue ?? 0;
                    r.BringValue = r.BringValue ?? 0;

                    if (r.Value > r.BringValue + r.JustifiedValue)
                    {
                        // �������� ����������� �� ����� ��������� ���������� ��� � ������ 
                        List<LimitVolumeAppropriationResult> ChecResult = CheckParents(context, r, result, sbpBlankBringing);
                        if (ChecResult.Any() && !(ChecResult[0].Value > ChecResult[0].BringValue + ChecResult[0].JustifiedValue)) continue;

                        var estimatedLine = r.GetEstimatedLine(context);
                        r.Year = year;

                        if (SBP.SBPType == DbEnums.SBPType.GeneralManager)
                            limitErrors.Add(r,
                                String.Format("{5}, {0} - ����� ������� �� ��������� = {1}, �������������� ����� = {2}, ���������� = {3}, �������� = {4}",
                                    estimatedLine,
                                    r.Value,
                                    r.BringValue,
                                    r.JustifiedValue,
                                    r.Value - r.JustifiedValue - r.BringValue,
                                    year));
                        if (SBP.SBPType == DbEnums.SBPType.Manager)
                            limitErrors.Add(r,
                                String.Format("{4}, {0} - ����� ������� �� ��������� = {1}, ���������� = {2}, �������� = {3}",
                                    estimatedLine,
                                    r.Value,
                                    r.JustifiedValue,
                                    r.Value - r.JustifiedValue,
                                    year));
                    }
                }
                year++;
            }

            if (limitErrors.Any())
            {
                var msg = new StringBuilder();
                foreach (var limitError in limitErrors)
                    msg.AppendFormat(" - {0}<br/>", limitError.Value);

                var errorEstimatedLine0514Results = limitErrors.Select(l => l.Key).ToList();

                Controls.Throw(
                    String.Format("����� ������������ ��������� ������������ �� �������������� ������������, ��������� �������� ����� ��������� ������������ �� �������������� ������������ ��� �{0}�.<br/>���������� ���������� �� �������:<br/>{1}", SBP.Caption, msg));

                afterControlSkip_0514(context, errorEstimatedLine0514Results);
            }
        }


        /// <summary>   
        /// �������� "�������� ������������ �������� ������ ��������� � ���������� "
        /// </summary>         
        [ControlInitial(InitialUNK = "0523", InitialCaption = "�������� ������������ �������� ������ ��������� � ���������� ", InitialSkippable = true)]
        public void Control_0523(DataContext context)
        {
            IQueryable<SBP_Blank> newBlanks;
            
            byte idchekblanktype;
            if (this.SBP.SBPType == DbEnums.SBPType.GeneralManager)
            {
                idchekblanktype = (byte) DbEnums.BlankType.BringingGRBS;
                newBlanks = context.SBP_Blank.Where(r =>
                                                    r.IdBudget == this.IdBudget && r.IdOwner == this.IdSBP &&
                                                    r.IdBlankType == idchekblanktype);
            }
            else
            {
                idchekblanktype = (byte) DbEnums.BlankType.BringingRBS;
                newBlanks = context.SBP_Blank.Where(r =>
                                                    r.IdBudget == this.IdBudget && r.IdOwner == this.SBP.IdParent &&
                                                    r.IdBlankType == idchekblanktype);
            }

            var oldBlank = this.SBP_BlankActual;

            var newBlank = newBlanks.FirstOrDefault();

            bool fc;
            if (oldBlank == null)
            {
                fc = true;
            }
            else
            {
                fc = !SBP_BlankHelper.IsEqualBlank(newBlank, oldBlank);
            }

            if (fc)
                Controls.Throw(string.Format("��� ������� ����� �{0}�. " +
                                             "���������� ��������������� �������� � ������� ����������� ������ ��������� ������������, " +
                                             "� ������� ����� ������� ���, �� ��������������� ������ ���������, � ���������� ����������� ������� �����.",
                                             newBlank.BlankType.Caption()));
        }

        #region ��������������� �-��� ��� ���������

        private string GetQueryForAllocations(ISBP_Blank blank, int idQuerySBP, DataContext context, bool isIncludeHaving = true)
        {
            var costLineProperties = blank.GetBlankCostMandatoryProperties().ToList();

            const String limitAllocationYear = "{0}";
            const String limitVolumeIdHierarchy = "{1}";

            var kbkString = costLineProperties.GetQueryString();
            var estimatedLineKbkString = costLineProperties.GetQueryString("EL");

            var estimatedLineKbkRequieredConditions = new StringBuilder();
            foreach (var costLineProperty in costLineProperties)
                estimatedLineKbkRequieredConditions.Append("EL." + costLineProperty + " IS NOT NULL And ");

            var limitAllocationQuery = String.Format("Select {0}, " +
                                                "{1} as value, " +
                                                "Null as regValue, " +
                                                "Null as idValueType " +
                                              "From tp.LimitBudgetAllocations_LimitAllocations " +
                                              "Inner Join ref.FinanceSource FS on FS.id = idFinanceSource " +
                                              "Where " +
                                                     "idOwner = {2} And FS.idFinanceSourceType <> {3} ", kbkString, limitAllocationYear, Id, (byte)FinanceSourceType.UnconfirmedFunds );

            var planLimitVolumeAppropriationsQuery = String.Format("Select {0}, " +
                                                                    "Null as value, " +
                                                                    "L.Value as regValue, " +
                                                                    "L.idValueType " +
                                                                   "From reg.LimitVolumeAppropriations L " +
                                                                   "Inner Join reg.EstimatedLine EL on EL.id = L.idEstimatedLine " +
                                                                   "Inner Join ref.FinanceSource FS on FS.id = EL.idFinanceSource " +
                                                                   "Where EL.idSBP = {8} And " +
                                                                            "FS.idFinanceSourceType <> {11} And " +
                                                                            "(L.idRegistratorEntity <> {9} Or L.idRegistrator Not In ({10}) ) And " +
                                                                            "{7} (((L.idValueType = {1} Or L.idValueType = {6}) and (HasAdditionalNeed = 0 or HasAdditionalNeed is null)) or (L.idValueType = {12} and HasAdditionalNeed = 1)) And " +
                                                                            "L.idHierarchyPeriod = {2} And " +
                                                                            "L.idPublicLegalFormation = {3} And " +
                                                                            "L.idBudget = {4} And " +
                                                                            "L.idVersion = {5}",
                                                                   estimatedLineKbkString, (int)ValueType.Plan, limitVolumeIdHierarchy, IdPublicLegalFormation, IdBudget, IdVersion, (int) ValueType.Bring,
                                                                   estimatedLineKbkRequieredConditions, idQuerySBP, EntityId, PrevVersionIds(context).GetQueryString(),
                                                                   (byte)FinanceSourceType.UnconfirmedFunds, (int)ValueType.JustifiedGRBS);

            var innerQuery = limitAllocationQuery + " Union All " + planLimitVolumeAppropriationsQuery;

            var cmd = String.Format("Select {0}, " +
                             "CAST(SUM(G.Value) as numeric(20,2) ) as value, " +
                             "CAST(SUM(CASE WHEN G.idValueType = {2} THEN G.regValue ELSE NULL END) as numeric(20,2) ) as planValue, " +
                             "CAST(SUM(CASE WHEN G.idValueType = {3} THEN G.regValue ELSE NULL END) as numeric(20,2) ) as bringValue, " +
                             "CAST(SUM(CASE WHEN G.idValueType = {4} THEN G.regValue ELSE NULL END) as numeric(20,2) ) as JustifiedValue " +
                             "From ({1}) G " +
                             (string.IsNullOrEmpty(kbkString) ? "" : "Group By {0} ") +
                             (isIncludeHaving ? "Having SUM(G.Value) > 0 " : String.Empty), kbkString, innerQuery, (int)ValueType.Plan, (int)ValueType.Bring, (int)ValueType.JustifiedGRBS);

            return cmd;
        }

        private Dictionary<LimitVolumeAppropriationResult, String> CheckLimitAllocations0511(DataContext context, SBP_Blank blank, int? idKosgu000 = null)
        {
            var limitErrors = new Dictionary<LimitVolumeAppropriationResult, string>();

            var query = GetQueryForAllocations(blank, SBP.IdParent.Value, context);
            var year = Budget.Year;

            for (int i = 0; i < 3; i++)
            {
                var idHierarchy = year.GetIdHierarchyPeriodYear(context);
                var yearQuery = String.Format(query, (i == 0) ? "OFG" : (i == 1 ? "PFG1" : "PFG2"), idHierarchy);

                var result = context.Database.SqlQuery<LimitVolumeAppropriationResult>(yearQuery).ToList();

                if (idKosgu000.HasValue)
                {
                    //�������� �� �������, ��� ������� ������� ���������� � �������� � ���������
                    foreach (var volumeAppropriationResult in result.Where(r => r.PlanValue.HasValue || r.BringValue.HasValue || r.JustifiedValue.HasValue))
                    {
                        volumeAppropriationResult.PlanValue = volumeAppropriationResult.PlanValue ?? 0;
                        volumeAppropriationResult.BringValue = volumeAppropriationResult.BringValue ?? 0;
                        volumeAppropriationResult.JustifiedValue = volumeAppropriationResult.JustifiedValue ?? 0;

                        if (volumeAppropriationResult.Value > volumeAppropriationResult.PlanValue - volumeAppropriationResult.BringValue - volumeAppropriationResult.JustifiedValue)
                        {
                            var estimatedLine = volumeAppropriationResult.GetEstimatedLine(context);

                            if (limitErrors.ContainsKey(volumeAppropriationResult))
                            {
                                continue;
                            }

                            limitErrors.Add(volumeAppropriationResult,
                                String.Format(
                                    "{4}, {0} - ���������������� ������� = {1}, ����� ������� �� ��������� = {2}, �������� = {3}",
                                    estimatedLine,
                                    volumeAppropriationResult.PlanValue - volumeAppropriationResult.BringValue - volumeAppropriationResult.JustifiedValue,
                                    volumeAppropriationResult.Value,
                                    volumeAppropriationResult.PlanValue - volumeAppropriationResult.BringValue - volumeAppropriationResult.JustifiedValue - volumeAppropriationResult.Value,
                                    year));
                        }
                    }

                    result = result.Where(r => !r.PlanValue.HasValue && !r.BringValue.HasValue).ToList();
                    var kosguResult = new List<LimitVolumeAppropriationResult>() { };
                    //�������� �� �������, ��� ������� �� �������� ������������. � ���� ����� ��������� ������, ���� ����������� ����� = 0
                    foreach (var volumeAppropriationResult in result)
                    {
                        var idKOSGU = volumeAppropriationResult.IdKOSGU;
                        
                        //����������� ����� = 0
                        volumeAppropriationResult.IdKOSGU = idKosgu000;

                        if (!volumeAppropriationResult.GetQueryForExistingReg(context, "EstimatedLine").Any())
                        {
                            volumeAppropriationResult.IdKOSGU = idKOSGU;
                            var estimatedLine = volumeAppropriationResult.GetEstimatedLine(context);

                            if (limitErrors.ContainsKey(volumeAppropriationResult))
                            {
                                continue;
                            }

                            limitErrors.Add(volumeAppropriationResult,
                                String.Format(
                                    "{3}, {0} - ���������������� ������� = 0, ����� ������� �� ��������� = {1}, �������� = {2}",
                                    estimatedLine,
                                    volumeAppropriationResult.Value,
                                    -volumeAppropriationResult.Value,
                                    year));

                            //result.Remove(volumeAppropriationResult);
                        }
                        else
                        {
                            //volumeAppropriationResult.Value = null;
                            volumeAppropriationResult.PlanValue = null;
                            volumeAppropriationResult.BringValue = null;
                            volumeAppropriationResult.JustifiedValue = null;

                            kosguResult.Add(volumeAppropriationResult);
                        }
                    }

                    //�������� ������ ��� ������� ������� ������ � �������� � ����� = 0
                    var kosguKbkResults = new List<LimitVolumeAppropriationResult>();
                    foreach (var limitVolumeAppropriationResult in kosguResult)
                    {
                        var t = limitVolumeAppropriationResult.CloneAsLineCost<LimitVolumeAppropriationResult>();
                        t.Value = null;
                        t.BringValue = null;
                        t.PlanValue = null;
                        t.JustifiedValue = null;

                        kosguKbkResults.Add(t);
                    }
                    kosguKbkResults = kosguKbkResults.Distinct().ToList();

                    foreach (var limitVolumeAppropriationResult in kosguKbkResults)
                    {
                        var planLimitVolumeAppropriationsQuery =
                            String.Format("Select ISNULL(SUM(CASE L.idValueType WHEN {0} THEN L.Value ELSE NULL END), 0) - " +
                                                    "ISNULL(SUM(CASE L.idValueType WHEN {1} THEN L.Value ELSE NULL END), 0) " +
                                          "From reg.LimitVolumeAppropriations L " +
                                          "Inner Join reg.EstimatedLine EL on EL.id = L.idEstimatedLine " +
                                          "Where L.idHierarchyPeriod = {6} And " +
                                            "L.idPublicLegalFormation = {2} And " +
                                            "L.idBudget = {3} And " +
                                            "L.idVersion = {4} {5}", 
                                                (int)ValueType.Plan, 
                                                (int)ValueType.Bring, 
                                                IdPublicLegalFormation, IdBudget, IdVersion, limitVolumeAppropriationResult.GetWhereQuery("EL"), idHierarchy);
                        var freeValues = context.Database.SqlQuery<decimal?>(planLimitVolumeAppropriationsQuery).FirstOrDefault() ?? 0;

                        var docValue = kosguResult.AsQueryable().ApplyWhere(limitVolumeAppropriationResult).Sum(s => s.Value);

                        if (docValue > freeValues)
                        {
                            var estimatedLine = limitVolumeAppropriationResult.GetEstimatedLine(context);

                            if (limitErrors.ContainsKey(limitVolumeAppropriationResult))
                            {
                                continue;
                            }

                            limitErrors.Add(limitVolumeAppropriationResult,
                                String.Format(
                                    "{4}, {0} - ���������������� ������� = {1}, ����� ������� �� ��������� = {2}, �������� = {3}",
                                    estimatedLine,
                                    freeValues,
                                    docValue,
                                    freeValues - docValue,
                                    year));
                        }
                    }

                }
                else
                    foreach (var r in result)
                    {
                        r.Value = r.Value ?? 0;
                        r.PlanValue = r.PlanValue ?? 0;
                        r.BringValue = r.BringValue ?? 0;
                        r.JustifiedValue = r.JustifiedValue ?? 0;

                        if ( r.Value > r.PlanValue - r.BringValue- r.JustifiedValue)
                        {
                            // �������� ����������� �� ����� ��������� ���������� ��� � ������ 
                            List<LimitVolumeAppropriationResult> ChecResult = CheckParents(context, r, result, blank);
                            if (ChecResult.Any() && !(ChecResult[0].Value > ChecResult[0].PlanValue - ChecResult[0].BringValue - ChecResult[0].JustifiedValue)) continue;
                            
                            var estimatedLine = r.GetEstimatedLine(context);

                            if (limitErrors.ContainsKey(r))
                            {
                                continue;
                            }

                            limitErrors.Add(r,
                                String.Format(
                                    "{4}, {0} - ���������������� ������� = {1}, ����� ������� �� ��������� = {2}, �������� = {3}",
                                    estimatedLine,
                                    r.PlanValue - r.BringValue,
                                    r.Value,
                                    r.PlanValue - r.BringValue - r.Value - r.JustifiedValue,
                                    year));
                        }
                    }

                year++;
            }

            return limitErrors;
        }

        private string GetQueryForAllocations_Control_0514(List<string> costLineProperties, int idQuerySBP, DataContext context, bool isIncludeHaving = true)
        {
            const String limitAllocationYear = "{0}";
            const String limitVolumeIdHierarchy = "{1}";

            var kbkString = costLineProperties.GetQueryString();
            var estimatedLineKbkString = costLineProperties.GetQueryString("EL");

            var estimatedLineKbkRequieredConditions = new StringBuilder();
            foreach (var costLineProperty in costLineProperties)
                estimatedLineKbkRequieredConditions.Append("EL." + costLineProperty + " IS NOT NULL And ");

            var limitAllocationQuery = String.Format("Select {0}, " +
                                                "{1} as value, " +
                                                "Null as regValue, " +
                                                "Null as idValueType " +
                                              "From tp.LimitBudgetAllocations_LimitAllocations " +
                                              "Inner Join ref.FinanceSource FS on FS.id = idFinanceSource " +
                                              "Where idOwner = {2} And FS.idFinanceSourceType <> {3} ", kbkString, limitAllocationYear, Id, (byte)FinanceSourceType.UnconfirmedFunds);

            var Type = "";
            if (SBP.SBPType == DbEnums.SBPType.GeneralManager)
                Type = String.Format("(L.idValueType = {0} Or L.idValueType = {1})", (int)ValueType.JustifiedGRBS,
                               (int)ValueType.Bring);
            if (SBP.SBPType == DbEnums.SBPType.Manager)
                Type = String.Format("L.idValueType = {0}", (int)ValueType.JustifiedGRBS);


            var planLimitVolumeAppropriationsQuery = String.Format("Select {0}, " +
                                                                    "Null as value, " +
                                                                    "L.Value as regValue, " +
                                                                    "L.idValueType " +
                                                                   "From reg.LimitVolumeAppropriations L " +
                                                                   "Inner Join reg.EstimatedLine EL on EL.id = L.idEstimatedLine " +
                                                                   "Inner Join ref.FinanceSource FS on FS.id = EL.idFinanceSource " +
                                                                   "Where EL.idSBP = {7} And " +
                                                                            "FS.idFinanceSourceType <> {10} And " +
                                                                            "(L.idRegistratorEntity <> {8} Or L.idRegistrator Not In ({9}) ) And " +
                                                                            "{6} {1} And (HasAdditionalNeed != 1 OR HasAdditionalNeed IS NULL) And " +
                                                                            "L.idHierarchyPeriod = {2} And " +
                                                                            "L.idPublicLegalFormation = {3} And " +
                                                                            "L.idBudget = {4} And " +
                                                                            "L.idVersion = {5}",
                                                                   estimatedLineKbkString, Type, limitVolumeIdHierarchy, IdPublicLegalFormation, IdBudget, IdVersion,
                                                                   estimatedLineKbkRequieredConditions, idQuerySBP, EntityId, PrevVersionIds(context).GetQueryString(),
                                                                   (byte)FinanceSourceType.UnconfirmedFunds);

            var innerQuery = limitAllocationQuery + " Union All " + planLimitVolumeAppropriationsQuery;

            var cmd = String.Format("Select {0}, " +
                             "CAST(SUM(G.Value) as numeric(20,2) ) as value, " +
                             "CAST(SUM(CASE WHEN G.idValueType = {2} THEN G.regValue ELSE NULL END) as numeric(20,2) ) as JustifiedValue, " +
                             "CAST(SUM(CASE WHEN G.idValueType = {3} THEN G.regValue ELSE NULL END) as numeric(20,2) ) as bringValue " +
                             "From ({1}) G " +
                             (string.IsNullOrEmpty(kbkString) ? "" : "Group By {0} ") +
                             (isIncludeHaving ? "Having SUM(G.Value) > 0 " : String.Empty), kbkString, innerQuery, (int)ValueType.JustifiedGRBS, (int)ValueType.Bring);

            return cmd;
        }

        private IEnumerable<String> CheckLimitAllocations0519(DataContext context, SBP_Blank blank, int? idKosgu000 = null)
        {
            
            var limitErrors = new List<string>();

            var query = GetQueryForAllocations_Control_0519(blank, SBP.IdParent.Value, context);
            var year = Budget.Year;

            for (int i = 0; i < 3; i++)
            {
                var idHierarchy = year.GetIdHierarchyPeriodYear(context);
                var yearQuery = String.Format(query, (i == 0) ? "OFG" : (i == 1 ? "PFG1" : "PFG2"), idHierarchy);

                var result = context.Database.SqlQuery<LimitVolumeAppropriationResult>(yearQuery).ToList();

                if (idKosgu000.HasValue)
                {
                    //�������� �� �������, ��� ������� ������� ���������� � �������� � ���������
                    foreach (var volumeAppropriationResult in result.Where(r => r.PlanValue.HasValue || r.BringValue.HasValue || r.JustifiedValue.HasValue))
                    {
                        volumeAppropriationResult.PlanValue = volumeAppropriationResult.PlanValue ?? 0;
                        volumeAppropriationResult.BringValue = volumeAppropriationResult.BringValue ?? 0;
                        volumeAppropriationResult.JustifiedValue = volumeAppropriationResult.JustifiedValue ?? 0;

                        if (volumeAppropriationResult.Value > volumeAppropriationResult.PlanValue - volumeAppropriationResult.BringValue - volumeAppropriationResult.JustifiedValue)
                        {
                            var estimatedLine = volumeAppropriationResult.GetEstimatedLine(context);

                            limitErrors.Add(
                                String.Format(
                                    "{4}, {0} - ���������������� ������� = {1}, ����� ������� �� ��������� = {2}, �������� = {3}",
                                    estimatedLine,
                                    volumeAppropriationResult.PlanValue - volumeAppropriationResult.BringValue - volumeAppropriationResult.JustifiedValue,
                                    volumeAppropriationResult.Value,
                                    volumeAppropriationResult.PlanValue - volumeAppropriationResult.BringValue - volumeAppropriationResult.JustifiedValue  - volumeAppropriationResult.Value,
                                    year));
                        }
                    }

                    result = result.Where(r => !r.PlanValue.HasValue && !r.BringValue.HasValue).ToList();
                    var kosguResult = new List<LimitVolumeAppropriationResult>() { };
                    //�������� �� �������, ��� ������� �� �������� ������������. � ���� ����� ��������� ������, ���� ����������� ����� = 0
                    foreach (var volumeAppropriationResult in result)
                    {
                        var idKOSGU = volumeAppropriationResult.IdKOSGU;

                        //����������� ����� = 0
                        volumeAppropriationResult.IdKOSGU = idKosgu000;

                        if (!volumeAppropriationResult.GetQueryForExistingReg(context, "EstimatedLine").Any())
                        {
                            volumeAppropriationResult.IdKOSGU = idKOSGU;
                            var estimatedLine = volumeAppropriationResult.GetEstimatedLine(context);

                            limitErrors.Add(
                                String.Format(
                                    "{3}, {0} - ���������������� ������� = 0, ����� ������� �� ��������� = {1}, �������� = {2}",
                                    estimatedLine,
                                    volumeAppropriationResult.Value,
                                    -volumeAppropriationResult.Value,
                                    year));

                            //result.Remove(volumeAppropriationResult);
                        }
                        else
                        {
                            //volumeAppropriationResult.Value = null;
                            volumeAppropriationResult.PlanValue = null;
                            volumeAppropriationResult.BringValue = null;
                            volumeAppropriationResult.JustifiedValue = null;

                            kosguResult.Add(volumeAppropriationResult);
                        }
                    }

                    //�������� ������ ��� ������� ������� ������ � �������� � ����� = 0
                    var kosguKbkResults = new List<LimitVolumeAppropriationResult>();
                    foreach (var limitVolumeAppropriationResult in kosguResult)
                    {
                        var t = limitVolumeAppropriationResult.CloneAsLineCost<LimitVolumeAppropriationResult>();
                        t.Value = null;
                        t.BringValue = null;
                        t.PlanValue = null;
                        t.JustifiedValue = null;

                        kosguKbkResults.Add(t);
                    }
                    kosguKbkResults = kosguKbkResults.Distinct().ToList();

                    foreach (var limitVolumeAppropriationResult in kosguKbkResults)
                    {
                        var planLimitVolumeAppropriationsQuery =
                            String.Format("Select ISNULL(SUM(CASE L.idValueType WHEN {0} THEN L.Value ELSE NULL END), 0) - " +
                                                    "ISNULL(SUM(CASE L.idValueType WHEN {1} THEN L.Value ELSE NULL END), 0) " +
                                          "From reg.LimitVolumeAppropriations L " +
                                          "Inner Join reg.EstimatedLine EL on EL.id = L.idEstimatedLine " +
                                          "Where L.idHierarchyPeriod = {6} And " +
                                            "L.idPublicLegalFormation = {2} And " +
                                            "L.idBudget = {3} And " +
                                            "L.idVersion = {4} {5}",
                                                (int)ValueType.Plan,
                                                (int)ValueType.Bring,
                                                IdPublicLegalFormation, IdBudget, IdVersion, limitVolumeAppropriationResult.GetWhereQuery("EL"), idHierarchy);
                        var freeValues = context.Database.SqlQuery<decimal?>(planLimitVolumeAppropriationsQuery).FirstOrDefault() ?? 0;

                        var docValue = kosguResult.AsQueryable().ApplyWhere(limitVolumeAppropriationResult).Sum(s => s.Value);

                        if (docValue > freeValues)
                        {
                            var estimatedLine = limitVolumeAppropriationResult.GetEstimatedLine(context);

                            limitErrors.Add(
                                String.Format(
                                    "{4}, {0} - ���������������� ������� = {1}, ����� ������� �� ��������� = {2}, �������� = {3}",
                                    estimatedLine,
                                    freeValues,
                                    docValue,
                                    freeValues - docValue,
                                    year));
                        }
                    }

                }
                else
                    foreach (var r in result)
                    {
                        r.Value = r.Value ?? 0;
                        r.PlanValue = r.PlanValue ?? 0;
                        r.BringValue = r.BringValue ?? 0;
                        r.JustifiedValue = r.JustifiedValue ?? 0;

                        if (r.Value > r.PlanValue - r.BringValue - r.JustifiedValue)
                        {
                            var estimatedLine = r.GetEstimatedLine(context);

                            limitErrors.Add(
                                String.Format(
                                    "{4}, {0} - ���������������� ������� = {1}, ����� ������� �� ��������� = {2}, �������� = {3}",
                                    estimatedLine,
                                    r.PlanValue - r.BringValue - r.JustifiedValue,
                                    r.Value,
                                    r.PlanValue - r.BringValue - r.JustifiedValue - r.Value,
                                    year));
                        }
                    }

                year++;
            }

            return limitErrors;
        }

        private string GetQueryForAllocations_Control_0520(List<string> costLineProperties, int idQuerySBP, DataContext context, bool isIncludeHaving = true)
        {
            const String limitAllocationYear = "{0}";
            const String limitVolumeIdHierarchy = "{1}";

            var kbkString = costLineProperties.GetQueryString();
            var estimatedLineKbkString = costLineProperties.GetQueryString("EL");

            var estimatedLineKbkRequieredConditions = new StringBuilder();
            foreach (var costLineProperty in costLineProperties)
                estimatedLineKbkRequieredConditions.Append("EL." + costLineProperty + " IS NOT NULL And ");

            var limitAllocationQuery = String.Format("Select {0}, " +
                                                "{1} as value, " +
                                                "Null as regValue, " +
                                                "Null as idValueType " +
                                              "From tp.LimitBudgetAllocations_LimitAllocations " +
                                              "Where idOwner = {2}", kbkString, limitAllocationYear, Id);

            var Type = "";
            if (SBP.SBPType == DbEnums.SBPType.GeneralManager)
                Type = String.Format("L.idValueType = {0} Or L.idValueType = {1}", (int)ValueType.JustifiedGRBS,
                               (int)ValueType.Bring);
            if (SBP.SBPType == DbEnums.SBPType.Manager)
                Type = String.Format("L.idValueType = {0}", (int)ValueType.JustifiedGRBS);


            var planLimitVolumeAppropriationsQuery = String.Format("Select {0}, " +
                                                                    "Null as value, " +
                                                                    "L.Value as regValue, " +
                                                                    "L.idValueType " +
                                                                   "From reg.LimitVolumeAppropriations L " +
                                                                   "Inner Join reg.EstimatedLine EL on EL.id = L.idEstimatedLine " +
                                                                   "Inner Join ref.FinanceSource FS on FS.id = EL.idFinanceSource " +
                                                                   "Where EL.idSBP = {8} And " +
                                                                            "FS.IdFinanceSourceType <> {10}" +
                                                                            " AND {7} " +
                                                                            "(L.idValueType = {1} Or L.idValueType = {6} or L.idValueType = {9}) and HasAdditionalNeed = 1 And " +
                                                                            "L.idHierarchyPeriod = {2} And " +
                                                                            "L.idPublicLegalFormation = {3} And " +
                                                                            "L.idBudget = {4} And " +
                                                                            "L.idVersion = {5}",
                                                                   estimatedLineKbkString, (int)ValueType.Plan, limitVolumeIdHierarchy, IdPublicLegalFormation, IdBudget, IdVersion, (int)ValueType.Bring,
                                                                   estimatedLineKbkRequieredConditions, idQuerySBP,(int)ValueType.JustifiedGRBS, (byte)FinanceSourceType.UnconfirmedFunds);

            var innerQuery = limitAllocationQuery + " Union All " + planLimitVolumeAppropriationsQuery;

            var cmd = String.Format("Select {0}, " +
                             "CAST(SUM(G.Value) as numeric(20,2) ) as value, " +
                             "CAST(SUM(CASE WHEN G.idValueType = {2} THEN G.regValue ELSE NULL END) as numeric(20,2) ) as JustifiedValue, " +
                             "CAST(SUM(CASE WHEN G.idValueType = {3} THEN G.regValue ELSE NULL END) as numeric(20,2) ) as bringValue, " +
                             "CAST(SUM(CASE WHEN G.idValueType = {4} THEN G.regValue ELSE NULL END) as numeric(20,2) ) as PlanValue " +
                             "From ({1}) G " +
                             (string.IsNullOrEmpty(kbkString) ? "" : "Group By {0} ") +
                             (isIncludeHaving ? "Having SUM(G.Value) > 0 " : String.Empty), kbkString, innerQuery, (int)ValueType.JustifiedGRBS, (int)ValueType.Bring, (int)ValueType.Plan);

            return cmd;
        }

        private string GetQueryForAllocations_Control_0519(ISBP_Blank blank, int idQuerySBP, DataContext context, bool isIncludeHaving = true)
        {

            var costLineProperties = blank.GetBlankCostMandatoryProperties().ToList();

            const String limitAllocationYear = "{0}";
            const String limitVolumeIdHierarchy = "{1}";

            var kbkString = costLineProperties.GetQueryString();
            var estimatedLineKbkString = costLineProperties.GetQueryString("EL");

            var estimatedLineKbkRequieredConditions = new StringBuilder();
            foreach (var costLineProperty in costLineProperties)
                estimatedLineKbkRequieredConditions.Append("EL." + costLineProperty + " IS NOT NULL And ");

            var limitAllocationQuery = String.Format("Select {0}, " +
                                                "{1} as value, " +
                                                "Null as regValue, " +
                                                "Null as idValueType " +
                                              "From tp.LimitBudgetAllocations_LimitAllocations " +
                                              "Inner Join ref.FinanceSource FS on FS.id = idFinanceSource " +
                                              "Where " +
                                                     "idOwner = {2} And FS.idFinanceSourceType <> {3} ", kbkString, limitAllocationYear, Id, (byte)FinanceSourceType.UnconfirmedFunds);

            var planLimitVolumeAppropriationsQuery = String.Format("Select {0}, " +
                                                                    "Null as value, " +
                                                                    "L.Value as regValue, " +
                                                                    "L.idValueType " +
                                                                   "From reg.LimitVolumeAppropriations L " +
                                                                   "Inner Join reg.EstimatedLine EL on EL.id = L.idEstimatedLine " +
                                                                   "Inner Join ref.FinanceSource FS on FS.id = EL.idFinanceSource " +
                                                                   "Where EL.idSBP = {8} And " +
                                                                            "FS.idFinanceSourceType <> {11} And " +
                                                                            "(L.idRegistratorEntity <> {9} Or L.idRegistrator Not In ({10}) ) And " +
                                                                            "{7} (((L.idValueType = {1} Or L.idValueType = {6}) and (HasAdditionalNeed = 0 or HasAdditionalNeed is null)) or (L.idValueType = {12} and HasAdditionalNeed = 1)) And " +
                                                                            "DateCommit is not null  And DateCommit <= '{13}' And " +
                                                                            "L.idHierarchyPeriod = {2} And " +
                                                                            "L.idPublicLegalFormation = {3} And " +
                                                                            "L.idBudget = {4} And " +
                                                                            "L.idVersion = {5}",
                                                                   estimatedLineKbkString, (int)ValueType.Plan, limitVolumeIdHierarchy, IdPublicLegalFormation, IdBudget, IdVersion, (int)ValueType.Bring,
                                                                   estimatedLineKbkRequieredConditions, idQuerySBP, EntityId, PrevVersionIds(context).GetQueryString(),
                                                                   (byte)FinanceSourceType.UnconfirmedFunds, (int)ValueType.JustifiedGRBS, Date.Date.ToString("s"));

            var innerQuery = limitAllocationQuery + " Union All " + planLimitVolumeAppropriationsQuery;

            var cmd = String.Format("Select {0}, " +
                             "CAST(SUM(G.Value) as numeric(20,2) ) as value, " +
                             "CAST(SUM(CASE WHEN G.idValueType = {2} THEN G.regValue ELSE NULL END) as numeric(20,2) ) as planValue, " +
                             "CAST(SUM(CASE WHEN G.idValueType = {3} THEN G.regValue ELSE NULL END) as numeric(20,2) ) as bringValue, " +
                             "CAST(SUM(CASE WHEN G.idValueType = {4} THEN G.regValue ELSE NULL END) as numeric(20,2) ) as JustifiedValue " +
                             "From ({1}) G " +
                             (string.IsNullOrEmpty(kbkString) ? "" : "Group By {0} ") +
                             (isIncludeHaving ? "Having SUM(G.Value) > 0 " : String.Empty), kbkString, innerQuery, (int)ValueType.Plan, (int)ValueType.Bring, (int)ValueType.JustifiedGRBS);

            return cmd;

        }

        private string GetQueryForAllocations_Control_0521(SBP_Blank blank, int idQuerySBP, DataContext context, bool isIncludeHaving = true)
        {
            var costLineProperties = blank.GetBlankCostMandatoryProperties().ToList();

            const String limitAllocationYear = "{0}";
            const String limitVolumeIdHierarchy = "{1}";

            var kbkString = costLineProperties.GetQueryString();
            var estimatedLineKbkString = costLineProperties.GetQueryString("EL");

            var estimatedLineKbkRequieredConditions = new StringBuilder();
            foreach (var costLineProperty in costLineProperties)
                estimatedLineKbkRequieredConditions.Append("EL." + costLineProperty + " IS NOT NULL And ");

            var limitAllocationQuery = String.Format("Select {0}, " +
                                                "{1} as value, " +
                                                "Null as regValue, " +
                                                "Null as idValueType " +
                                              "From tp.LimitBudgetAllocations_LimitAllocations " +
                                              "Where idOwner = {2}", kbkString, limitAllocationYear, Id);

 var planLimitVolumeAppropriationsQuery = String.Format("Select {0}, " +
                                                                    "Null as value, " +
                                                                    "L.Value as regValue, " +
                                                                    "L.idValueType " +
                                                                   "From reg.LimitVolumeAppropriations L " +
                                                                   "Inner Join reg.EstimatedLine EL on EL.id = L.idEstimatedLine " +
                                                                   "Inner Join ref.FinanceSource FS on FS.id = EL.idFinanceSource " +
                                                                   "Where EL.idSBP = {8} And " +
                                                                            "FS.idFinanceSourceType <> {10} And " +
                                                                            "{7} (L.idValueType = {1} Or L.idValueType = {6}) and HasAdditionalNeed = 1 And " +
                                                                            "L.idHierarchyPeriod = {2} And " +
                                                                            "L.idPublicLegalFormation = {3} And " +
                                                                            "L.idBudget = {4} And " +
                                                                            "L.idVersion = {5}",
                                                                   estimatedLineKbkString, (int)ValueType.Plan, limitVolumeIdHierarchy, IdPublicLegalFormation, IdBudget, IdVersion, (int)ValueType.Bring,
                                                                   estimatedLineKbkRequieredConditions, idQuerySBP, (int)ValueType.JustifiedGRBS, (byte)FinanceSourceType.UnconfirmedFunds);

            var innerQuery = limitAllocationQuery + " Union All " + planLimitVolumeAppropriationsQuery;

            var cmd = String.Format("Select {0}, " +
                             "CAST(SUM(G.Value) as numeric(20,2) ) as value, " +
                             "CAST(SUM(CASE WHEN G.idValueType = {2} THEN G.regValue ELSE NULL END) as numeric(20,2) ) as JustifiedValue, " +
                             "CAST(SUM(CASE WHEN G.idValueType = {3} THEN G.regValue ELSE NULL END) as numeric(20,2) ) as bringValue, " +
                             "CAST(SUM(CASE WHEN G.idValueType = {4} THEN G.regValue ELSE NULL END) as numeric(20,2) ) as PlanValue " +
                             "From ({1}) G " +
                             (string.IsNullOrEmpty(kbkString) ? "" : "Group By {0} ") +
                             (isIncludeHaving ? "Having SUM(G.Value) > 0 " : String.Empty), kbkString, innerQuery, (int)ValueType.JustifiedGRBS, (int)ValueType.Bring, (int)ValueType.Plan);

            return cmd;
        }

        private string GetQueryForAllocations_Control_0522(List<string> costLineProperties, int idQuerySBP, DataContext context, bool isIncludeHaving = true)
        {
            const String limitAllocationYear = "{0}";
            const String limitVolumeIdHierarchy = "{1}";

            var kbkString = costLineProperties.GetQueryString();
            var estimatedLineKbkString = costLineProperties.GetQueryString("EL");

            var estimatedLineKbkRequieredConditions = new StringBuilder();
            foreach (var costLineProperty in costLineProperties)
                estimatedLineKbkRequieredConditions.Append("EL." + costLineProperty + " IS NOT NULL And ");

            var limitAllocationQuery = String.Format("Select {0}, " +
                                                "{1} as value, " +
                                                "Null as regValue, " +
                                                "Null as idValueType " +
                                              "From tp.LimitBudgetAllocations_LimitAllocations " +
                                              "Where idOwner = {2}", kbkString, limitAllocationYear, Id);

            var Type = "";
            if (SBP.SBPType == DbEnums.SBPType.GeneralManager)
                Type = String.Format("(L.idValueType = {0} Or L.idValueType = {1})", (int)ValueType.JustifiedGRBS,(int)ValueType.Bring);
            if (SBP.SBPType == DbEnums.SBPType.Manager)
                Type = String.Format("L.idValueType = {0}", (int)ValueType.JustifiedGRBS);


            var planLimitVolumeAppropriationsQuery = String.Format("Select {0}, " +
                                                                    "Null as value, " +
                                                                    "L.Value as regValue, " +
                                                                    "L.idValueType " +
                                                                   "From reg.LimitVolumeAppropriations L " +
                                                                   "Inner Join reg.EstimatedLine EL on EL.id = L.idEstimatedLine " +
                                                                   "Inner Join ref.FinanceSource FS on FS.id = EL.idFinanceSource " +
                                                                   "Where EL.idSBP = {7} And " +
                                                                            "FS.idFinanceSourceType <> {10} And " +
                                                                            "(L.idRegistratorEntity <> {8} Or L.idRegistrator Not In ({9}) ) And " +
                                                                            "{6} {1} And HasAdditionalNeed = 0 And " +
                                                                            "L.idHierarchyPeriod = {2} And " +
                                                                            "L.idPublicLegalFormation = {3} And " +
                                                                            "L.idBudget = {4} And " +
                                                                            "L.idVersion = {5}",
                                                                   estimatedLineKbkString, Type, limitVolumeIdHierarchy, IdPublicLegalFormation, IdBudget, IdVersion,
                                                                   estimatedLineKbkRequieredConditions, idQuerySBP, EntityId, PrevVersionIds(context).GetQueryString(),
                                                                   (byte)FinanceSourceType.UnconfirmedFunds);

            var innerQuery = limitAllocationQuery + " Union All " + planLimitVolumeAppropriationsQuery;

            var cmd = String.Format("Select {0}, " +
                             "CAST(SUM(G.Value) as numeric(20,2) ) as value, " +
                             "CAST(SUM(CASE WHEN G.idValueType = {2} THEN G.regValue ELSE NULL END) as numeric(20,2) ) as JustifiedValue, " +
                             "CAST(SUM(CASE WHEN G.idValueType = {3} THEN G.regValue ELSE NULL END) as numeric(20,2) ) as bringValue " +
                             "From ({1}) G " +
                             (string.IsNullOrEmpty(kbkString) ? "" : "Group By {0} ") +
                             (isIncludeHaving ? "Having SUM(G.Value) > 0 " : String.Empty), kbkString, innerQuery, (int)ValueType.JustifiedGRBS, (int)ValueType.Bring);

            return cmd;
        }

        private Dictionary<LimitVolumeAppropriationResult, String> CheckLimitAllocations0512(DataContext context, SBP_Blank blank)
        {
            var limitErrors = new Dictionary<LimitVolumeAppropriationResult, string>();

            var query = GetQueryForAllocations(blank, IdSBP, context, false);
            var year = Budget.Year;

            for (int i = 0; i < 3; i++)
            {
                var idHierarchy = year.GetIdHierarchyPeriodYear(context);
                var yearQuery = String.Format(query, (i == 0) ? "OFG" : (i == 1 ? "PFG1" : "PFG2"), idHierarchy);

                var result = context.Database.SqlQuery<LimitVolumeAppropriationResult>(yearQuery).ToList();

                foreach (var r in result)
                {
                    r.Value = r.Value ?? 0;
                    r.PlanValue = r.PlanValue ?? 0;
                    r.BringValue = r.BringValue ?? 0;

                    if (r.Value + r.PlanValue < r.BringValue)
                    {
                        // �������� ����������� �� ����� ��������� ���������� ��� � ������ 
                        List<LimitVolumeAppropriationResult> ChecResult = CheckParents(context, r, result, blank);
                        if (ChecResult.Any() && !(ChecResult[0].Value + ChecResult[0].PlanValue < ChecResult[0].BringValue)) continue;

                        var estimatedLine = r.GetEstimatedLine(context);
                        r.Year = year;

                        limitErrors.Add( r,
                            String.Format("{4}, {0} - ����� ������� �� ��������� = {1}, �������������� ����� = {2}, �������� = {3}",
                                estimatedLine,
                                r.Value,
                                r.BringValue,
                                r.PlanValue - r.BringValue + r.Value,
                                year));
                    }
                }
                year++;
            }

            return limitErrors;
        }
        
        private SBP_Blank SBPBlank
        {
            get
            {
                if (_sbpBlank == null)
                {
                    if (SBP.SBPType == DbEnums.SBPType.GeneralManager)
                        _sbpBlank = SBP.SBP_Blank.FirstOrDefault(b => b.BlankType == BlankType.BringingGRBS && b.IdBudget == IdBudget);

                    if (SBP.SBPType == DbEnums.SBPType.Manager)
                    {
                        var parent = SBP.Parent;

                        if (parent != null)
                        {
                            _sbpBlank = parent.SBP_Blank.FirstOrDefault(b => b.BlankType == BlankType.BringingRBS && b.IdBudget == IdBudget);
                        }
                    }
                }

                return _sbpBlank;
            }
        }

        private SBP_Blank _sbpBlank;

        private List<LimitVolumeAppropriationResult> CheckParents(DataContext context, LimitVolumeAppropriationResult stingresut, List<LimitVolumeAppropriationResult> result, SBP_Blank blank)

        {
            // ������� ��������� ������������ �����
            var LastObligatoryBlank = blank.GetLastObligatoryBlank();

            // ������� �������� ���������� ������������� ������
            var Entity = context.Entity.FirstOrDefault(w => w.Name == LastObligatoryBlank);

            //������� ID ���� idParent
            var EntityField =
                context.EntityField.SingleOrDefault(s => s.IdEntity == Entity.Id && s.Name == "idParent");

            //��������� ��������� �� ��������� = ���������� ������������� ������
            var ParentsQuery = String.Format("select id from [dbo].[GetParents]({0},{1},'{2}','idParent',1)",
                                             stingresut.GetValue("Id" + LastObligatoryBlank), EntityField.Id, LastObligatoryBlank);
            var Parents = context.Database.SqlQuery<int>(ParentsQuery).ToList();

            var Data =
                result.Where(w =>
                             (
                                 (LastObligatoryBlank == "FinanceSource" && Parents.Contains(w.IdFinanceSource ?? 0)) ||
                                 (LastObligatoryBlank != "FinanceSource" &&
                                  w.IdFinanceSource == stingresut.IdFinanceSource)
                             ) &&
                             (
                                 (LastObligatoryBlank == "KFO" && Parents.Contains(w.IdKFO ?? 0)) ||
                                 (LastObligatoryBlank != "KFO" && w.IdKFO == stingresut.IdKFO)
                             ) &&
                             (
                                 (LastObligatoryBlank == "KVSR" && Parents.Contains(w.IdKVSR ?? 0)) ||
                                 (LastObligatoryBlank != "KVSR" && w.IdKVSR == stingresut.IdKVSR)
                             ) &&
                             (
                                 (LastObligatoryBlank == "RZPR" && Parents.Contains(w.IdRZPR ?? 0)) ||
                                 (LastObligatoryBlank != "RZPR" && w.IdRZPR == stingresut.IdRZPR)
                             ) &&
                             (
                                 (LastObligatoryBlank == "KCSR" && Parents.Contains(w.IdKCSR ?? 0)) ||
                                 (LastObligatoryBlank != "KCSR" && w.IdKCSR == stingresut.IdKCSR)
                             ) &&
                             (
                                 (LastObligatoryBlank == "KVR" && Parents.Contains(w.IdKVR ?? 0)) ||
                                 (LastObligatoryBlank != "KVR" && w.IdKVR == stingresut.IdKVR)
                             ) &&
                             (
                                 (LastObligatoryBlank == "KOSGU" && Parents.Contains(w.IdKOSGU ?? 0)) ||
                                 (LastObligatoryBlank != "KOSGU" && w.IdKOSGU == stingresut.IdKOSGU)
                             ) &&
                             (
                                 (LastObligatoryBlank == "DFK" && Parents.Contains(w.IdDFK ?? 0)) ||
                                 (LastObligatoryBlank != "DFK" && w.IdDFK == stingresut.IdDFK)
                             ) &&
                             (
                                 (LastObligatoryBlank == "DKR" && Parents.Contains(w.IdDKR ?? 0)) ||
                                 (LastObligatoryBlank != "DKR" && w.IdDKR == stingresut.IdDKR)
                             ) &&
                             (
                                 (LastObligatoryBlank == "DEK" && Parents.Contains(w.IdDEK ?? 0)) ||
                                 (LastObligatoryBlank != "DEK" && w.IdDEK == stingresut.IdDEK)
                             ) &&
                             (
                                 (LastObligatoryBlank == "BranchCode" && Parents.Contains(w.IdBranchCode ?? 0)) ||
                                 (LastObligatoryBlank != "BranchCode" && w.IdBranchCode == stingresut.IdBranchCode)
                             ) &&
                             (
                                 (LastObligatoryBlank == "CodeSubsidy" && Parents.Contains(w.IdCodeSubsidy ?? 0)) ||
                                 (LastObligatoryBlank != "CodeSubsidy" && w.IdCodeSubsidy == stingresut.IdCodeSubsidy)
                             )).ToList();
            var DataGroup = Data.GroupBy(g => new LimitVolumeAppropriationResult()
                                                  {
                                                      IdFinanceSource = g.IdFinanceSource,
                                                      IdKFO = g.IdKFO,
                                                      IdKVSR = g.IdKVSR,
                                                      IdRZPR = g.IdRZPR,
                                                      IdKCSR = g.IdKCSR,
                                                      IdKVR = g.IdKVR,
                                                      IdKOSGU = g.IdKOSGU,
                                                      IdDFK = g.IdDFK,
                                                      IdDKR = g.IdDKR,
                                                      IdDEK = g.IdDEK,
                                                      IdBranchCode = g.IdBranchCode,
                                                      IdCodeSubsidy = g.IdCodeSubsidy
                                                  }).Select( s=> new LimitVolumeAppropriationResult()
                                                                     {
                                                                         IdFinanceSource = s.Key.IdFinanceSource,
                                                                         IdKFO = s.Key.IdKFO,
                                                                         IdKVSR = s.Key.IdKVSR,
                                                                         IdRZPR = s.Key.IdRZPR,
                                                                         IdKCSR = s.Key.IdKCSR,
                                                                         IdKVR = s.Key.IdKVR,
                                                                         IdKOSGU = s.Key.IdKOSGU,
                                                                         IdDFK = s.Key.IdDFK,
                                                                         IdDKR = s.Key.IdDKR,
                                                                         IdDEK = s.Key.IdDEK,
                                                                         IdBranchCode = s.Key.IdBranchCode,
                                                                         IdCodeSubsidy = s.Key.IdCodeSubsidy,
                                                                         Value = s.Sum(ss => ss.Value ?? 0),
                                                                         PlanValue = s.Sum(ss => ss.PlanValue ?? 0),
                                                                         BringValue = s.Sum(ss => ss.BringValue ?? 0),
                                                                         JustifiedValue = s.Sum(ss => ss.JustifiedValue ?? 0),
                                                                     }).ToList();

            return DataGroup;
        }

        #endregion

        #endregion
    }
}
