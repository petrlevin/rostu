using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Globalization;
using System.Linq;
using Platform.BusinessLogic;
using Platform.BusinessLogic.ReportingServices.Reports;
using Platform.Common;
using Sbor.Reference;
using Sbor.Registry;
using Sbor.Reports.Reports.DirectionAndFundingOfDepartment;
using ValueType = Sbor.DbEnums.ValueType;

namespace Sbor.Reports.Report
{
    [Report]
    public partial class DirectionAndFundingOfDepartment
    {
        public List<DSHeader> DataSetHeader()
        {
            var context = IoC.Resolve<DbContext>().Cast<DataContext>();

            var header = (
                from p in context.Program.Where(w => w.Id == IdProgram)
                join jap in context.AttributeOfProgram.Where(w =>
                    (ByApproved == true && w.DateCommit <= DateReport && (w.DateTerminate > DateReport || !w.DateTerminate.HasValue))
                    || (ByApproved == false && !w.DateTerminate.HasValue))
                on p.Id equals jap.IdProgram into tmpap
                from ap in tmpap.DefaultIfEmpty()
                select new { ap, p }
            ).Take(1).ToList().Select(s => new DSHeader
            {
                PublicLegalFormationName = PublicLegalFormation.Caption,
                ProgramName = string.Format(
                    "«{0}» на {1}-{2} годы",
                    s.ap == null ? s.p.Caption : s.ap.Caption,
                    s.ap == null ? 0 : s.ap.DateStart.Year,
                    s.ap == null ? 0 : s.ap.DateEnd.Year
                ),
                Contractor = s.p.SBP.Organization.Description,
                CurrentDate = (ByApproved == true && DateReport.HasValue) ? DateReport.Value : DateTime.Now,
                ReportCaption = !string.IsNullOrEmpty(Caption) ? Caption : (
                    s.p.IdDocType == DocType.ProgramOfSBP
                    ? "ВЕДОМСТВЕННОЙ ЦЕЛЕВОЙ ПРОГРАММЫ"
                    : (
                            s.p.IdDocType == DocType.MainActivity
                            ? "ОСНОВНОГО МЕРОПРИЯТИЯ"
                            : (
                                    s.p.IdDocType == DocType.NonProgramActivity
                                    ? "НЕПРОГРАММНОЙ ДЕЯТЕЛЬНОСТИ"
                                    : ""
                              )
                      )
                ),
                IsRatingAdditionalNeeds = IsRatingAdditionalNeeds ?? false
            }).ToList();

            return header;
        }

        public List<DSMain> DataSetMain()
        {
            var context = IoC.Resolve<DbContext>().Cast<DataContext>();

            // сведения о Цели
            var goal = context.AttributeOfProgram.Where(w =>
                w.IdProgram == IdProgram
                && w.IdPublicLegalFormation == IdPublicLegalFormation
                && w.IdVersion == IdVersion
                && (
                    (ByApproved == true && w.DateCommit <= DateReport && (!w.DateTerminate.HasValue || w.DateTerminate > DateReport))
                    || (ByApproved == false && !w.IdTerminator.HasValue)
                )
            ).Select(s => new
            {
                Caption = s.GoalSystemElement.SystemGoal.Caption, 
                Id = s.IdGoalSystemElement,
                Year1 = s.DateStart.Year,
                Year2 = s.DateEnd.Year
            }).SingleOrDefault();

            if (goal == null) return new List<DSMain>();

            // Задачи
            var qTask = context.AttributeOfSystemGoalElement.Where(w =>
                w.IdPublicLegalFormation == IdPublicLegalFormation
                && w.IdVersion == IdVersion
                // && w.SystemGoalElement.IdProgram == IdProgram
                && (w.IdSystemGoalElement_Parent == goal.Id || w.IdSystemGoalElement == goal.Id)
                && (
                    (ByApproved == true && w.DateCommit <= DateReport && (!w.DateTerminate.HasValue || w.DateTerminate > DateReport))
                    || (ByApproved == false && !w.IdTerminator.HasValue)
                )
            ).Select(s => new
            {
                IdSystemGoalElement = s.IdSystemGoalElement, 
                CapSystemGoalElement = s.SystemGoalElement.SystemGoal.Caption
            });

            // получение данных из регистра объекмов
            var regTask = context.TaskVolume.Where(w =>
                w.IdPublicLegalFormation == IdPublicLegalFormation
                // && w.IdBudget == IdBudget
                && w.IdVersion == IdVersion
                && w.IdValueType == (byte)ValueType.Plan
                && (
                    (ByApproved == true && w.DateCommit <= DateReport && (!w.DateTerminate.HasValue || w.DateTerminate > DateReport))
                    || (ByApproved == false && !w.IdTerminator.HasValue)
                )
                && w.IdSystemGoalElement.HasValue
            ).Select(s => new
            {
                s.IdSystemGoalElement,
                s.IdTaskCollection,
                s.IdSBP,
                CapActivity = s.TaskCollection.Activity.Caption,
                s.HierarchyPeriod.DateStart.Year
            }).Join(
                qTask,
                a => a.IdSystemGoalElement, b => b.IdSystemGoalElement,
                (a, b) => new
                {
                    b.IdSystemGoalElement, 
                    b.CapSystemGoalElement, 
                    a.IdTaskCollection,
                    a.CapActivity,
                    IdSBP = a.IdSBP,
                    Year = a.Year
                }
            ).Distinct().ToList();

            // получение данных из регистра фин.средств
            var volumeAppropriationses = context.LimitVolumeAppropriations.
                                             Where(
                                                 w =>
                                                 w.IdPublicLegalFormation == IdPublicLegalFormation && w.IdBudget == IdBudget &&
                                                 w.IdVersion == IdVersion && (ByApproved == false || w.DateCommit <= DateReport)
                                                 && (IsRatingAdditionalNeeds == true || w.HasAdditionalNeed != true) &&
                                                 w.IdTaskCollection.HasValue);

            IQueryable<LimitVolumeAppropriations> limitVolumeAppropriationses;

            if (IdSourcesDataReports == (byte)DbEnums.SourcesDataReports.BudgetEstimates)
            {
                limitVolumeAppropriationses = volumeAppropriationses.
                                                      Where(r =>
                                                          r.IdValueType == (int)Sbor.DbEnums.ValueType.Justified &&
                                                          r.EstimatedLine.SBP.IdSBPType == (byte)Sbor.DbEnums.SBPType.TreasuryEstablishment);
            }
            else if (IdSourcesDataReports == (byte)DbEnums.SourcesDataReports.JustificationBudget)
            {
                limitVolumeAppropriationses = volumeAppropriationses.
                                                      Where(r =>
                                                          r.IdValueType == (int)Sbor.DbEnums.ValueType.JustifiedGRBS);
            }
            else if (IdSourcesDataReports == (byte)DbEnums.SourcesDataReports.InstrumentBalancingSourceEstimates)
            {
                limitVolumeAppropriationses = volumeAppropriationses.
                                                      Where(r =>
                                                          (r.IdValueType == (int)Sbor.DbEnums.ValueType.BalancingIFDB_Estimate || r.IdValueType == (int)Sbor.DbEnums.ValueType.Justified) &&
                                                          r.EstimatedLine.SBP.IdSBPType == (byte)Sbor.DbEnums.SBPType.TreasuryEstablishment);
            }
            else if (IdSourcesDataReports == (byte)DbEnums.SourcesDataReports.InstrumentBalancingSourceActivityOfSBP)
            {
                limitVolumeAppropriationses = volumeAppropriationses.
                                                      Where(r =>
                                                          (r.IdValueType == (int)Sbor.DbEnums.ValueType.BalancingIFDB_ActivityOfSBP || r.IdValueType == (int)Sbor.DbEnums.ValueType.JustifiedGRBS));
            }
            else
            {
                return new List<DSMain>();
            }

            var regLva = limitVolumeAppropriationses.Select(s => new
            {
                s.IdTaskCollection,
                IdSBP = (IdSourcesDataReports == (byte)DbEnums.SourcesDataReports.BudgetEstimates || IdSourcesDataReports == (byte)DbEnums.SourcesDataReports.InstrumentBalancingSourceEstimates) ? s.EstimatedLine.SBP.IdParent : s.EstimatedLine.IdSBP,
                Year = s.HierarchyPeriod.DateStart.Year,
                s.EstimatedLine,
                HasAdditionalNeed = s.HasAdditionalNeed ?? false,
                s.Value
            }).GroupBy(g => new
            {
                IdTaskCollection = g.IdTaskCollection.Value,
                IdSBP = g.IdSBP.Value,
                g.Year,
                FinanceSource = g.EstimatedLine.FinanceSource == null ? null : g.EstimatedLine.FinanceSource.Caption,
                KVSR = g.EstimatedLine.KVSR == null ? null : g.EstimatedLine.KVSR.Caption,
                RZPR = g.EstimatedLine.RZPR == null ? null : g.EstimatedLine.RZPR.Code,
                KCSR = g.EstimatedLine.KCSR == null ? null : g.EstimatedLine.KCSR.Code,
                KVR = g.EstimatedLine.KVR == null ? null : g.EstimatedLine.KVR.Code
            }).Select(s => new
            {
                s.Key,
                BaseValue = s.Sum(c => c.HasAdditionalNeed ? (decimal?)null : c.Value),
                Value = s.Sum(c => c.Value)
            }).
            Where(r => r.Value > 0).
                //Where(r => r.Key.IdTaskCollection == 110). //Тест
            ToList();

            // получение данных
            List<DSMain> res0 =
                regTask.GroupJoin(regLva,
                                  a => new { q1 = a.IdTaskCollection, q2 = a.IdSBP, q3 = a.Year },
                                  b0 => new { q1 = b0.Key.IdTaskCollection, q2 = b0.Key.IdSBP, q3 = b0.Key.Year },
                                  (a, tmp) => new { a, tmp })
                       .SelectMany(a => a.tmp.DefaultIfEmpty(), (a, b) => new DSMain
                       {
                           Id = goal.Id,
                           Caption = "Цель «" + goal.Caption + "»",
                           IdSystemGoalElement = a.a.IdSystemGoalElement,
                           CapSystemGoalElement = "Задача «" + a.a.CapSystemGoalElement + "»",
                           IdTaskCollection = a.a.IdTaskCollection,
                           CapActivity = a.a.CapActivity,
                           Year = a.a.Year,
                           FinanceSource = (b == null) ? null : b.Key.FinanceSource,
                           KVSR = (b == null) ? null : b.Key.KVSR,
                           RZPR = (b == null) ? null : b.Key.RZPR,
                           KCSR = (b == null) ? null : b.Key.KCSR,
                           KVR = (b == null) ? null : b.Key.KVR,
                           BaseValue = (b == null) ? (decimal?)null : b.BaseValue,
                           Value = (b == null) ? (decimal?)null : b.Value
                       }
                    ).ToList();

            List<DSMain> res = new List<DSMain>();

            if (res0.Any())
            {
                res = res0.
                    Where(r =>
                          (r.Value != 0 && r.Value != null) ||
                          !res0.Any(w =>
                                    w.IdTaskCollection == r.IdTaskCollection &&
                                    w.IdSystemGoalElement == r.IdSystemGoalElement &&
                                    (w.Value != 0 && w.Value != null))
                    ).ToList();

                // нумерация результата
                int cnt1 = 1;
                var g1 = res.GroupBy(g => new { g.IdSystemGoalElement, g.CapSystemGoalElement }).OrderBy(o => o.Key.CapSystemGoalElement);
                foreach (var t in g1)
                {
                    string rn1 = cnt1.ToString(CultureInfo.InvariantCulture);

                    int cnt2 = 1;
                    var g2 = t.GroupBy(g => new { g.IdTaskCollection, g.CapActivity }).OrderBy(o => o.Key.CapActivity);
                    foreach (var a in g2)
                    {
                        string rn2 = cnt2.ToString(CultureInfo.InvariantCulture);
                        foreach (var d in a)
                        {
                            d.RN1 = rn1;
                            d.RN2 = rn1 + "." + rn2;
                        }
                        cnt2++;
                    }
                    cnt1++;
                }

                // добавление отсутсвующих годов
                var existsYears = res.Select(s => s.Year).Distinct().ToArray();
                var firstRes = res.First();
                for (int y = goal.Year1; y <= goal.Year2; y++)
                {
                    if (!existsYears.Contains(y))
                    {
                        res.Add(firstRes.Clone(y, null, null));
                    }
                }
            }

            return res;
        }
    }
}
