using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Globalization;
using System.Linq;
using Platform.BusinessLogic;
using Platform.BusinessLogic.ReportingServices.Reports;
using Platform.Common;
using Sbor.DbEnums;
using Sbor.Reference;
using Sbor.Registry;
using Sbor.Reports.ResourceMaintenanceOfTheStateProgram;
using ValueType = Sbor.DbEnums.ValueType;

namespace Sbor.Reports.Report
{
    [Report]
    public partial class ResourceMaintenanceOfTheStateProgram
    {
        public List<DSHeader> DataSetHeader()
        {
            var context = IoC.Resolve<DbContext>().Cast<DataContext>();

            var programType = context.Program.Single(w => w.Id == IdProgram).IdDocType;

            var typeCaption = programType == DocType.StateProgram ? "государственной программы" : "подпрограммы государственной программы";

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
                StateProgramName = s.ap == null ? s.p.Caption : s.ap.Caption,
                ImplementationPeriodStart = s.ap == null ? 0 : s.ap.DateStart.Year,
                ImplementationPeriodEnd = s.ap == null ? 0 : s.ap.DateEnd.Year,
                Header = string.Format(
                    "«{0}» на {1}-{2} годы", 
                    s.ap == null ? s.p.Caption : s.ap.Caption, 
                    s.ap == null ? 0 : s.ap.DateStart.Year,
                    s.ap == null ? 0 : s.ap.DateEnd.Year
                ),
                Executive = s.p.SBP.Organization.Description,
                CurrentDate = (ByApproved == true && DateReport.HasValue) ? DateReport.Value : DateTime.Now,
                ReportCaption = !string.IsNullOrEmpty(Caption) ? Caption : (IsRatingAdditionalNeeds ?? false
                    ? "ОЦЕНКА ДОПОЛНИТЕЛЬНОЙ ПОТРЕБНОСТИ В СРЕДСТВАХ НА РЕАЛИЗАЦИЮ " + typeCaption.ToUpper()
                    : "РЕСУРСНОЕ ОБЕСПЕЧЕНИЕ РЕАЛИЗАЦИИ " + typeCaption.ToUpper()),
                BySource = BySource ?? false,
                YearCaption = IsRatingAdditionalNeeds == true
                    ? "Оценка дополнительной потребности в средствах областного бюджета (тыс.руб.), годы"
                    : "Расходы (тыс.руб.), годы",
                TypeCaption = typeCaption,
                HasNoFunds = HasNoFunds ?? false
            }).ToList();

            return header;
        }

        private class DSProg
        {
            public int ProgramId;
            public int? ParentProgramId;
            public string StrName;
            public string Contractor;
            public string SortKey;
            public int Year1;
            public int Year2;
        }

        // названия типов программ
        private readonly Dictionary<int, string> _types = new Dictionary<int, string>
            {
                { DocType.SubProgramSP,        "Подпрограмма" },
                { DocType.LongTermGoalProgram, "ДЦП" },
                { DocType.ProgramOfSBP,        "ВЦП" },
                { DocType.MainActivity,        "Основное мероприятие" },
                { DocType.SubProgramDGP,       "Подпрограмма ДЦП" }
            };

        private void AddGroupDataBySource(List<DSMain> source, List<DSMain> target)
        {
            var g3 = source.Where(w => !w.IsSourceGrp).GroupBy(g => new
            {
                g.SortKey,
                g.ProgramId,
                g.ParentProgramId,
                g.StrName,
                g.ActivityId,
                g.IsContractorGrp,
                g.Contractor,
                IsSourceGrp = true,
                SourceOrd = 0,
                SourceName = "всего",
                g.Year
            }).Select(s => new DSMain
            {
                SortKey = s.Key.SortKey,
                ProgramId = s.Key.ProgramId,
                ParentProgramId = s.Key.ParentProgramId,
                StrName = s.Key.StrName,
                ActivityId = s.Key.ActivityId,
                IsContractorGrp = s.Key.IsContractorGrp,
                Contractor = s.Key.Contractor,
                IsSourceGrp = s.Key.IsContractorGrp,
                SourceOrd = s.Key.SourceOrd,
                SourceName = s.Key.SourceName,
                Year = s.Key.Year,
                Value = s.Sum(c => c.Value)
            }).ToList();

            target.AddRange(g3);
        }

        private void AddGroupDataByContractor(List<DSMain> source, List<DSMain> target)
        {
            var ppd = _types.Single(s => s.Key == DocType.SubProgramDGP).Value;
            var vcp = _types.Single(s => s.Key == DocType.ProgramOfSBP).Value;
            var ma = _types.Single(s => s.Key == DocType.MainActivity).Value;

            var g2 = source.Where(w => 
                !w.IsContractorGrp 
                && !w.ActivityId.HasValue     // по мероприятиям не нужны под-итоги по исполнителям  
                // вообщн-то это конечно не красиво... нужно будет подумать
                && !w.StrName.StartsWith(ppd) // по Подпрограмма ДЦП не нужны под-итоги по исполнителям
                && !w.StrName.StartsWith(vcp) // по ВЦП не нужны под-итоги по исполнителям
                && !w.StrName.StartsWith(ma)  // по основным мероприятиям не нужны под-итоги по исполнителям
            ).GroupBy(g => new
            {
                g.SortKey,
                g.ProgramId,
                g.ParentProgramId,
                g.StrName,
                g.ActivityId,
                IsContractorGrp = true,
                Contractor = g.ParentProgramId.HasValue ? "всего" : "всего, в том числе:",
                g.IsSourceGrp,
                g.SourceOrd,
                g.SourceName,
                g.Year
            }).Select(s => new DSMain
            {
                SortKey = s.Key.SortKey,
                ProgramId = s.Key.ProgramId,
                ParentProgramId = s.Key.ParentProgramId,
                StrName = s.Key.StrName,
                ActivityId = s.Key.ActivityId,
                IsContractorGrp = s.Key.IsContractorGrp,
                Contractor = s.Key.Contractor,
                IsSourceGrp = s.Key.IsContractorGrp,
                SourceOrd = s.Key.SourceOrd,
                SourceName = s.Key.SourceName,
                Year = s.Key.Year,
                Value = s.Sum(c => c.Value)
            }).ToList();

            target.AddRange(g2);
        }

        private void AddGroupDataByProgram(List<DSProg> tree, List<DSMain> source, List<DSMain> target)
        {
//            var q1 = tree.Where(w => w.ProgramId == 113);
//
//            var q2 = source.Where(w => w.ParentProgramId == 113);

            List<DSMain> nextPrg = tree.Join(
                source,
                a => a.ProgramId, b => b.ParentProgramId,
                (a, b) => new DSMain
                {
                    SortKey = a.SortKey,
                    ProgramId = a.ProgramId,
                    ParentProgramId = a.ParentProgramId,
                    StrName = a.StrName,
                    ActivityId = null,
                    IsContractorGrp = false,
                    // мероприятия на программу относим на исполнителя программы, иначе исполнителя берем из нижестоящеих записей
                    Contractor = b.ActivityId.HasValue ? a.Contractor : b.Contractor,
                    ContractorOrd = (b.ActivityId.HasValue ? a.Contractor : b.Contractor) == a.Contractor ? 0 : 1, // это чтобы исполнитель по программе оказывался первым
                    IsSourceGrp = b.IsSourceGrp,
                    SourceOrd = b.SourceOrd,
                    SourceName = b.SourceName,
                    Year = b.Year,
                    Value = b.Value
                }
            ).ToList();

            // исли нет денег по исполнителю текщей программы, то нужно дополнить данные пустой строкой
            var addreq = nextPrg.GroupBy(g => g.ProgramId).Where(w => w.All(a => a.ContractorOrd != 0)).SelectMany(s => s).ToList();
            if (addreq.Any()) 
            {
                List<DSMain> nextPrg0 = tree.Join(
                    addreq,
                    a => a.ProgramId, b => b.ProgramId,
                    (a, b) => new DSMain
                    {
                        SortKey = a.SortKey,
                        ProgramId = a.ProgramId,
                        ParentProgramId = a.ParentProgramId,
                        StrName = a.StrName,
                        ActivityId = null,
                        IsContractorGrp = false,
                        ContractorOrd = 0, // это чтобы исполнитель по программе оказывался первым
                        Contractor = a.Contractor,
                        IsSourceGrp = true,
                        SourceOrd = 0,
                        SourceName = "всего",
                        Year = b.Year,
                        Value = null
                    }
                ).Distinct().ToList();

                nextPrg.AddRange(nextPrg0);
            }

            if (nextPrg.Any())
            {
                // добавляем полученное
                target.AddRange(nextPrg);

                // по вышестоящим
                AddGroupDataByProgram(tree, nextPrg, target);
            }
        }

        private void numHier(List<DSMain> list, int? IdParent = null, string prefix = "")
        {
            var data = list.Where(w => w.ParentProgramId == IdParent).ToList();
            if (!data.Any()) return;

            var gdata =
                data.GroupBy(s => new { s.ProgramId, s.ParentProgramId, s.SortKey })
                    .Distinct()
                    .OrderBy(o => o.Key.SortKey);

            int cnt = 1;
            foreach (var g in gdata)
            {
                var nn = prefix + cnt.ToString("D5",CultureInfo.InvariantCulture) + ".";
                foreach (var d in g)
                {
                    d.SortKey = nn;
                }
                if (g.Key.ProgramId != null) numHier(list, g.Key.ProgramId, nn);
                cnt++;
            }
        }

        private void AddNotExistsYears(List<DSMain> data, DSProg prg)
        {
            if (data.Any())
            {
                var existsYears = data.Select(s => s.Year).Distinct().ToArray();
                var firstRes = data.First(f => f.ProgramId == prg.ProgramId);
                for (int y = prg.Year1; y <= prg.Year2; y++)
                {
                    if (!existsYears.Contains(y))
                    {
                        data.Add(firstRes.Clone(y, null));
                    }
                }
            }
        }

        public List<DSMain> DataSetMain()
        {
            var context = IoC.Resolve<DbContext>().Cast<DataContext>();

            // это результат
            var res = new List<DSMain>();

            // получение данных из регистра программ и аттрибутов
            var regAllProg = context.AttributeOfProgram.Where(w => 
                w.IdPublicLegalFormation == IdPublicLegalFormation
                && w.IdVersion == IdVersion
                && (
                    (ByApproved == true && w.DateCommit <= DateReport && (!w.DateTerminate.HasValue || w.DateTerminate > DateReport))
                    || (ByApproved == false && !w.IdTerminator.HasValue)
                )
            ).Select(s => new {
                p = s.Program, 
                pa = s, 
                sbp = s.Program.SBP, 
                org = s.Program.SBP.Organization,
                code = (s.AnalyticalCodeStateProgram == null ? "" : s.AnalyticalCodeStateProgram.AnalyticalCode)
            }).ToList();

            // получаем список программ нужной ветки иерархии
            var tmp = new List<int>();
            var next = regAllProg.Where(s => s.p.Id == IdProgram).ToList();
            while (next.Any())
            {
                var tmp0 = next.Select(s => s.p.Id).ToList();
                tmp.AddRange(tmp0);
                next = regAllProg.Where(s => tmp0.Contains(s.pa.IdParent ?? 0)).ToList();
            }

            // список выбранных программ
            int[] ids = tmp.ToArray();

            // получаем выбранную ветку иерархии програм
            var regProg = regAllProg.Where(w => ids.Contains(w.p.Id)).Select(s => new DSProg {
                ProgramId = s.p.Id,
                ParentProgramId = s.p.Id == IdProgram ? null : s.pa.IdParent,
                StrName = 
                    (s.p.Id == IdProgram ? "" : (_types.Where(t => t.Key == s.p.IdDocType).Select(a => a.Value + " ").SingleOrDefault() ?? "")) 
                    +  "«" + s.pa.Caption + "» "
                    + "на " + s.pa.DateStart.Year.ToString(CultureInfo.InvariantCulture) 
                    + " - " + s.pa.DateEnd.Year.ToString(CultureInfo.InvariantCulture) + " годы",
                Contractor = string.IsNullOrEmpty(s.org.Description) ? s.org.Caption : s.org.Description,
                SortKey = s.code, // s.pa.Caption,
                Year1 = s.pa.DateStart.Year,
                Year2 = s.pa.DateEnd.Year
            }).ToList();

            // это выбранная ГП
            DSProg root = regProg.SingleOrDefault(s => s.ProgramId == IdProgram);
            if (root == null) return res;

            // получение данных из регистра объекмов
            var regTask = context.TaskVolume.Where(w =>
                w.IdPublicLegalFormation == IdPublicLegalFormation
                && w.IdVersion == IdVersion
                && w.IdValueType == (byte)ValueType.Plan
                && (
                    (ByApproved == true && w.DateCommit <= DateReport && (!w.DateTerminate.HasValue || w.DateTerminate > DateReport))
                    || (ByApproved == false && !w.IdTerminator.HasValue)
                )
                && w.IdProgram.HasValue
                && ids.Contains(w.IdProgram.Value)
            ).Select(s => new {
                s.IdProgram,
                s.IdTaskCollection,
                CapActivity = s.TaskCollection.Activity.FullCaption,
                s.IdSBP,
                Contractor = string.IsNullOrEmpty(s.SBP.Organization.Description) ? s.SBP.Organization.Caption : s.SBP.Organization.Description,
                s.HierarchyPeriod.DateStart.Year
            }).GroupBy(g => new {
                IdPrg = g.IdProgram,
                IdTask = g.IdTaskCollection,
                g.CapActivity, 
                g.IdSBP,
                g.Contractor,
                g.Year
            }).Select(s => s.Key).ToList(); // сгруппировать по... программе, мероприятию, сбп, году периода

            // фильтр по источникам
            int[] idsrc = FinanceSource.Select(s => s.Id).ToArray();
            bool isFilterSource = idsrc.Any();

            // года бюджета
            var years = new List<int>();
            for (int y = root.Year1; y <= root.Year2; y++)
            {
                years.Add(y);                
            }
            var budgetYears = years.ToArray();


            Func<LimitVolumeAppropriations, bool> filterByReportSource;
            bool needParent = false;
            switch (SourcesDataReports)
            {
                case DbEnums.SourcesDataReports.BudgetEstimates:
                    filterByReportSource = w => w.IdValueType == (byte) ValueType.Justified && w.EstimatedLine.SBP.IdSBPType == (byte)SBPType.TreasuryEstablishment;
                    needParent = true;
                    break;
                case DbEnums.SourcesDataReports.JustificationBudget:
                    filterByReportSource = w => w.IdValueType == (byte)ValueType.JustifiedGRBS; break;
                case DbEnums.SourcesDataReports.InstrumentBalancingSourceEstimates:
                    filterByReportSource = w => (w.IdValueType == (byte)ValueType.Justified || w.IdValueType == (byte)ValueType.BalancingIFDB_Estimate) && w.EstimatedLine.SBP.IdSBPType == (byte)SBPType.TreasuryEstablishment;
                    needParent = true;
                    break;
                case DbEnums.SourcesDataReports.InstrumentBalancingSourceActivityOfSBP:
                    filterByReportSource = w => w.IdValueType == (byte)ValueType.JustifiedGRBS || w.IdValueType == (byte)ValueType.BalancingIFDB_ActivityOfSBP; break;
                default: filterByReportSource = w => w.IdValueType == (byte)ValueType.Justified; break;
            }

            // получение данных из регистра фин.средств
            int[] idtsks = regTask.Select(s => s.IdTask).Distinct().ToArray();
            var regLva = context.LimitVolumeAppropriations.Where(w =>
                w.IdPublicLegalFormation == IdPublicLegalFormation
                && w.IdBudget == IdBudget
                && w.IdVersion == IdVersion
                && (ByApproved == false || w.DateCommit <= DateReport)
                //&& w.EstimatedLine.SBP.IdSBPType == (byte)SBPType.TreasuryEstablishment // КУ
                //&& w.EstimatedLine.SBP.IdParent.HasValue
                && (w.HasAdditionalNeed ?? false) == IsRatingAdditionalNeeds
                && (!isFilterSource || idsrc.Contains(w.EstimatedLine.IdFinanceSource ?? 0))
                && w.IdTaskCollection.HasValue
                && budgetYears.Contains(w.HierarchyPeriod.DateStart.Year)
                && idtsks.Contains(w.IdTaskCollection ?? 0)
            ).ToList().Where(w => filterByReportSource(w)).Select(s => new Money
            {
                IdPrg = (int?)null,
                IdTask = s.IdTaskCollection ?? 0,
                CapActivity = s.TaskCollection.Activity.FullCaption,
                IdFinanceSource = s.EstimatedLine.IdFinanceSource.HasValue ? s.EstimatedLine.FinanceSource : null,
                IdSBP = needParent ? s.EstimatedLine.SBP.IdParent : s.EstimatedLine.IdSBP,
                Year = s.HierarchyPeriod.DateStart.Year,
                Value = s.Value
            }).ToList();

            // Данные из регистра ресурсного обеспечения
            var regPrm = context.Program_ResourceMaintenance.Where(w =>
                w.IdPublicLegalFormation == IdPublicLegalFormation
                && w.IdVersion == IdVersion
                && ((ByApproved == true && w.DateCommit <= DateReport && (!w.DateTerminate.HasValue || w.DateTerminate > DateReport))
                    || (ByApproved == false && !w.IdTerminator.HasValue))
                && w.IdValueType == (byte)ValueType.Plan
                && w.IsAdditionalNeed == IsRatingAdditionalNeeds
                && (!isFilterSource || idsrc.Contains(w.IdFinanceSource ?? 0))
                && w.IdTaskCollection.HasValue
                && ids.Contains(w.IdProgram)
                && budgetYears.Contains(w.HierarchyPeriod.DateStart.Year)
            ).Select(s => new Money
            {
                IdPrg = (int?)s.IdProgram,
                IdTask = s.IdTaskCollection ?? 0,
                CapActivity = s.TaskCollection.Activity.FullCaption,
                IdFinanceSource = s.IdFinanceSource.HasValue ? s.FinanceSource : null,
                IdSBP = (int?)null,
                Year = s.HierarchyPeriod.DateStart.Year,
                Value = s.Value
            }).ToList();

            var hasNoFunds = HasNoFunds ?? false;

            // данные в разрезе самой "нижней" групировки
            var data = regTask.Where(w => hasNoFunds).GroupJoin(
                regLva.Where(w => hasNoFunds),
                a => new { a.IdTask, a.IdSBP,              a.Year },
                b => new { b.IdTask, IdSBP = b.IdSBP ?? 0, b.Year },
                (a, g) => g.Select(s => new { task = a, lva = s, s.Year }).DefaultIfEmpty(new { task = a, lva = new Money(), Year = years[0] })).SelectMany(g => g)// years[0] колонки формируются по годам, поэтому на пустой год нельзя
            .Concat(regTask.Where(w => !hasNoFunds).Join(
                regLva.Where(w => !hasNoFunds),
                a => new { a.IdTask, a.IdSBP,              a.Year },
                b => new { b.IdTask, IdSBP = b.IdSBP ?? 0, b.Year },
                (a, b) => new { task = a, lva = b, b.Year }
            )).Concat(
                regTask.Join(
                    regPrm,
                    a => new { a.IdPrg, a.IdTask, a.Year },
                    b => new { b.IdPrg, b.IdTask, b.Year },
                    (a, b) => new { task = a, lva = b, b.Year }
                )            
            ).GroupBy(g => new {
                ParentProgramId = g.task.IdPrg,
                StrName = g.task.CapActivity,
                ActivityId = g.task.IdTask,
                g.task.Contractor,
                SourceName = BySource != true ? "всего" :
                    (   g.lva.IdFinanceSource != null ? (
                        g.lva.IdFinanceSource.Code == "01" || g.lva.IdFinanceSource.FinanceSourceType == FinanceSourceType.UnconfirmedFunds ? "средства, планируемые к привлечению из федерального бюджета (ФБ)" : (
                        g.lva.IdFinanceSource.Code == "02" ? "областной бюджет (ОБ)" : (
                        g.lva.IdFinanceSource.Code == "03" ? "бюджеты муниципальных образований Иркутской области (МБ)" : "иные источники (ИИ)")))
                        : "иные источники (ИИ)"
                    ),
                SourceOrd = BySource != true ? (int?)null :
                    (
                        g.lva.IdFinanceSource != null ? (
                        g.lva.IdFinanceSource.Code == "01" || g.lva.IdFinanceSource.FinanceSourceType == FinanceSourceType.UnconfirmedFunds ? 2 : (
                        g.lva.IdFinanceSource.Code == "02" ? 1 : (
                        g.lva.IdFinanceSource.Code == "03" ? 3
                                                      : 4)))
                                                      : 4
                    ),
                g.Year
            }).Select(s => new DSMain {
                SortKey = s.Key.StrName,
                ProgramId = null,
                ParentProgramId = s.Key.ParentProgramId,
                StrName = s.Key.StrName,
                ActivityId = s.Key.ActivityId,
                IsContractorGrp = false,
                Contractor = s.Key.Contractor,
                IsSourceGrp = false,
                SourceOrd = s.Key.SourceOrd,
                SourceName = s.Key.SourceName,
                Year = s.Key.Year,
                Value = s.Sum(c => c.lva.Value)
            }).Where(w => hasNoFunds || w.Value > 0).ToList();

            // добавляем данные в разрезе мероприятий если заказали
            if (ShowActivities == true)
            {
                res.AddRange(data);
            }

            // группировка данных по иерархии программ
            AddGroupDataByProgram(regProg, data, res);

            // под-итоги по всем источникам
            if (BySource == true)
            {
                AddGroupDataBySource(res, res);
            }

            // под-итоги по всем исполнителям
            AddGroupDataByContractor(res, res);

            // нумерация для группировки
            numHier(res);

            // дополняем годами которых нет в регистрах
            AddNotExistsYears(res, root);

            return res;
        }

        private class Money
        {
            public int? IdPrg;
            public int IdTask;
            public string CapActivity;
            public FinanceSource IdFinanceSource;
            public int? IdSBP;
            public int Year;
            public decimal Value;
        }
    }
}
