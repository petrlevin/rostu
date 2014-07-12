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
using Sbor.Reports.RegistryGoal;

//using Sbor.Reports.Reports;

namespace Sbor.Reports.Report
{
    [Report]
    public partial class RegistryGoal
    {
        //public int idPublicLegalFormation { get; set; } //ППО
        //public int idSourcesDataReports { get; set; } //Источник данных для отчета
        //public int idVersion { get; set; } //Версия
        //public bool ConstructReportApprovedData { get; set; } //Строить отчет по утвержденным данным
        //public DateTime DateReport { get; set; } //Дата отчета
        //public int idSBP { get; set; } //СБП
        //public bool OutputGoalOperatingPeriod { get; set; } //Выводить цели, действующие в период
        //public DateTime DateStart { get; set; } //Начало периода
        //public DateTime DateEnd { get; set; } //Конец периода
        //public bool DisplayReportCodeS { get; set; } //Выводить в отчет код элемента СЦ справочника «Система целеполагания»
        //public bool DisplayReportDataGoal { get; set; } //Выводить в отчет показатели цели (задачи)
        //public bool DispleySelectedParameterValues { get; set; } //Выводить выбранные значения параметров в отчет
        //public bool RepeatTableHeader { get; set; } //Повторять заголовки  таблиц на каждой странице
        //public bool GenerateValuesWithDetails { get; set; } //Формировать значения с расшифровкой (drilldown пока не нужно делать позже будет сделано )
        
        public List<PrimeDataSet> res = new List<PrimeDataSet>();
        
        public List<PrimeDataSet> hierarchyGoal()
        {
            switch (IdSourcesDataReports) // (idSourcesDataReports)
            {
                case 1:
                    res = HierarchyRegister();
                    break;
                case 5:
                    res = HierarchySystemGoal();
                    break;
            }
            return res;

        }

    public List<PrimeDataSet> datereport()
    {
        if (DateReport.ToString() == "01.01.0001 0:00:00")
        {
            DataContext context = IoC.Resolve<DbContext>().Cast<DataContext>();
            var res1 = new List<PrimeDataSet>();
            PrimeDataSet pds = new PrimeDataSet();
            pds.DateStart = DateTime.Now;
            res1.Add(pds);
            return res1;
        }
        else
        {
            DataContext context = IoC.Resolve<DbContext>().Cast<DataContext>();
            var pres =  new List<PrimeDataSet>();
            PrimeDataSet pds =  new PrimeDataSet();
            if (ConstructReportApprovedData == false)
            {
                pds.DateStart = DateTime.Now;
                pres.Add(pds);
            }
            else
            {
                pds.DateStart = DateReport.Value;
                pres.Add(pds);
            }
            
            return pres;
        }
    }


        private List<PrimeDataSet> HierarchySystemGoal() //данные из справочника
        {
            //ввести проверку на флаг действия в отчет период и если он отсутсвует увелить интервал отбора с 1900 по 2100 года
            DateTime DtStart = DateStart ?? new DateTime(1901, 1, 1, 0, 00, 00);
            DateTime DtEnd = DateEnd ?? new DateTime(2099, 1, 1, 0, 00, 00);
            DateTime DtStartBudget = new DateTime(1901, 1, 1, 0, 00, 00);
            DateTime DtEndBudget = new DateTime(2099, 1, 1, 0, 00, 00);
            DateTime DtReport = DateReport ?? new DateTime(2099, 1, 1, 0, 00, 00);
            bool ApprovedData = ConstructReportApprovedData ?? false;
            bool OutputRepPeriod = OutputGoalOperatingPeriod ?? false;
            bool CodeOrHierarchy = DisplayReportCodeS ?? false;
            bool DisplayGoal = DisplayReportDataGoal ?? false;
            int Version = IdVersion ?? 0; // idVersion;
            int? SBP = IdSBP;

            if (OutputRepPeriod == false)
            {
                DtStart = new DateTime(1901, 1, 1, 0, 00, 00);
                DtEnd = new DateTime(2099, 1, 1, 0, 00, 00);
            }

            DataContext context = IoC.Resolve<DbContext>().Cast<DataContext>();

            int[] idtsg = ElementTypeSystemGoal.Select(s => s.Id).ToArray();
            bool isFilterTypeSG = idtsg.Any();
            //(
            //(s.DateStart >= DtStart && s.DateEnd <= DtStart)||
            //(s.DateStart >= DtEnd && s.DateEnd <= DtEnd)||
            //(s.DateStart<= DtStart && s.DateEnd >= DtEnd)
            //)
            List<PrimeDataSet> pres = (
                                            from sg in
                                                context.SystemGoal.Where(s =>
                                                    (
                                                     (s.DateStart >= DtStart && s.DateStart <= DtEnd) ||
                                                     (s.DateEnd >= DtStart && s.DateEnd <= DtEnd) ||
                                                     (s.DateStart <= DtStart && s.DateEnd >= DtEnd)
                                                    )
                                                    && (!isFilterTypeSG || idtsg.Contains(s.IdElementTypeSystemGoal))
                                                    && s.IdPublicLegalFormation == IdPublicLegalFormation
                                                    && s.IdRefStatus != 4
                                                    )
                                            join plf in context.PublicLegalFormation on sg.IdPublicLegalFormation equals plf.Id

                                            join jsbp in context.SBP on sg.IdSBP equals jsbp.Id into tmjsbp
                                            from sbp in tmjsbp.DefaultIfEmpty()

                                            join jorg in context.Organization on sbp.IdOrganization equals jorg.Id into tmjorg
                                            from org in tmjorg.DefaultIfEmpty()

                                            //join jsggi in context.SystemGoal_GoalIndicator on sg.Id equals jsggi.IdOwner into tmjsggi
                                            //from sggi in tmjsggi.DefaultIfEmpty()

                                            //join jgi in context.GoalIndicator on sggi.IdGoalIndicator equals jgi.Id into tmjgi
                                            //from gi in tmjgi.DefaultIfEmpty()

                                            //join jsggiv in context.SystemGoal_GoalIndicatorValue on sggi.Id equals jsggiv.IdMaster into tmjsggiv
                                            //from sggiv in tmjsggiv.DefaultIfEmpty()

                                            //join jud in context.UnitDimension on gi.IdUnitDimension equals jud.Id into tmjud
                                            //from ud in tmjud.DefaultIfEmpty()
                                            
                                            //join jetsg in context.ElementTypeSystemGoal on sg.IdElementTypeSystemGoal equals jetsg.Id into tmjetsg
                                            //from etsg in tmjetsg.DefaultIfEmpty()

                                            //join jhp in context.HierarchyPeriod on sggiv.IdHierarchyPeriod equals jhp.Id into
                                            //  tmjhp
                                            //from hp in tmjhp.DefaultIfEmpty()
                                            #region Analog_SQL
                                            //ref.SystemGoal AS SG
                                            //LEFT JOIN tp.SystemGoal_GoalIndicator AS SGGI -- ТЧ «Элементы СЦ»
                                            //    ON SGGI.idOwner = SG.id
                                            //LEFT JOIN  ref.GoalIndicator As GI
                                            //    ON SGGI.idGoalIndicator = GI.id
                                            //LEFT JOIN tp.SystemGoal_GoalIndicatorValue AS SGGIV
                                            //    ON SGGIV.idMaster = SGGI.id
                                            //LEFT JOIN ref.UnitDimension AS UD
                                            //    ON UD.id = GI.idUnitDimension
                                            //INNER JOIN ref.SBP AS SBP 
                                            //    ON Q.idSBP = SBP.id 
                                            //INNER JOIN ref.Organization AS Org
                                            //    ON SBP.idOrganization = Org.id 
                                            //LEFT JOIN ref.ElementTypeSystemGoal AS ETSG
                                            //ON SG.IdElementTypeSystemGoal = ETSG.Id
                                            #endregion

                                            select new PrimeDataSet()
                                                       {
                                                           Id = sg.Id,//элемент
                                                           IdParent = sg.IdParent,//родитель элемента
                                                           //TypeGoal = etsg.Caption,//Тип элемента СЦ 
                                                           CaptionGoal = sg.Caption,//наименование цели
                                                           SubjectBP = org.Caption,//ППО
                                                           DateStart = sg.DateStart,//Дата начала реализации
                                                           DateEnd = sg.DateEnd,//Дата окончания реализации
                                                           GoalCode = sg.Code,//код цели
                                                           //CapGoalIndicator = gi.Caption,//Наименование целевого показателя
                                                           //CapUnitDimension = ud.Caption,//Единица измерения целевого показателя
                                                           //Year = hp.DateEnd.Year,//год значения целевых показателей
                                                           //GIValue = sggiv.Value,//Значения целевых показателей
                                                           IdElementTypeSystemGoal = sg.IdElementTypeSystemGoal,//тип елемента
                                                           NN = sg.Code,//Код цели
                                                           IdSubjectBP = sbp.Id, //id sbp
                                                           //code = (SBP != null ? sbp.Caption : null), //наименование СБП
                                                           CaptionSBP = (SBP != null ? sbp.Caption : null), //наименование СБП
                                                           PPO = plf.Caption
                                                       }
                                      ).ToList();

            //убираем лишние СБП
            if (SBP != null)
            {
                var SBPlist = SBPHierarchy();
                pres = (from t1 in pres
                        join t2 in SBPlist on t1.IdSubjectBP equals t2.Id into r
                        from sbpNull in r.DefaultIfEmpty()
                        select new PrimeDataSet
                        {
                            Id = t1.Id,
                            IdParent = t1.IdParent,
                            TypeGoal = t1.TypeGoal,
                            CaptionGoal = t1.CaptionGoal,
                            GoalCode = t1.GoalCode,//код цели
                            SubjectBP = t1.SubjectBP, //входной параметр отчета
                            DateStart = t1.DateStart,
                            DateEnd = t1.DateEnd,
                            CapGoalIndicator = t1.CapGoalIndicator,
                            CapUnitDimension = t1.CapUnitDimension,
                            Year = t1.Year,
                            GIValue = t1.GIValue,
                            NN = t1.NN, //в зависимости от флага
                            IdSubjectBP = (sbpNull != null ? sbpNull.Id : null), //id sbp
                            CaptionSBP = t1.CaptionSBP,
                            PPO = t1.PPO,
                            IdElementTypeSystemGoal = t1.IdElementTypeSystemGoal
                        }).ToList();
                pres = pres.Where(s => s.IdSubjectBP != null).ToList();
            }

            


            //зануляем родителей у элементов у которых родители отсутсвуют в списке 
            List<PrimeDataSet> sdf = (from p1 in pres
                                      join p2 in pres
                                      on p1.IdParent equals p2.Id into g
                                      from result in g.DefaultIfEmpty()
                                      select new PrimeDataSet
                                      {
                                          Id = p1.Id,
                                          IdParent = (result != null ? result.Id : null),
                                          TypeGoal = p1.TypeGoal,
                                          CaptionGoal = p1.CaptionGoal,
                                          GoalCode = p1.GoalCode,//код цели
                                          SubjectBP = p1.SubjectBP, //входной параметр отчета
                                          DateStart = p1.DateStart,
                                          DateEnd = p1.DateEnd,
                                          CapGoalIndicator = p1.CapGoalIndicator,
                                          CapUnitDimension = p1.CapUnitDimension,
                                          Year = p1.Year,
                                          GIValue = p1.GIValue,
                                          NN = p1.NN, //в зависимости от флага
                                          IdSubjectBP = p1.IdSubjectBP, //id sbp
                                          CaptionSBP = p1.CaptionSBP,
                                          PPO = p1.PPO
                                      }).ToList();

            //обрезать цели со статусом архив
            var NotValidElem = NotValidSystemGoal();

            List<PrimeDataSet> TemporaryList = (from p1 in sdf
                                                join p2 in NotValidElem on p1.Id equals p2.Id into g
                                                from amount in g.DefaultIfEmpty()
                                                select new PrimeDataSet
                                                {
                                                    Id = (amount == null ? p1.Id : null),
                                                    IdParent = p1.IdParent,
                                                    TypeGoal = p1.TypeGoal,
                                                    CaptionGoal = p1.CaptionGoal,
                                                    GoalCode = p1.GoalCode,//код цели
                                                    SubjectBP = p1.SubjectBP, //входной параметр отчета
                                                    DateStart = p1.DateStart,
                                                    DateEnd = p1.DateEnd,
                                                    CapGoalIndicator = p1.CapGoalIndicator,
                                                    CapUnitDimension = p1.CapUnitDimension,
                                                    Year = p1.Year,
                                                    GIValue = p1.GIValue,
                                                    NN = p1.NN, //в зависимости от флага
                                                    IdSubjectBP = p1.IdSubjectBP, //id sbp
                                                    CaptionSBP = p1.CaptionSBP,
                                                    PPO = p1.PPO
                                                }).ToList();
            TemporaryList = TemporaryList.Where(s => s.Id != null).ToList();
            //foreach (var t in TemporaryList)
            //    t.GoalCodeInt = Convert.ToInt32(t.GoalCode);

            //Строим иерархию
            if (DisplayReportCodeS == false)
            {
                numHier(TemporaryList);
                numHierView(TemporaryList);
            }
            else
            {
                numHierCodes(TemporaryList);
            }

            var budgetyear =
                    context.Budget.Where(s => s.Id == IdBudget)
                           .Select(d => d.Year)
                           .FirstOrDefault();

            if (DisplayDataBudgetPeriod == true)
            {
                DtStartBudget = new DateTime(budgetyear, 1, 1, 0, 00, 00);
                DtEndBudget = new DateTime(budgetyear + 2, 1, 1, 0, 00, 00);
            }

            List<PrimeDataSet> resume = (
                                            from sg in
                                                context.SystemGoal.Where(s =>
                                                    (!isFilterTypeSG || idtsg.Contains(s.IdElementTypeSystemGoal))
                                                    && s.IdPublicLegalFormation == IdPublicLegalFormation
                                                    && s.IdRefStatus != 4
                                                    )
                                            join plf in context.PublicLegalFormation on sg.IdPublicLegalFormation equals plf.Id

                                            join jsbp in context.SBP on sg.IdSBP equals jsbp.Id into tmjsbp
                                            from sbp in tmjsbp.DefaultIfEmpty()

                                            join jorg in context.Organization on sbp.IdOrganization equals jorg.Id into tmjorg
                                            from org in tmjorg.DefaultIfEmpty()

                                            join jsggi in context.SystemGoal_GoalIndicator on sg.Id equals jsggi.IdOwner into tmjsggi
                                            from sggi in tmjsggi.DefaultIfEmpty()

                                            join jgi in context.GoalIndicator on sggi.IdGoalIndicator equals jgi.Id into tmjgi
                                            from gi in tmjgi.DefaultIfEmpty()

                                            join jsggiv in context.SystemGoal_GoalIndicatorValue on sggi.Id equals jsggiv.IdMaster into tmjsggiv
                                            from sggiv in tmjsggiv.DefaultIfEmpty()

                                            join jud in context.UnitDimension on gi.IdUnitDimension equals jud.Id into tmjud
                                            from ud in tmjud.DefaultIfEmpty()

                                            join jetsg in context.ElementTypeSystemGoal on sg.IdElementTypeSystemGoal equals jetsg.Id into tmjetsg
                                            from etsg in tmjetsg.DefaultIfEmpty()

                                            join jhp in context.HierarchyPeriod.Where(s => s.DateEnd >= DtStartBudget && s.DateEnd <= DtEndBudget) on sggiv.IdHierarchyPeriod equals jhp.Id into
                                              tmjhp
                                            from hp in tmjhp.DefaultIfEmpty()

                                            select new PrimeDataSet()
                                            {
                                                Id = sg.Id,//элемент
                                                IdParent = sg.IdParent,//родитель элемента
                                                GoalCode = sg.Code,
                                                TypeGoal = etsg.Caption,//Тип элемента СЦ 
                                                CaptionGoal = sg.Caption,//наименование цели
                                                SubjectBP = org.Caption,//ППО
                                                DateStart = sg.DateStart,//Дата начала реализации
                                                DateEnd = sg.DateEnd,//Дата окончания реализации
                                                CapGoalIndicator = gi.Caption,//Наименование целевого показателя
                                                CapUnitDimension = ud.Caption,//Единица измерения целевого показателя
                                                Year = hp.DateEnd.Year,//год значения целевых показателей
                                                GIValue = sggiv.Value,//Значения целевых показателей
                                                IdElementTypeSystemGoal = sg.IdElementTypeSystemGoal,//тип елемента
                                                NN = sg.Code,//Код цели
                                                IdSubjectBP = sbp.Id, //id sbp
                                                CaptionSBP = (SBP != null ? sbp.Caption : null), //наименование СБП
                                                PPO = plf.Caption
                                            }
                                      ).ToList();

            List<PrimeDataSet> summation = (from p1 in resume
                                            join p2 in TemporaryList on p1.Id equals p2.Id
                                                
                                                select new PrimeDataSet
                                                {
                                                    Id = p1.Id,
                                                    IdParent = p2.IdParent,
                                                    TypeGoal = p1.TypeGoal,
                                                    GoalCode = p1.GoalCode,
                                                    CaptionGoal = p1.CaptionGoal,
                                                    SubjectBP = p1.SubjectBP, //входной параметр отчета
                                                    DateStart = p1.DateStart,
                                                    DateEnd = p1.DateEnd,
                                                    CapGoalIndicator = p1.CapGoalIndicator,
                                                    CapUnitDimension = p1.CapUnitDimension,
                                                    Year = p1.Year,
                                                    GIValue = p1.GIValue,
                                                    NN = p2.NN, //в зависимости от флага
                                                    numb = p2.numb,
                                                    IdSubjectBP = p1.IdSubjectBP, //id sbp
                                                    CaptionSBP = p1.CaptionSBP,
                                                    PPO = p1.PPO
                                                }).ToList();
           



            //заполняем всеми годами и убираем пустые столбцы
            if (summation.Count() != 0)
            {
                DateTime? minDate = summation.Select(s => s.DateStart).Min();
                DateTime? maxDate = summation.Select(s => s.DateStart).Max();
                PrimeDataSet pds = new PrimeDataSet();

                pds = summation[0];
                for (int i = minDate.Value.Year; i <= maxDate.Value.Year; i++)
                {
                    pds.Year = i;
                    summation.Add(pds);
                }

                foreach (var t in summation)
                    if (t.Year == null)
                        t.Year = minDate.Value.Year;
            }

            //if (SBP != null)
            //    pres = pres.Where(s => s.IdSubjectBP == SBP).ToList();
            //if (DisplayReportCodeS == false)
            //{
            //    var b =
            //        pres.Select(s => s.Id).Except(pres.Join(pres, t1 => t1.Id, t2 => t2.IdParent,
            //                                                (t1, t2) => new PrimeDataSet()
            //                                                    {
            //                                                        Id = t2.Id,
            //                                                        CaptionGoal = t2.CaptionGoal
            //                                                    }).Select(t => t.Id));
            //    foreach (var f in pres)
            //        if (b.Contains(f.Id))
            //            f.IdParent = null;


            //    numHier(pres);
            //    //AddNull(pres);
            //}

            //if (DisplayReportCodeS == false)
            //{
                foreach (var t in summation)
                    t.code = Convert.ToInt32(t.NN.Substring(0, t.NN.IndexOf('.')));
            //}

            return summation;
        }


        private List<PrimeDataSet> HierarchyRegister()//данные из регистров
        {
            DateTime DtStart = DateStart ?? new DateTime(1901, 1, 1, 0, 00, 00);
            DateTime DtEnd = DateEnd ?? new DateTime(2099, 1, 1, 0, 00, 00);
            DateTime DtReport = DateReport ?? new DateTime(2099, 1, 1, 0, 00, 00);
            int? SBP = IdSBP;
            int? Version = IdVersion;
            DateTime DtStartBudget = new DateTime(1901, 1, 1, 0, 00, 00);
            DateTime DtEndBudget = new DateTime(2099, 1, 1, 0, 00, 00);

            DataContext context = IoC.Resolve<DbContext>().Cast<DataContext>();

            var budgetyear =
                    context.Budget.Where(s => s.Id == IdBudget)
                           .Select(d => d.Year)
                           .FirstOrDefault();

            if (DisplayDataBudgetPeriod == true)
            {
                DtStartBudget = new DateTime(budgetyear, 1, 1, 0, 00, 00);
                DtEndBudget = new DateTime(budgetyear + 2, 1, 1, 0, 00, 00);
            }  


            if (OutputGoalOperatingPeriod == false)
            {
                DtStart = new DateTime(1901, 1, 1, 0, 00, 00);
                DtEnd = new DateTime(2099, 1, 1, 0, 00, 00);
            }

            if (ConstructReportApprovedData == false)
            {
                DtReport = new DateTime(2099, 1, 1, 0, 00, 00);               
            }

            int[] idtsg = ElementTypeSystemGoal.Select(s => s.Id).ToArray();
            bool isFilterTypeSG = idtsg.Any();


            if (ConstructReportApprovedData == false)
            {

                List<PrimeDataSet> pres = (
                                        from aosge in context.AttributeOfSystemGoalElement.Where(
                                        t => t.IdVersion == Version
                                        && t.IdTerminator == null
                                        && t.IdPublicLegalFormation == IdPublicLegalFormation )
                                             join jsge in context.SystemGoalElement.Where(
                                                 t => t.IdVersion == Version
                                                       && t.IdTerminator == null
                                                       && t.IdPublicLegalFormation == IdPublicLegalFormation
                                                       ) on aosge.IdSystemGoalElement equals jsge.Id
                                                 into tmjsge
                                             from sge in tmjsge.DefaultIfEmpty()

                                             join jsg in
                                                 context.SystemGoal.Where(s =>
                                                                           ( 
		                                                                    (s.DateStart >= DtStart && s.DateStart <= DtEnd) ||
                                                                            (s.DateEnd >= DtStart && s.DateEnd <= DtEnd) ||
                                                                            (s.DateStart <= DtStart && s.DateEnd >= DtEnd)
                                                                           )
                                                                          &&
                                                                          (!isFilterTypeSG ||
                                                                           idtsg.Contains(s.IdElementTypeSystemGoal))
                                                                          &&
                                                                          s.IdPublicLegalFormation ==
                                                                          IdPublicLegalFormation
                                                 ) on sge.IdSystemGoal equals jsg.Id into tmjsg
                                             from sg in tmjsg.DefaultIfEmpty()

                                             join plf in context.PublicLegalFormation on sg.IdPublicLegalFormation
                                                 equals plf.Id

                                             join jetsg in context.ElementTypeSystemGoal on
                                                 aosge.IdElementTypeSystemGoal
                                                 equals jetsg.Id into tmjetsg
                                             from etsg in tmjetsg.DefaultIfEmpty()

                                             join jsbp in context.SBP on aosge.IdSBP equals jsbp.Id into tmjsbp
                                             from sbp in tmjsbp.DefaultIfEmpty()

                                             join jorg in context.Organization on sbp.IdOrganization equals jorg.Id
                                                 into
                                                 tmjorg
                                             from org in tmjorg.DefaultIfEmpty()

                                             //join jgt in context.GoalTarget.Where(
                                             //    t => t.IdVersion == Version
                                             //          && t.IdTerminator == null
                                             //          && t.IdPublicLegalFormation == IdPublicLegalFormation
                                             //           ) on sge.Id equals
                                             //    jgt.IdSystemGoalElement into //reg
                                             //    tmjgt
                                             //from gt in tmjgt.DefaultIfEmpty()

                                             //join jgi in context.GoalIndicator on gt.IdGoalIndicator equals jgi.Id into
                                             //    tmjgi
                                             //from gi in tmjgi.DefaultIfEmpty()

                                             //join jud in context.UnitDimension on gi.IdUnitDimension equals jud.Id into
                                             //    tmjud
                                             //from ud in tmjud.DefaultIfEmpty()

                                             //join jvgt in context.ValuesGoalTarget.Where(p =>  p.IdVersion == Version
                                             //                                                  && p.IdTerminator == null
                                             //                                                  && p.IdPublicLegalFormation == IdPublicLegalFormation
                                             //    ) on gt.Id //reg с типом "план"
                                             //    equals jvgt.IdGoalTarget into tmjvgt
                                             //from vgt in tmjvgt.DefaultIfEmpty()

                                             //join jhp in context.HierarchyPeriod on vgt.IdHierarchyPeriod equals jhp.Id
                                             //    into
                                             //    tmjhp
                                             //from hp in tmjhp.DefaultIfEmpty()

                                              select new PrimeDataSet()
                                                  {
                                                      Id = aosge.IdSystemGoalElement,
                                                      IdParent = aosge.IdSystemGoalElement_Parent,
                                                      IdSystemGoal = sg.Id,
                                                      TypeGoal = etsg.Caption,
                                                      CaptionGoal = sg.Caption,
                                                      GoalCode = sg.Code,
                                                      SubjectBP = org.Caption, //входной параметр отчета
                                                      DateStart = aosge.DateStart,
                                                      DateEnd = aosge.DateEnd,
                                                      //CapGoalIndicator = gi.Caption,
                                                      //CapUnitDimension = ud.Caption,
                                                      //Year = hp.DateEnd.Year,
                                                      //GIValue = vgt.Value,
                                                      NN = sg.Code, //в зависимости от флага
                                                      IdSubjectBP = sbp.Id, //id sbp
                                                      CaptionSBP = (SBP != null ? org.Description : ""), //наименование СБП
                                                      PPO = plf.Caption
                                                  }

                                          #region SQL analog
                                          //LEFT JOIN reg.SystemGoalElement AS SGE
                                          //    ON Q.id = SGE.idSystemGoal
                                          //LEFT JOIN ref.ElementTypeSystemGoal AS ETSG
                                          //    ON SGE.IdElementTypeSystemGoal = ETSG.Id
                                          //LEFT JOIN ref.SBP AS SBP 
                                          //    ON SGE.idSBP = SBP.id 
                                          //LEFT JOIN ref.Organization As Org
                                          //    ON SBP.idOrganization = Org.id 
                                          //LEFT JOIN reg.GoalTarget AS GT
                                          //    ON SGE.id = GT.idSystemGoalElement
                                          //LEFT JOIN ref.GoalIndicator AS GI
                                          //    ON GT.idGoalIndicator = GI.id 
                                          //LEFT JOIN ref.UnitDimension AS UD
                                          //    ON GI.idUnitDimension = UD.id 
                                          //LEFT JOIN reg.ValuesGoalTarget AS VGT
                                          //    ON GT.id = VGT.idGoalTarget 
                                          //LEFT JOIN enm.ValueType AS VT
                                          //    ON VGT.idValueType = VT.id 
                                          #endregion

                                          ).ToList();

                //убираем лишние СБП
                if (SBP != null)
                {
                    var SBPlist = SBPHierarchy();
                    pres = (from t1 in pres
                            join t2 in SBPlist on t1.IdSubjectBP equals t2.Id into r
                            from sbpNull in r.DefaultIfEmpty()
                            select new PrimeDataSet
                            {
                                Id = t1.Id,
                                IdParent = t1.IdParent,
                                IdSystemGoal = t1.IdSystemGoal,
                                TypeGoal = t1.TypeGoal,
                                CaptionGoal = t1.CaptionGoal,
                                GoalCode = t1.GoalCode,//код цели
                                SubjectBP = t1.SubjectBP, //входной параметр отчета
                                DateStart = t1.DateStart,
                                DateEnd = t1.DateEnd,
                                CapGoalIndicator = t1.CapGoalIndicator,
                                CapUnitDimension = t1.CapUnitDimension,
                                Year = t1.Year,
                                GIValue = t1.GIValue,
                                NN = t1.NN, //в зависимости от флага
                                IdSubjectBP = (sbpNull != null ? sbpNull.Id : null), //id sbp
                                CaptionSBP = t1.CaptionSBP,
                                PPO = t1.PPO,
                                IdElementTypeSystemGoal = t1.IdElementTypeSystemGoal
                            }).ToList();
                    pres = pres.Where(s => s.IdSubjectBP != null).ToList();
                }




                //зануляем родителей у элементов у которых родители отсутсвуют в списке 
                List<PrimeDataSet> sdf = (from p1 in pres
                                          join p2 in pres
                                          on p1.IdParent equals p2.Id into g
                                          from result in g.DefaultIfEmpty()
                                          select new PrimeDataSet
                                          {
                                              Id = p1.Id,
                                              IdParent = (result != null ? result.Id : null),
                                              IdSystemGoal = p1.IdSystemGoal,
                                              TypeGoal = p1.TypeGoal,
                                              CaptionGoal = p1.CaptionGoal,
                                              GoalCode = p1.GoalCode,//код цели
                                              SubjectBP = p1.SubjectBP, //входной параметр отчета
                                              DateStart = p1.DateStart,
                                              DateEnd = p1.DateEnd,
                                              CapGoalIndicator = p1.CapGoalIndicator,
                                              CapUnitDimension = p1.CapUnitDimension,
                                              Year = p1.Year,
                                              GIValue = p1.GIValue,
                                              NN = p1.NN, //в зависимости от флага
                                              IdSubjectBP = p1.IdSubjectBP, //id sbp
                                              CaptionSBP = p1.CaptionSBP,
                                              PPO = p1.PPO
                                          }).ToList();

                //обрезать цели со статусом архив
                var NotValidElem = NotValidSystemGoal();

                List<PrimeDataSet> TemporaryList = (from p1 in sdf
                                                    join p2 in NotValidElem on p1.IdSystemGoal equals p2.Id into g
                                                    from amount in g.DefaultIfEmpty()
                                                    select new PrimeDataSet
                                                    {
                                                        Id = (amount == null ? p1.Id : null),
                                                        IdParent = p1.IdParent,
                                                        IdSystemGoal = p1.IdSystemGoal,
                                                        TypeGoal = p1.TypeGoal,
                                                        CaptionGoal = p1.CaptionGoal,
                                                        GoalCode = p1.GoalCode,//код цели
                                                        SubjectBP = p1.SubjectBP, //входной параметр отчета
                                                        DateStart = p1.DateStart,
                                                        DateEnd = p1.DateEnd,
                                                        CapGoalIndicator = p1.CapGoalIndicator,
                                                        CapUnitDimension = p1.CapUnitDimension,
                                                        Year = p1.Year,
                                                        GIValue = p1.GIValue,
                                                        NN = p1.NN, //в зависимости от флага
                                                        IdSubjectBP = p1.IdSubjectBP, //id sbp
                                                        CaptionSBP = p1.CaptionSBP,
                                                        PPO = p1.PPO
                                                    }).ToList();
                TemporaryList = TemporaryList.Where(s => s.Id != null).ToList();
                //foreach (var t in TemporaryList)
                //    t.GoalCodeInt = Convert.ToInt32(t.GoalCode);

                //Строим иерархию
                if (DisplayReportCodeS == false)
                {
                    numHier(TemporaryList);
                    numHierView(TemporaryList);
                }
                else
                {
                    numHierCodes(TemporaryList);
                }

                //VolumeOfExpenses(TemporaryList);
                

                var query = "";
                var res = context.Database.SqlQuery<PrimeDataSet>("SELECT 1");
                if (DisplayResourceProvision == true)
                {
                    switch (DisplayResourceSupport)
                    {
                        case 0:
                            query = "EXECUTE [sbor].[RegistryGoal_VolumeExpensesImplementation]" + budgetyear.ToString() + "," + IdVersion.ToString() + "," + IdPublicLegalFormation.ToString() + "," + IdBudget.ToString() + ",  4 , NULL";
                            //@YearBudget int, @idVersion int, @PPO int, @IdBudget int, @idValueType int, @idValueType1 int
                            break;
                        case 2:
                            query = "EXECUTE [sbor].[RegistryGoal_VolumeExpensesImplementation]" + budgetyear.ToString() + "," + IdVersion.ToString() + "," + IdPublicLegalFormation.ToString() + "," + IdBudget.ToString() + ",  9 , NULL";
                            //query = "SELECT * FROM [sbor].[ProgramPassportSBP16_20_ApprovedData] (" + idProgram + "," + "NULL , NULL , NULL,  9 , NULL ," + budgetyear.ToString() + "," + idVersion.ToString() + "," + idPublicLegalFormation.ToString() + ",'" + DateReport.ToString() + "' )";
                            break;
                        case 3:
                            query = "EXECUTE [sbor].[RegistryGoal_VolumeExpensesImplementation]" + budgetyear.ToString() + "," + IdVersion.ToString() + "," + IdPublicLegalFormation.ToString() + "," + IdBudget.ToString() + ",   4 , 10";
                            //query = "SELECT * FROM [sbor].[ProgramPassport16_20_ApprovedData] (" + idProgram + "," + "NULL , NULL , NULL,  4 , 10 ," + budgetyear.ToString() + "," + idVersion.ToString() + "," + idPublicLegalFormation.ToString() + ",'" + DateReport.ToString() + "' )";
                            break;
                        case 4:
                            query = "EXECUTE [sbor].[RegistryGoal_VolumeExpensesImplementation]" + budgetyear.ToString() + "," + IdVersion.ToString() + "," + IdPublicLegalFormation.ToString() + "," + IdBudget.ToString() + ",   9 , 11 ";
                            //query = "SELECT * FROM [sbor].[ProgramPassportSBP16_20_ApprovedData] (" + idProgram + "," + "NULL , NULL , NULL,  9 , 11 ," + budgetyear.ToString() + "," + idVersion.ToString() + "," + idPublicLegalFormation.ToString() + ",'" + DateReport.ToString() + "' )";
                            break;
                    }

                    res = context.Database.SqlQuery<PrimeDataSet>(query);
                    TemporaryList = (
                               from p1 in TemporaryList
                               join p2 in res on p1.Id equals p2.IdSystemGoal into tmp2
                               from result in tmp2.DefaultIfEmpty()

                               select new PrimeDataSet
                               {
                                   Id = p1.Id,
                                   IdParent = p1.IdParent,
                                   TypeGoal = p1.TypeGoal,
                                   GoalCode = p1.GoalCode,
                                   CaptionGoal = p1.CaptionGoal,
                                   SubjectBP = p1.SubjectBP, //входной параметр отчета
                                   DateStart = p1.DateStart,
                                   DateEnd = p1.DateEnd,
                                   CapGoalIndicator = p1.CapGoalIndicator,
                                   CapUnitDimension = p1.CapUnitDimension,
                                   Year = p1.Year,
                                   GIValue = p1.GIValue,
                                   NN = p1.NN, //в зависимости от флага
                                   numb = p1.numb,
                                   VolumeOfExpenses = result.VolumeOfExpenses,
                                   VolumeOfExpenses1 = result.VolumeOfExpenses1,
                                   VolumeOfExpenses2 = result.VolumeOfExpenses2,
                                   IdSubjectBP = p1.IdSubjectBP, //id sbp
                                   CaptionSBP = p1.CaptionSBP,
                                   PPO = p1.PPO
                               }).ToList();

                }
                
                //var res = context.Database.SqlQuery<PrimeDataSet>("EXECUTE [dbo].[RegistryGoal_VolumeExpensesImplementation]" + budgetyear.ToString() + "," + IdVersion.ToString() + "," + IdPublicLegalFormation.ToString() + "," + IdBudget.ToString()).ToList();  

                List<PrimeDataSet> resume = (

                                            from aosge in context.AttributeOfSystemGoalElement.Where(
                                        t => t.IdVersion == Version
                                        && t.IdTerminator == null
                                        && t.IdPublicLegalFormation == IdPublicLegalFormation)
                                            join jsge in context.SystemGoalElement.Where(
                                                t => t.IdVersion == Version
                                                      && t.IdTerminator == null
                                                      && t.IdPublicLegalFormation == IdPublicLegalFormation
                                                      ) on aosge.IdSystemGoalElement equals jsge.Id
                                                into tmjsge
                                            from sge in tmjsge.DefaultIfEmpty()

                                            join jsg in
                                                context.SystemGoal.Where(s =>
                                                                          (
                                                                           (s.DateStart >= DtStart && s.DateStart <= DtEnd) ||
                                                                           (s.DateEnd >= DtStart && s.DateEnd <= DtEnd) ||
                                                                           (s.DateStart <= DtStart && s.DateEnd >= DtEnd)
                                                                          )
                                                                         &&
                                                                         (!isFilterTypeSG ||
                                                                          idtsg.Contains(s.IdElementTypeSystemGoal))
                                                                         &&
                                                                         s.IdPublicLegalFormation ==
                                                                         IdPublicLegalFormation
                                                ) on sge.IdSystemGoal equals jsg.Id into tmjsg
                                            from sg in tmjsg.DefaultIfEmpty()

                                            join plf in context.PublicLegalFormation on sg.IdPublicLegalFormation equals plf.Id

                                            join jetsg in context.ElementTypeSystemGoal on aosge.IdElementTypeSystemGoal equals jetsg.Id into tmjetsg
                                            from etsg in tmjetsg.DefaultIfEmpty()

                                            join jsbp in context.SBP on aosge.IdSBP equals jsbp.Id into tmjsbp
                                            from sbp in tmjsbp.DefaultIfEmpty()

                                            join jorg in context.Organization on sbp.IdOrganization equals jorg.Id into tmjorg
                                            from org in tmjorg.DefaultIfEmpty()

                                            join jgt in context.GoalTarget.Where(
                                                t => t.IdVersion == Version
                                                      && t.IdTerminator == null
                                                      && t.IdPublicLegalFormation == IdPublicLegalFormation
                                                       ) on sge.Id equals jgt.IdSystemGoalElement into tmjgt
                                            from gt in tmjgt.DefaultIfEmpty()

                                            join jgi in context.GoalIndicator on gt.IdGoalIndicator equals jgi.Id into tmjgi
                                            from gi in tmjgi.DefaultIfEmpty()

                                            join jud in context.UnitDimension on gi.IdUnitDimension equals jud.Id into tmjud
                                            from ud in tmjud.DefaultIfEmpty()

                                            join jvgt in context.ValuesGoalTarget.Where(p => p.IdVersion == Version
                                                                                              && p.IdTerminator == null
                                                                                              && p.IdPublicLegalFormation == IdPublicLegalFormation
                                                ) on gt.Id equals jvgt.IdGoalTarget into tmjvgt
                                            from vgt in tmjvgt.DefaultIfEmpty()

                                            join jhp in context.HierarchyPeriod on vgt.IdHierarchyPeriod equals jhp.Id into tmjhp //.Where(s => s.DateEnd >= DtStartBudget && s.DateEnd <= DtEndBudget)
                                            from hp in tmjhp.DefaultIfEmpty()


                                            select new PrimeDataSet()
                                            {
                                                Id = aosge.IdSystemGoalElement,
                                                IdParent = aosge.IdSystemGoalElement_Parent,
                                                TypeGoal = etsg.Caption,
                                                CaptionGoal = sg.Caption,
                                                GoalCode = sg.Code,
                                                SubjectBP = org.Caption, //входной параметр отчета
                                                DateStart = aosge.DateStart,
                                                DateEnd = aosge.DateEnd,
                                                CapGoalIndicator = gi.Caption,
                                                CapUnitDimension = ud.Caption,
                                                Year = hp.DateEnd.Year,
                                                GIValue = vgt.Value,
                                                NN = sg.Code, //в зависимости от флага
                                                IdSubjectBP = sbp.Id, //id sbp
                                                CaptionSBP = (SBP != null ? org.Description : ""), //наименование СБП
                                                PPO = plf.Caption
                                            }
                                      ).ToList();

                List<PrimeDataSet> summation = (from p1 in resume
                                                join p2 in TemporaryList on p1.Id equals p2.Id

                                                select new PrimeDataSet
                                                {
                                                    Id = p1.Id,
                                                    IdParent = p2.IdParent,
                                                    TypeGoal = p1.TypeGoal,
                                                    GoalCode = p1.GoalCode,
                                                    CaptionGoal = p1.CaptionGoal,
                                                    SubjectBP = p1.SubjectBP, //входной параметр отчета
                                                    DateStart = p1.DateStart,
                                                    DateEnd = p1.DateEnd,
                                                    CapGoalIndicator = p1.CapGoalIndicator,
                                                    CapUnitDimension = p1.CapUnitDimension,
                                                    Year = p1.Year,
                                                    GIValue = p1.GIValue,
                                                    NN = p2.NN, //в зависимости от флага
                                                    numb = p2.numb,
                                                    VolumeOfExpenses = p2.VolumeOfExpenses,
                                                    VolumeOfExpenses1 = p2.VolumeOfExpenses1,
                                                    VolumeOfExpenses2 = p2.VolumeOfExpenses2,
                                                    IdSubjectBP = p1.IdSubjectBP, //id sbp
                                                    CaptionSBP = p1.CaptionSBP,
                                                    PPO = p1.PPO
                                                }).ToList();




                //заполняем всеми годами и убираем пустые столбцы
                if (summation.Count() != 0)
                {
                    DateTime? minDate = summation.Select(s => s.DateStart).Min();
                    DateTime? maxDate = summation.Select(s => s.DateStart).Max();
                    PrimeDataSet pds = new PrimeDataSet();

                    pds = summation[0];
                    for (int i = minDate.Value.Year; i <= maxDate.Value.Year; i++)
                    {
                        pds.Year = i;
                        summation.Add(pds);
                    }

                    foreach (var t in summation)
                        if (t.Year == null)
                            t.Year = minDate.Value.Year;
                }

                foreach (var t in summation)
                    t.code = Convert.ToInt32(t.NN.Substring(0, t.NN.IndexOf('.')));
                
                #region olden
                //убираем из списка лишнее СБП
                //if (SBP != null)
                //{
                //    var SBPlist = SBPHierarchy();
                //    pres = (from t1 in pres
                //            join t2 in SBPlist on t1.IdSubjectBP equals t2.Id into r
                //            from sbpNull in r.DefaultIfEmpty()
                //            select new PrimeDataSet
                //            {
                //                Id = t1.Id,
                //                IdParent = t1.IdParent,
                //                TypeGoal = t1.TypeGoal,
                //                CaptionGoal = t1.CaptionGoal,
                //                SubjectBP = t1.SubjectBP, //входной параметр отчета
                //                DateStart = t1.DateStart,
                //                DateEnd = t1.DateEnd,
                //                CapGoalIndicator = t1.CapGoalIndicator,
                //                CapUnitDimension = t1.CapUnitDimension,
                //                GoalCode = t1.GoalCode,
                //                Year = t1.Year,
                //                GIValue = t1.GIValue,
                //                NN = t1.NN, //в зависимости от флага
                //                IdSubjectBP = (sbpNull != null ? sbpNull.Id : null), //id sbp
                //                CaptionSBP = t1.CaptionSBP,
                //                PPO = t1.PPO
                //            }).ToList();
                //    pres = pres.Where(s => s.IdSubjectBP != null).ToList();
                //}


                ////зануляем родителей у элементов у которых родители отсутсвуют в списке
                //List<PrimeDataSet> sdf = (from p1 in pres
                //          join p2 in pres on p1.IdParent equals p2.Id into g
                //          from result in g.DefaultIfEmpty()
                //          select new PrimeDataSet { Id = p1.Id,
                //                                    IdParent = (result != null ? result.Id : null) ,
                //                                    TypeGoal = p1.TypeGoal,
                //                                    CaptionGoal = p1.CaptionGoal,
                //                                    GoalCode = p1.GoalCode,
                //                                    SubjectBP = p1.SubjectBP, //входной параметр отчета
                //                                    DateStart = p1.DateStart,
                //                                    DateEnd = p1.DateEnd,
                //                                    CapGoalIndicator = p1.CapGoalIndicator,
                //                                    CapUnitDimension = p1.CapUnitDimension,
                //                                    Year = p1.Year,
                //                                    GIValue = p1.GIValue,
                //                                    NN = p1.NN, //в зависимости от флага
                //                                    IdSubjectBP = p1.IdSubjectBP, //id sbp
                //                                    CaptionSBP = p1.CaptionSBP, 
                //                                    PPO = p1.PPO
                //                                    }).ToList();

                ////обрезать цели со статусом архив

                //var NotValidElem = NotValidSystemGoal();
                //List<PrimeDataSet> resume = (from p1 in sdf
                //                             join p2 in NotValidElem on p1.Id equals p2.Id into g
                //                             from amount in g.DefaultIfEmpty()
                //                             select new PrimeDataSet
                //                             {
                //                                 Id = (amount == null ? p1.Id : null),
                //                                 IdParent = p1.IdParent,
                //                                 TypeGoal = p1.TypeGoal,
                //                                 CaptionGoal = p1.CaptionGoal,
                //                                 GoalCode = p1.GoalCode, 
                //                                 SubjectBP = p1.SubjectBP, //входной параметр отчета
                //                                 DateStart = p1.DateStart,
                //                                 DateEnd = p1.DateEnd,
                //                                 CapGoalIndicator = p1.CapGoalIndicator,
                //                                 CapUnitDimension = p1.CapUnitDimension,
                //                                 Year = p1.Year,
                //                                 GIValue = p1.GIValue,
                //                                 NN = p1.NN, //в зависимости от флага
                //                                 IdSubjectBP = p1.IdSubjectBP, //id sbp
                //                                 CaptionSBP = p1.CaptionSBP,
                //                                 PPO = p1.PPO
                //                             }).ToList();
                //resume = resume.Where(s => s.Id != null).ToList();

                ////resume = resume.OrderBy(s => s.CaptionGoal).ToList();

                ////Строим иерархию
                //if (DisplayReportCodeS == false)
                //{
                //    numHier(resume);
                //}
                //else
                //{
                //    numHierCodes(resume);
                //}
                ////заполняем всеми годами и убираем пустые столбцы
                //if (resume.Count() != 0)
                //{
                //DateTime? minDate = resume.Select(s => s.DateStart).Min();
                //DateTime? maxDate = resume.Select(s => s.DateStart).Max();
                //PrimeDataSet pds = new PrimeDataSet();

                //pds = resume[0];
                //    for (int i = minDate.Value.Year; i <= maxDate.Value.Year; i++)
                //    {
                //        pds.Year = i;
                //        resume.Add(pds);
                //    }
                //    foreach (var t in resume)
                //        if (t.Year == null)
                //            t.Year = minDate.Value.Year;
                //}


                ////if (DisplayReportCodeS == false)
                ////{
                //    foreach (var t in resume)
                //        t.code = Convert.ToInt32(t.NN.Substring(0, t.NN.IndexOf('.')));
                //    //}
                #endregion 

                summation = summation.Where(s => s.Year >= DtStartBudget.Year && s.Year <= DtEndBudget.Year).ToList();
                return summation.ToList();
            }
            else
            {

                List<PrimeDataSet> pres = (
                              from aosge in context.AttributeOfSystemGoalElement.Where(
                                  t => t.DateCommit <= DateReport &&
                                       (t.DateTerminate >= DateReport ||
                                        t.DateTerminate.Equals(null))
                                        && t.IdVersion == Version
                                        && t.IdTerminator == null)
                              join jsge in context.SystemGoalElement.Where(
                                  t => t.DateCommit <= DateReport
                                      && t.DateCommit != null
                                      && (t.DateTerminate >= DateReport ||
                                        t.DateTerminate == null)
                                        && t.IdVersion == Version
                                        && t.IdTerminator == null
                                        ) on aosge.IdSystemGoalElement equals jsge.Id
                                  into tmjsge
                              from sge in tmjsge.DefaultIfEmpty()

                              join jsg in
                                  context.SystemGoal.Where(s =>
                                                           (
                                                            (s.DateStart >= DtStart && s.DateStart <= DtEnd) ||
                                                            (s.DateEnd >= DtStart && s.DateEnd <= DtEnd) ||
                                                            (s.DateStart <= DtStart && s.DateEnd >= DtEnd)
                                                           )
                                                           &&
                                                           (!isFilterTypeSG ||
                                                            idtsg.Contains(s.IdElementTypeSystemGoal))
                                                           &&
                                                           s.IdPublicLegalFormation ==
                                                           IdPublicLegalFormation
                                  ) on sge.IdSystemGoal equals jsg.Id into tmjsg
                              from sg in tmjsg.DefaultIfEmpty()

                              join plf in context.PublicLegalFormation on sg.IdPublicLegalFormation
                                  equals plf.Id

                              join jetsg in context.ElementTypeSystemGoal on
                                  aosge.IdElementTypeSystemGoal
                                  equals jetsg.Id into tmjetsg
                              from etsg in tmjetsg.DefaultIfEmpty()

                              join jsbp in context.SBP on aosge.IdSBP equals jsbp.Id into tmjsbp
                              from sbp in tmjsbp.DefaultIfEmpty()

                              join jorg in context.Organization on sbp.IdOrganization equals jorg.Id
                                  into
                                  tmjorg
                              from org in tmjorg.DefaultIfEmpty()

                              join jgt in context.GoalTarget.Where(
                                  t => t.DateCommit <= DateReport &&
                                       (t.DateTerminate <= DateReport ||
                                        t.DateTerminate==null)
                                        && t.IdVersion == Version
                                        && t.IdTerminator == null
                                        ) on sge.Id equals
                                  jgt.IdSystemGoalElement into //reg
                                  tmjgt
                              from gt in tmjgt.DefaultIfEmpty()

                              join jgi in context.GoalIndicator on gt.IdGoalIndicator equals jgi.Id into
                                  tmjgi
                              from gi in tmjgi.DefaultIfEmpty()

                              join jud in context.UnitDimension on gi.IdUnitDimension equals jud.Id into
                                  tmjud
                              from ud in tmjud.DefaultIfEmpty()

                              join jvgt in context.ValuesGoalTarget.Where(p => p.IdValueType == 1 &&
                                                                               p.DateCommit <=
                                                                               DateReport &&
                                                                               (p.DateTerminate <=
                                                                                DateReport ||
                                                                                p.DateTerminate==null)
                                                                                && p.IdVersion == Version
                                                                                && p.IdTerminator == null
                                  ) on gt.Id //reg с типом "план"
                                  equals jvgt.IdGoalTarget into tmjvgt
                              from vgt in tmjvgt.DefaultIfEmpty()

                              join jhp in context.HierarchyPeriod on vgt.IdHierarchyPeriod equals jhp.Id
                                  into
                                  tmjhp
                              from hp in tmjhp.DefaultIfEmpty()

                              select new PrimeDataSet()
                              {
                                  Id = aosge.IdSystemGoalElement,
                                  IdParent = aosge.IdSystemGoalElement_Parent,
                                  TypeGoal = etsg.Caption,
                                  CaptionGoal = sg.Caption,
                                  GoalCode = sg.Code,
                                  SubjectBP = org.Caption, //входной параметр отчета
                                  DateStart = aosge.DateStart,
                                  DateEnd = aosge.DateEnd,
                                  CapGoalIndicator = gi.Caption,
                                  CapUnitDimension = ud.Caption,
                                  Year = hp.DateEnd.Year,
                                  GIValue = vgt.Value,
                                  NN = sg.Code, //в зависимости от флага
                                  IdSubjectBP = sbp.Id, //id sbp
                                  CaptionSBP = (SBP != null ? org.Description : ""), //наименование СБП
                                  PPO = plf.Caption
                              }

                #region SQL analog
                    //LEFT JOIN reg.SystemGoalElement AS SGE
                    //    ON Q.id = SGE.idSystemGoal
                    //LEFT JOIN ref.ElementTypeSystemGoal AS ETSG
                    //    ON SGE.IdElementTypeSystemGoal = ETSG.Id
                    //LEFT JOIN ref.SBP AS SBP 
                    //    ON SGE.idSBP = SBP.id 
                    //LEFT JOIN ref.Organization As Org
                    //    ON SBP.idOrganization = Org.id 
                    //LEFT JOIN reg.GoalTarget AS GT
                    //    ON SGE.id = GT.idSystemGoalElement
                    //LEFT JOIN ref.GoalIndicator AS GI
                    //    ON GT.idGoalIndicator = GI.id 
                    //LEFT JOIN ref.UnitDimension AS UD
                    //    ON GI.idUnitDimension = UD.id 
                    //LEFT JOIN reg.ValuesGoalTarget AS VGT
                    //    ON GT.id = VGT.idGoalTarget 
                    //LEFT JOIN enm.ValueType AS VT
                    //    ON VGT.idValueType = VT.id 
                #endregion
                    ).ToList();



                //убираем лишние СБП
                if (SBP != null)
                {
                    var SBPlist = SBPHierarchy();
                    pres = (from t1 in pres
                            join t2 in SBPlist on t1.IdSubjectBP equals t2.Id into r
                            from sbpNull in r.DefaultIfEmpty()
                            select new PrimeDataSet
                            {
                                Id = t1.Id,
                                IdParent = t1.IdParent,
                                TypeGoal = t1.TypeGoal,
                                CaptionGoal = t1.CaptionGoal,
                                GoalCode = t1.GoalCode,//код цели
                                SubjectBP = t1.SubjectBP, //входной параметр отчета
                                DateStart = t1.DateStart,
                                DateEnd = t1.DateEnd,
                                CapGoalIndicator = t1.CapGoalIndicator,
                                CapUnitDimension = t1.CapUnitDimension,
                                Year = t1.Year,
                                GIValue = t1.GIValue,
                                NN = t1.NN, //в зависимости от флага
                                IdSubjectBP = (sbpNull != null ? sbpNull.Id : null), //id sbp
                                CaptionSBP = t1.CaptionSBP,
                                PPO = t1.PPO,
                                IdElementTypeSystemGoal = t1.IdElementTypeSystemGoal
                            }).ToList();
                    pres = pres.Where(s => s.IdSubjectBP != null).ToList();
                }




                //зануляем родителей у элементов у которых родители отсутсвуют в списке 
                List<PrimeDataSet> sdf = (from p1 in pres
                                          join p2 in pres
                                          on p1.IdParent equals p2.Id into g
                                          from result in g.DefaultIfEmpty()
                                          select new PrimeDataSet
                                          {
                                              Id = p1.Id,
                                              IdParent = (result != null ? result.Id : null),
                                              TypeGoal = p1.TypeGoal,
                                              CaptionGoal = p1.CaptionGoal,
                                              GoalCode = p1.GoalCode,//код цели
                                              SubjectBP = p1.SubjectBP, //входной параметр отчета
                                              DateStart = p1.DateStart,
                                              DateEnd = p1.DateEnd,
                                              CapGoalIndicator = p1.CapGoalIndicator,
                                              CapUnitDimension = p1.CapUnitDimension,
                                              Year = p1.Year,
                                              GIValue = p1.GIValue,
                                              NN = p1.NN, //в зависимости от флага
                                              IdSubjectBP = p1.IdSubjectBP, //id sbp
                                              CaptionSBP = p1.CaptionSBP,
                                              PPO = p1.PPO
                                          }).ToList();

                //обрезать цели со статусом архив
                var NotValidElem = NotValidSystemGoal();

                List<PrimeDataSet> TemporaryList = (from p1 in sdf
                                                    join p2 in NotValidElem on p1.Id equals p2.Id into g
                                                    from amount in g.DefaultIfEmpty()
                                                    select new PrimeDataSet
                                                    {
                                                        Id = (amount == null ? p1.Id : null),
                                                        IdParent = p1.IdParent,
                                                        TypeGoal = p1.TypeGoal,
                                                        CaptionGoal = p1.CaptionGoal,
                                                        GoalCode = p1.GoalCode,//код цели
                                                        SubjectBP = p1.SubjectBP, //входной параметр отчета
                                                        DateStart = p1.DateStart,
                                                        DateEnd = p1.DateEnd,
                                                        CapGoalIndicator = p1.CapGoalIndicator,
                                                        CapUnitDimension = p1.CapUnitDimension,
                                                        Year = p1.Year,
                                                        GIValue = p1.GIValue,
                                                        NN = p1.NN, //в зависимости от флага
                                                        IdSubjectBP = p1.IdSubjectBP, //id sbp
                                                        CaptionSBP = p1.CaptionSBP,
                                                        PPO = p1.PPO
                                                    }).ToList();
                TemporaryList = TemporaryList.Where(s => s.Id != null).ToList();
                //foreach (var t in TemporaryList)
                //    t.GoalCodeInt = Convert.ToInt32(t.GoalCode);

                //Строим иерархию
                if (DisplayReportCodeS == false)
                {
                    numHier(TemporaryList);
                    numHierView(TemporaryList);
                }
                else
                {
                    numHierCodes(TemporaryList);
                }



                var query = "";
                var res = context.Database.SqlQuery<PrimeDataSet>("SELECT 1");
                if (DisplayResourceProvision == true)
                {
                    switch (DisplayResourceSupport)
                    {
                        case 0:
                            query = "EXECUTE [sbor].[RegistryGoal_VolumeExpensesImplementation_ApprovedData]" + budgetyear.ToString() + "," + IdVersion.ToString() + "," + IdPublicLegalFormation.ToString() + "," + IdBudget.ToString() + ",'" + DateReport.ToString() + "'" + ",  4 , NULL";
                            //@YearBudget int, @idVersion int, @PPO int, @IdBudget int, @idValueType int, @idValueType1 int
                            break;
                        case 2:
                            query = "EXECUTE [sbor].[RegistryGoal_VolumeExpensesImplementation_ApprovedData]" + budgetyear.ToString() + "," + IdVersion.ToString() + "," + IdPublicLegalFormation.ToString() + "," + IdBudget.ToString() + ",'" + DateReport.ToString() + "'" + ",  9 , NULL";
                            //query = "SELECT * FROM [sbor].[ProgramPassportSBP16_20_ApprovedData] (" + idProgram + "," + "NULL , NULL , NULL,  9 , NULL ," + budgetyear.ToString() + "," + idVersion.ToString() + "," + idPublicLegalFormation.ToString() + ",'" + DateReport.ToString() + "' )";
                            break;
                        case 3:
                            query = "EXECUTE [sbor].[RegistryGoal_VolumeExpensesImplementation_ApprovedData]" + budgetyear.ToString() + "," + IdVersion.ToString() + "," + IdPublicLegalFormation.ToString() + "," + IdBudget.ToString() + ",'" + DateReport.ToString() + "'" + ",   4 , 10";
                            //query = "SELECT * FROM [sbor].[ProgramPassport16_20_ApprovedData] (" + idProgram + "," + "NULL , NULL , NULL,  4 , 10 ," + budgetyear.ToString() + "," + idVersion.ToString() + "," + idPublicLegalFormation.ToString() + ",'" + DateReport.ToString() + "' )";
                            break;
                        case 4:
                            query = "EXECUTE [sbor].[RegistryGoal_VolumeExpensesImplementation_ApprovedData]" + budgetyear.ToString() + "," + IdVersion.ToString() + "," + IdPublicLegalFormation.ToString() + "," + IdBudget.ToString() + ",'" + DateReport.ToString() + "'" + ",   9 , 11 ";
                            //query = "SELECT * FROM [sbor].[ProgramPassportSBP16_20_ApprovedData] (" + idProgram + "," + "NULL , NULL , NULL,  9 , 11 ," + budgetyear.ToString() + "," + idVersion.ToString() + "," + idPublicLegalFormation.ToString() + ",'" + DateReport.ToString() + "' )";
                            break;
                    }

                    res = context.Database.SqlQuery<PrimeDataSet>(query);
                    TemporaryList = (
                               from p1 in TemporaryList
                               join p2 in res on p1.Id equals p2.IdSystemGoal into tmp2
                               from result in tmp2.DefaultIfEmpty()

                               select new PrimeDataSet
                               {
                                   Id = p1.Id,
                                   IdParent = p1.IdParent,
                                   TypeGoal = p1.TypeGoal,
                                   GoalCode = p1.GoalCode,
                                   CaptionGoal = p1.CaptionGoal,
                                   SubjectBP = p1.SubjectBP, //входной параметр отчета
                                   DateStart = p1.DateStart,
                                   DateEnd = p1.DateEnd,
                                   CapGoalIndicator = p1.CapGoalIndicator,
                                   CapUnitDimension = p1.CapUnitDimension,
                                   Year = p1.Year,
                                   GIValue = p1.GIValue,
                                   NN = p1.NN, //в зависимости от флага
                                   numb = p1.numb,
                                   VolumeOfExpenses = result.VolumeOfExpenses,
                                   VolumeOfExpenses1 = result.VolumeOfExpenses1,
                                   VolumeOfExpenses2 = result.VolumeOfExpenses2,
                                   IdSubjectBP = p1.IdSubjectBP, //id sbp
                                   CaptionSBP = p1.CaptionSBP,
                                   PPO = p1.PPO
                               }).ToList();

                }


                //var res = context.Database.SqlQuery<PrimeDataSet>("EXECUTE [dbo].[RegistryGoal_VolumeExpensesImplementation_ApprovedData]" + budgetyear.ToString() + "," + IdVersion.ToString() + "," + IdPublicLegalFormation.ToString() + "," + IdBudget.ToString() + ",'" + DateReport.ToString() + "'").ToList();
                //TemporaryList = (
                //           from p1 in TemporaryList
                //           join p2 in res on p1.Id equals p2.IdSystemGoal into tmp2
                //           from result in tmp2.DefaultIfEmpty()

                //           select new PrimeDataSet
                //           {
                //               Id = p1.Id,
                //               IdParent = p1.IdParent,
                //               TypeGoal = p1.TypeGoal,
                //               GoalCode = p1.GoalCode,
                //               CaptionGoal = p1.CaptionGoal,
                //               SubjectBP = p1.SubjectBP, //входной параметр отчета
                //               DateStart = p1.DateStart,
                //               DateEnd = p1.DateEnd,
                //               CapGoalIndicator = p1.CapGoalIndicator,
                //               CapUnitDimension = p1.CapUnitDimension,
                //               Year = p1.Year,
                //               GIValue = p1.GIValue,
                //               NN = p1.NN, //в зависимости от флага
                //               numb = p1.numb,
                //               VolumeOfExpenses = result.VolumeOfExpenses,
                //               VolumeOfExpenses1 = result.VolumeOfExpenses1,
                //               VolumeOfExpenses2 = result.VolumeOfExpenses2,
                //               IdSubjectBP = p1.IdSubjectBP, //id sbp
                //               CaptionSBP = p1.CaptionSBP,
                //               PPO = p1.PPO
                //           }).ToList();

                List<PrimeDataSet> resume = (
                                                from aosge in context.AttributeOfSystemGoalElement.Where(
                                            t => t.DateCommit <= DateReport &&
                                           (t.DateTerminate >= DateReport || t.DateTerminate.Equals(null))
                                            && t.IdVersion == Version
                                            && t.IdTerminator == null)
                                            join jsge in context.SystemGoalElement.Where(
                                                t => t.DateCommit <= DateReport
                                                    && t.DateCommit != null
                                                    && (t.DateTerminate >= DateReport || t.DateTerminate == null)
                                                      && t.IdVersion == Version
                                                      && t.IdTerminator == null
                                                      ) on aosge.IdSystemGoalElement equals jsge.Id
                                                into tmjsge
                                            from sge in tmjsge.DefaultIfEmpty()

                                            join jsg in
                                                context.SystemGoal.Where(s =>
                                                                         (
                                                                          (s.DateStart >= DtStart && s.DateStart <= DtEnd) ||
                                                                          (s.DateEnd >= DtStart && s.DateEnd <= DtEnd) ||
                                                                          (s.DateStart <= DtStart && s.DateEnd >= DtEnd)
                                                                         )
                                                                         &&
                                                                         (!isFilterTypeSG || idtsg.Contains(s.IdElementTypeSystemGoal))
                                                                         &&
                                                                         s.IdPublicLegalFormation == IdPublicLegalFormation
                                                ) on sge.IdSystemGoal equals jsg.Id into tmjsg
                                            from sg in tmjsg.DefaultIfEmpty()

                                            join plf in context.PublicLegalFormation on sg.IdPublicLegalFormation
                                                equals plf.Id

                                            join jetsg in context.ElementTypeSystemGoal on
                                                aosge.IdElementTypeSystemGoal
                                                equals jetsg.Id into tmjetsg
                                            from etsg in tmjetsg.DefaultIfEmpty()

                                            join jsbp in context.SBP on aosge.IdSBP equals jsbp.Id into tmjsbp
                                            from sbp in tmjsbp.DefaultIfEmpty()

                                            join jorg in context.Organization on sbp.IdOrganization equals jorg.Id
                                                into
                                                tmjorg
                                            from org in tmjorg.DefaultIfEmpty()

                                            join jgt in context.GoalTarget.Where(
                                                t => t.DateCommit <= DateReport &&
                                                     (t.DateTerminate <= DateReport || t.DateTerminate == null)
                                                      && t.IdVersion == Version
                                                      && t.IdTerminator == null
                                                      ) on sge.Id equals
                                                jgt.IdSystemGoalElement into //reg
                                                tmjgt
                                            from gt in tmjgt.DefaultIfEmpty()

                                            join jgi in context.GoalIndicator on gt.IdGoalIndicator equals jgi.Id into
                                                tmjgi
                                            from gi in tmjgi.DefaultIfEmpty()

                                            join jud in context.UnitDimension on gi.IdUnitDimension equals jud.Id into
                                                tmjud
                                            from ud in tmjud.DefaultIfEmpty()

                                            join jvgt in context.ValuesGoalTarget.Where(p => p.IdValueType == 1 &&
                                                                                             p.DateCommit <=
                                                                                             DateReport &&
                                                                                             (p.DateTerminate <=
                                                                                              DateReport || p.DateTerminate == null)
                                                                                              && p.IdVersion == Version
                                                                                              && p.IdTerminator == null
                                                ) on gt.Id //reg с типом "план"
                                                equals jvgt.IdGoalTarget into tmjvgt
                                            from vgt in tmjvgt.DefaultIfEmpty()

                                                join jhp in context.HierarchyPeriod on vgt.IdHierarchyPeriod equals jhp.Id//.Where(s => s.DateEnd >= DtStartBudget && s.DateEnd <= DtEndBudget)
                                                into tmjhp
                                            from hp in tmjhp.DefaultIfEmpty()


                                            select new PrimeDataSet()
                                            {
                                                Id = aosge.IdSystemGoalElement,
                                                IdParent = aosge.IdSystemGoalElement_Parent,
                                                TypeGoal = etsg.Caption,
                                                CaptionGoal = sg.Caption,
                                                GoalCode = sg.Code,
                                                SubjectBP = org.Caption, //входной параметр отчета
                                                DateStart = aosge.DateStart,
                                                DateEnd = aosge.DateEnd,
                                                CapGoalIndicator = gi.Caption,
                                                CapUnitDimension = ud.Caption,
                                                Year = hp.DateEnd.Year,
                                                GIValue = vgt.Value,
                                                NN = sg.Code, //в зависимости от флага
                                                IdSubjectBP = sbp.Id, //id sbp
                                                CaptionSBP = (SBP != null ? org.Description : ""), //наименование СБП
                                                PPO = plf.Caption
                                            }
                                      ).ToList();

                List<PrimeDataSet> summation = (from p1 in resume
                                                join p2 in TemporaryList on p1.Id equals p2.Id

                                                select new PrimeDataSet
                                                {
                                                    Id = p1.Id,
                                                    IdParent = p2.IdParent,
                                                    TypeGoal = p1.TypeGoal,
                                                    GoalCode = p1.GoalCode,
                                                    CaptionGoal = p1.CaptionGoal,
                                                    SubjectBP = p1.SubjectBP, //входной параметр отчета
                                                    DateStart = p1.DateStart,
                                                    DateEnd = p1.DateEnd,
                                                    CapGoalIndicator = p1.CapGoalIndicator,
                                                    CapUnitDimension = p1.CapUnitDimension,
                                                    Year = p1.Year,
                                                    GIValue = p1.GIValue,
                                                    NN = p2.NN, //в зависимости от флага
                                                    numb = p2.numb,
                                                    VolumeOfExpenses = p2.VolumeOfExpenses,
                                                    VolumeOfExpenses1 = p2.VolumeOfExpenses1,
                                                    VolumeOfExpenses2 = p2.VolumeOfExpenses2,
                                                    IdSubjectBP = p1.IdSubjectBP, //id sbp
                                                    CaptionSBP = p1.CaptionSBP,
                                                    PPO = p1.PPO
                                                }).ToList();




                //заполняем всеми годами и убираем пустые столбцы
                if (summation.Count() != 0)
                {
                    DateTime? minDate = summation.Select(s => s.DateStart).Min();
                    DateTime? maxDate = summation.Select(s => s.DateStart).Max();
                    PrimeDataSet pds = new PrimeDataSet();

                    pds = summation[0];
                    for (int i = minDate.Value.Year; i <= maxDate.Value.Year; i++)
                    {
                        pds.Year = i;
                        summation.Add(pds);
                    }

                    foreach (var t in summation)
                        if (t.Year == null)
                            t.Year = minDate.Value.Year;
                }

                foreach (var t in summation)
                    t.code = Convert.ToInt32(t.NN.Substring(0, t.NN.IndexOf('.')));

                summation = summation.Where(s => s.Year >= DtStartBudget.Year && s.Year <= DtEndBudget.Year).ToList();
                return summation.ToList();

                #region olden1

                ////убираем лишние СБП
                //if (SBP != null)
                //{
                //    var SBPlist = SBPHierarchy();
                //    pres = (from t1 in pres 
                //                join t2 in SBPlist on t1.IdSubjectBP equals t2.Id into r
                //                from sbpNull in r.DefaultIfEmpty()
                //                select new PrimeDataSet
                //                {
                //                              Id = t1.Id,
                //                              IdParent = t1.IdParent,
                //                              TypeGoal = t1.TypeGoal,
                //                              CaptionGoal = t1.CaptionGoal,
                //                              GoalCode = t1.GoalCode,
                //                              SubjectBP = t1.SubjectBP, //входной параметр отчета
                //                              DateStart = t1.DateStart,
                //                              DateEnd = t1.DateEnd,
                //                              CapGoalIndicator = t1.CapGoalIndicator,
                //                              CapUnitDimension = t1.CapUnitDimension,
                //                              Year = t1.Year,
                //                              GIValue = t1.GIValue,
                //                              NN = t1.NN, //в зависимости от флага
                //                              IdSubjectBP = (sbpNull != null ? sbpNull.Id : null), //id sbp
                //                              CaptionSBP = t1.CaptionSBP,
                //                              PPO = t1.PPO
                //                          }).ToList();
                //    pres = pres.Where(s => s.IdSubjectBP != null).ToList();
                //}




                ////зануляем родителей у элементов у которых родители отсутсвуют в списке 
                //List<PrimeDataSet> sdf = (from p1 in pres
                //                          join p2 in pres on p1.IdParent equals p2.Id into g
                //                          from result in g.DefaultIfEmpty()
                //                          select new PrimeDataSet
                //                          {
                //                              Id = p1.Id,
                //                              IdParent = (result != null ? result.Id : null),
                //                              TypeGoal = p1.TypeGoal,
                //                              CaptionGoal = p1.CaptionGoal,
                //                              GoalCode = p1.GoalCode,
                //                              SubjectBP = p1.SubjectBP, //входной параметр отчета
                //                              DateStart = p1.DateStart,
                //                              DateEnd = p1.DateEnd,
                //                              CapGoalIndicator = p1.CapGoalIndicator,
                //                              CapUnitDimension = p1.CapUnitDimension,
                //                              Year = p1.Year,
                //                              GIValue = p1.GIValue,
                //                              NN = p1.NN, //в зависимости от флага
                //                              IdSubjectBP = p1.IdSubjectBP, //id sbp
                //                              CaptionSBP = p1.CaptionSBP,
                //                              PPO = p1.PPO
                //                          }).ToList();

                ////обрезать цели со статусом архив

                //var NotValidElem = NotValidSystemGoal();
                //List<PrimeDataSet> resume = (from p1 in sdf
                //                             join p2 in NotValidElem on p1.Id equals p2.Id into g
                //                             from amount in g.DefaultIfEmpty()
                //                             select new PrimeDataSet
                //                             {
                //                                 Id = (amount == null ? p1.Id : null),
                //                                 IdParent = p1.IdParent,
                //                                 TypeGoal = p1.TypeGoal,
                //                                 CaptionGoal = p1.CaptionGoal,
                //                                 GoalCode = p1.GoalCode,
                //                                 SubjectBP = p1.SubjectBP, //входной параметр отчета
                //                                 DateStart = p1.DateStart,
                //                                 DateEnd = p1.DateEnd,
                //                                 CapGoalIndicator = p1.CapGoalIndicator,
                //                                 CapUnitDimension = p1.CapUnitDimension,
                //                                 Year = p1.Year,
                //                                 GIValue = p1.GIValue,
                //                                 NN = p1.NN, //в зависимости от флага
                //                                 IdSubjectBP = p1.IdSubjectBP, //id sbp
                //                                 CaptionSBP = p1.CaptionSBP,
                //                                 PPO = p1.PPO
                //                             }).ToList();
                //resume = resume.Where(s => s.Id != null).ToList();

                ////resume = resume.OrderBy(s => s.CaptionGoal).ToList();

                ////Строим иерархию
                //if (DisplayReportCodeS == false)
                //{
                //    numHier(resume);
                //}
                //else
                //{
                //    numHierCodes(resume);
                //}

                ////заполняем всеми годами и убираем пустые столбцы
                //if (resume.Count() != 0)
                //{
                //DateTime? minDate = resume.Select(s => s.DateStart).Min();
                //DateTime? maxDate = resume.Select(s => s.DateStart).Max();
                //PrimeDataSet pds = new PrimeDataSet();

                //    pds = resume[0];
                //    for (int i = minDate.Value.Year; i <= maxDate.Value.Year; i++)
                //    {
                //        pds.Year = i;
                //        resume.Add(pds);
                //    }

                //    foreach (var t in resume)
                //        if (t.Year == null)
                //            t.Year = minDate.Value.Year;
                //}

                ////if (DisplayReportCodeS == false)
                ////{
                //    foreach (var t in resume)
                //        t.code = Convert.ToInt32(t.NN.Substring(0, t.NN.IndexOf('.')));
                ////}

                //return resume.ToList();

                #endregion
            }
        }


        private List<PrimeDataSet> rootslist(List<PrimeDataSet> listpd) //нахожждение корневых элементов
        {
            var b =
                listpd.Select(s => s.Id).Except(listpd.Join(listpd, t1 => t1.Id, t2 => t2.IdParent,
                                                            (t1, t2) => new PrimeDataSet()
                                                                            {
                                                                                Id = t2.Id,
                                                                                CaptionGoal = t2.CaptionGoal
                                                                            }).Select(t => t.Id));
            var c = listpd.GroupBy(x => new {x.Id, x.IdParent}).Select(x => new
                                                                                PrimeDataSet
                {
                    Id = x.Key.Id,
                    IdParent = x.Key.IdParent
                }).Where(x => b.Contains(x.Id));//Equals(b, x.Id));
            return c.ToList();

        }

        private void hierarchy(List<PrimeDataSet> list, int? IdParent, string prefix = "")//построение иерархии
        {
            var data = list.Where(w => w.IdParent == IdParent);
            if (!data.Any()) return;

            var gdata =
                data.GroupBy(s => new { s.Id, s.IdParent, s.CaptionGoal  })
                    .Distinct()
                    .OrderBy(o => o.Key.CaptionGoal);

            int cnt = 1;
            foreach (var g in gdata)
            {
                var NN = prefix + cnt.ToString(CultureInfo.InvariantCulture) + ".";
                foreach (var d in g)
                {
                    d.NN = NN;
                }
                hierarchy(list, g.Key.Id, NN);
                cnt++;
            }
        }

        private void numHier(List<PrimeDataSet> list, int? IdParent = null, string prefix = "")
        {
            var data = list.Where(w => w.IdParent == IdParent);
            if (!data.Any()) return;

            var gdata =
                data.GroupBy(s => new { s.Id, s.IdParent, s.SortKeyHierarhy, s.CaptionGoal })
                    .Distinct()
                    .OrderBy(o => o.Key.CaptionGoal);

            int cnt = 1;
            foreach (var g in gdata)
            {
                var NN = prefix + cnt.ToString(CultureInfo.InvariantCulture) + ".";
                foreach (var d in g)
                {
                    d.NN = NN;
                    
                }
                numHier(list, g.Key.Id, NN);
                cnt++;
            }
        }

        private void numHierView(List<PrimeDataSet> list, int? IdParent = null, string prefix = "")
        {
            var data = list.Where(w => w.IdParent == IdParent);
            if (!data.Any()) return;

            var gdata =
                data.GroupBy(s => new { s.Id, s.IdParent, s.SortKeyHierarhy, s.CaptionGoal })
                    .Distinct()
                    .OrderBy(o => o.Key.CaptionGoal);

            int cnt = 1;
            foreach (var g in gdata)
            {
                var NN = prefix + cnt.ToString("D5", CultureInfo.InvariantCulture) + ".";
                foreach (var d in g)
                {
                    d.numb = NN;

                }
                numHierView(list, g.Key.Id, NN);
                cnt++;
            }
        }

        private void numHierCodes(List<PrimeDataSet> list, int? IdParent = null, string prefix = "")
        {
            var data = list.Where(w => w.IdParent == IdParent);
            if (!data.Any()) return;

            var gdata =
                //data.GroupBy(s => new { s.Id, s.IdParent, s.SortKeyHierarhy, s.GoalCodeInt })
                //    .Distinct()
                //    .OrderBy(o => o.Key.GoalCodeInt);
                data.GroupBy(s => new { s.Id, s.IdParent, s.SortKeyHierarhy, s.GoalCode })
                    .Distinct()
                    .OrderBy(o => o.Key.GoalCode);

            int cnt = 1;
            foreach (var g in gdata)
            {
                var NN = prefix + cnt.ToString(CultureInfo.InvariantCulture) + ".";
                foreach (var d in g)
                {
                    d.NN = NN;
                }
                numHierCodes(list, g.Key.Id, NN);
                cnt++;
            }
        }


        private List<PrimeDataSet> SBPHierarchy() //иерархия СБП елементов 
        {
            DataContext context;
            context = IoC.Resolve<DbContext>().Cast<DataContext>();
            var res = context.Database.SqlQuery<PrimeDataSet>("SELECT * FROM [sbor].[SBPHierarchy] (" + IdSBP.ToString() + ")").ToList();
            return res;
        }

        private List<PrimeDataSet> NotValidSystemGoal() //нахожждение корневых элементов 
        {
            DataContext context;
            context = IoC.Resolve<DbContext>().Cast<DataContext>();
            var res = context.Database.SqlQuery<PrimeDataSet>("SELECT * FROM [sbor].[NotValidSystemGoal] ()").ToList();
            return res;
        }

        private void VolumeOfExpenses(List<PrimeDataSet> list)
        {
            DataContext context;
            context = IoC.Resolve<DbContext>().Cast<DataContext>();
            var budgetyear =
                    context.Budget.Where(s => s.Id == IdBudget)
                           .Select(d => d.Year)
                           .FirstOrDefault();

            var res = context.Database.SqlQuery<PrimeDataSet>("EXECUTE [dbo].[RegistryGoal_VolumeExpensesImplementation]" + budgetyear.ToString() + "," + IdVersion.ToString() + "," + IdPublicLegalFormation.ToString() + "," + IdBudget.ToString() ).ToList();
            list = (
                       from p1 in list
                       join p2 in res on p1.Id equals p2.Id into tmp2
                       from result in tmp2.DefaultIfEmpty()
                       
                                                select new PrimeDataSet
                                                {
                                                    Id = p1.Id,
                                                    IdParent = p1.IdParent,
                                                    TypeGoal = p1.TypeGoal,
                                                    GoalCode = p1.GoalCode,
                                                    CaptionGoal = p1.CaptionGoal,
                                                    SubjectBP = p1.SubjectBP, //входной параметр отчета
                                                    DateStart = p1.DateStart,
                                                    DateEnd = p1.DateEnd,
                                                    CapGoalIndicator = p1.CapGoalIndicator,
                                                    CapUnitDimension = p1.CapUnitDimension,
                                                    Year = p1.Year,
                                                    GIValue = p1.GIValue,
                                                    NN = p1.NN, //в зависимости от флага
                                                    numb = p1.numb,
                                                    VolumeOfExpenses  = result.VolumeOfExpenses,
                                                    VolumeOfExpenses1 = result.VolumeOfExpenses1,
                                                    VolumeOfExpenses2 = result.VolumeOfExpenses2,
                                                    IdSubjectBP = p1.IdSubjectBP, //id sbp
                                                    CaptionSBP = p1.CaptionSBP,
                                                    PPO = p1.PPO
                                                }).ToList();
        }
    }


}
