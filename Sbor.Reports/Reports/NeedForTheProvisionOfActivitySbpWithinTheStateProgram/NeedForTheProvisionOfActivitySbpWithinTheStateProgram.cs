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
using Sbor.Reports.NeedForTheProvisionOfActivitySbpWithinTheStateProgram;

namespace Sbor.Reports.Report
{
    [Report]
    public partial class NeedForTheProvisionOfActivitySbpWithinTheStateProgram
    {
        private int IdGoalSystemElement;

        public List<PrimeDataSet> MainData()
        {
            DataContext context = IoC.Resolve<DbContext>().Cast<DataContext>();

            var res = new List<PrimeDataSet>();

            var program = context.Program.Single(r => r.Id == IdProgram);
            var aprogram = context.AttributeOfProgram.FirstOrDefault(r => r.IdProgram == IdProgram &&
                                                                          ((!ByApproved.Value && !r.IdTerminator.HasValue) ||
                                                                          (ByApproved.Value && r.DateCommit <= DateReport &&
                                                                          (r.DateTerminate > DateReport || !r.IdTerminator.HasValue))));

            string sn5;
            var ppo = context.PublicLegalFormation.FirstOrDefault(r => r.Id == IdPublicLegalFormation);
            if (ppo.IdBudgetLevel == -1879048162)
            {
                sn5 = "государственной";
            }
            else
            {
                sn5 = "муниципальной";
            }

            res.Add(new PrimeDataSet()
                {
                    Program = string.Format("«{0}» на {1} - {2} годы", program.Caption,
                                            aprogram == null ? 0 : aprogram.DateStart.Year,
                                            aprogram == null ? 0 : aprogram.DateEnd.Year),
                    SBP = context.Organization.Single(o => o.Id == program.SBP.IdOrganization).Description,
                    curDate = ByApproved.HasValue && ByApproved.Value ? DateReport.Value.ToShortDateString() : DateTime.Now.ToShortDateString(),
                    sn5 = sn5
                });


            return res;
        }

        private Dictionary<int, int> dirHP;

        private List<TableSet> res;
        int firstYear;
        private TableSet newTableSet;

        public List<TableSet> TableData()
        {
            res = new List<TableSet>();

            DataContext context = IoC.Resolve<DbContext>().Cast<DataContext>();

            var aprogram = context.AttributeOfProgram.FirstOrDefault(r => r.IdProgram == IdProgram &&
                                                                          ((!ByApproved.Value && !r.IdTerminator.HasValue) || 
                                                                          (ByApproved.Value && r.DateCommit <= DateReport && 
                                                                          (r.DateTerminate > DateReport || !r.IdTerminator.HasValue))));

            var aprogram2 = context.AttributeOfProgram.FirstOrDefault(r => r.IdProgram == IdProgram && !r.IdTerminator.HasValue);

            var y1 = (aprogram == null ? aprogram2.DateStart.Year : aprogram.DateStart.Year);
            var y2 = (aprogram == null ? aprogram2.DateEnd.Year : aprogram.DateEnd.Year);

            firstYear = context.HierarchyPeriod.Where(r =>
                                                      r.DateStart.Year == y1 &&
                                                      !r.IdParent.HasValue)
                               .FirstOrDefault().DateStart.Year;

            dirHP = new Dictionary<int, int>();

            int i = 0;
            for (int year = y1; year <= y2; year++)
            {
                dirHP.Add(year, i);

                newTableSet = new TableSet()
                {
                    typeline = 1,
                    key = Sbor.Logic.DocSGEMethod.GetSKeyReportLine("000"),
                    YearHP = year,
                    iHP = dirHP[year] + 3
                };
                res.Add(newTableSet);

                i++;
            }

            var idSubProgram = IdProgram ?? 0;

            bool isDemand;

            var subProgsForRes = GetSubProgsForRes(context, idSubProgram, out isDemand);

            if (isDemand)
            {
                res = res.Concat(subProgsForRes).ToList();
            }

            return res ;
              
        }

        private List<TableSet> GetSubProgsForRes(DataContext context, int idSubProgram, out bool isDemand, string prefix = "", string num = "")
        {
            List<TableSet> resSub0 = new List<TableSet>();

            List<TableSet> resAct = GetActivityForRes(context, idSubProgram, out isDemand, prefix, num);

            var subprogs = context.AttributeOfProgram.Where(r => r.IdParent == idSubProgram &&
                                                                 ((!ByApproved.Value && !r.IdTerminator.HasValue) ||
                                                                  (ByApproved.Value && r.DateCommit <= DateReport &&
                                                                   (r.DateTerminate > DateReport ||
                                                                    !r.IdTerminator.HasValue)))).
                                   Select(s => new
                                       {
                                           type = s.Program.DocType.Caption,
                                           prog = s
                                       }).
                                   OrderBy(o => o.prog.AnalyticalCodeStateProgram.AnalyticalCode).
                                   ToList();

            var i = 1;

            foreach (var subprog in subprogs)
            {
                bool isDemand0;

                string newNum = num + (num == "" ? "" : ".") + i.ToString();
                string newkey = prefix + (i > 9 ? "" : "0") + i.ToString() + ".";

                List<TableSet> resSub = GetSubProgsForRes(context, subprog.prog.IdProgram, out isDemand0, newkey, newNum);

                if (isDemand0)
                {
                    newTableSet = new TableSet()
                    {
                        typeline = 2,
                        key = Sbor.Logic.DocSGEMethod.GetSKeyReportLine("100", newkey),
                        c1 = newNum,
                        c2 = string.Format("{3} «{0}» на {1} - {2} годы",
                                           subprog.prog.Caption,
                                           subprog.prog.DateStart.Year,
                                           subprog.prog.DateEnd.Year,
                                           subprog.type),
                        YearHP = firstYear,
                        iHP = dirHP[firstYear] + 3
                    };
                    resSub0.Add(newTableSet);

                    i++;
                    
                    isDemand = true;
                    resSub0 = resSub0.Concat(resSub).ToList();
                }

            }

            return resAct.Concat(resSub0).ToList();
        }

        private List<TableSet> GetActivityForRes(DataContext context, int idSubProgram, out bool isDemand, string prefix, string num)
        {

            List<TableSet> resAct = new List<TableSet>();

            var activities =
                context.TaskVolume.
                        Where(r =>
                              ((!ByApproved.Value && !r.IdTerminator.HasValue) ||
                               (ByApproved.Value && r.DateCommit <= DateReport &&
                                (r.DateTerminate > DateReport ||
                                 !r.IdTerminator.HasValue)))).
                        Where(r =>
                              r.IdProgram == idSubProgram
                              &&
                              r.IdValueType == (int)Sbor.DbEnums.ValueType.Demand
                              ).
                        Select(s => new
                            {
                                k = new
                                    {
                                        a = s.TaskCollection.Activity,
                                        ind = s.IndicatorActivity_Volume.Caption,
                                        um = s.IndicatorActivity_Volume.UnitDimension.Caption,
                                        yearhp = s.HierarchyPeriod.DateStart.Year,
                                    },
                                tv = s
                            }).
                        ToList();

            var i = 1;

            if (!activities.Any())
            {
                isDemand = false;
                return resAct;
            }
            isDemand = true;

            var actdist = activities.Select(s => s.k.a).Distinct().OrderBy(o => o.FullCaption);
            foreach (var activity in actdist)
            {
                string newNum = num + (num == "" ? "" : ".") + i.ToString();
                string newkey = prefix + (i > 9 ? "" : "0") + i.ToString() + ".";

                var dataact = activities.Where(r => r.k.a == activity);

                foreach (var actval in dataact)
                {
                    newTableSet = new TableSet()
                    {
                        typeline = 3,
                        key = Sbor.Logic.DocSGEMethod.GetSKeyReportLine("100", newkey),
                        c1 = newNum,
                        c2 = activity.FullCaption,
                        c3 = string.Format("{0}, {1}", actval.k.ind, actval.k.um),
                        c4 = Sbor.Logic.DocSGEMethod.ExcludeLastZeros(actval.tv.Value),
                        YearHP = actval.k.yearhp,
                        iHP = dirHP[actval.k.yearhp] + 3
                    };
                    resAct.Add(newTableSet);
                }

                i++;
            }

            return resAct;
        }

        [ControlInitial(ExcludeFromSetup = true)]
        [Control(ControlType.Insert, Sequence.Before, ExecutionOrder = -1500)]
        public void FillCaption(DataContext context, ControlType ctType)
        {
            var ppo = context.PublicLegalFormation.FirstOrDefault(r => r.Id == IdPublicLegalFormation);
            if (ppo.IdBudgetLevel == -1879048162)
            {
                Caption = "ПОТРЕБНОСТЬ В ОКАЗАНИИ ГОСУДАРСТВЕННЫХ УСЛУГ (ВЫПОЛНЕНИИ РАБОТ) ГОСУДАРСТВЕННЫМИ УЧРЕЖДЕНИЯМИ В РАМКАХ ГОСУДАРСТВЕННОЙ ПРОГРАММЫ";
            }
            else
            {
                Caption = "ПОТРЕБНОСТЬ В ОКАЗАНИИ МУНИЦИПАЛЬНЫХ УСЛУГ (ВЫПОЛНЕНИИ РАБОТ) МУНИЦИПАЛЬНЫМИ УЧРЕЖДЕНИЯМИ В РАМКАХ ГОСУДАРСТВЕННОЙ ПРОГРАММЫ";
            }
        }  

    }
    
}
