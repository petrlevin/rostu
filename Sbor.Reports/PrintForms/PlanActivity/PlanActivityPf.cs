using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using Platform.BusinessLogic;
using Platform.BusinessLogic.ReportingServices.PrintForms;
using Platform.Common;
using Platform.Common.Extensions;
using Platform.Utils.Extensions;
using Sbor.DbEnums;
using Sbor.Logic;

namespace Sbor.Reports.PrintForms.PlanActivity
{
    /// <summary>
    /// Для просмотра: http://localhost/platform3/Services/PrintForm.aspx?entityName=PlanActivity&printFormClassName=PlanActivityPf&docId=25
    ///                http://localhost/platform3/Services/PrintForm.aspx?entityName=PlanActivity&printFormClassName=PlanActivityPf&docId=26
    ///                http://localhost/platform3/Services/PrintForm.aspx?entityName=PlanActivity&printFormClassName=PlanActivityPf&docId=27
    /// </summary>
    [PrintForm(Caption = "План деятельности")]
    class PlanActivityPf : PrintFormBase
    {
        private readonly DataContext context;
        private readonly int _budgetYear ;
        private readonly bool _IsAdditionalNeed;

        public PlanActivityPf(int docId): base(docId)
        {
            context = IoC.Resolve<DbContext>().Cast<DataContext>();
            _budgetYear = context.PlanActivity.Where(w => w.Id == DocId).Select(s => s.Budget.Year).FirstOrDefault();
            //_IsAdditionalNeed = context.PlanActivity.Where(w => w.Id == DocId).Select(s => s.IsAdditionalNeed ?? false).FirstOrDefault();
        }

        //Шапка
        public IEnumerable<DSHeader> DataSetHeader()
        {
            IEnumerable<DSHeader> header = context.
                PlanActivity.Where(w => w.Id == DocId).Select(s =>
                    new DSHeader
                        {
                            Id = s.Id,
                            OrganizDescript = s.SBP.Organization.Description,
                            OFG = s.Budget.Year,
                            PFG1 = s.Budget.Year + 1,
                            PFG2 = s.Budget.Year + 2,
                            IsAdditionalNeed = s.IsAdditionalNeed ?? false,
                            CurrentDate = DateTime.Now // Дата формирования документа
                        });
            return header;
        }

        //Мероприятия
        public IEnumerable<DSActivity> DataSetActivity()
        {
            #region Показатель объёма
            List<DSActivity> activityAction = (
                from a in context.PlanActivity_Activity.Where(w=>w.IdOwner == DocId)
                join av in context.PlanActivity_ActivityVolume on a.Id equals av.IdMaster

                join jhp in context.HierarchyPeriod.Where(w => w.DateStart.Year >= _budgetYear && w.DateStart.Year <= _budgetYear + 2)
                    on av.IdHierarchyPeriod equals jhp.Id
                    into tmphp
                from hp in tmphp.DefaultIfEmpty()
                #region
                //join jhpOFG in context.HierarchyPeriod on 
                //    new { hpid = av.IdHierarchyPeriod,  a.Owner.Budget.Year  } equals
                //    new { hpid = jhpOFG.Id,             jhpOFG.DateStart.Year }
                //    into tmphpOFG 
                //from hpOFG in tmphpOFG.DefaultIfEmpty()

                //join jhpPFG1 in context.HierarchyPeriod on
                //    new { hpid = av.IdHierarchyPeriod, a.Owner.Budget.Year } equals
                //    new { hpid = jhpPFG1.Id, jhpPFG1.DateStart.Year }
                //    into tmphpPFG1
                //from hpPFG1 in tmphpPFG1.DefaultIfEmpty()

                //join jhpPFG2 in context.HierarchyPeriod on
                //    new { hpid = av.IdHierarchyPeriod, a.Owner.Budget.Year } equals
                //    new { hpid = jhpPFG2.Id, jhpPFG2.DateStart.Year }
                //    into tmphpPFG2
                //from hpPFG2 in tmphpPFG2.DefaultIfEmpty()
                #endregion
                 select new DSActivity
                    {
                        Number = 0,
                        ActivityTypeId = a.Activity.IdActivityType,
                        ActivityType = "",
                        ActivityId = a.Id,
                        ActivityName = a.Activity.FullCaption,
                        Contingent = a.Contingent.Caption,
                        IndicatorType = "Показатель объёма",
                        IndicatorName = a.IndicatorActivity.Caption, //Наименование объёмного показателя
                        UnitDimension = a.IndicatorActivity.UnitDimension.Symbol,
                        PeriodYear = hp.DateStart.Year, //SqlFunctions.StringConvert((double)hp.DateStart.Year).Replace(" ", "") + " г.",
                        Period = hp.IdParent != null ? hp.Caption.Substring(0, hp.Caption.Length - 5) : hp.Caption,
                        Plan = av.Volume,
                        AdditionalNeed = av.AdditionalVolume
                    }).ToList();
            activityAction.ForEach(f => f.ActivityType = ((ActivityType)f.ActivityTypeId).Caption());
            #endregion

            #region Показатель качества
            List<DSActivity> activityQuality = (
                from a in context.PlanActivity_Activity.Where(w => w.IdOwner == DocId)
                join iqa in context.PlanActivity_IndicatorQualityActivity on a.Id equals iqa.IdMaster
                join iqav in context.PlanActivity_IndicatorQualityActivityValue on iqa.Id equals iqav.IdMaster

                join hp in context.HierarchyPeriod.Where(w => w.DateStart.Year >= _budgetYear && w.DateStart.Year <= _budgetYear + 2) on
                    iqav.IdHierarchyPeriod equals hp.Id

                select new DSActivity
                {
                    Number = 1,
                    ActivityTypeId = a.Activity.IdActivityType,
                    ActivityType = "",
                    ActivityId = a.Id,
                    ActivityName = a.Activity.FullCaption,
                    Contingent = a.Contingent.Caption,
                    IndicatorType = "Показатель качества",
                    IndicatorName = iqa.IndicatorActivity.Caption, //Наименование показателя качества
                    UnitDimension = iqa.IndicatorActivity.UnitDimension.Symbol,
                    PeriodYear = hp.DateStart.Year, //SqlFunctions.StringConvert((double)hp.DateStart.Year).Replace(" ", "") + " г.",
                    Period = hp.IdParent != null ? hp.Caption.Substring(0, hp.Caption.Length - 5) : hp.Caption,
                    Plan = iqav.Value,
                    AdditionalNeed = iqav.AdditionalValue
                }).ToList();
            activityQuality.ForEach(f => f.ActivityType = ((ActivityType)f.ActivityTypeId).Caption());
            #endregion

            List<DSActivity> activity = activityAction.Concat(activityQuality).ToList();
            
            //Добавление отсутствующих годов
            activity.AddMissingInRange(_budgetYear, _budgetYear + 2, s => s.PeriodYear, (obj, y) => (obj.Clone(y, null, null)));

            activityAction.NumerateGroups(gBy => new {gBy.ActivityId, gBy.ActivityName, gBy.ActivityType},
                                          ord => ord.ActivityName + ord.ActivityType, (s, num) => s.Number = num);

            return activity;
        }

        //Мероприятия АУ/БУ (Деятельность бюджетных и автономных учреждений
        public IEnumerable<DSActivity> DataSetActivityAUBU()
        {
            List<DSActivity> activityAUBUAction = (
                from a in context.PlanActivity_ActivityAUBU.Where(w => w.IdOwner == DocId)
                join av in context.PlanActivity_ActivityVolumeAUBU on a.Id equals av.IdMaster

                join jhp in context.HierarchyPeriod.Where(w => w.DateStart.Year >= _budgetYear && w.DateStart.Year <= _budgetYear + 2)
                    on av.IdHierarchyPeriod equals jhp.Id
                    into tmphp
                from hp in tmphp.DefaultIfEmpty()

                select new DSActivity
                {
                    Number = 0,
                    ActivityTypeId = a.Activity.IdActivityType,
                    ActivityType = "",
                    ActivityId = a.Id,
                    ActivityName = a.Activity.FullCaption,
                    Contingent = a.Contingent.Caption,
                    IndicatorType = "Показатель объёма",
                    IndicatorName = a.IndicatorActivity.Caption, //Наименование объёмного показателя
                    UnitDimension = a.IndicatorActivity.UnitDimension.Symbol,
                    PeriodYear = hp.DateStart.Year, //SqlFunctions.StringConvert((double)hp.DateStart.Year).Replace(" ", "") + " г.",
                    Period = hp.IdParent != null ? hp.Caption.Substring(0, hp.Caption.Length - 5) : hp.Caption,
                    Plan = av.Volume,
                    AdditionalNeed = av.AdditionalVolume
                }).ToList();

            activityAUBUAction.ForEach(f => f.ActivityType = ((ActivityType)f.ActivityTypeId).Caption());

            //Добавление отсутствующих годов
            activityAUBUAction.AddMissingInRange(_budgetYear, _budgetYear + 2, s => s.PeriodYear, (obj, y) => (obj.Clone(y, null, null)));

            activityAUBUAction.NumerateGroups(gBy => new {gBy.ActivityId, gBy.ActivityName, gBy.ActivityType},
                                              ord => ord.ActivityName + ord.ActivityType, (s, num) => s.Number = num);
 
            return activityAUBUAction;
        }

        //Финансовое обеспечение деятельности
        public IEnumerable<DSFinancialProvision> DataSetFinancialProvision()
        {
            List<DSFinancialProvision> fp = (
                from kbk in context.PlanActivity_KBKOfFinancialProvision
                join p in context.PlanActivity_PeriodsOfFinancialProvision on kbk.Id equals p.IdMaster
                join hp in context.HierarchyPeriod on p.IdHierarchyPeriod equals hp.Id
                where kbk.IdOwner == DocId
                      && !kbk.IsMeansAUBU //Средства АУ/БУ = Ложь
                select new DSFinancialProvision
                    {
                        Number = 0,
                        KBKId = kbk.Id,
                        FinanceSource = kbk.IdFinanceSource != null ? kbk.FinanceSource.Caption : null,
                        KVSR = kbk.IdKVSR != null ? kbk.KVSR.Caption : null,
                        RZPR = kbk.IdRZPR != null ? kbk.RZPR.Code : null,
                        KCSR = kbk.IdKCSR != null ? kbk.KCSR.Code : null,
                        KVR = kbk.IdKVR != null ? kbk.KVR.Code : null,
                        KOSGU = kbk.IdKOSGU != null ? kbk.KOSGU.Code : null,
                        KFO = kbk.IdKFO != null ? kbk.KFO.Code : null,
                        CodeSubsidy = kbk.IdCodeSubsidy != null ? kbk.CodeSubsidy.Code : null,
                        DFK = kbk.IdDFK != null ? kbk.DFK.Code : null,
                        DKR = kbk.IdDKR != null ? kbk.DKR.Code : null,
                        DEK = kbk.IdDEK != null ? kbk.DEK.Code : null,
                        BranchCode = kbk.IdBranchCode != null ? kbk.BranchCode.Code : null,

                        PeriodYear = hp.DateStart.Year, //SqlFunctions.StringConvert((double)hp.DateStart.Year).Replace(" ", "") + " г.",

                        Period = hp.IdParent != null ? hp.Caption.Substring(0, hp.Caption.Length - 5) : hp.Caption,
                        Plan = p.Value.HasValue ? p.Value.Value / 1000 :(decimal?)null
                    }).ToList();

            //Добавление отсутствующих годов
            fp.AddMissingInRange(_budgetYear, _budgetYear + 2, s => s.PeriodYear, (obj, y) => (obj.Clone(y, null, null)));
            
            //Нумерация
            fp.NumerateGroups(gBy => new
                                {
                                    gBy.KBKId,
                                    ord =
                                        (gBy.FinanceSource ?? "$") +
                                        (gBy.KVSR ?? "$") +
                                        (gBy.RZPR ?? "$") +
                                        (gBy.KCSR ?? "$") +
                                        (gBy.KVR ?? "$") +
                                        (gBy.KOSGU ?? "$") +
                                        (gBy.KFO ?? "$") +
                                        (gBy.CodeSubsidy ?? "$") +
                                        (gBy.DFK ?? "$") +
                                        (gBy.DKR ?? "$") +
                                        (gBy.DEK ?? "$") +
                                        (gBy.BranchCode ?? "$")
                                }, o => o.ord, (s, num) => s.Number = num);

            return fp;
        }
            
        //Финансовое обеспечение деятельности бюджетных и автономных учреждений
        public IEnumerable<DSFinancialProvision> DataSetFinancialProvisionAUBU()
        {
            List<DSFinancialProvision> fpAUBU = (
                from kbk in context.PlanActivity_KBKOfFinancialProvision
                join p in context.PlanActivity_PeriodsOfFinancialProvision on kbk.Id equals p.IdMaster
                join hp in context.HierarchyPeriod on p.IdHierarchyPeriod equals hp.Id
                where kbk.IdOwner == DocId
                      && kbk.IsMeansAUBU //Средства АУ/БУ = Истина
                select new DSFinancialProvision
                {
                    Number = 0,
                    KBKId = kbk.Id,
                    FinanceSource = kbk.IdFinanceSource != null ? kbk.FinanceSource.Caption : null,
                    KVSR = kbk.IdKVSR != null ? kbk.KVSR.Caption : null,
                    RZPR = kbk.IdRZPR != null ? kbk.RZPR.Code : null,
                    KCSR = kbk.IdKCSR != null ? kbk.KCSR.Code : null,
                    KVR = kbk.IdKVR != null ? kbk.KVR.Code : null,
                    KOSGU = kbk.IdKOSGU != null ? kbk.KOSGU.Code : null,
                    KFO = kbk.IdKFO != null ? kbk.KFO.Code : null,
                    CodeSubsidy = kbk.IdCodeSubsidy != null ? kbk.CodeSubsidy.Code : null,
                    DFK = kbk.IdDFK != null ? kbk.DFK.Code : null,
                    DKR = kbk.IdDKR != null ? kbk.DKR.Code : null,
                    DEK = kbk.IdDEK != null ? kbk.DEK.Code : null,
                    BranchCode = kbk.IdBranchCode != null ? kbk.BranchCode.Code : null,

                    PeriodYear = hp.DateStart.Year, //SqlFunctions.StringConvert((double)hp.DateStart.Year).Replace(" ", "") + " г.",

                    Period = hp.IdParent != null ? hp.Caption.Substring(0, hp.Caption.Length - 5) : hp.Caption,
                    Plan = p.Value.HasValue ? p.Value.Value / 1000 : (decimal?)null
                }).ToList();

            //Добавление отсутствующих годов
            fpAUBU.AddMissingInRange(_budgetYear, _budgetYear + 2, s => s.PeriodYear, (obj, y) => (obj.Clone(y, null, null)));

            //Нумерация
            fpAUBU.NumerateGroups(gBy => new
                                {
                                    gBy.KBKId,
                                    ord =
                                            (gBy.FinanceSource ?? "$") +
                                            (gBy.KVSR ?? "$") +
                                            (gBy.RZPR ?? "$") +
                                            (gBy.KCSR ?? "$") +
                                            (gBy.KVR ?? "$") +
                                            (gBy.KOSGU ?? "$") +
                                            (gBy.KFO ?? "$") +
                                            (gBy.CodeSubsidy ?? "$") +
                                            (gBy.DFK ?? "$") +
                                            (gBy.DKR ?? "$") +
                                            (gBy.DEK ?? "$") +
                                            (gBy.BranchCode ?? "$")
                                }, o => o.ord, (s, num) => s.Number = num);

            return fpAUBU;
        }

        public List<DSBlankKBK> DataSetBlankKBK()
        {

            var doc = context.PlanActivity.SingleOrDefault(s => s.Id == DocId);
            var sbp = doc != null ? doc.SBP : null;

            if (sbp == null || doc == null)
                return null;

            var sbpBlank = context.SBP_Blank.FirstOrDefault(s => s.IdBudget == doc.IdBudget && (
                   (sbp.IdSBPType == (byte)SBPType.TreasuryEstablishment    && s.IdOwner == sbp.IdParent && s.IdBlankType == (byte)BlankType.BringingKU)
                || (sbp.IdSBPType == (byte)SBPType.IndependentEstablishment && s.IdOwner == sbp.IdParent && s.IdBlankType == (byte)BlankType.BringingAUBU)
                || (sbp.IdSBPType == (byte)SBPType.BudgetEstablishment      && s.IdOwner == sbp.IdParent && s.IdBlankType == (byte)BlankType.BringingAUBU)
            ));

            if (sbpBlank == null)
                return null;

            var blankCostProperties = sbpBlank.GetBlankCostProperties();

            var req = blankCostProperties["hidden"].Select(s => new DSBlankKBK() { KBK = s }).ToList();
            return req;
        }
    }
}
