using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using Platform.BusinessLogic;
using Platform.BusinessLogic.Reference;
using Platform.BusinessLogic.ReportingServices.Reports;
using Platform.Common;
using Platform.Utils.Extensions;
using Sbor.DbEnums;
using Sbor.Document;
using Sbor.Reference;
using Sbor.Registry;
using ValueType = Sbor.DbEnums.ValueType;
using Sbor.Reports.ForecastConsolidatedIndicators;

namespace Sbor.Reports.Report
{
    [Report]
    public class ForecastConsolidatedIndicators
    {
        public bool ByApproved { get; set; } //Строить отчет по утвержденным данным	Bool
        public string Caption { get; set; } //	Наименование	String
        public DateTime? DateReport { get; set; } //	Дата отчета	DateTime
        public bool EvaluationAdditionalNeeds { get; set; } //	Оценка дополнительной потребности	Bool
        public bool RepeatTableHeader { get; set; } //	Повторять заголовки таблиц на каждой странице	Bool
        // ReSharper disable InconsistentNaming
        public int id { get; set; } // Идентификатор	Int
        public int idBudget { get; set; } //	Бюджет	Link
        public int idProgram { get; set; } //	Программа	Link
        public int idPublicLegalFormation { get; set; }   //	ППО	Link
        public int idVersion { get; set; } //	Версия	Link
        public bool isTemporary { get; set; } //	Временный экземпляр	Bool
        public int idSourcesDataReports { get; set; } //Источник данных для вывода ресурсного обеспечения в отчет
      
        public bool showNoFinanced { get; set; } //	Выводить мероприятие без финансирования

        // ReSharper restore InconsistentNaming

        private int? startYear { get; set; }
        private int? endYear { get; set; }

        public List<DSHeader> DataSetHeader()
        {

            var context = IoC.Resolve<DbContext>().Cast<DataContext>();

            var program = context.Program.Where(w => w.Id == idProgram).Select(s =>
                            new
                                {
                                    StateProgramName = s.Caption, //1
                                    Executive = s.SBP.Organization.Description, //3
                                    CurrentDate = DateTime.Now
                                }).FirstOrDefault();

            var attrProgram =  context.AttributeOfProgram.Where(w => 
                                            w.IdProgram  == idProgram
                                            && (
                                                (ByApproved && w.DateCommit <= DateReport && (w.DateTerminate > DateReport || w.DateTerminate == null))
                                                || (!ByApproved && w.DateTerminate == null)
                                              )).Select(s =>
                            new
                                {
                                    ImplementationPeriodStart = s.DateStart.Year, //2
                                    ImplementationPeriodEnd = s.DateEnd.Year, //2
                                });


            var header = new DSHeader();
            if (program!=null) 
            {
                header.PPOCaption = context.PublicLegalFormation.FirstOrDefault(w => w.Id == idPublicLegalFormation).Caption;
                header.Executive = program.Executive;
                header.ReportDate = (ByApproved && DateReport.HasValue) ? DateReport.Value : program.CurrentDate;
            }

            if (attrProgram.Any() )
            {
                header.Header = string.Format("«{0}» на {1}-{2} годы", program.StateProgramName,
                                              attrProgram.FirstOrDefault().ImplementationPeriodStart,
                                              attrProgram.FirstOrDefault().ImplementationPeriodEnd);
            }
            else
            {
                header.Header = string.Format("«{0}»", program.StateProgramName);
            }

            return new List<DSHeader>() { header };
        }

        public List<DSMain> DataSetMain()
        {
            var context = IoC.Resolve<DbContext>().Cast<DataContext>();

            #region Подготовка параметров

            var pr = context.AttributeOfProgram.Where(w =>
                              w.IdProgram == idProgram
                              && (
                                    (ByApproved && w.DateCommit <= DateReport && (w.DateTerminate > DateReport || w.DateTerminate == null))
                                    || (!ByApproved && w.DateTerminate == null)
                                  )).Select(s =>
                                        new
                                        {
                                            startYear = s.DateStart.Year, //2
                                            endYear = s.DateEnd.Year, //2
                                        });
            var prf = pr.FirstOrDefault();

            #region Если не найден однозначно атрибт программы, заполнение пустотой и выход
            //"Невозможно однозначно определить запись регистра 'Атрибуты программ', количество найденных записей - " + pr.Count()
            if (pr.Count() != 1)
            {
                int emptYear = (ByApproved && DateReport.HasValue) ? DateReport.Value.Year : DateTime.Now.Year;
                var empt = new List<DSMain>()
                        {
                            new DSMain()
                                {
                                    
                                    TopGroupBy = 0,
                                    TopNumber = "",
                                    TopName = null,
                                    TopAnalyticalCode = "",
                                    MiddleGroupBy = 0,
                                    MiddleNumber = "",
                                    MiddleHide = true,
                                    MiddleName = null,
                                    MiddleAnalyticalCode = "",

                                    ActivityGroupByPart1 = null,
                                    ActivityGroupByPart2 = null,
                                    ActivityNumber = "",
                                    ActivityName = null,

                                    InditorActivity = null,
                                    VolumeOrExpense = true,
                                    Year = emptYear,

                                    //агрегация
                                    Value = null,
                                    ValueWhithAdditionalNeed = null
                                }};

                //Добавление отсутствующих годов
                empt.AddMissingInRange(emptYear, emptYear + 3, s => s.Year, (obj, y) => (obj.Clone(y, null, null, null)));

                return empt;

            }

            #endregion Если не найден однозначно атрибут программы, заполнение пустотой и выход


            #endregion Подготовка параметров запроса

            #region Сноски 7,8. Обработка регистров "Программы" и "Атрибуты программ"

            #region Выборка
            //Получение сносок (6,14,22)(подчиненная к ГП)(часть top)
            //        и сносок (7,15) (подчиненная подчиненной ГП)(часть mid)
            //Выборка
            var t1 = (from top in context.AttributeOfProgram
                      join jmid in context.AttributeOfProgram on top.IdProgram equals jmid.IdParent into tmpmid
                      from mid in tmpmid.DefaultIfEmpty()

                      where
                          top.IdParent == idProgram
                          && (
                                ((top.Program.IdDocType == DocType.SubProgramSP) //6
                                      && (mid.Program.IdDocType == DocType.ProgramOfSBP || mid.Program.IdDocType == DocType.MainActivity)) //7
                                || ((top.Program.IdDocType == DocType.LongTermGoalProgram) //14      
                                      && ((mid.Program.IdDocType == DocType.SubProgramDGP) || (mid == null))) //15 
                                || ((top.Program.IdDocType == DocType.ProgramOfSBP || top.Program.IdDocType == DocType.MainActivity) //22
                                      && (mid == null)) //22-23
                              )

                              && (ByApproved ? (
                                                    top.DateCommit <= DateReport && (top.DateTerminate > DateReport || top.DateTerminate == null)
                                                 && mid.DateCommit <= DateReport && (mid.DateTerminate > DateReport || mid.DateTerminate == null)
                                                 )
                                              : (
                                                     top.DateTerminate == null
                                                  && mid.DateTerminate == null
                                                )

                                )
                      select new
                      {
                          topId = top.Id,
                          //topIdProgram = top.IdProgram,
                          //idParent1 = top.IdParent,
                          topType = top.Program.IdDocType,
                          topTypeCaption = top.Program.IdDocType == DocType.SubProgramSP ? "Подпрограмма" :
                                           top.Program.IdDocType == DocType.ProgramOfSBP ? "ВЦП" :
                                           top.Program.DocType.Caption,
                          topName = top.Caption,
                          topAnalyticalCode = top.IdAnalyticalCodeStateProgram != null ? top.AnalyticalCodeStateProgram.AnalyticalCode : "",

                          topStartYear = top.DateStart.Year,
                          topEndYear = top.DateEnd.Year,

                          midId = (int?)mid.Id,
                          midHide = (mid == null),
                          midIdProgram = (mid != null) ? mid.IdProgram : top.IdProgram,
                          //idParent2 = (mid != null)? mid.IdParent, top.IdProgram
                          midType = (mid != null) ? mid.Program.IdDocType : 0,
                          midTypeCaption = (mid != null) ? mid.Program.DocType.Caption : null,
                          midName = mid.Caption,
                          midAnalyticalCode = mid.IdAnalyticalCodeStateProgram != null ? mid.AnalyticalCodeStateProgram.AnalyticalCode : "",
                          midStartYear = (int?)mid.DateStart.Year,
                          midEndYear = (int?)mid.DateEnd.Year
                          //midSBP = (mid != null) ? mid.Program.IdSBP: top.Program.IdSBP
                      }).ToList();
            #endregion Выборка

            #region Оформление
            //Получение сносок (6,14,22)(подчиненная к ГП)(часть top)
            //        и сносок (7,15) (поддиченнная подчиненной ГП)(часть mid)
            //Оформление
            var t2 = t1.Select(s =>
                new
                {
                    topId = s.topId,
                    sn6 = string.Format("{0} «{1}» на {2}-{3} годы", s.topTypeCaption, s.topName, s.topStartYear.ToString(), s.topEndYear.ToString()),
                    s.topAnalyticalCode,

                    midId = s.midId,
                    midHide = s.midHide,
                    midIdProgram = s.midIdProgram,
                    sn7 = (s.midHide) ? null : (string.Format("{0} «{1}» на {2}-{3} годы", s.midTypeCaption, s.midName, s.midStartYear.ToString(), s.midEndYear.ToString())),
                    s.midAnalyticalCode
                }
                );
            #endregion Оформление

            #endregion Сноски 7,8. Обработка регистров "Программы" и "Атрибуты программ"

            #region Закомментировано Сноски 8,9. Обработка регистров "Объемы задач" и "Набор задач"

            ////1. Для программы, найденной по <Сноска 7>  найти неаннулированные строки в регистре «Объемы задач» по полю «Программа» 
            ////(Регистр,АтрибутыПрограммы.Программа = Регистр.ОбъемаЗадач.Программа). 
            ////Исключить дубли строк, у которых одинаковые значения в поле «НаборЗадач».
            //var t3 =    (from tv in context.TaskVolume
            //             join ap in t2 on tv.IdProgram equals ap.midIdProgram
            //             where
            //                 (ByApproved
            //                      ? (tv.DateCommit <= DateReport && (tv.DateTerminate > DateReport || tv.DateTerminate == null))
            //                      : (tv.DateTerminate == null))
            //             select tv.IdTaskCollection).Distinct();


            //var t4 = from tc in context.TaskCollection
            //         join tv in context.TaskVolume on tc.Id equals tv.IdTaskCollection
            //         where
            //             //3. Далее отобрать строки с типом = «Услуга» или «Работа» (НаборЗадач.Мероприятие – справочник «Перечень мероприятий» - поле «Тип»).
            //             (tc.Activity.IdActivityType == (byte) ActivityType.Service ||
            //              tc.Activity.IdActivityType == (byte) ActivityType.Work)
            //             //2. Далее по полю «НаборЗадач» найденных строк отобрать строки в регистре «НаборЗадач». 
            //             && t3.Contains(tc.Id)
            //         select new
            //             {
            //                 //idProgram = tv.IdProgram,  //для связи с подпрограммами подпрограмм ГП
            //                 sn8 = tc.Activity.Caption, //4. Из отобранных строк вывести значение из поля «Мероприятие»
            //                 //sn9 = string.Format("{0}, {1}",tv.IndicatorActivity_Volume.Caption,tv.IndicatorActivity_Volume.UnitDimension.Caption),
            //                 //sbp = tv.IdSBP
            //             };

            #endregion Закомментировано Сноски 8,9. Обработка регистров "Объемы задач" и "Набор задач"

            #region Сноски 10,11

            List<SBP> lsbps = context.SBP.Where(r => r.IdPublicLegalFormation == this.idPublicLegalFormation).ToList();



            var taskVolumes = context.TaskVolume.Where(r => r.IdPublicLegalFormation == this.idPublicLegalFormation &&
                                                            (ByApproved
                                                                 ? (r.DateCommit <= DateReport &&
                                                                    (r.DateTerminate > DateReport ||
                                                                     r.DateTerminate == null))
                                                                 : (r.DateTerminate == null))
                //&& r.IdTaskCollection == 2499
                                                                 ).
                                      Select(s => new { tv = s, tc = s.TaskCollection, sbp = s.SBP, ind = s.IndicatorActivity_Volume }).ToList();

            // список ID регистраторов из регистра Объемы задач с типом регистратора - План деятельности
            var sd = taskVolumes.Where(r => r.tv.IdRegistratorEntity == PlanActivity.EntityIdStatic)
                                .Select(s => s.tv.IdRegistrator)
                                .Distinct()
                                .ToList();

            // список всех Планов деятельности текущего бюджета
            var planactivity = context.PlanActivity.Where(r => r.IdBudget == this.idBudget).ToList();

            // список владельце строк ТЧ «Требования к заданию»
            var pa_rft = context.PlanActivity_RequirementsForTheTask.
                                 Where(w => w.Owner.IdBudget == this.idBudget).
                                 Select(s => s.IdOwner).
                                 Distinct().
                                 ToList();

            // составляем список ID ЭД План деятельности которые необходимо игнорировать в отчете, 
            // потому что у их листовых версий-документов не заполнена ТЧ «Требования к заданию»
            List<int> ignored_pa = new List<int>();

            foreach (var padoc in planactivity.Where(w => sd.Contains(w.Id)))
            {
                var doc = padoc;
                do
                {
                    var next = planactivity.FirstOrDefault(i => i.IdParent == doc.Id);
                    if (next != null)
                    {
                        doc = next;
                        continue;
                    }
                    break;
                } while (true);

                if (!pa_rft.Contains(doc.Id))
                {
                    ignored_pa.Add(padoc.Id);
                }

            }

            var sn8_1s = (from ap in t2
                          join tv in taskVolumes on ap.midIdProgram equals tv.tv.IdProgram
                          where
                              ((tv.tc.Activity.IdActivityType == (byte)ActivityType.Service) ||
                               (tv.tc.Activity.IdActivityType == (byte)ActivityType.Work))

                              && tv.tv.IdValueType == (byte)ValueType.Plan //Тип значения = План
                              && tv.tv.HierarchyPeriod.DateStart.Year >= prf.startYear &&
                              tv.tv.HierarchyPeriod.DateStart.Year <= prf.endYear &&
                              !(tv.sbp.IdSBPType == (int)Sbor.DbEnums.SBPType.TreasuryEstablishment && tv.sbp.IsFounder)
                          select new { ap, tv }).ToList();

            List<DSMain> t3 = new List<DSMain>();

            List<int> outactivity = new List<int>();// список ID выводимых в отчет Наборов задач

            // года бюджета
            List<int> years = new List<int>();
            var budgetYear = context.Budget.Single(s => s.Id == idBudget).Year;
            for (int y = budgetYear; y < budgetYear + 3; y++)
            {
                years.Add(y);
            }
            var budgetYears = years.ToArray();

            foreach (var sn8_1 in sn8_1s)
            {
                #region Если срок реализации ГП <Сноска 5> входит в период планирования бюджета
                var sn8_4s = taskVolumes.Where(r => r.tv.IdBudget == this.idBudget &&
                                                    r.tv.IdTaskCollection == sn8_1.tv.tc.Id &&
                                                    IsHiParent(r.tv.IdSBP, sn8_1.tv.tv.IdSBP, lsbps) &&
                                                    !(r.sbp.IdSBPType == (int)Sbor.DbEnums.SBPType.TreasuryEstablishment && r.sbp.IsFounder));

                var sn8_5s =
                    sn8_4s.Where(r =>
                                 r.tv.IdRegistratorEntity == PlanActivity.EntityIdStatic &&
                                 !ignored_pa.Contains(r.tv.IdRegistrator));

                outactivity.AddRange(sn8_5s.Select(s => s.tc.Id).ToList());

                t3.AddRange((from t3_0 in sn8_5s
                             select new
                             {
                                 TopGroupBy = sn8_1.ap.topId,
                                 TopName = sn8_1.ap.sn6,
                                 TopAnalyticalCode = sn8_1.ap.topAnalyticalCode,
                                 MiddleGroupBy = sn8_1.ap.midId,
                                 MiddleHide = (sn8_1.ap != null) ? sn8_1.ap.midHide : true,
                                 MiddleName = sn8_1.ap.sn7,
                                 MiddleAnalyticalCode = sn8_1.ap.midAnalyticalCode,

                                 ActivityGroupByPart1 = t3_0.tc.IdActivity,
                                 ActivityGroupByPart2 = t3_0.tv.IdIndicatorActivity_Volume,
                                 ActivityName = t3_0.tc.Activity.Caption, //sn8

                                 InditorActivity = t3_0.tv.IndicatorActivity_Volume.Caption + ", " + t3_0.tv.IndicatorActivity_Volume.UnitDimension.Caption, //sn9
                                 Year = t3_0.tv.HierarchyPeriod.DateStart.Year,
                                 IsAdditionalNeed = t3_0.tv.IsAdditionalNeed,

                                 //AttributeOfProgram_Id = tv.Id,
                                 //AttributeOfProgram_IdSBP = tv.IdSBP,
                                 //AttributeOfProgram_SBPCaption = tv.SBP.Caption,

                                 Value = t3_0.tv.Value

                             } into items
                             group items by new
                             {
                                 items.TopGroupBy,
                                 items.TopName,
                                 items.TopAnalyticalCode,

                                 items.MiddleGroupBy,
                                 items.MiddleName,
                                 items.MiddleHide,
                                 items.MiddleAnalyticalCode,

                                 items.ActivityGroupByPart1,
                                 items.ActivityGroupByPart2,
                                 items.ActivityName,

                                 items.InditorActivity,

                                 //items.AttributeOfProgram_Id,
                                 //items.AttributeOfProgram_IdSBP,
                                 //items.AttributeOfProgram_SBPCaption,

                                 items.Year

                             } into gr
                             select new
                             {
                                 //ключ
                                 TopGroupBy = gr.Key.TopGroupBy,
                                 TopNumber = "",
                                 TopName = gr.Key.TopName,
                                 TopAnalyticalCode = gr.Key.TopAnalyticalCode,
                                 MiddleGroupBy = gr.Key.MiddleGroupBy,
                                 MiddleNumber = "",
                                 MiddleHide = gr.Key.MiddleHide,
                                 MiddleName = gr.Key.MiddleName,
                                 MiddleAnalyticalCode = gr.Key.MiddleAnalyticalCode,

                                 ActivityGroupByPart1 = gr.Key.ActivityGroupByPart1,
                                 ActivityGroupByPart2 = gr.Key.ActivityGroupByPart2,
                                 ActivityNumber = "",
                                 ActivityName = gr.Key.ActivityName,

                                 InditorActivity = gr.Key.InditorActivity,
                                 VolumeOrExpense = true,
                                 Year = gr.Key.Year,

                                 //агрегация
                                 Value = gr.Sum(item => !item.IsAdditionalNeed ? item.Value : 0),
                                 ValueWhithAdditionalNeed = gr.Sum(item => item.Value)
                             })
                          .ToList()
                          .Select(s =>
                              new DSMain
                              {
                                  TopGroupBy = s.TopGroupBy,
                                  TopNumber = "",
                                  TopName = s.TopName,
                                  TopAnalyticalCode = s.TopAnalyticalCode,
                                  MiddleGroupBy = s.MiddleGroupBy,
                                  MiddleNumber = "",
                                  MiddleHide = s.MiddleHide,
                                  MiddleName = s.MiddleName,
                                  MiddleAnalyticalCode = s.MiddleAnalyticalCode,

                                  ActivityGroupByPart1 = s.ActivityGroupByPart1,
                                  ActivityGroupByPart2 = s.ActivityGroupByPart2,
                                  ActivityNumber = "",
                                  ActivityName = s.ActivityName,

                                  InditorActivity = s.InditorActivity,
                                  VolumeOrExpense = true,
                                  Year = s.Year,
                                  Value = Sbor.Logic.DocSGEMethod.ExcludeLastZeros(s.Value),
                                  ValueD = s.Value,
                                  ValueWhithAdditionalNeed = Sbor.Logic.DocSGEMethod.ExcludeLastZeros(s.ValueWhithAdditionalNeed)
                              })
                          .ToList());
                #endregion Если срок реализации ГП <Сноска 5> входит в период планирования бюджета
            }

            #region Если срок реализации ГП <Сноска 5> НЕ входит в период планирования бюджета

            t3.AddRange((from t3_0 in sn8_1s.Where(r =>
                                                        !budgetYears.Contains(r.tv.tv.HierarchyPeriod.DateStart.Year) &&
                                                        outactivity.Contains(r.tv.tc.Id)
                                                        )
                         select new
                         {
                             TopGroupBy = t3_0.ap.topId,
                             TopName = t3_0.ap.sn6,
                             TopAnalyticalCode = t3_0.ap.topAnalyticalCode,
                             MiddleGroupBy = t3_0.ap.midId,
                             MiddleHide = (t3_0.ap != null) ? t3_0.ap.midHide : true,
                             MiddleName = t3_0.ap.sn7,
                             MiddleAnalyticalCode = t3_0.ap.midAnalyticalCode,

                             ActivityGroupByPart1 = t3_0.tv.tc.IdActivity,
                             ActivityGroupByPart2 = t3_0.tv.ind.Id,
                             ActivityName = t3_0.tv.tc.Activity.Caption, //sn8

                             InditorActivity = t3_0.tv.ind.Caption + ", " + t3_0.tv.ind.UnitDimension.Caption, //sn9
                             Year = t3_0.tv.tv.HierarchyPeriod.DateStart.Year,
                             IsAdditionalNeed = t3_0.tv.tv.IsAdditionalNeed,

                             //AttributeOfProgram_Id = tv.Id,
                             //AttributeOfProgram_IdSBP = tv.IdSBP,
                             //AttributeOfProgram_SBPCaption = tv.SBP.Caption,

                             Value = t3_0.tv.tv.Value

                         } into items
                         group items by new
                         {
                             items.TopGroupBy,
                             items.TopName,
                             items.TopAnalyticalCode,

                             items.MiddleGroupBy,
                             items.MiddleName,
                             items.MiddleHide,
                             items.MiddleAnalyticalCode,

                             items.ActivityGroupByPart1,
                             items.ActivityGroupByPart2,
                             items.ActivityName,

                             items.InditorActivity,

                             //items.AttributeOfProgram_Id,
                             //items.AttributeOfProgram_IdSBP,
                             //items.AttributeOfProgram_SBPCaption,

                             items.Year

                         } into gr
                         select new
                         {
                             //ключ
                             TopGroupBy = gr.Key.TopGroupBy,
                             TopNumber = "",
                             TopName = gr.Key.TopName,
                             TopAnalyticalCode = gr.Key.TopAnalyticalCode,
                             MiddleGroupBy = gr.Key.MiddleGroupBy,
                             MiddleNumber = "",
                             MiddleHide = gr.Key.MiddleHide,
                             MiddleName = gr.Key.MiddleName,
                             MiddleAnalyticalCode = gr.Key.MiddleAnalyticalCode,

                             ActivityGroupByPart1 = gr.Key.ActivityGroupByPart1,
                             ActivityGroupByPart2 = gr.Key.ActivityGroupByPart2,
                             ActivityNumber = "",
                             ActivityName = gr.Key.ActivityName,

                             InditorActivity = gr.Key.InditorActivity,
                             VolumeOrExpense = true,
                             Year = gr.Key.Year,

                             //агрегация
                             Value = gr.Sum(item => !item.IsAdditionalNeed ? item.Value : 0),
                             ValueWhithAdditionalNeed = gr.Sum(item => item.Value)
                         })
                      .ToList()
                      .Select(s =>
                          new DSMain
                          {
                              TopGroupBy = s.TopGroupBy,
                              TopNumber = "",
                              TopName = s.TopName,
                              TopAnalyticalCode = s.TopAnalyticalCode,
                              MiddleGroupBy = s.MiddleGroupBy,
                              MiddleNumber = "",
                              MiddleHide = s.MiddleHide,
                              MiddleName = s.MiddleName,
                              MiddleAnalyticalCode = s.MiddleAnalyticalCode,

                              ActivityGroupByPart1 = s.ActivityGroupByPart1,
                              ActivityGroupByPart2 = s.ActivityGroupByPart2,
                              ActivityNumber = "",
                              ActivityName = s.ActivityName,

                              InditorActivity = s.InditorActivity,
                              VolumeOrExpense = true,
                              Year = s.Year,
                              Value = Sbor.Logic.DocSGEMethod.ExcludeLastZeros(s.Value),
                              ValueD = s.Value,
                              ValueWhithAdditionalNeed = Sbor.Logic.DocSGEMethod.ExcludeLastZeros(s.ValueWhithAdditionalNeed)
                          })
                      .ToList());
            #endregion Если срок реализации ГП <Сноска 5> НЕ входит в период планирования бюджета


            #endregion Сноски 10,11

            #region Сноски 12,13

            var pre4 = (from tv in sn8_1s.Where(r => outactivity.Contains(r.tv.tc.Id))
                        select new
                        {
                            tv.ap.topId,
                            tv.ap.sn6,
                            tv.ap.topAnalyticalCode,
                            tv.ap.midId,
                            tv.ap.midHide,
                            tv.ap.midIdProgram,
                            tv.ap.sn7,
                            tv.ap.midAnalyticalCode,

                            tv.tv.tc.IdActivity,
                            IdIndicatorActivity_Volume = tv.tv.ind.Id,
                            IdTaskCollection = tv.tv.tc.Id,

                            TaskVolumeIdSBP = tv.tv.sbp.Id,
                            ActivityCaption = tv.tv.tc.Activity.Caption,
                            IndicatorActivity = tv.tv.ind.Caption + ", " + tv.tv.ind.UnitDimension.Caption
                        }).Distinct();

            var lvas0 =
                context.LimitVolumeAppropriations.Where(
                    lva => lva.IdBudget == idBudget && //Тип значения = Обосновано или Обосновано ПФХД
                           lva.HierarchyPeriod.DateStart.Year >= prf.startYear && lva.HierarchyPeriod.DateStart.Year <= prf.endYear // это возможно лишнее, но убирать не стану, к автору !!! (c) Кузеванов
                           && budgetYears.Contains(lva.HierarchyPeriod.DateStart.Year));

            IQueryable<LimitVolumeAppropriations> lvas1;

            if (idSourcesDataReports == (byte)DbEnums.SourcesDataReports.BudgetEstimates)
            {
                lvas1 = lvas0.Where(lva =>
                                    (lva.IdValueType == (byte)ValueType.Justified &&
                                     lva.EstimatedLine.SBP.IdSBPType == (byte)SBPType.TreasuryEstablishment &&
                                     !lva.EstimatedLine.SBP.IsFounder)
                                    ||
                                    (lva.IdValueType == (byte)ValueType.JustifiedFBA &&
                                     (lva.EstimatedLine.SBP.IdSBPType == (byte)SBPType.IndependentEstablishment ||
                                      lva.EstimatedLine.SBP.IdSBPType == (byte)SBPType.BudgetEstablishment)));
            }
            else if (idSourcesDataReports == (byte)DbEnums.SourcesDataReports.JustificationBudget)
            {
                lvas1 = lvas0.Where(lva =>
                                    (lva.IdValueType == (byte)ValueType.JustifiedGRBS));
            }
            else if (idSourcesDataReports == (byte)DbEnums.SourcesDataReports.InstrumentBalancingSourceEstimates)
            {
                lvas1 = lvas0.Where(lva =>
                                    ((lva.IdValueType == (byte)ValueType.Justified || lva.IdValueType == (byte)ValueType.BalancingIFDB_Estimate) &&
                                     lva.EstimatedLine.SBP.IdSBPType == (byte)SBPType.TreasuryEstablishment && !lva.EstimatedLine.SBP.IsFounder)
                                    ||
                                    (lva.IdValueType == (byte)ValueType.JustifiedFBA &&
                                     (lva.EstimatedLine.SBP.IdSBPType == (byte)SBPType.IndependentEstablishment ||
                                      lva.EstimatedLine.SBP.IdSBPType == (byte)SBPType.BudgetEstablishment)));
            }
            else if (idSourcesDataReports == (byte)DbEnums.SourcesDataReports.InstrumentBalancingSourceActivityOfSBP)
            {
                lvas1 = lvas0.Where(lva =>
                                    (lva.IdValueType == (byte)ValueType.JustifiedGRBS || lva.IdValueType == (byte)ValueType.BalancingIFDB_ActivityOfSBP));
            }
            else
            {
                lvas1 = lvas0.Where(lva =>
                                    (lva.IdValueType == (byte)ValueType.JustifiedGRBS));

            }

            var limitVolumeAppropriationses = lvas1.Where(r => r.IdTaskCollection.HasValue && outactivity.Contains(r.IdTaskCollection.Value) && r.EstimatedLine.FinanceSource.Code == "02")
                                                   .Select(s =>
                                                           new ClvaExt
                                                           {
                                                               lva = s,
                                                               estl = s.EstimatedLine,
                                                               hp = s.HierarchyPeriod
                                                           })
                                                   .ToList();

            List<DSMain> t4 = new List<DSMain>();

            foreach (var pr4 in pre4)
            {
                IEnumerable<ClvaExt> lvas;

                if (idSourcesDataReports == (byte)DbEnums.SourcesDataReports.BudgetEstimates)
                {
                    lvas = limitVolumeAppropriationses.Where(r =>
                                                             IsHiParent(r.estl.IdSBP, pr4.TaskVolumeIdSBP, lsbps) && r.lva.IdTaskCollection == pr4.IdTaskCollection);
                }
                else if (idSourcesDataReports == (byte)DbEnums.SourcesDataReports.JustificationBudget)
                {
                    lvas = limitVolumeAppropriationses.Where(r =>
                                                             r.estl.IdSBP == pr4.TaskVolumeIdSBP && r.lva.IdTaskCollection == pr4.IdTaskCollection);
                }
                else if (idSourcesDataReports == (byte)DbEnums.SourcesDataReports.InstrumentBalancingSourceEstimates)
                {
                    lvas = limitVolumeAppropriationses.Where(r =>
                                                             IsHiParent(r.estl.IdSBP, pr4.TaskVolumeIdSBP, lsbps) && r.lva.IdTaskCollection == pr4.IdTaskCollection);
                }
                else if (idSourcesDataReports == (byte)DbEnums.SourcesDataReports.InstrumentBalancingSourceActivityOfSBP)
                {
                    lvas = limitVolumeAppropriationses.Where(r =>
                                                             r.estl.IdSBP == pr4.TaskVolumeIdSBP && r.lva.IdTaskCollection == pr4.IdTaskCollection);
                }
                else
                {
                    lvas = limitVolumeAppropriationses.Where(r =>
                                                             r.estl.IdSBP == pr4.TaskVolumeIdSBP && r.lva.IdTaskCollection == pr4.IdTaskCollection);
                }

                t4.AddRange((from lva in lvas

                             select new
                             {
                                 TopGroupBy = pr4.topId,
                                 TopName = pr4.sn6,
                                 TopAnalyticalCode = pr4.topAnalyticalCode,
                                 MiddleGroupBy = pr4.midId,
                                 MiddleHide = pr4.midHide,
                                 MiddleName = pr4.sn7,
                                 MiddleAnalyticalCode = pr4.midAnalyticalCode,

                                 ActivityGroupByPart1 = pr4.IdActivity,
                                 ActivityGroupByPart2 = pr4.IdIndicatorActivity_Volume,
                                 ActivityName = pr4.ActivityCaption, //sn8

                                 InditorActivity = pr4.IndicatorActivity, //sn9
                                 Year = lva.hp.DateStart.Year,
                                 IsAdditionalNeed = lva.lva.HasAdditionalNeed ?? false,

                                 Value = lva.lva.Value / 1000

                             }
                                 into items
                                 group items by new
                                 {
                                     items.TopGroupBy,
                                     items.TopName,
                                     items.TopAnalyticalCode,

                                     items.MiddleGroupBy,
                                     items.MiddleName,
                                     items.MiddleHide,
                                     items.MiddleAnalyticalCode,

                                     items.ActivityGroupByPart1,
                                     items.ActivityGroupByPart2,
                                     items.ActivityName,

                                     items.InditorActivity,

                                     items.Year

                                 }
                                     into gr
                                     select new
                                     {
                                         //ключ
                                         TopGroupBy = gr.Key.TopGroupBy,
                                         TopNumber = "",
                                         TopName = gr.Key.TopName,
                                         TopAnalyticalCode = gr.Key.TopAnalyticalCode,
                                         MiddleGroupBy = gr.Key.MiddleGroupBy,
                                         MiddleNumber = "",
                                         MiddleHide = gr.Key.MiddleHide,
                                         MiddleName = gr.Key.MiddleName,
                                         MiddleAnalyticalCode = gr.Key.MiddleAnalyticalCode,

                                         ActivityGroupByPart1 = gr.Key.ActivityGroupByPart1,
                                         ActivityGroupByPart2 = gr.Key.ActivityGroupByPart2,
                                         ActivityNumber = "",
                                         ActivityName = gr.Key.ActivityName,

                                         InditorActivity = gr.Key.InditorActivity,
                                         VolumeOrExpense = false,
                                         Year = gr.Key.Year,

                                         //агрегация
                                         Value = gr.Sum(item => !item.IsAdditionalNeed ? item.Value : 0),
                                         ValueWhithAdditionalNeed = gr.Sum(item => item.Value)
                                     })
                          .ToList()
                          .Select(s =>
                              new DSMain
                              {
                                  TopGroupBy = s.TopGroupBy,
                                  TopNumber = "",
                                  TopName = s.TopName,
                                  TopAnalyticalCode = s.TopAnalyticalCode,
                                  MiddleGroupBy = s.MiddleGroupBy,
                                  MiddleNumber = "",
                                  MiddleHide = s.MiddleHide,
                                  MiddleName = s.MiddleName,
                                  MiddleAnalyticalCode = s.MiddleAnalyticalCode,

                                  ActivityGroupByPart1 = s.ActivityGroupByPart1,
                                  ActivityGroupByPart2 = s.ActivityGroupByPart2,
                                  ActivityNumber = "",
                                  ActivityName = s.ActivityName,

                                  InditorActivity = s.InditorActivity,
                                  VolumeOrExpense = s.VolumeOrExpense,
                                  Year = s.Year,
                                  Value = Math.Round(s.Value, 1).ToString(),
                                  ValueD = s.Value,
                                  ValueWhithAdditionalNeed = Math.Round(s.ValueWhithAdditionalNeed, 1).ToString()
                              })
                          .ToList());

            }

            t4.AddRange((from ap in pre4
                         join prm in context.Program_ResourceMaintenance on ap.IdTaskCollection equals
                             prm.IdTaskCollection

                         where

                             prm.IdVersion == idVersion
                             && (prm.IdFinanceSource.HasValue && prm.FinanceSource.Code == "02")
                             && prm.IdValueType == (byte)ValueType.Plan // План
                             && prm.IdProgram == ap.midIdProgram
                             && !prm.IdTerminator.HasValue
                             && prm.HierarchyPeriod.DateStart.Year >= prf.startYear &&
                             prm.HierarchyPeriod.DateStart.Year <= prf.endYear
                             // это возможно лишнее, но убирать не стану, к автору !!! (c) Кузеванов

                             && !budgetYears.Contains(prm.HierarchyPeriod.DateStart.Year)

                         select new
                         {
                             TopGroupBy = ap.topId,
                             TopName = ap.sn6,
                             TopAnalyticalCode = ap.topAnalyticalCode,
                             MiddleGroupBy = ap.midId,
                             MiddleHide = ap.midHide,
                             MiddleName = ap.sn7,
                             MiddleAnalyticalCode = ap.midAnalyticalCode,

                             ActivityGroupByPart1 = ap.IdActivity,
                             ActivityGroupByPart2 = ap.IdIndicatorActivity_Volume,
                             ActivityName = ap.ActivityCaption, //sn8

                             InditorActivity = ap.IndicatorActivity, //sn9
                             prm.HierarchyPeriod.DateStart.Year,
                             prm.IsAdditionalNeed,

                             Value = prm.Value / 1000

                         }
                             into items
                             group items by new
                             {
                                 items.TopGroupBy,
                                 items.TopName,
                                 items.TopAnalyticalCode,

                                 items.MiddleGroupBy,
                                 items.MiddleName,
                                 items.MiddleHide,
                                 items.MiddleAnalyticalCode,

                                 items.ActivityGroupByPart1,
                                 items.ActivityGroupByPart2,
                                 items.ActivityName,

                                 items.InditorActivity,

                                 items.Year

                             }
                                 into gr
                                 select new
                                 {
                                     //ключ
                                     TopGroupBy = gr.Key.TopGroupBy,
                                     TopNumber = "",
                                     TopName = gr.Key.TopName,
                                     TopAnalyticalCode = gr.Key.TopAnalyticalCode,
                                     MiddleGroupBy = gr.Key.MiddleGroupBy,
                                     MiddleNumber = "",
                                     MiddleHide = gr.Key.MiddleHide,
                                     MiddleName = gr.Key.MiddleName,
                                     MiddleAnalyticalCode = gr.Key.MiddleAnalyticalCode,

                                     ActivityGroupByPart1 = gr.Key.ActivityGroupByPart1,
                                     ActivityGroupByPart2 = gr.Key.ActivityGroupByPart2,
                                     ActivityNumber = "",
                                     ActivityName = gr.Key.ActivityName,

                                     InditorActivity = gr.Key.InditorActivity,
                                     VolumeOrExpense = false,
                                     Year = gr.Key.Year,

                                     //агрегация
                                     Value = gr.Sum(item => !item.IsAdditionalNeed ? item.Value : 0),
                                     ValueWhithAdditionalNeed = gr.Sum(item => item.Value)
                                 })
                          .ToList()
                          .Select(s =>
                              new DSMain
                              {
                                  TopGroupBy = s.TopGroupBy,
                                  TopNumber = "",
                                  TopName = s.TopName,
                                  TopAnalyticalCode = s.TopAnalyticalCode,
                                  MiddleGroupBy = s.MiddleGroupBy,
                                  MiddleNumber = "",
                                  MiddleHide = s.MiddleHide,
                                  MiddleName = s.MiddleName,
                                  MiddleAnalyticalCode = s.MiddleAnalyticalCode,

                                  ActivityGroupByPart1 = s.ActivityGroupByPart1,
                                  ActivityGroupByPart2 = s.ActivityGroupByPart2,
                                  ActivityNumber = "",
                                  ActivityName = s.ActivityName,

                                  InditorActivity = s.InditorActivity,
                                  VolumeOrExpense = s.VolumeOrExpense,
                                  Year = s.Year,
                                  Value = Math.Round(s.Value, 1).ToString(),
                                  ValueD = s.Value,
                                  ValueWhithAdditionalNeed = Math.Round(s.ValueWhithAdditionalNeed, 1).ToString()
                              })
                          .ToList());


            #endregion Сноски 12,13

            if (!t3.Any())
                t3.Add(new DSMain()
                {
                    TopGroupBy = 0,
                    TopNumber = "",
                    TopName = null,
                    TopAnalyticalCode = "",
                    MiddleGroupBy = 0,
                    MiddleNumber = "",
                    MiddleHide = true,
                    MiddleName = null,
                    MiddleAnalyticalCode = "",

                    ActivityGroupByPart1 = null,
                    ActivityGroupByPart2 = null,
                    ActivityNumber = "",
                    ActivityName = null,

                    InditorActivity = null,
                    VolumeOrExpense = true,
                    Year = prf.startYear,

                    //агрегация
                    Value = null,
                    ValueD = null,
                    ValueWhithAdditionalNeed = null

                });

            if (!showNoFinanced)
            {
                var nonFinanced = t4.GroupBy(t => new { t.ActivityGroupByPart1, t.ActivityGroupByPart2 })
                                    .Where(g => g.Sum(e => e.ValueD) == 0)
                                    .Select(g => g.Key).ToList();

                t3.RemoveAll(
                    e =>
                    nonFinanced.Any(
                        a =>
                        a.ActivityGroupByPart1 == e.ActivityGroupByPart1 &&
                        a.ActivityGroupByPart2 == e.ActivityGroupByPart2));
                t4.RemoveAll(
                    e =>
                    nonFinanced.Any(
                        a =>
                        a.ActivityGroupByPart1 == e.ActivityGroupByPart1 &&
                        a.ActivityGroupByPart2 == e.ActivityGroupByPart2));
            }

            //Добавление отсутствующих годов
            t3.AddMissingInRange(prf.startYear, prf.endYear, s => s.Year, (obj, y) => (obj.Clone(y, null, null, null)));
            t4.AddMissingInRange(prf.startYear, prf.endYear, s => s.Year, (obj, y) => (obj.Clone(y, null, null, null)));

            t3.AddRange(t4);



            // Нумерация групп
            t3.NumerateGroups(gBy => new { TopGroupBy = gBy.TopGroupBy ?? 0, gBy.TopAnalyticalCode },
                              ord => ord.TopAnalyticalCode, (s, i) => s.TopNumber = i.ToString());
            t3.NumerateInternalGroupsInWhere(nIn => nIn.TopGroupBy ?? 0,
                                      nBy => new { MiddleGroupBy = nBy.MiddleGroupBy ?? 0, nBy.MiddleAnalyticalCode },
                                      ord => ord.MiddleAnalyticalCode,
                                      w => true,
                                       (s, i) => s.MiddleNumber = string.Format("{0}.{1}", s.TopNumber, i.ToString()));
            t3.NumerateInternalGroupsInWhere(nIn => nIn.MiddleGroupBy ?? 0,
                                      nBy => new { nBy.ActivityGroupByPart1, nBy.ActivityGroupByPart2, nBy.ActivityName, nBy.InditorActivity },
                                      ord => ord.ActivityName + "$" + ord.InditorActivity,
                                      w => true,
                                      (s, num) => s.ActivityNumber = string.Format("{0}.{1}", s.MiddleNumber, num.ToString()));


            return t3;
        }

        private class ClvaExt
        {
            public LimitVolumeAppropriations lva { get; set; }
            public EstimatedLine estl { get; set; }
            public HierarchyPeriod hp { get; set; }
        }

        private bool IsHiParent(int idChild, int idParent, List<SBP> lsbp)
        {
            var ch = lsbp.Where(r => r.Id == idChild).FirstOrDefault();
            do
            {
                if (!ch.IdParent.HasValue)
                {
                    return false;
                }

                if (ch.IdParent == idParent)
                {
                    return true;
                }

                ch = lsbp.Where(r => r.Id == ch.IdParent).FirstOrDefault();

            } while (true);

        }
    }
}
