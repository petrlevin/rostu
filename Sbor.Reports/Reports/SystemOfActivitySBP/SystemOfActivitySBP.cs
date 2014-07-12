using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Platform.BusinessLogic;
using Platform.BusinessLogic.ReportingServices.Reports;
using Platform.Common;
using System.Globalization;
using Sbor.Reference;
using Sbor.Registry;
using Sbor.Reports.SystemOfActivitySBP;

namespace Sbor.Reports.Report
{
    [Report]
    public class SystemOfActivitySBP
    {
        public int id { get; set; }
        public int idProgram { get; set; }
        public string Caption { get; set; }
        public int idPublicLegalFormation { get; set; } //ППО
        public int idBudget { get; set; } //Бюджет
        public int idVersion { get; set; } //Версия
        public bool byApproved { get; set; } //Строить отчет по утвержденным данным
        public DateTime DateReport { get; set; } //Дата отчета
        public bool RepeatTableHeader { get; set; } //Повторять заголовки  таблиц на каждой странице
        public bool HasAdditionalNeed { get; set; } //Оценка дополнительной потребности
        public bool IsShowActivityWithoutFinance { get; set; } //Выводить мероприятия не имеющие финансирования
        public int idSourcesDataReports { get; set; } //Источник данных для вывода ресурсного обеспечения в отчет

        public List<PrimeDataSet> MainData()
        {
            DataContext context = IoC.Resolve<DbContext>().Cast<DataContext>();

            var res = new List<PrimeDataSet>();
            var CaptionReport = string.Empty;

            var program = context.Program.Single(r => r.Id == idProgram);
            var aprogram = context.AttributeOfProgram.FirstOrDefault(r => r.IdProgram == idProgram && !r.IdTerminator.HasValue);

            if (Caption == string.Empty)
            {
                if (program.IdDocType == DocType.MainActivity)
                {
                    CaptionReport = "СИСТЕМА МЕРОПРИЯТИЙ ОСНОВНОГО МЕРОПРИЯТИЯ";
                }
                else if (program.IdDocType == DocType.NonProgramActivity)
                {
                    CaptionReport = "СИСТЕМА МЕРОПРИЯТИЙ НЕПРОГРАММНОЙ ДЕЯТЕЛЬНОСТИ";
                }
                else if (program.IdDocType == DocType.ProgramOfSBP)
                {
                    CaptionReport = "СИСТЕМА МЕРОПРИЯТИЙ ВЕДОМСТВЕННОЙ ЦЕЛЕВОЙ ПРОГРАММЫ";
                }
            }
            else
            {
                CaptionReport = Caption;
            }

            res.Add(new PrimeDataSet()
                {
                    Report = CaptionReport,
                    Program = string.Format("\"{0}\" на {1} - {2} годы", program.Caption, aprogram.DateStart.Year, aprogram.DateEnd.Year),
                    SBP = context.Organization.Single(o => o.Id == program.SBP.IdOrganization).Description,
                    curDate = byApproved ? DateReport.ToShortDateString() : DateTime.Now.ToShortDateString(),
                    PPO = context.PublicLegalFormation.Single(r => r.Id == idPublicLegalFormation).Caption
                });

            return res;
        }

        private Dictionary<int, int> dirHP;

        List<Program_ResourceMaintenance> programResourceMaintenances;
        List<LimitVolumeAppropriations> limitVolumeAppropriationses;
        List<TaskIndicatorQuality> indicatorQualities;
        List<C_Tc_Tv> taskVolumes;

        string emptykey = "000000000000000";

        public List<TableSet> TableData()
        {
            var res = new List<TableSet>();

            DataContext context = IoC.Resolve<DbContext>().Cast<DataContext>();

            var program = context.Program.Single(r => r.Id == idProgram);

            var yearbudget = context.Budget.Where(r => r.Id == idBudget).FirstOrDefault().Year;

            programResourceMaintenances = context.Program_ResourceMaintenance.
                                                  Where(r =>
                                                        r.IdTaskCollection.HasValue &&
                                                        r.IdValueType ==
                                                        (int) Sbor.DbEnums.ValueType.Plan &&
                                                        r.IdProgram == idProgram &&
                                                        (
                                                            // Фильтр.Источник данных для вывода ресурсного обеспечения в отчет  = Смета казенного учреждения или Деятельность ведомства  
                                                            // и источник финансирования <Сноска 11>  имеет  «Вид источника» равен «Местный бюджет»:  то  Период =  <Сноска 26>  
                                                            ((idSourcesDataReports == (byte) DbEnums.SourcesDataReports.BudgetEstimates ||
                                                              idSourcesDataReports == (byte) DbEnums.SourcesDataReports.JustificationBudget)
                                                             &&
                                                             r.FinanceSource.IdFinanceSourceType == (int) Sbor.DbEnums.FinanceSourceType.LocalBudget)
                                                            ||
                                                            // иначе :только периоды не входящие в период планирования  бюджета 
                                                            ((r.HierarchyPeriod.DateStart.Year < yearbudget) ||
                                                             (r.HierarchyPeriod.DateStart.Year > (yearbudget + 2)))
                                                        )
                                                        &&
                                                        ((!byApproved && !r.IdTerminator.HasValue) ||
                                                         (byApproved && r.DateCommit <= DateReport &&
                                                          (r.DateTerminate > DateReport ||
                                                           !r.IdTerminator.HasValue))))
                                                 .ToList();

            var volumeAppropriationses =
                context.LimitVolumeAppropriations.Where(
                    r =>
                    r.IdTaskCollection.HasValue &&
                    r.IdBudget == idBudget && ((!byApproved) || (byApproved && r.DateCommit <= DateReport)) &&
                    r.IdPublicLegalFormation == idPublicLegalFormation);

            if (idSourcesDataReports == (byte)DbEnums.SourcesDataReports.BudgetEstimates)
            {
                limitVolumeAppropriationses = volumeAppropriationses.
                                                      Where(r => 
                                                          r.IdValueType == (int)Sbor.DbEnums.ValueType.Justified &&
                                                          r.EstimatedLine.SBP.IdSBPType == (byte)Sbor.DbEnums.SBPType.TreasuryEstablishment)
                                                     .ToList();
            }
            else if (idSourcesDataReports == (byte)DbEnums.SourcesDataReports.JustificationBudget)
            {
                limitVolumeAppropriationses = volumeAppropriationses.
                                                      Where(r => 
                                                          r.IdValueType == (int)Sbor.DbEnums.ValueType.JustifiedGRBS)
                                                     .ToList();
            }
            else if (idSourcesDataReports == (byte)DbEnums.SourcesDataReports.InstrumentBalancingSourceEstimates)
            {
                limitVolumeAppropriationses = volumeAppropriationses.
                                                      Where(r =>
                                                          (r.IdValueType == (int)Sbor.DbEnums.ValueType.BalancingIFDB_Estimate || r.IdValueType == (int)Sbor.DbEnums.ValueType.Justified) &&
                                                          r.EstimatedLine.SBP.IdSBPType == (byte)Sbor.DbEnums.SBPType.TreasuryEstablishment)
                                                     .ToList();
            }
            else if (idSourcesDataReports == (byte)DbEnums.SourcesDataReports.InstrumentBalancingSourceActivityOfSBP)
            {
                limitVolumeAppropriationses = volumeAppropriationses.
                                                      Where(r =>
                                                          (r.IdValueType == (int)Sbor.DbEnums.ValueType.BalancingIFDB_ActivityOfSBP || r.IdValueType == (int)Sbor.DbEnums.ValueType.JustifiedGRBS))
                                                     .ToList();
            }
            else
            {
                return res;
            }

            taskVolumes = context.TaskVolume.Where(r =>
                                                   r.IdPublicLegalFormation == idPublicLegalFormation &&
                                                   ((!byApproved && !r.IdTerminator.HasValue) ||
                                                    (byApproved && r.DateCommit <= DateReport &&
                                                     (r.DateTerminate > DateReport ||
                                                      !r.IdTerminator.HasValue)))).
                                  Select(s => new C_Tc_Tv
                                      {
                                          Tc = s.TaskCollection,
                                          Tv = s
                                      })
                                 .ToList();

            indicatorQualities =
                context.TaskIndicatorQuality.Where(
                    r =>
                    r.IdPublicLegalFormation == idPublicLegalFormation && 
                    r.IdProgram == program.Id &&
                    ((!byApproved && !r.IdTerminator.HasValue) ||
                     (byApproved && r.DateCommit <= DateReport &&
                      (r.DateTerminate > DateReport || !r.IdTerminator.HasValue))) &&
                    r.IdValueType == (int) Sbor.DbEnums.ValueType.Plan).ToList();

            // <Сноска 5>	Цель	По полю «Программа» настроечной формы  найти в регистре «Атрибуты программы» неаннулированную запись, вывести значение из поля «Основная цель». Выводить в кавычках.
            var sn5s = context.AttributeOfProgram.Where(r =>
                                                        r.IdProgram == idProgram &&
                                                        ((!byApproved && !r.IdTerminator.HasValue) || (byApproved && r.DateCommit <= DateReport && (r.DateTerminate > DateReport || !r.IdTerminator.HasValue)))
                                                        );
            if (!sn5s.Any())
                return res;

            var firstYear = sn5s.FirstOrDefault().DateStart.Year;

            TableSet newTableSet;

            if (HasAdditionalNeed)
            {
                newTableSet = new TableSet()
                {
                    typeline = 15,
                    key = Sbor.Logic.DocSGEMethod.GetSKeyReportLine("00"),
                    YearHP = firstYear
                };
                res.Add(newTableSet);
            }

            dirHP = new Dictionary<int, int>();

            int i = 0;
            for (int year = sn5s.FirstOrDefault().DateStart.Year; year <= sn5s.FirstOrDefault().DateEnd.Year; year++)
            {
                dirHP.Add(year, i);

                newTableSet = new TableSet()
                {
                    typeline = 16,
                    key = Sbor.Logic.DocSGEMethod.GetSKeyReportLine("01"),
                    YearHP = year,
                    iHP = i + 8
                };
                res.Add(newTableSet);

                i = i + (HasAdditionalNeed ? 2 : 1);
            }

            var sn5 = sn5s.FirstOrDefault().GoalSystemElement;

            newTableSet = new TableSet()
                {
                    typeline = 4,
                    c2 = string.Format("Цель \"{0}\"", sn5.SystemGoal.Caption),
                    key = Sbor.Logic.DocSGEMethod.GetSKeyReportLine("02"),
                    YearHP = firstYear,
                    iHP = dirHP[firstYear] + 8
                };
            res.Add(newTableSet);

            // <Сноска 6>	Задачи	По полю «Программа» настроечной формы найти в регистре «Элементы СЦ» (поле «Программа») все неаннулированные записи. 
            // Далее по найденным записям по полю «Элемент СЦ» найти неаннулированные записи в регистре «Атрибуты элементов СЦ», если в найденных строках 
            // в поле «Вышестоящий» значение =  <Сноска 5>, то вывести значение поля «Элемент СЦ». Выводить в кавычках. 

            var sn6s =
                (from sge in context.SystemGoalElement.Where(r => 
                    r.IdProgram == idProgram &&
                    ((!byApproved && !r.IdTerminator.HasValue) || (byApproved && r.DateCommit <= DateReport && (r.DateTerminate > DateReport || !r.IdTerminator.HasValue)))
                    )
                join asge in context.AttributeOfSystemGoalElement.Where(r => 
                    ((!byApproved && !r.IdTerminator.HasValue) || (byApproved && r.DateCommit <= DateReport && (r.DateTerminate > DateReport || !r.IdTerminator.HasValue)))
                    ) on sge.Id equals asge.IdSystemGoalElement
                where asge.IdSystemGoalElement_Parent == sn5.Id
                orderby sge.SystemGoal.Caption
                select sge).ToList();

            int numline = 1;

            if (sn6s.Any())
            {
                foreach (var sn6 in sn6s.ToList())
                {
                    var prefixkey = "1" + (numline > 9 ? "" : "0") + numline.ToString();

                    newTableSet = new TableSet()
                    {
                        typeline = 3,
                        c0 = numline,
                        c2 = string.Format("Задача \"{0}\"", sn6.SystemGoal.Caption),
                        YearHP = firstYear,
                        key = Sbor.Logic.DocSGEMethod.GetSKeyReportLine(prefixkey),
                        iHP = dirHP[firstYear] + 8
                    };
                    res.Add(newTableSet);
                    
                    // По <Сноска 6>  найти неаннулированные записи в регистре «Объемы задач» (по полю «Элемент СЦ»). 
                    // Далее по отобранным строкам найти строку в регистре «Набор задач», вывести значение из поля «Мероприятие».

                    var sn7s1 = taskVolumes.Where(r => r.Tv.IdSystemGoalElement == sn6.Id).ToList();

                    int numline2 = 1;

                    var cTcSbps = sn7s1.Select(s => new { tc = s.Tc, sbp = s.Tv.SBP }).Distinct().Select(s => new C_Tc_Sbp { tc = s.tc, sbp = s.sbp }).OrderBy(o => o.tc.Activity.Caption).ToList();
                        
                    foreach (var sn7_tc in cTcSbps)
                    {
                        FillData(context, sn7_tc, program, sn5s, res, sn7s1.ToList(), numline, numline2, prefixkey + "000");

                        numline2 = numline2 + 1;
                    }

                    AddDataBottom(context, cTcSbps, program, res, prefixkey + "001", sn6.SystemGoal.Caption, 5);

                    numline = numline + 1;
                }

                var sn7s0 =
                    taskVolumes.Where(r => sn6s.Select(s => s.Id).Contains(r.Tv.IdSystemGoalElement ?? 0)).
                                Select(s => new C_Tc_Tv { Tc = s.Tc, Tv = s.Tv })
                               .OrderBy(o => o.Tc.Activity.Caption)
                               .ToList();

                var cTcSbps0 = sn7s0.Select(s => new { tc = s.Tc, sbp = s.Tv.SBP }).Distinct().Select(s => new C_Tc_Sbp { tc = s.tc, sbp = s.sbp }).OrderBy(o => o.tc.Activity.Caption).ToList();

                AddDataBottom(context, cTcSbps0, program, res, "999");

            }
            else
            {
                // Если данных по <Сноска 6>  нет, то по <Сноска 5> найти неаннулированные записи в регистре «Объемы задач» (по полю «Элемент СЦ»). 
                // Далее по отобранным строкам найти строку в регистре «Набор задач», вывести значение из поля «Мероприятие»
                var sn7s2 =
                    taskVolumes.Where(r => r.Tv.IdSystemGoalElement == sn5.Id).
                                Select(s => new C_Tc_Tv { Tc = s.Tc, Tv = s.Tv })
                               .OrderBy(o => o.Tc.Activity.Caption)
                               .ToList();

                var cTcSbps = sn7s2.Select(s => new { tc = s.Tc, sbp = s.Tv.SBP }).Distinct().Select(s => new C_Tc_Sbp { tc = s.tc, sbp = s.sbp }).OrderBy(o => o.tc.Activity.Caption).ToList();

                int numline2 = 1;

                foreach (var sn7_tc in cTcSbps)
                {
                    FillData(context, sn7_tc, program, sn5s, res, sn7s2.ToList(), numline, numline2, "k");

                    numline2 = numline2 + 1;
                }

                AddDataBottom(context, cTcSbps, program, res, "zzz");
            }


            return res;
        }

        private void AddDataBottom(DataContext context, List<C_Tc_Sbp> sn7s2, Program program, List<TableSet> res, string prefixkey, string sc2 = "", int tfirstline = 6, int tnextline = 7)
        {

            TableSet newTableSet;


            List<LimitVolumeAppropriations> volumeAppropriationses;
            if (idSourcesDataReports == (byte)DbEnums.SourcesDataReports.BudgetEstimates || idSourcesDataReports == (byte)DbEnums.SourcesDataReports.InstrumentBalancingSourceEstimates)
            {
                volumeAppropriationses = limitVolumeAppropriationses.Where(
                    w =>
                    sn7s2.Select(s => s.tc.Id).Distinct().Contains(w.IdTaskCollection ?? 0) &&
                    sn7s2.Select(s => s.sbp.Id).Distinct().Contains(w.EstimatedLine.SBP.IdParent ?? 0)).ToList();
            }
            else
            {
                volumeAppropriationses = limitVolumeAppropriationses.Where(
                    w =>
                    sn7s2.Select(s => s.tc.Id).Distinct().Contains(w.IdTaskCollection ?? 0) &&
                    sn7s2.Select(s => s.sbp.Id).Distinct().Contains(w.EstimatedLine.IdSBP)).ToList();
            }


            var costs = (from sn7 in sn7s2
                         join lva in
                             volumeAppropriationses on
                              new
                              {
                                  idtc = sn7.tc.Id,
                                  idsbp = sn7.sbp.Id
                              }
                              equals
                              new
                              {
                                  idtc = lva.IdTaskCollection ?? 0,
                                  idsbp = (lva.EstimatedLine.SBP.IdParent ?? 0)
                              }
                         group lva by new
                         {
                             fs = lva.EstimatedLine.FinanceSource,
                             hp = lva.HierarchyPeriod
                         }
                             into grlva
                             orderby grlva.Key.fs.Caption
                             select
                                 new
                                 {
                                     grlva.Key,
                                     sum = grlva.Where(w => !(w.HasAdditionalNeed ?? false)).Sum(s => s.Value),
                                     sumHAN = grlva.Sum(s => s.Value)
                                 }).ToList();

            /*
                     var costs = (from sn7 in sn7s2
                                    join lva in
                                        limitVolumeAppropriationses.Where(
                                            w => w.IdTaskCollection.HasValue && w.EstimatedLine.SBP.IdParent.HasValue).ToList() on
                                        new
                                            {
                                                idtc = sn7.tc.Id,
                                                idsbp = sn7.sbp.Id
                                            }
                                        equals
                                        new
                                            {
                                                idtc = lva.IdTaskCollection ?? 0,
                                                idsbp = (lva.EstimatedLine.SBP.IdParent ?? 0)
                                            }
                                             group lva by new
                                       {
                                           fs = lva.EstimatedLine.FinanceSource,
                                           hp = lva.HierarchyPeriod
                                       }
                                   into grlva
                                   orderby grlva.Key.fs.Caption
                                   select
                                       new
                                           {
                                               grlva.Key,
                                               sum = grlva.Where(w => !(w.HasAdditionalNeed ?? false)).Sum(s => s.Value),
                                               sumHAN = grlva.Sum(s => s.Value)
                                           }).ToList();
           */

            bool firstLine = true;
            // добавляем данные по Объёму ФС - в целом по программе
            foreach (var lva in costs)
            {
                newTableSet = new TableSet()
                    {
                        typeline = firstLine ? tfirstline : tnextline,
                        c2 = sc2,
                        c6 = lva.Key.fs.Caption,
                        c7 = "тыс. руб.",
                        c8 = Math.Round((lva.sum / 1000), 1).ToString(),
                        c9 = Math.Round((lva.sumHAN / 1000), 1).ToString(),
                        YearHP = lva.Key.hp.DateStart.Year,
                        key = Sbor.Logic.DocSGEMethod.GetSKeyReportLine(prefixkey, lva.Key.fs.Caption.Substring(0, 3))
                    };
                res.Add(newTableSet);
                firstLine = false;
            }

            var resman =
                from lva in
                    programResourceMaintenances
                group lva by new
                {
                    fs = lva.FinanceSource,
                    hp = lva.HierarchyPeriod
                }
                    into grlva
                    orderby grlva.Key.fs.Caption
                    select
                        new
                        {
                            grlva.Key,
                            sum = grlva.Where(w => !w.IsAdditionalNeed).Sum(s => s.Value),
                            sumHAN = grlva.Sum(s => s.Value)
                        };
            
            firstLine = true;
            // добавляем данные по Объёму ФС - в целом по программе
            foreach (var lva in resman.ToList())
            {
                newTableSet = new TableSet()
                {
                    typeline = firstLine ? tfirstline : tnextline,
                    c2 = sc2,
                    c6 = lva.Key.fs.Caption,
                    c7 = "тыс. руб.",
                    c8 = Math.Round((lva.sum / 1000), 1).ToString(),
                    c9 = Math.Round((lva.sumHAN / 1000), 1).ToString(),
                    YearHP = lva.Key.hp.DateStart.Year,
                    key = Sbor.Logic.DocSGEMethod.GetSKeyReportLine(prefixkey, lva.Key.fs.Caption.Substring(0, 3))
                };
                res.Add(newTableSet);
                firstLine = false;
            }


        }


        private class C_Tc_Sbp
        {
            public TaskCollection tc;
            public SBP sbp;
        }

        private class C_Tc_Tv
        {
            public TaskCollection Tc { get; set; }

            public TaskVolume Tv { get; set; }
        }

        private void FillData(DataContext context, C_Tc_Sbp sn7_tc, Program program, IQueryable<AttributeOfProgram> sn5s, List<TableSet> res, List<C_Tc_Tv> sn7s1, int numline, int numline2, string prefixkey)
        {
            var keyA = (numline2 > 9 ? "" : "0") + numline2.ToString();

            TableSet newTableSet;
            IEnumerable<LimitVolumeAppropriations> volumeAppropriationses;
            if (idSourcesDataReports == (byte)DbEnums.SourcesDataReports.BudgetEstimates || idSourcesDataReports == (byte)DbEnums.SourcesDataReports.InstrumentBalancingSourceEstimates)
            {
                volumeAppropriationses = limitVolumeAppropriationses.Where(r => r.IdTaskCollection == sn7_tc.tc.Id && r.EstimatedLine.SBP.IdParent == sn7_tc.sbp.Id);
            }
            else 
            {
                volumeAppropriationses = limitVolumeAppropriationses.Where(r => r.IdTaskCollection == sn7_tc.tc.Id && r.EstimatedLine.SBP.Id == sn7_tc.sbp.Id);
            }

            var costs = from lva in volumeAppropriationses
                        group lva by new
                            {
                                fs = lva.EstimatedLine.FinanceSource,
                                hp = lva.HierarchyPeriod
                            }
                        into grlva
                        orderby grlva.Key.fs.Caption
                        select
                            new
                                {
                                    grlva.Key,
                                    sum = grlva.Where(w => !(w.HasAdditionalNeed ?? false)).Sum(s => s.Value),
                                    sumHAN = grlva.Sum(s => s.Value)
                                };

            bool hasfinance = false;

            bool firstLine = true;

            var lvas = costs.Where(r => r.sum > 0 || r.sumHAN > 0).ToList();

            if (lvas.Any())
            {
                hasfinance = true;
                
                // добавляем данные по Объёму ФС
                foreach (var lva in lvas)
                {
                    newTableSet = new TableSet()
                        {
                            typeline = firstLine ? 1 : 2,
                            c0 = numline,
                            c1 = numline2,
                            c2 = firstLine ? string.Format("Мероприятие \"{0}\"", sn7_tc.tc.Activity.Caption) : "",
                            c3 = firstLine ? sn7_tc.sbp.Organization.Description : "",
                            c4 = firstLine ? sn5s.FirstOrDefault().DateStart.ToString("MM/yyyy") : "",
                            c5 = firstLine ? sn5s.FirstOrDefault().DateEnd.ToString("MM/yyyy") : "",
                            c6 = lva.Key.fs.Caption,
                            c7 = "тыс. руб.",
                            c8 = Math.Round((lva.sum/1000), 1).ToString(),
                            c9 = Math.Round((lva.sumHAN/1000), 1).ToString(),
                            YearHP = lva.Key.hp.DateStart.Year,
                            key =
                                Sbor.Logic.DocSGEMethod.GetSKeyReportLine(prefixkey + keyA,
                                                                          "1" + lva.Key.fs.Caption.Substring(0, 3)),
                            iHP = dirHP[lva.Key.hp.DateStart.Year] + 8
                        };
                    res.Add(newTableSet);
                    firstLine = false;
                }

            }

            var resman =
                from lva in
                    programResourceMaintenances.Where( r => r.IdTaskCollection == sn7_tc.tc.Id)
                group lva by new
                    {
                        fs = lva.FinanceSource,
                        hp = lva.HierarchyPeriod
                    }
                into grlva
                orderby grlva.Key.fs.Caption
                select
                    new
                        {
                            grlva.Key,
                            sum = grlva.Where(w => !w.IsAdditionalNeed).Sum(s => s.Value),
                            sumHAN = grlva.Sum(s => s.Value)
                        };

            firstLine = true;

            var lvasR = resman.Where(r => r.sum > 0 || r.sumHAN > 0).ToList();

            if (lvasR.Any())
            {
                hasfinance = true;

                // добавляем данные по Ресурсному обеспечению
                foreach (var lva in lvasR)
                {
                    newTableSet = new TableSet()
                        {
                            typeline = firstLine ? 1 : 2,
                            c0 = numline,
                            c1 = numline2,
                            c2 = firstLine ? string.Format("Мероприятие \"{0}\"", sn7_tc.tc.Activity.Caption) : "",
                            c3 = firstLine ? sn7_tc.sbp.Organization.Description : "",
                            c4 = firstLine ? sn5s.FirstOrDefault().DateStart.ToString("MM/yyyy") : "",
                            c5 = firstLine ? sn5s.FirstOrDefault().DateEnd.ToString("MM/yyyy") : "",
                            c6 = lva.Key.fs.Caption,
                            c7 = "тыс. руб.",
                            c8 = Math.Round((lva.sum/1000), 1).ToString(),
                            c9 = Math.Round((lva.sumHAN/1000), 1).ToString(),
                            YearHP = lva.Key.hp.DateStart.Year,
                            key =
                                Sbor.Logic.DocSGEMethod.GetSKeyReportLine(prefixkey + keyA,
                                                                          "1" + lva.Key.fs.Caption.Substring(0, 3)),
                            iHP = dirHP[lva.Key.hp.DateStart.Year] + 8
                        };
                    res.Add(newTableSet);
                    firstLine = false;
                }
            }

            if (!hasfinance && !IsShowActivityWithoutFinance)
            {
                return;
            }

            var tvg = from t in sn7s1.Where(r => r.Tc == sn7_tc.tc && r.Tv.IdSBP == sn7_tc.sbp.Id)
                      group t by new
                          {
                              t.Tc.Activity,
                              t.Tv.IndicatorActivity_Volume,
                              t.Tv.SBP,
                              hp = t.Tv.HierarchyPeriod
                          }
                      into tgr
                      select new
                          {
                              tgr.Key,
                              sum = tgr.Where(w => !w.Tv.IsAdditionalNeed).Sum(s => s.Tv.Value),
                              sumHAN = tgr.Sum(s => s.Tv.Value)
                          };

            // добавляем данные по Объёму задач
            foreach (var sn7v in tvg.ToList())
            {
                newTableSet = new TableSet()
                    {
                        typeline = firstLine ? 1 : 2,
                        c0 = numline,
                        c1 = numline2,
                        c2 = firstLine ? string.Format("Мероприятие \"{0}\"", sn7_tc.tc.Activity.Caption) : "",
                        c3 = firstLine ? sn7v.Key.SBP.Organization.Description : "",
                        c4 = firstLine ? sn5s.FirstOrDefault().DateStart.ToString("MM/yyyy") : "",
                        c5 = firstLine ? sn5s.FirstOrDefault().DateEnd.ToString("MM/yyyy") : "",
                        c6 = string.Format("Показатель объема \"{0}\"", sn7v.Key.IndicatorActivity_Volume.Caption),
                        c7 = sn7v.Key.IndicatorActivity_Volume.UnitDimension.Caption,
                        c8 = Sbor.Logic.DocSGEMethod.ExcludeLastZeros(sn7v.sum),
                        c9 = Sbor.Logic.DocSGEMethod.ExcludeLastZeros(sn7v.sumHAN),
                        YearHP = sn7v.Key.hp.DateStart.Year,
                        key = Sbor.Logic.DocSGEMethod.GetSKeyReportLine(prefixkey + keyA, "2" + sn7v.Key.IndicatorActivity_Volume.Caption.Substring(0,3)),
                        iHP = dirHP[sn7v.Key.hp.DateStart.Year] + 8
                    };
                res.Add(newTableSet);
                firstLine = false;
            }

            var taskIndicatorQualities = indicatorQualities.Where(r => r.IdTaskCollection == sn7_tc.tc.Id).ToList();

            var indtcs = taskIndicatorQualities.GroupBy(t =>
                                                        new
                                                            {
                                                                t.IndicatorActivity_Quality,
                                                                hp = t.HierarchyPeriod
                                                            })
                                               .OrderBy(tgr => tgr.Key.IndicatorActivity_Quality.Caption)
                                               .Select(tgr =>
                                                       new
                                                           {
                                                               tgr.Key,
                                                               sum =
                                                           tgr.Where(w => !w.IsAdditionalNeed).Sum(s => s.Value),
                                                               sumHAN = tgr.Sum(s => s.Value)
                                                           })
                                               .ToList();

            if (indtcs.Any())
            {
                var iq = 11;
                // добавляем данные по Показателям задач
                foreach (var indtc in indtcs.Select(s => s.Key.IndicatorActivity_Quality).Distinct().ToList())
                {
                    foreach (var tiq in indtcs.Where(r => r.Key.IndicatorActivity_Quality == indtc).ToList())
                    {
                        newTableSet = new TableSet()
                        {
                            typeline = firstLine ? 1 : 2,
                            c0 = numline,
                            c1 = numline2,
                            c2 = firstLine ? string.Format("Мероприятие \"{0}\"", sn7_tc.tc.Activity.Caption) : "",
                            c3 = firstLine ? sn7_tc.sbp.Organization.Description : "",
                            c4 = firstLine ? sn5s.FirstOrDefault().DateStart.ToString("MM/yyyy") : "",
                            c5 = firstLine ? sn5s.FirstOrDefault().DateEnd.ToString("MM/yyyy") : "",
                            c6 = string.Format("Показатель качества \"{0}\"", tiq.Key.IndicatorActivity_Quality.Caption),
                            c7 = tiq.Key.IndicatorActivity_Quality.UnitDimension.Caption,
                            c8 = Sbor.Logic.DocSGEMethod.ExcludeLastZeros(tiq.sum),
                            c9 = Sbor.Logic.DocSGEMethod.ExcludeLastZeros(tiq.sumHAN),
                            YearHP = tiq.Key.hp.DateStart.Year,
                            key = Sbor.Logic.DocSGEMethod.GetSKeyReportLine(prefixkey + keyA, "3" + iq.ToString()),
                            iHP = dirHP[tiq.Key.hp.DateStart.Year] + 8
                        };
                        res.Add(newTableSet);
                        firstLine = false;
                    }
                    iq++;
                }
            }
        }
    }

  

}
