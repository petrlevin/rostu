using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using BaseApp.Numerators;
using Platform.BusinessLogic;
using Platform.BusinessLogic.ReportingServices.Reports;
using Platform.Common;
using Platform.Utils.Extensions;
using Sbor.Reference;
using Sbor.Reports.StateProgramGoalIndicatorValue;


namespace Sbor.Reports.Report
{
    //О4 - Сведения о составе и значениях целевых показателей ГП
    [Report]
    public class StateProgramGoalIndicatorValue
    {
        public int idProgram { get; set; }
        public bool ByApproved { get; set; }
        public DateTime? DateReport { get; set; }
        public bool RepeatTableHeader { get; set; }

        private Dictionary<int, string> yearCaptions;
        private Dictionary<int, string> YearCaptionsPrepare(int yearBudget, int yearFrom, int yearTo)
        {
            var result = new Dictionary<int, string>();
            result.Add(yearTo, "год завершения действия программы"); //заголовок последнего года

            for (int y = yearFrom; y < yearTo; y++)
            {
                switch (y - yearBudget)
                {
                    case -2:
                        result.Add(y, "отчетный год");
                        break;
                    case -1:
                        result.Add(y, "текущий год");
                        break;
                    case 0:
                        result.Add(y, "очередной год");
                        break;
                    case 1:
                        result.Add(y, "первый год планового периода");
                        break;
                    case 2:
                        result.Add(y, "второй год планового периода");
                        break;
                    default:
                        result.Add(y, y.ToString() + " г.");
                        break;
                }
            }
            return result;
        }

        public List<DSHeader> DataSetHeader()
        {
            var context = IoC.Resolve<DbContext>().Cast<DataContext>();
            var header = (
                            from p in context.Program.Where(w => w.Id == idProgram)
                            join jap in context.AttributeOfProgram
                                .Where(w =>    (ByApproved && w.DateCommit <= DateReport && (w.DateTerminate > DateReport || w.DateTerminate == null))
                                            || (!ByApproved && w.DateTerminate == null)) on p.Id equals jap.IdProgram into tmpap
                            from ap in tmpap.DefaultIfEmpty()
                            select new DSHeader
                                {
                                    StateProgramName = p.Caption, //1
                                    ImplementationPeriodStart = ap !=null ? ap.DateStart.Year : 0, //2
                                    ImplementationPeriodEnd = ap !=null ? ap.DateEnd.Year: 0, //2
                                    Header = null,
                                    Executive = p.SBP.Organization.Description, //3
                                    CurrentDate = (ByApproved && DateReport.HasValue) ? DateReport.Value : DateTime.Now
                                }).Take(1).ToList();
            if (header.FirstOrDefault().ImplementationPeriodStart != 0)
                header.ForEach(f =>
                    f.Header = string.Format("«{0}» на {1}-{2} годы", f.StateProgramName, f.ImplementationPeriodStart, f.ImplementationPeriodEnd));
            else
                header.ForEach(f =>
                    f.Header = string.Format("«{0}»", f.StateProgramName));

            return header;
        }

        public List<DSMain> DataSetMain()
        {
            var context = IoC.Resolve<DbContext>().Cast<DataContext>();
            int yearBudget = new BaseAppNumerators().BudgetYear();

            #region Проверка корректности входных данных
            string causeOfCancel = null;
            if (yearBudget == 0)
                causeOfCancel = "Не инициализированы параметры входа, не откуда взять год бюджета.";
            else
            {
                int count = context.AttributeOfProgram.Count(w => w.IdProgram == idProgram
                                                              &&
                                                              (ByApproved
                                                                   ? (w.DateCommit <= DateReport &&
                                                                      (w.DateTerminate > DateReport ||
                                                                       w.DateTerminate == null))
                                                                   : w.DateTerminate == null));
                if (count != 1)
                    //causeOfCancel = string.Format("Настройки фильтра не позволяют однозначно определить запись в регистре 'Атрибуты программ', найденное количество записей - {0}", count);
                    causeOfCancel = "";
            }
            if (causeOfCancel != null)
            {
                var empty = new List<DSMain>()
                    {
                        new DSMain()
                            {
                                ProgramId = 0,
                                ProgramNumber = "",
                                ProgramType = 0,
                                ProgramName = causeOfCancel,
                                AnalyticalCode = "",
                                ImplementationPeriodStart = null, //2
                                ImplementationPeriodEnd = null, //2

                                GoalIndicatorNumber = "",
                                GoalIndicatorId = 0,
                                GoalIndicatorName = "", //5
                                GoalIndicatorUnitDimension = "", //6

                                Year = yearBudget,
                                YearCaption = (string) null,
                                Value = null, //7,8,9,10,11
                            }

                    };

                yearCaptions = YearCaptionsPrepare(yearBudget, yearBudget - 2, yearBudget + 2);
                empty.AddMissingInRange(yearBudget - 2, yearBudget + 2, s => s.Year, (obj, y) => (obj.Clone(y, null)));
                empty.ForEach(f => f.YearCaption = f.Year.HasValue ? yearCaptions[f.Year.Value] : null);

                return empty;
            }
            #endregion Проверка корректности входных данных

            DateTime yearFrom = new DateTime(yearBudget-2,1,1); //4
            DateTime yearTo = new DateTime(
                context.AttributeOfProgram.Single(w => w.IdProgram == idProgram
                                                       &&
                                                       (ByApproved
                                                            ? (w.DateCommit <= DateReport &&
                                                               (w.DateTerminate > DateReport || w.DateTerminate == null))
                                                            : (w.DateTerminate == null))
                    ).DateEnd.Year 
                , 1, 1);//4

            #region Запрос данных
            var sp = (from p in context.Program
                        .Where(w => (ByApproved && w.DateCommit <= DateReport && (w.DateTerminate > DateReport || w.DateTerminate == null))
                                    || (!ByApproved && w.DateTerminate == null))

                      //join ap in context.AttributeOfProgram on sge.Id equals ap.IdGoalSystemElement
                      join jap in context.AttributeOfProgram
                        .Where(w => (ByApproved && w.DateCommit <= DateReport && (w.DateTerminate > DateReport || w.DateTerminate == null))
                                    || (!ByApproved && w.DateTerminate == null))
                        on p.Id equals jap.IdProgram into tmpap
                      from ap in tmpap.DefaultIfEmpty()

                      ////join sge in context.SystemGoalElement on p.Id equals sge.IdProgram
                      //join jsge in context.SystemGoalElement
                      //  .Where(w => (ByApproved && w.DateCommit <= DateReport && (w.DateTerminate > DateReport || w.DateTerminate == null))
                      //              || (!ByApproved && w.DateTerminate == null))
                      //  on p.Id equals jsge.IdProgram into tmpsge
                      //  from sge in tmpsge.DefaultIfEmpty()

                      //join gt in context.GoalTarget on sge.Id equals gt.IdSystemGoalElement
                      join jgt in context.GoalTarget
                        .Where(w => (ByApproved && w.DateCommit <= DateReport && (w.DateTerminate > DateReport || w.DateTerminate == null))
                                    || (!ByApproved && w.DateTerminate == null))
                        //on sge.Id equals jgt.IdSystemGoalElement into tmpgt
                        on ap.IdGoalSystemElement equals jgt.IdSystemGoalElement into tmpgt
                        from gt in tmpgt.DefaultIfEmpty()

                      //join vgt in context.ValuesGoalTarget on gt.Id equals vgt.IdGoalTarget
                      join jvgt in context.ValuesGoalTarget
                        .Where(w => (ByApproved && w.DateCommit <= DateReport && (w.DateTerminate > DateReport || w.DateTerminate == null))
                                    || (!ByApproved && w.DateTerminate == null))
                        .Where( w =>
                                   (w.IdValueType == 1) //Тип значения = План (7,8,9,10,11,12,13)
                                && (w.HierarchyPeriod.IdParent == null)
                                && (w.HierarchyPeriod.DateStart >= yearFrom)
                                && (w.HierarchyPeriod.DateStart <= yearTo))
                              on gt.Id equals jvgt.IdGoalTarget into tmpvgt
                              from vgt in tmpvgt.DefaultIfEmpty()

                      //join gi in context.GoalIndicator on gt.IdGoalIndicator equals gi.Id
                      join jgi in context.GoalIndicator on gt.IdGoalIndicator equals jgi.Id into tmpgi
                      from gi in tmpgi.DefaultIfEmpty()

                     where 
                        ( p.Id == idProgram
                          ||  (ap.IdParent == idProgram
                                 && ( p.IdDocType == DocType.SubProgramSP || p.IdDocType == DocType.LongTermGoalProgram)
                              )
                        )
 
                        //&& (vgt.IdValueType == 1) //Тип значения = План (7,8,9,10,11,12,13)
                        //&& (vgt.HierarchyPeriod.IdParent == null)
                        //&& (vgt.HierarchyPeriod.DateStart >= yearFrom)
                        //&& (vgt.HierarchyPeriod.DateStart <= yearTo)
                        //&& ( ByApproved ? (    p.DateCommit <= DateReport && (p.DateTerminate > DateReport || p.DateTerminate == null)
                        //                    && sge.DateCommit <= DateReport && (sge.DateTerminate > DateReport || sge.DateTerminate == null)
                        //                    && gt.DateCommit <= DateReport && (gt.DateTerminate > DateReport || gt.DateTerminate == null)
                        //                    && vgt.DateCommit <= DateReport && (vgt.DateTerminate > DateReport || vgt.DateTerminate == null)
                        //                    && ap.DateCommit <= DateReport && (ap.DateTerminate > DateReport || ap.DateTerminate == null)
                        //                  )
                        //                 : (   p.DateTerminate == null
                        //                    && sge.DateTerminate == null
                        //                    && gt.DateTerminate == null
                        //                    && vgt.DateTerminate == null
                        //                    && ap.DateTerminate == null
                        //                   )
                        //   )

                        select new DSMain
                         {
                             ProgramId = p.Id,
                             ProgramNumber = "",
                             ProgramType = p.IdDocType,
                             ProgramName = p.IdDocType == DocType.StateProgram ? p.Caption : ap.Caption, //1,15,27
                             AnalyticalCode = ap.IdAnalyticalCodeStateProgram !=null ? ap.AnalyticalCodeStateProgram.AnalyticalCode : "",
                             ImplementationPeriodStart = ap.DateStart, //2
                             ImplementationPeriodEnd = ap.DateEnd, //2

                             GoalIndicatorNumber = "",
                             GoalIndicatorId = gi.Id,
                             GoalIndicatorName = gi.Caption, //5
                             GoalIndicatorUnitDimension = gi.UnitDimension.Symbol, //6

                             Year = vgt.HierarchyPeriod.DateStart.Year,
                             YearCaption = (string)null,
                             Value = vgt.Value, //7,8,9,10,11
                         }).ToList();
            
            #endregion

            sp.ForEach(
                f =>
                f.ProgramName = (f.ImplementationPeriodStart.HasValue && f.ImplementationPeriodEnd.HasValue)
                                    ? string.Format("«{0}» на {1}-{2} годы", f.ProgramName,
                                                    f.ImplementationPeriodStart.Value.Year.ToString(),
                                                    f.ImplementationPeriodEnd.Value.Year.ToString())
                                    : string.Format("«{0}»", f.ProgramName));

            #region Добавление отсутствующих годов
            sp.AddMissingInRange(yearFrom.Year, yearTo.Year, s => s.Year, (obj, y) => (obj.Clone(y, null)));
            #endregion

            #region Установка заголовков года

            yearCaptions = YearCaptionsPrepare(yearBudget, yearFrom.Year, yearTo.Year);
            sp.ForEach(f => f.Year = f.Year.HasValue ? f.Year : yearFrom.Year);
            sp.ForEach(f => f.YearCaption = f.Year.HasValue ? yearCaptions[f.Year.Value] : null);

            #endregion

            #region Нумерация пГП и ДЦП: начало

            //возможны ошибки нумерации при совпадающих именах
            //sp.Where(w => w.ProgramType != DocType.StateProgram).ToList()
            //  .NumerateInternalGroups(inS => inS.ProgramType, byS => byS.ProgramName,
            //                          (col, num) => col.ProgramNumber = num.ToString());
            sp.NumerateInternalGroups(inS => inS.ProgramType, byS => byS.AnalyticalCode + "$" + byS.ProgramName,
                                      (col, num) => col.ProgramNumber =
                                          col.ProgramType != DocType.StateProgram ? num.ToString() + "." : "");
            
            #endregion

            #region Нумерация целевых показателей

            //возможны ошибки нумерации при совпадающих именах
            //sp.NumerateInternalGroups(inS => inS.ProgramId, byS => byS.GoalIndicatorName ?? "",
            //(col, num) => col.GoalIndicatorNumber = col.ProgramNumber + num.ToString());
            
            //нормальный, но не проверенный
            //sp.NumerateInternalGroups(inS => inS.ProgramId, byS => byS.GoalIndicatorId.ToString() + byS.GoalIndicatorName,
            //(col, num) => col.GoalIndicatorNumber = col.ProgramNumber + num.ToString());
            
            //лучший вариант
            //sp.NumerateInternalGroupsInWhere(inS => inS.ProgramId, byS => new { byS.GoalIndicatorId, byS.GoalIndicatorName }, oS => oS.GoalIndicatorName, w=> true,
            //    (col, num) => col.GoalIndicatorNumber = col.ProgramNumber + num.ToString());

            //лучший вариант
            //sp.NumerateInternalGroupsInWhere(inS => inS.ProgramId, byS => byS.GoalIndicatorId, oS => oS.GoalIndicatorName, w => true,
            //    (col, num) => col.GoalIndicatorNumber = col.ProgramNumber + num.ToString());
            sp.NumerateInternalGroupsInWhere(inS => inS.ProgramId, byS => new { byS.GoalIndicatorId, byS.GoalIndicatorName }, oS => oS.GoalIndicatorName, w => true,
                (col, num) => col.GoalIndicatorNumber = col.ProgramNumber + num.ToString());

            #endregion

            #region Нумерация пГП и ДЦП: окончание
            sp.ForEach(f =>
            {
                if (f.ProgramType == DocType.StateProgram)
                    f.ProgramNumber = "Государственная программа ";
                else if (f.ProgramType == DocType.SubProgramSP)
                    f.ProgramNumber = "Подпрограмма " + f.ProgramNumber + " ";
                else if (f.ProgramType == DocType.LongTermGoalProgram)
                    f.ProgramNumber = "Долгосрочная целевая программа " + f.ProgramNumber+ " ";
            });
            #endregion

            #region Установка ProgramType для сортировки по типу документа
            sp.ForEach(f =>
                {
                    if (f.ProgramType == DocType.StateProgram)
                        f.ProgramType = 0;
                    else if (f.ProgramType == DocType.SubProgramSP)
                        f.ProgramType = 1;
                    else if (f.ProgramType == DocType.LongTermGoalProgram)
                        f.ProgramType = 2;
                });
            #endregion

            return sp;
        }
    }
}
