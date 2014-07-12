using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Platform.BusinessLogic;
using Platform.BusinessLogic.ReportingServices.Reports;
using Platform.Common;
using Sbor.DbEnums;
using Sbor.Reference;
using Sbor.Reports.BaseLogic;
using ValueType = Sbor.DbEnums.ValueType;

namespace Sbor.Reports.ForecastConsolidatedIndicators
{
    [Report]
    public class ForecastConsolidatedIndicatorsReport
    {
        public bool ByApproved { get; set; } //Строить отчет по утвержденным данным	Bool
        public  string Caption { get; set; } //	Наименование	String
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
        // ReSharper restore InconsistentNaming

        public List<DSHeader> DataSetHeader()
        {
            
            var context = IoC.Resolve<DbContext>().Cast<DataContext>();
            
            var pr = (
                            from p in context.Program
                            join ap in context.AttributeOfProgram on p.Id equals ap.IdProgram
                            where p.Id == idProgram
                                          && (
                                                (ByApproved && ap.DateCommit <= DateReport && (ap.DateTerminate > DateReport || ap.DateTerminate == null))
                                                || (!ByApproved && ap.DateTerminate == null)
                                              )
                            select new
                            {
                                StateProgramName = p.Caption, //1
                                ImplementationPeriodStart = ap.DateStart.Year, //2
                                ImplementationPeriodEnd = ap.DateEnd.Year, //2
                                Executive = p.SBP.Organization.Description, //3
                            });

            var header = new DSHeader();

            header.ReportDate = this.DateReport;
            header.PPOCaption = context.PublicLegalFormation.FirstOrDefault(w => w.Id == idPublicLegalFormation).Caption;
            if (!pr.Any())
            {
                header.Header = "Не найдено ни одной соответствующей записи в регистре 'Атрибуты программ'";
            }
            else if (pr.Count()>1)
                header.Header = "Невозможно однозначно определить запись регистра 'Атрибуты программ', количество найденных записей - " + pr.Count();
            else
            {
                var prf = pr.First();
                header.Header = string.Format("«{0}» на {1}-{2} годы", prf.StateProgramName, prf.ImplementationPeriodStart,prf.ImplementationPeriodEnd);
                header.Executive = prf.Executive;
            }

            return new List<DSHeader>() { header };
        }

        public List<DSMain> DataSetMain()
        {
            var context = IoC.Resolve<DbContext>().Cast<DataContext>();

            #region Подготовка параметров

                var pr = (
                    from p in context.Program
                    join ap in context.AttributeOfProgram on p.Id equals ap.IdProgram
                    where p.Id == idProgram
                                  && (
                                        (ByApproved && ap.DateCommit <= DateReport && (ap.DateTerminate > DateReport || ap.DateTerminate == null))
                                        || (!ByApproved && ap.DateTerminate == null)
                                      )
                    select new
                    {
                        startYear = ap.DateStart.Year, //2
                        endYear = ap.DateEnd.Year, //2
                    });

                if (pr.Count()!=1)
                {
                    return new List<DSMain>()
                        {
                            new DSMain()
                                {
                                    TopName = "Невозможно однозначно определить запись регистра 'Атрибуты программ', количество найденных записей - " + pr.Count()
                                }
                        };
                }

                var prf = pr.First();

            #endregion Подготовка параметров запроса

            #region Сноски 7,8. Обработка регистров "Программы" и "Атрибуты программ"

            #region Выборка
            //Получение сносок (6,14,22)(подчиненная к ГП)(часть top)
            //        и сносок (7,15) (поддиченнная подчиненной ГП)(часть mid)
            //Выборка
            var t1 = (from top in context.AttributeOfProgram
                     join jmid in context.AttributeOfProgram on top.IdProgram equals jmid.IdParent into tmpmid 
                     from mid in tmpmid.DefaultIfEmpty()
                     
                     where 
                         top.IdParent == idProgram 
                         &&  (    (     (top.Program.IdDocType == DocType.SubProgramSP) //6
                                     && (mid.Program.IdDocType == DocType.ProgramOfSBP  || mid.Program.IdDocType == DocType.MainActivity)) //7
                               || (     (top.Program.IdDocType == DocType.LongTermGoalProgram) //14      
                                     && (mid.Program.IdDocType == DocType.SubProgramDGP)) //15 
                               || (     (top.Program.IdDocType == DocType.ProgramOfSBP  || top.Program.IdDocType == DocType.MainActivity) //22
                                     && (mid == null)) //22-23
                             )

                             && ( ByApproved ? (
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
                            topStartYear = top.DateStart.Year,
                            topEndYear = top.DateEnd.Year,

                            midId = mid.Id,
                            midHide = (mid == null),
                            midIdProgram = (mid != null) ? mid.IdProgram : top.IdProgram,
                            //idParent2 = (mid != null)? mid.IdParent, top.IdProgram
                            midType = (mid != null) ? mid.Program.IdDocType: 0,
                            midTypeCaption = (mid != null)?mid.Program.DocType.Caption: null,
                            midName = mid.Caption,
                            midStartYear = mid.DateStart.Year,
                            midEndYear = mid.DateEnd.Year
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

                        midId = s.midId,
                        midHide = s.midHide,
                        midIdProgram = s.midIdProgram,
                        sn7 = (s.midHide) ? null : (string.Format("{0} «{1}» на {2}-{3} годы", s.midTypeCaption, s.topName, s.midStartYear.ToString(), s.midEndYear.ToString()))
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

            var t3 = (from ap in t2
                      join tv in context.TaskVolume on ap.midIdProgram equals tv.IdProgram
                      join tc in context.TaskCollection on tv.IdTaskCollection equals tc.Id
                      
                      where
                        ( ByApproved ? ( tv.DateCommit <= DateReport && (tv.DateTerminate > DateReport || tv.DateTerminate == null))
                                     : (tv.DateTerminate == null))
                        && ((tc.Activity.IdActivityType == (byte)ActivityType.Service) || (tc.Activity.IdActivityType == (byte)ActivityType.Work))

                        && tv.IdValueType == (byte)ValueType.Plan //Тип значения = План
                        && tv.HierarchyPeriod.DateStart.Year >= prf.startYear && tv.HierarchyPeriod.DateStart.Year <= prf.endYear

                      select new DSMain()
                          {
                              TopGroupBy = ap.topId,
                              TopNumber = "",
                              TopName = ap.sn6,
                              MiddleGroupBy = ap.midId,
                              MiddleNumber = "",
                              MiddleHide = ap.midHide,
                              MiddleName = ap.sn7,
                              ActivityGroupByPart1 = tc.IdActivity,
                              ActivityGroupByPart2 = tv.IdIndicatorActivity_Volume,
                              ActivityNumber = "",
                              ActivityName = tc.Activity.Caption, //sn8
                              InditorActivity = tv.IndicatorActivity_Volume.Caption + ", " + tv.IndicatorActivity_Volume.UnitDimension.Caption, //sn9
                              VolumeOrExpense = true,
                              Year = tv.HierarchyPeriod.DateStart.Year,
                              IsAdditionalNeed = tv.IsAdditionalNeed,
                              Value = tv.Value
                          }
                      ).ToList();

            #endregion 

            #region Сноски 12,13

            var t4 = (from ap in t2
                      join tv in context.TaskVolume on ap.midIdProgram equals tv.IdProgram
                      join tc in context.TaskCollection on tv.IdTaskCollection equals tc.Id
                      join lva in context.LimitVolumeAppropriations on tc.Id equals lva.IdTaskCollection
                      
                      where
                        ( ByApproved ? ( tv.DateCommit <= DateReport && (tv.DateTerminate > DateReport || tv.DateTerminate == null))
                                     : (tv.DateTerminate == null))
                        && ((tc.Activity.IdActivityType == (byte)ActivityType.Service) || (tc.Activity.IdActivityType == (byte)ActivityType.Work))

                        && lva.IdBudget == idBudget
                        && lva.IdValueType == (byte)ValueType.Justified //Тип значения = Обосновано
                        && lva.EstimatedLine.SBP.IdSBPType == (byte)SBPType.TreasuryEstablishment //СБП с типом «Казенное учреждение»
                        && lva.EstimatedLine.SBP.IdParent == tv.IdSBP //Сметная строка.СБП , у которых в поле вышестоящий ссылка на СБП, найденный в <Сноска 8>
                        
                        && lva.HierarchyPeriod.DateStart.Year >= prf.startYear && lva.HierarchyPeriod.DateStart.Year <= prf.endYear

                      select new DSMain()
                      {
                            TopGroupBy = ap.topId,
                            TopNumber = "",
                            TopName = ap.sn6,
                            MiddleGroupBy = ap.midId,
                            MiddleNumber = "",
                            MiddleHide = ap.midHide,
                            MiddleName = ap.sn7,
                            ActivityGroupByPart1 = tc.IdActivity,
                            ActivityGroupByPart2 = tv.IdIndicatorActivity_Volume,
                            ActivityNumber = "",
                            ActivityName = tc.Activity.Caption, //sn8
                            InditorActivity = tv.IndicatorActivity_Volume.Caption + ", " + tv.IndicatorActivity_Volume.UnitDimension.Caption, //sn9
                            VolumeOrExpense = false,
                            Year = lva.HierarchyPeriod.DateStart.Year,
                            IsAdditionalNeed = lva.HasAdditionalNeed ?? false,
                            Value = lva.Value
                      }).ToList();

            #endregion Сноски 12,13

            //Добавление отсутствующих годов
            t3.AddMissingInRange(prf.startYear, prf.endYear, s => s.Year, (obj, y) => (obj.Clone(y, null)));
            t4.AddMissingInRange(prf.startYear, prf.endYear, s => s.Year, (obj, y) => (obj.Clone(y, null)));

            t4.AddRange(t3);

            // Нумерация групп
            t4.NumerateGroups(gBy => gBy.TopGroupBy, (s, i) => s.TopNumber = i.ToString());
            t4.NumerateInternalGroups(nIn => nIn.TopGroupBy, nBy => nBy.MiddleGroupBy,
                                      (s, i) => s.MiddleNumber = string.Format("{0}.{1}", s.TopNumber, i.ToString()));
            t4.NumerateInternalGroups(nIn => nIn.MiddleGroupBy,
                                      nBy => new {nBy.ActivityGroupByPart1, nBy.ActivityGroupByPart2},
                                      (s, i) => string.Format("{0}.{1}", s.MiddleNumber, i.ToString()));


            return t4;
        }
    }
}
