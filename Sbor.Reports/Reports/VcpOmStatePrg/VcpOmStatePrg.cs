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
using Sbor.Reports.VcpOmStatePrg;
using Sbor.Reports.VcpOmStatePrg;

namespace Sbor.Reports.Report
{
    [Report]
    public class VcpOmStatePrg
    {
        public int idPublicLegalFormation { get; set; } //ППО
        public int idProgram { get; set; } //Государственная программа
        public int idVersion { get; set; } //Версия 
        public bool ConstructReportApprovedData { get; set; } //Строить отчет по утвержденным данным
        public DateTime DateReport { get; set; } //Дата отчета
        public bool RepeatTableHeader { get; set; } //Повторять заголовки  таблиц на каждой странице


        #region gvn
        //public List<DataSetVOSP> VcpOmStateProgram() //данные из регистра
        //{
        //    DateTime DtReport = DateReport;
        //    if (ConstructReportApprovedData == false)
        //    {
        //        DtReport = new DateTime(2099, 1, 1, 0, 00, 00);
        //    }

        //    DataContext context = IoC.Resolve<DbContext>().Cast<DataContext>();
        //    List<DataSetVOSP> pres = (
        //                                  //from sbp in context.SBP 
        //                                  //  join sbp1 in context.SBP on sbp.Id equals sbp1.IdParent

        //                                  from prg in
        //                                      context.Program.Where(s => s.Id == id
        //                                          && s.IdPublicLegalFormation == idPublicLegalFormation
        //                                          && s.IdVersion == idVersion) // Гос прог

        //                                  join tjsa in context.AttributeOfProgram on prg.Id equals tjsa.IdParent // подпрограммы 
        //                                  #region dt
        //                                  //join jaop in context.AttributeOfProgram on tjsa.IdProgram equals jaop.IdParent into tmjaop // ВЦП и ОМ
        //                                  //from aop in tmjaop.DefaultIfEmpty()

        //                                  //join vosp in context.Program on aop.IdProgram equals vosp.Id //наименование подпрограммы  

        //                                  //join tsp in context.DocType on vosp.IdDocType equals tsp.Id //тип подпрограммы

        //                                  //join jsge in context.SystemGoalElement on aop.IdGoalSystemElement equals jsge.Id into tmjsge // регистр Элемент сц
        //                                  //from sge in tmjsge.DefaultIfEmpty()

        //                                  //join jsg in context.SystemGoal on sge.IdSystemGoal equals jsg.Id into tmjsg //справочник система целеполагания
        //                                  //from sg in tmjsg.DefaultIfEmpty()

        //                                  //join jgt in context.GoalTarget on sge.Id equals jgt.IdSystemGoalElement into tmjgt // целевые показатели
        //                                  //from gt in tmjgt.DefaultIfEmpty()

        //                                  //join jgi in context.GoalIndicator on gt.IdGoalIndicator equals jgi.Id into //наименование показателя
        //                                  //    tmjgi
        //                                  //from gi in tmjgi.DefaultIfEmpty()

        //                                  //join jvgt in context.ValuesGoalTarget.Where(s => s.IdValueType == 1) on gi.Id equals jvgt.IdGoalTarget into tmjvgt // значение целевых показателей
        //                                  //from vgt in tmjvgt.DefaultIfEmpty()

        //                                  //join jhp in context.HierarchyPeriod on vgt.IdHierarchyPeriod equals jhp.Id into tmjhp //период 
        //                                  //from hp in tmjhp.DefaultIfEmpty()

        //                                  //join jsbp in context.SBP on prg.IdSBP equals jsbp.Id into tmjsbp //
        //                                  //from sbp in tmjsbp.DefaultIfEmpty()

        //                                  //join jorg in context.Organization on sbp.IdOrganization equals jorg.Id into //Отвественный исполнитель
        //                                  //    tmjorg
        //                                  //from org in tmjorg.DefaultIfEmpty()

        //                                  //join jud in context.UnitDimension on gi.IdUnitDimension equals jud.Id into //еденица измерения
        //                                  //    tmjud
        //                                  //from ud in tmjud.DefaultIfEmpty()
        //                                  #endregion 

        //                                  select new DataSetVOSP()
        //                                  {
                                              
        //                                        Id = tjsa.IdProgram , //индетификатор подпрограммы
                                                
        //                                        //Type = tsp.Caption//Тип (ВЦП и ОМ)
        //                                        //Numberssp = , //номер подпрограммы
        //                                        //Numberinprog //номер в подпрограмме 
        //                                        //Numberintypessp //номер в типе подпрограммы
        //                                        Substatep = tjsa.Caption, //Наименование подпрограммы ГП
        //                                        Statep = prg.Caption //Наименование ГП
        //                                        //Captionvcpom //Наименование ВЦП и ОМ
        //                                        //Executive //ответственный исполнитель
        //                                        //Datestartsub //дата начала Подпрограммы ГП   
        //                                        //DAteendsub //дата окончания Подпрограммы ГП
        //                                        //Goalindicator //наименование целевых показателей основной цели 
        //                                        //Unitd //единица измерения целевого показателя
        //                                        //Value //значение целевого показателя с самой поздней датой из всех значений с типом "План" 
        //                                        //Datestart //дата начала ГП   
        //                                        //public DateTime? DAteend //дата окончания ГП
        //                                  }
        //                              ).ToList();


        //    return pres;
        //}
        #endregion


        public IEnumerable<DataSetVOSP> VOSP() //данные из регистра
        {
            DataContext context;
            context = IoC.Resolve<DbContext>().Cast<DataContext>();
            switch (ConstructReportApprovedData)
            {
                case true:
                    var res = context.Database.SqlQuery<DataSetVOSP>("SELECT * FROM [sbor].[VCPOMProg1_23_ApprovedData] (" + idProgram.ToString() + ",'" + DateReport.ToString() + "'," + idVersion.ToString() + "," + idPublicLegalFormation.ToString() + ")").ToList();
                    res.ForEach(f => f.Goalindicator = f.Goalindicator + " - " + (f.Value.HasValue ? f.Value.Value.ToString("0.#####") : ""));
                    return res;
                default:
                    var res1 = context.Database.SqlQuery<DataSetVOSP>("SELECT * FROM [sbor].[VCPOMProg1_23] (" + idProgram.ToString() + ",'" + DateReport.ToString() + "'," + idVersion.ToString() +","+idPublicLegalFormation.ToString()+ ")").ToList();
                    res1.ForEach(f => f.Goalindicator = f.Goalindicator + (f.Value.HasValue ? " - " + f.Value.Value.ToString("0.#####") : ""));
                    return res1;//return context.Database.SqlQuery<DataSetVOSP>("SELECT * FROM [sbor].[VCPOMProg1_23] (" + idProgram.ToString() + ",'" + DateReport.ToString() + "')");
            }
            
        }

        public IEnumerable<DataSetVOSP> Executer()
        {
            DataContext context;
            context = IoC.Resolve<DbContext>().Cast<DataContext>();
            switch (ConstructReportApprovedData)
            {
                case true:
                    var pres = (

                                   from prg in
                                       context.Program.Where(s => s.Id == idProgram
                                                                  && s.IdTerminator == null
                                                                  && s.IdPublicLegalFormation == idPublicLegalFormation
                                                                  && s.DateCommit <= DateReport
                                                                  && s.DateCommit != null
                                                                  && s.IdVersion == idVersion)
                                   join sbp in context.SBP on prg.IdSBP equals sbp.Id
                                   join org in context.Organization on sbp.IdOrganization equals org.Id
                                   select new DataSetVOSP()
                                       {
                                           Executive = org.Caption
                                       });

                    return pres;
                default:
                    var pres1 = (

                                    from prg in
                                        context.Program.Where(s => s.Id == idProgram
                                                                   && s.IdTerminator == null
                                                                   && s.IdPublicLegalFormation == idPublicLegalFormation
                                                                   && s.IdVersion == idVersion)
                                    join sbp in context.SBP on prg.IdSBP equals sbp.Id
                                    join org in context.Organization on sbp.IdOrganization equals org.Id
                                    select new DataSetVOSP()
                                        {
                                            Executive = org.Caption
                                        });

                    return pres1;
            }
        }

    }
}
