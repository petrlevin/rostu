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
using Sbor.Reports.CostGoals;

namespace Sbor.Reports.Report
{
    [Report]
    public partial class CostGoals
    {
        public List<PrimeDataSet> MainData()
        {
            DataContext context = IoC.Resolve<DbContext>().Cast<DataContext>();

            var res = new List<PrimeDataSet>();

            res.Add(new PrimeDataSet()
                {
                    curDate = ByApproved.HasValue && ByApproved.Value ? DateReport.Value.ToShortDateString() : DateTime.Now.ToShortDateString(),
                });


            return res;
        }

        private Dictionary<int, int> dirHP;

        public List<TableSet> TableData()
        {
            DataContext context = IoC.Resolve<DbContext>().Cast<DataContext>();
            var sbps = context.SBP.Where(r => r.IdPublicLegalFormation == IdPublicLegalFormation).ToList();

            yearBudget = context.Budget.First(r => r.Id == IdBudget).Year;

            var lva = context.LimitVolumeAppropriations.
                              Where(r =>
                                    r.IdPublicLegalFormation == IdPublicLegalFormation &&
                                    r.IdBudget == IdBudget &&
                                    r.IdVersion == IdVersion &&
                                    r.IdValueType == (int) Sbor.DbEnums.ValueType.Justified
                                    && r.HasAdditionalNeed != true
                ).ToList().Select(s => new
                    {
                        tc = s.IdTaskCollection ?? 0,
                        sbp = GetFirstParentRBS(s.EstimatedLine.SBP, sbps).Id,
                        value = s.Value,
                        year = s.HierarchyPeriod.DateStart.Year
                    }).ToList();

            var tv = context.TaskVolume.Where(r =>
                                              r.IdPublicLegalFormation == IdPublicLegalFormation &&
                                              r.IdSystemGoalElement.HasValue &&
                                              !r.IdTerminator.HasValue).
                             Select(s => new
                                 {
                                     SystemGoalElement = s.SystemGoalElement.Id,
                                     SBP = s.SBP.Id,
                                     TaskCollection = s.TaskCollection.Id,
                                     year = s.HierarchyPeriod.DateStart.Year
                                 }).Distinct().ToList();

            var costs = tv.Select(s => new
                {
                    s.SystemGoalElement,
                    s.SBP,
                    s.year,
                    value = lva.Where(w=> w.sbp == s.SBP && w.tc == s.TaskCollection && w.year == s.year).Sum(su=> su.value)
                }).Where(w=> w.value > 0).ToList();

            gcost = (costs.GroupBy(g => new {g.SystemGoalElement, g.SBP, g.year}).
                          Select(g =>
                                 new CCost()
                                     {
                                         SystemGoalElement = g.Key.SystemGoalElement,
                                         SBP = g.Key.SBP,
                                         year = g.Key.year,
                                         value = g.Sum(s => s.value)
                                     })).ToList();


            goals = context.AttributeOfSystemGoalElement.
                            Where(r =>
                                  r.SystemGoalElement.IdPublicLegalFormation == IdPublicLegalFormation 
                                  && r.SystemGoalElement.IdVersion == IdVersion 
                                  && !r.IdTerminator.HasValue 
                                  && !r.SystemGoalElement.IdTerminator.HasValue
                                  ).
                            Select(s =>
                                   new
                                       {
                                           idsge = s.IdSystemGoalElement,
                                           idsge_p = s.IdSystemGoalElement_Parent,
                                           Caption = s.SystemGoalElement.SystemGoal.Caption,
                                           Type = s.ElementTypeSystemGoal.Caption,
                                           sbp = s.SBP.Caption,
                                           s.DateStart,
                                           s.DateEnd
                                       }).
                            ToList().
                            Select(s =>
                                   new CGoal()
                                       {
                                           idsge = s.idsge,
                                           idsge_p = s.idsge_p,
                                           Caption = s.Caption,
                                           Type = s.Type,
                                           datas =
                                               s.DateStart.ToShortDateString() + " - " + s.DateEnd.ToShortDateString(),
                                           sbp = s.sbp
                                       }).ToList();

            foreach (var goal in goals)
            {
                var indicators = context.GoalTarget.Where(t => t.IdSystemGoalElement == goal.idsge && !t.IdTerminator.HasValue).ToList();
                if (indicators.Any())
                {
                    goal.indicators = indicators.
                        Select(i => string.Format("- {0}", i.GoalIndicator.Caption)).
                        Aggregate((a, b) => a + ";" + b);

                }
            }

            decimal Cost1;
            decimal Cost2;
            decimal Cost3;
            var res = GetDataForRes(0, out Cost1, out Cost2, out Cost3);

            return res;
        }

        private List<CGoal> goals;
        private List<CCost> gcost;
        private int yearBudget;

        private List<TableSet> GetDataForRes(int idSGE, out decimal Cost1, out decimal Cost2, out decimal Cost3, string num = "")
        {

            List<TableSet> resSub0 = new List<TableSet>();

            IOrderedEnumerable<CGoal> curgoals;
            if (idSGE == 0)
            {
                curgoals = goals.Where(r => !r.idsge_p.HasValue).OrderBy(o => o.Caption);
            }
            else
            {
                curgoals = goals.Where(r => r.idsge_p == idSGE).OrderBy(o => o.Caption);
            }

            if (!curgoals.Any())
            {
                Cost1 = gcost.Where(r => r.SystemGoalElement == idSGE && r.year == yearBudget).Sum(s => s.value);
                Cost2 = gcost.Where(r => r.SystemGoalElement == idSGE && r.year == (yearBudget + 1)).Sum(s => s.value);
                Cost3 = gcost.Where(r => r.SystemGoalElement == idSGE && r.year == (yearBudget + 2)).Sum(s => s.value);

                return resSub0;

            }
            
            var inums = 1;

            Cost1 = 0;
            Cost2 = 0;
            Cost3 = 0;

            foreach (var curgoal in curgoals)
            {
                var sn = (num != string.Empty ? num + "." : "") + inums.ToString();

                decimal cost1;
                decimal cost2;
                decimal cost3;
                var resSub1 = GetDataForRes(curgoal.idsge, out cost1, out cost2, out cost3, sn);


                Cost1 = Cost1 + cost1;
                Cost2 = Cost2 + cost2;
                Cost3 = Cost3 + cost3;

                var rs = new TableSet()
                    {
                        c1 = string.Format("{0}  {1}", curgoal.Type, sn),
                        c2 = curgoal.Caption,
                        c3 = curgoal.datas,
                        c7 = curgoal.sbp,
                        c8 = curgoal.indicators,
                        Cost1 = cost1,
                        Cost2 = cost2,
                        Cost3 = cost3
                    };

                inums++;

                resSub0.Add(rs);
                resSub0 = resSub0.Concat(resSub1).ToList();
            }

            return resSub0;

        }

        /// <summary>
        /// первое вышестоящее СБП с типом «ГРБС» или «РБС» 
        /// </summary>
        /// <param name="sbp">СБП, от которого начинаем искать</param>
        /// <param name="lsbp"></param>
        /// <returns></returns>
        public SBP GetFirstParentRBS(SBP sbp, List<SBP> lsbp)
        {
            SBP ret = sbp;
            do
            {
                if (!ret.IdParent.HasValue)
                {
                    ret = null;
                    break;
                }
                ret = lsbp.FirstOrDefault(r => r.Id == ret.IdParent);
                if (ret.IdSBPType == (int)Sbor.DbEnums.SBPType.GeneralManager ||
                    ret.IdSBPType == (int)Sbor.DbEnums.SBPType.Manager)
                {
                    break;
                }
            } while (true);

            return ret;
        }

        public class CGoal
        {
            public int idsge;
            public int? idsge_p;
            public string Caption;
            public string Type;
            public string indicators;
            public string datas;
            public string sbp;
        }

        public class CCost
        {
            public int SBP;
            public int SystemGoalElement;
            public int year;
            public decimal value;
        }

    }

}
