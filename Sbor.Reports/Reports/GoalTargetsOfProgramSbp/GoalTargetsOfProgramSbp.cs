using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BaseApp.SystemDimensions;
using Platform.BusinessLogic;
using Platform.BusinessLogic.Activity.Controls;
using Platform.BusinessLogic.Common.Enums;
using Platform.BusinessLogic.ReportingServices.Reports;
using Platform.Common;
using System.Globalization;
using Sbor.Reference;
using Sbor.Registry;
using Sbor.Reports.GoalTargetsOfProgramSbp;

namespace Sbor.Reports.Report
{
    [Report]
    public partial class GoalTargetsOfProgramSbp
    {
        private int IdGoalSystemElement;

        public List<PrimeDataSet> MainData()
        {
            DataContext context = IoC.Resolve<DbContext>().Cast<DataContext>();

            var res = new List<PrimeDataSet>();
            var CaptionReport = string.Empty;

            var program = context.Program.Single(r => r.Id == IdProgram);
            var aprogram = context.AttributeOfProgram.FirstOrDefault(r => r.IdProgram == IdProgram &&
                                                                          ((!ByApproved.Value && !r.IdTerminator.HasValue) ||
                                                                          (ByApproved.Value && r.DateCommit <= DateReport &&
                                                                          (r.DateTerminate > DateReport || !r.IdTerminator.HasValue))));

            var aprogram2 = context.AttributeOfProgram.FirstOrDefault(r => r.IdProgram == IdProgram && !r.IdTerminator.HasValue);

            IdGoalSystemElement = (aprogram == null ? aprogram2.IdGoalSystemElement : aprogram.IdGoalSystemElement);

            var childgoals = context.SystemGoalElement.Any(r => r.IdProgram == IdGoalSystemElement);
            string sn5;

            if (childgoals)
            {
                sn5 = "Наименование цели, задачи, целевого показателя";
            }
            else
            {
                sn5 = "Наименование цели, целевого показателя";
            }

            res.Add(new PrimeDataSet()
            {
                Program = string.Format("\"{0}\" на {1} - {2} годы", program.Caption, aprogram == null ? 0 : aprogram.DateStart.Year, aprogram == null ? 0 : aprogram.DateEnd.Year),
                SBP = context.Organization.Single(o => o.Id == program.SBP.IdOrganization).Description,
                curDate = ByApproved.HasValue && ByApproved.Value ? DateReport.Value.ToShortDateString() : DateTime.Now.ToShortDateString(),
                sn5 = sn5
            });


            return res;
        }

        public List<TableSet> TableData()
        {
            var res = new List<TableSet>();

            DataContext context = IoC.Resolve<DbContext>().Cast<DataContext>();

            var aprogram = context.AttributeOfProgram.FirstOrDefault(r => r.IdProgram == IdProgram &&
                                                                          ((!ByApproved.Value && !r.IdTerminator.HasValue) ||
                                                                          (ByApproved.Value && r.DateCommit <= DateReport &&
                                                                          (r.DateTerminate > DateReport || !r.IdTerminator.HasValue))));

            var budgetyear = IoC.Resolve<SysDimensionsState>("CurentDimensions").Budget.Year;

            var sn6s = context.SystemGoalElement.Where(r => r.Id == IdGoalSystemElement && !r.IdTerminator.HasValue);
            if (!sn6s.Any())
            {
                return res;
            }

            var sn6 = sn6s.FirstOrDefault().SystemGoal;

            TableSet newTableSet;

            newTableSet = new TableSet()
            {
                typeline = 1,
                c2 = string.Format("Цель «{0}»", sn6.Caption),
            };
            res.Add(newTableSet);

            if (aprogram == null)
            {
                return res;
            }

            var goaltargets0 = context.GoalTarget
                                      .Where(r =>
                                             r.IdPublicLegalFormation == IdPublicLegalFormation &&
                                             ((!ByApproved.Value && !r.IdTerminator.HasValue) ||
                                              (ByApproved.Value && r.DateCommit <= DateReport &&
                                               (r.DateTerminate > DateReport || !r.IdTerminator.HasValue))))
                                      .ToList();

            var valuesGoalTargets = context.ValuesGoalTarget
                                           .Where(r =>
                                                  r.IdValueType == (int)Sbor.DbEnums.ValueType.Plan &&
                                                  ((!ByApproved.Value && !r.IdTerminator.HasValue) ||
                                                   (ByApproved.Value && r.DateCommit <= DateReport &&
                                                    (r.DateTerminate > DateReport || !r.IdTerminator.HasValue))))
                                           .ToList();

            var goalTargets = goaltargets0.Where(r => r.IdSystemGoalElement == aprogram.IdGoalSystemElement);

            var sn7s = goalTargets.
                               Join(valuesGoalTargets,
                                    goalTarget => goalTarget.Id, valuesGoalTarget => valuesGoalTarget.IdGoalTarget,
                                    (goalTarget, valuesGoalTarget) =>
                                    new
                                    {
                                        goalTarget,
                                        valuesGoalTarget.HierarchyPeriod.DateStart.Year,
                                        valuesGoalTarget.Value
                                    }).
                               GroupBy(k => k.goalTarget).
                               Select(s => new
                               {
                                   gt = s.Key,
                                   vp = s.Where(r => r.Year == budgetyear).Sum(r => r.Value),
                                   vp1 = s.Where(r => r.Year == budgetyear + 1).Sum(r => r.Value),
                                   vp2 = s.Where(r => r.Year == budgetyear + 2).Sum(r => r.Value)
                               });

            var numstr = 1;

            foreach (var gt in sn7s.ToList())
            {
                var sn7 = gt.gt.GoalIndicator;
                var sn8 = sn7.UnitDimension;

                decimal sn9 = gt.vp;
                decimal sn10 = gt.vp1;
                decimal sn11 = gt.vp2;

                var sn12 = gt.gt.GoalIndicator.CalculationFormula;

                newTableSet = new TableSet()
                {
                    typeline = 2,
                    c1 = numstr.ToString(),
                    c2 = sn7.Caption,
                    c3 = sn8.Caption,
                    c6 = Sbor.Logic.DocSGEMethod.ExcludeLastZeros(sn9),
                    c7 = Sbor.Logic.DocSGEMethod.ExcludeLastZeros(sn10),
                    c8 = Sbor.Logic.DocSGEMethod.ExcludeLastZeros(sn11),
                    c9 = sn12 == null ? "" : sn12.Caption
                };
                res.Add(newTableSet);
                numstr++;
            }

            var sn14s = context.AttributeOfSystemGoalElement.Where(r => r.IdSystemGoalElement_Parent == aprogram.IdGoalSystemElement &&
                ((!ByApproved.Value && !r.IdTerminator.HasValue) ||
                                              (ByApproved.Value && r.DateCommit <= DateReport &&
                                               (r.DateTerminate > DateReport || !r.IdTerminator.HasValue))));

            numstr = 1;

            foreach (var goalElement in sn14s.ToList())
            {

                newTableSet = new TableSet()
                {
                    typeline = 1,
                    c1 = numstr.ToString(),
                    c2 = string.Format("Задача {0}. «{1}»", numstr, goalElement.SystemGoalElement.SystemGoal.Caption),
                };
                res.Add(newTableSet);

                var targets = goaltargets0.Where(r => r.IdSystemGoalElement == goalElement.IdSystemGoalElement);

                var sn15s = targets.Join(valuesGoalTargets,
                                         goalTarget => goalTarget.Id, valuesGoalTarget => valuesGoalTarget.IdGoalTarget,
                                         (goalTarget, valuesGoalTarget) => new
                                         {
                                             goalTarget,
                                             valuesGoalTarget.HierarchyPeriod.DateStart.Year,
                                             valuesGoalTarget.Value
                                         })
                                   .Select(s => new
                                   {
                                       s.goalTarget,
                                       vp = (s.Year == budgetyear ? s.Value : 0),
                                       vp1 = (s.Year == budgetyear + 1 ? s.Value : 0),
                                       vp2 = (s.Year == budgetyear + 2 ? s.Value : 0)
                                   })
                                   .GroupBy(k => k.goalTarget)
                                   .Select(s => new
                                   {
                                       gt = s.Key,
                                       vp = s.Sum(r => r.vp),
                                       vp1 = s.Sum(r => r.vp1),
                                       vp2 = s.Sum(r => r.vp2)
                                   });

                var numstr2 = 1;

                foreach (var gt in sn15s.ToList())
                {

                    var sn15 = gt.gt.GoalIndicator;
                    var sn16 = sn15.UnitDimension;

                    decimal sn17 = gt.vp;
                    decimal sn18 = gt.vp1;
                    decimal sn19 = gt.vp2;

                    var sn20 = gt.gt.GoalIndicator.CalculationFormula;

                    newTableSet = new TableSet()
                    {
                        typeline = 2,
                        c1 = numstr + "." + numstr2,
                        c2 = sn15.Caption,
                        c3 = sn16.Caption,
                        c6 = Sbor.Logic.DocSGEMethod.ExcludeLastZeros(sn17),
                        c7 = Sbor.Logic.DocSGEMethod.ExcludeLastZeros(sn18),
                        c8 = Sbor.Logic.DocSGEMethod.ExcludeLastZeros(sn19),
                        c9 = sn20 == null ? "" : sn20.Caption
                    };
                    res.Add(newTableSet);

                    numstr2++;
                }

                numstr++;
            }


            return res;
        }
    }



}
