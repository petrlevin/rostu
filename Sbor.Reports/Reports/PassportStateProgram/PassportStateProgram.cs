using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Practices.ObjectBuilder2;
using Platform.BusinessLogic;
using Platform.BusinessLogic.ReportingServices.Reports;
using Platform.Common;
using System.Globalization;
using Sbor.Reports.Reports.PassportStateProgram; 

namespace Sbor.Reports.Reports.PassportStateProgram
{
    [Report]
    public class PassportStateProgram
    {
        public int idPublicLegalFormation { get; set; } //ППО
        public int idBudget { get; set; } //Бюджет
        public int idVersion { get; set; } //Версия
        public string Caption { get; set; } //Наименование
        public int idProgram { get; set; } //Государственная программа
        public DateTime DateReport { get; set; } //Дата отчета
        public bool buildReportApprovedData { get; set; } //Строить отчет по утвержденным данным
        public bool RepeatTableHeader { get; set; } //Повторять заголовки  таблиц на каждой странице
        public int idSourcesDataReports { get; set; } //Источник данных для  ресурсного обеспечения 

        //-1543503843	-1543503815	Государственная программа
        //-1543503842	-1543503815	Подпрограмма ГП
        //-1543503841	-1543503797	Ведомственная целевая программа
        //-1543503837	-1543503791	Долгосрочная целевая программа

        public IEnumerable<DataSetStateProgPass> PassportData() //данные из регистра
        {
            //idProgram = 817;
            if (buildReportApprovedData == false)
            {
                DataContext context = IoC.Resolve<DbContext>().Cast<DataContext>();
                IEnumerable<DataSetStateProgPass> pres = (

                                                             from prg in
                                                                 context.Program.Where(s => s.Id == idProgram
                                                                                            && s.IdTerminator == null
                                                                 && s.IdPublicLegalFormation == idPublicLegalFormation
                                                                 && s.IdVersion == idVersion
                                                                 )
                                                             join plf in context.PublicLegalFormation on
                                                                 prg.IdPublicLegalFormation equals plf.Id
                                                             //join jprogview in programview on prg.IdDocType equals jprogview.Key into tmprogview
                                                             //from progview in tmprogview.DefaultIfEmpty()
                                                             join jaop in
                                                                 context.AttributeOfProgram.Where(
                                                                     s => s.IdTerminator == null
                                                                     && s.IdPublicLegalFormation == idPublicLegalFormation
                                                                     && s.IdVersion == idVersion
                                                                     ) on prg.Id equals
                                                                 jaop.IdProgram into tmjaop
                                                             from aop in tmjaop.DefaultIfEmpty()
                                                             //получаем вышестояющую если есть  
                                                             join jvsprg in
                                                                 context.Program.Where(s => s.IdTerminator == null
                                                                 && s.IdPublicLegalFormation == idPublicLegalFormation
                                                                 && s.IdVersion == idVersion
                                                                 ) on
                                                                 aop.IdParent equals jvsprg.Id into tmjvsprg
                                                             from vsprg in tmjvsprg.DefaultIfEmpty()
                                                             //получаем наименование вышестояющей программы если есть 
                                                             join sbp in context.SBP on prg.IdSBP equals sbp.Id
                                                             join org in context.Organization on sbp.IdOrganization
                                                                 equals org.Id
                                                             join sge in context.SystemGoalElement on
                                                                 aop.IdGoalSystemElement equals sge.Id
                                                             join sg in context.SystemGoal on sge.IdSystemGoal equals
                                                                 sg.Id

                                                             select new DataSetStateProgPass()
                                                                 {

                                                                     Ppo = plf.Caption, //ППО
                                                                     Viewprog =
                                                                         ((prg.IdDocType == -1543503843)
                                                                              ? "ГОСУДАРСТВЕННОЙ ПРОГРАММЫ"
                                                                              : ((prg.IdDocType == -1543503841)
                                                                                     ? "ВЕДОМСТВЕННОЙ ЦЕЛЕВОЙ ПРОГРАММЫ"
                                                                                     : ((prg.IdDocType == -1543503837)
                                                                                            ? "ДОЛГОСРОЧНОЙ ЦЕЛЕВОЙ ПРОГРАММЫ"
                                                                                            : ((prg.IdDocType ==
                                                                                                -1543503842)
                                                                                                   ? "ПОДПРОГРАММЫ ГОСУДАРСТВЕННОЙ ПРОГРАММЫ"
                                                                                                   : "")
                                                                                       )
                                                                                )
                                                                         ), //Вид программы
                                                                     AdditionalViewProg =
                                                                          ((prg.IdDocType == -1543503843 || prg.IdDocType == -1543503842)
                                                                               ? "ГОСУДАРСТВЕННОЙ ПРОГРАММЫ"
                                                                               : ((prg.IdDocType == -1543503841)
                                                                                      ? "ВЕДОМСТВЕННОЙ ЦЕЛЕВОЙ ПРОГРАММЫ"
                                                                                      : ((prg.IdDocType == -1543503837)
                                                                                             ? "ДОЛГОСРОЧНОЙ ЦЕЛЕВОЙ ПРОГРАММЫ"
                                                                                             : ""
                                                                                        )
                                                                                 )
                                                                          ), //дополнительный Вид программы

                                                                     Caption = Caption,
                                                                     Typeprog =
                                                                         ((prg.IdDocType == -1543503843 ||
                                                                           prg.IdDocType == -1543503841 ||
                                                                           prg.IdDocType == -1543503837)
                                                                              ? "программы"
                                                                              : "подпрограммы"), //Тип программы
                                                                     Captionprog =
                                                                         ((prg.IdDocType == -1543503843 ||
                                                                           prg.IdDocType == -1543503841 ||
                                                                           prg.IdDocType == -1543503837)
                                                                              ? prg.Caption
                                                                              : vsprg.Caption), //Наименование программы
                                                                     Subprog = ((prg.IdDocType == -1543503842)
                                                                              ? prg.Caption
                                                                              : null), //Наименование подпрограммы 
                                                                     Executer = org.Description,
                                                                     //ответственный исполнитель
                                                                     Goal = sg.Caption,
                                                                     //Цель = из атрибутов программы основная цель
                                                                     Datestart = aop.DateStart, //дата начала ГП   
                                                                     Dateend = aop.DateEnd, //дата окончания ГП


                                                                 }); // Гос прог
                return pres;
            }
            else
            {
                DataContext context = IoC.Resolve<DbContext>().Cast<DataContext>();
                var pres1 = (

                                                             from prg in
                                                                 context.Program.Where(s => s.Id == idProgram
                                                                                            && s.IdTerminator == null
                                                                                            && s.DateCommit <= DateReport
                                                                                            && s.DateCommit != null
                                                                                            && s.IdPublicLegalFormation == idPublicLegalFormation
                                                                                            && s.IdVersion == idVersion
                                                                     //s.IdPublicLegalFormation == idPublicLegalFormation
                                                                     //&& s.IdVersion == idVersion
                                                                 )
                                                             join plf in context.PublicLegalFormation on
                                                                 prg.IdPublicLegalFormation equals plf.Id
                                                             //join jprogview in programview on prg.IdDocType equals jprogview.Key into tmprogview
                                                             //from progview in tmprogview.DefaultIfEmpty()
                                                             join jaop in
                                                                 context.AttributeOfProgram.Where(
                                                                     s => s.IdTerminator == null
                                                                     && s.IdPublicLegalFormation == idPublicLegalFormation
                                                                     && s.IdVersion == idVersion
                                                                     ) on prg.Id equals
                                                                 jaop.IdProgram into tmjaop
                                                             from aop in tmjaop.DefaultIfEmpty()
                                                             //получаем вышестояющую если есть  
                                                             join jvsprg in
                                                                 context.Program.Where(s => s.IdTerminator == null
                                                                                         && s.IdPublicLegalFormation == idPublicLegalFormation
                                                                                         && s.IdVersion == idVersion
                                                                 ) on
                                                                 aop.IdParent equals jvsprg.Id into tmjvsprg
                                                             from vsprg in tmjvsprg.DefaultIfEmpty()
                                                             //получаем наименование вышестояющей программы если есть 
                                                             join sbp in context.SBP on prg.IdSBP equals sbp.Id
                                                             join org in context.Organization on sbp.IdOrganization
                                                                 equals org.Id
                                                             join sge in context.SystemGoalElement on
                                                                 aop.IdGoalSystemElement equals sge.Id
                                                             join sg in context.SystemGoal on sge.IdSystemGoal equals
                                                                 sg.Id

                                                             select new DataSetStateProgPass()
                                                             {

                                                                 Ppo = plf.Caption, //ППО
                                                                 Viewprog =
                                                                     ((prg.IdDocType == -1543503843)
                                                                          ? "ГОСУДАРСТВЕННОЙ ПРОГРАММЫ"
                                                                          : ((prg.IdDocType == -1543503841)
                                                                                 ? "ВЕДОМСТВЕННОЙ ЦЕЛЕВОЙ ПРОГРАММЫ"
                                                                                 : ((prg.IdDocType == -1543503837)
                                                                                        ? "ДОЛГОСРОЧНОЙ ЦЕЛЕВОЙ ПРОГРАММЫ"
                                                                                        : ((prg.IdDocType ==
                                                                                            -1543503842)
                                                                                               ? "ПОДПРОГРАММЫ ГОСУДАРСТВЕННОЙ ПРОГРАММЫ"
                                                                                               : "")
                                                                                   )
                                                                            )
                                                                     ), //Вид программы
                                                                 AdditionalViewProg =
                                                                           ((prg.IdDocType == -1543503843 || prg.IdDocType == -1543503842)
                                                                                ? "ГОСУДАРСТВЕННОЙ ПРОГРАММЫ"
                                                                                : ((prg.IdDocType == -1543503841)
                                                                                       ? "ВЕДОМСТВЕННОЙ ЦЕЛЕВОЙ ПРОГРАММЫ"
                                                                                       : ((prg.IdDocType == -1543503837)
                                                                                              ? "ДОЛГОСРОЧНОЙ ЦЕЛЕВОЙ ПРОГРАММЫ"
                                                                                              : ""
                                                                                         )
                                                                                  )
                                                                           ), //дополнительный Вид программы

                                                                 Typeprog =
                                                                     ((prg.IdDocType == -1543503843 ||
                                                                       prg.IdDocType == -1543503841 ||
                                                                       prg.IdDocType == -1543503837)
                                                                          ? "программы"
                                                                          : "подпрограммы"), //Тип программы
                                                                 Captionprog =
                                                                     ((prg.IdDocType == -1543503843 ||
                                                                       prg.IdDocType == -1543503841 ||
                                                                       prg.IdDocType == -1543503837)
                                                                          ? prg.Caption
                                                                          : vsprg.Caption), //Наименование программы
                                                                 Caption = Caption,
                                                                 Subprog = ((prg.IdDocType == -1543503842)
                                                                              ? prg.Caption
                                                                              : null), //Наименование подпрограммы
                                                                 Executer = org.Description,
                                                                 //ответственный исполнитель
                                                                 Goal = sg.Caption,
                                                                 //Цель = из атрибутов программы основная цель
                                                                 Datestart = aop.DateStart, //дата начала ГП   
                                                                 Dateend = aop.DateEnd, //дата окончания ГП


                                                             }); // Гос прог
                if (!pres1.Any())
                {
                    pres1 = (from prg in context.Program.Where(s => s.Id == idProgram)
                             select new DataSetStateProgPass()
                                 {
                                     Ppo = null, //ППО
                                     AdditionalViewProg =
                                                                           ((prg.IdDocType == -1543503843 || prg.IdDocType == -1543503842)
                                                                                ? "ГОСУДАРСТВЕННОЙ ПРОГРАММЫ"
                                                                                : ((prg.IdDocType == -1543503841)
                                                                                       ? "ВЕДОМСТВЕННОЙ ЦЕЛЕВОЙ ПРОГРАММЫ"
                                                                                       : ((prg.IdDocType == -1543503837)
                                                                                              ? "ДОЛГОСРОЧНОЙ ЦЕЛЕВОЙ ПРОГРАММЫ"
                                                                                              : ""
                                                                                         )
                                                                                  )
                                                                           )
                                 });
                }

                return pres1;
            }
        }


        public IEnumerable<DataSetStateProgPass> SubProgram() //подпрограмма
        {
            if (buildReportApprovedData == false)
            {
                DataContext context = IoC.Resolve<DbContext>().Cast<DataContext>();
                IEnumerable<DataSetStateProgPass> pres = (

                                                             from prg in
                                                                 context.Program.Where(s => s.Id == idProgram
                                                                                            && s.IdTerminator == null
                                                                                            && s.IdDocType == -1543503842
                                                                     && s.IdPublicLegalFormation == idPublicLegalFormation
                                                                     && s.IdVersion == idVersion
                                                                 )
                                                             select new DataSetStateProgPass()
                                                                 {
                                                                     Subprog = prg.Caption
                                                                     //Наименование подпрограммы Наименование подпрограммы
                                                                 });
                return pres;
            }
            else
            {
                 DataContext context = IoC.Resolve<DbContext>().Cast<DataContext>();
                var pres1 = (

                                                             from prg in
                                                                 context.Program.Where(s => s.Id == idProgram
                                                                                            && s.IdDocType == -1543503842
                                                                                            && s.IdTerminator == null
                                                                                            && s.DateCommit <= DateReport
                                                                                            && s.DateCommit != null
                                                                     && s.IdPublicLegalFormation == idPublicLegalFormation
                                                                     && s.IdVersion == idVersion
                                                                 )
                                                             select new DataSetStateProgPass()
                                                             {
                                                                 Subprog = prg.Caption
                                                                 //Наименование подпрограммы Наименование подпрограммы
                                                             });
                return pres1;
            }
        }


        public IEnumerable<DataSetStateProgPass> CoexecuterProgram() //соисполнители
        {
            if (buildReportApprovedData == false)
            {
                DataContext context = IoC.Resolve<DbContext>().Cast<DataContext>();
                IEnumerable<DataSetStateProgPass> pres = (from aop in
                                                              context.AttributeOfProgram.Where(
                                                                  s => s.IdParent == idProgram && s.IdTerminator == null
                                                                  && s.IdPublicLegalFormation == idPublicLegalFormation
                                                                  && s.IdVersion == idVersion
                                                                  )
                                                          //получаем всех детей 
                                                          join prg in context.Program.Where(s => s.IdTerminator == null
                                                                    && s.IdPublicLegalFormation == idPublicLegalFormation
                                                                    && s.IdVersion == idVersion
                                                                    && (s.IdDocType == -1543503842 || s.IdDocType == -1543503842 || s.IdDocType == -1543503835)
                                                          )
                                                              on aop.IdProgram equals prg.Id
                                                          //получаем наименование вышестояющей программы если есть и тип «Подпрограмма ГП» или «Долгосрочная целевая программа» или «Подпрограмма ДЦП»
                                                          join sbp in context.SBP on prg.IdSBP equals sbp.Id
                                                          join org in context.Organization on sbp.IdOrganization equals
                                                              org.Id
                                                          join osnprg in context.Program on aop.IdParent equals
                                                              osnprg.Id
                                                          //Вид основной рпограммы
                                                          //join progview in programview on prg.IdDocType equals progview.Key //Вид основной рпограммы
                                                          select new DataSetStateProgPass()
                                                              {
                                                                  Coexecuter = org.Description, //Соисполнители
                                                                  Viewprog =
                                                                      ((osnprg.IdDocType == -1543503843)
                                                                           ? "государственной программы"
                                                                           : ((osnprg.IdDocType == -1543503841)
                                                                                  ? "ведомственной целевой программы"
                                                                                  : ((osnprg.IdDocType == -1543503837)
                                                                                         ? "долгосрочной целевой программы"
                                                                                         : ((osnprg.IdDocType == -1543503842)
                                                                                                ? "ПОДПРОГРАММЫ"
                                                                                                : "")
                                                                                    )
                                                                             )
                                                                      ) //Вид программы
                                                              });

                if (context.Program.Any(s => s.Id == idProgram && s.IdDocType == -1543503843 || s.IdDocType == -1543503837))
                    if (!pres.Any())
                        pres = ViewProgramType().ToList();

                return pres;
            }
            else
            {
                DataContext context = IoC.Resolve<DbContext>().Cast<DataContext>();
                var pres1 = (from aop in
                                 context.AttributeOfProgram.Where(
                                     s => s.IdParent == idProgram && s.IdTerminator == null
                                     && s.DateCommit <= DateReport
                                     && s.DateCommit != null
                                     && s.IdPublicLegalFormation == idPublicLegalFormation
                                     && s.IdVersion == idVersion
                                     )
                             //получаем всех детей 
                             join prg in context.Program.Where(s => s.IdTerminator == null
                                                                 && s.IdPublicLegalFormation == idPublicLegalFormation
                                                                 && s.IdVersion == idVersion
                                                                 && (s.IdDocType == -1543503842 || s.IdDocType == -1543503842 || s.IdDocType == -1543503835)
                             )
                                 on aop.IdProgram equals prg.Id
                             //получаем наименование вышестояющей программы если есть и тип «Подпрограмма ГП» или «Долгосрочная целевая программа» или «Подпрограмма ДЦП»
                             join sbp in context.SBP on prg.IdSBP equals sbp.Id
                             join org in context.Organization on sbp.IdOrganization equals
                                 org.Id
                             join osnprg in context.Program on aop.IdParent equals
                                 osnprg.Id
                             //Вид основной рпограммы
                             //join progview in programview on prg.IdDocType equals progview.Key //Вид основной рпограммы
                             select new DataSetStateProgPass()
                             {
                                 Coexecuter = org.Description, //Соисполнители
                                 Viewprog =
                                     ((osnprg.IdDocType == -1543503843)
                                          ? "государственной программы"
                                          : ((osnprg.IdDocType == -1543503841)
                                                 ? "ведомственной целевой программы"
                                                 : ((osnprg.IdDocType == -1543503837)
                                                        ? "долгосрочной целевой программы"
                                                        : ((osnprg.IdDocType == -1543503842)
                                                               ? "ПОДПРОГРАММЫ"
                                                               : "")
                                                   )
                                            )
                                     ) //Вид программы
                             });
                return pres1;
            }
        }

        public IEnumerable<DataSetStateProgPass> Participant() //Участники (для программ с типом = «Ведомственная целевая программа» или «Основное мероприятие».  которой являеться вышестоящей вывести СПБ.Организацию )
        {
            if (buildReportApprovedData == false)
            {
                DataContext context = IoC.Resolve<DbContext>().Cast<DataContext>();

                IEnumerable<DataSetStateProgPass> pres = (from aop in
                                                              context.AttributeOfProgram.Where(
                                                                  s => s.IdParent == idProgram && s.IdTerminator == null
                                                                    && s.IdPublicLegalFormation == idPublicLegalFormation
                                                                    && s.IdVersion == idVersion
                                                                  )
                                                          //получаем всех детей 
                                                          join prg in
                                                              context.Program.Where(s => (s.IdDocType == -1543503841
                                                                                         || s.IdDocType == -1543503839)
                                                                                         && s.IdTerminator == null
                                                                                         && s.IdPublicLegalFormation == idPublicLegalFormation
                                                                                         && s.IdVersion == idVersion
                                                                                         && (s.IdDocType == -1543503841 || s.IdDocType == -1543503839)
                                                                                         ) on
                                                              aop.IdProgram equals prg.Id
                                                          //получаем наименование вышестояющей программы если есть и тип «Ведомственная целевая программа» или «Основное мероприятие».
                                                          join sbp in context.SBP on prg.IdSBP equals sbp.Id
                                                          join org in context.Organization on sbp.IdOrganization equals
                                                              org.Id
                                                          join osnprg in context.Program on aop.IdParent equals
                                                              osnprg.Id
                                                          //Вид основной рпограммы 
                                                          select new DataSetStateProgPass()
                                                              {
                                                                  Participant = org.Description, //Соисполнители
                                                                  Viewprog =
                                                                      ((osnprg.IdDocType == -1543503843)
                                                                           ? "государственной программы"
                                                                           : ((osnprg.IdDocType == -1543503841)
                                                                                  ? "ведомственной целевой программы"
                                                                                  : ((osnprg.IdDocType == -1543503837)
                                                                                         ? "долгосрочной целевой программы"
                                                                                         : ((osnprg.IdDocType ==
                                                                                             -1543503842)
                                                                                                ? "ПОДПРОГРАММЫ"
                                                                                                : "")
                                                                                    )
                                                                             )
                                                                      ) //Вид программы
                                                              });

                IEnumerable<DataSetStateProgPass> subProgParticipant =
                                                        (from aop in
                                                              context.AttributeOfProgram.Where(
                                                                  s => s.IdParent == idProgram && s.IdTerminator == null
                                                                    && s.IdPublicLegalFormation == idPublicLegalFormation
                                                                    && s.IdVersion == idVersion
                                                                  )
                                                          //получаем всех детей 
                                                          join prg in
                                                             context.Program.Where(s =>  s.IdDocType == -1543503842
                                                                                         && s.IdTerminator == null
                                                                                         && s.IdPublicLegalFormation == idPublicLegalFormation
                                                                                         && s.IdVersion == idVersion
                                                                                         ) on
                                                              aop.IdProgram equals prg.Id
                                                          join atrsubprog in context.AttributeOfProgram.Where(
                                                                  s => s.IdTerminator == null
                                                                    && s.IdPublicLegalFormation == idPublicLegalFormation
                                                                    && s.IdVersion == idVersion
                                                                  ) on prg.Id equals atrsubprog.IdParent
                                                          join subprg in
                                                              context.Program.Where(s => s.IdTerminator == null
                                                                                         && s.IdPublicLegalFormation == idPublicLegalFormation
                                                                                         && s.IdVersion == idVersion
                                                                                         && (s.IdDocType == -1543503841 || s.IdDocType == -1543503839)
                                                                                         ) on atrsubprog.IdProgram equals  subprg.Id

                                                          //получаем наименование вышестояющей программы если есть и тип «Ведомственная целевая программа» или «Основное мероприятие».
                                                          join sbp in context.SBP on subprg.IdSBP equals sbp.Id
                                                          join org in context.Organization on sbp.IdOrganization equals
                                                              org.Id
                                                          join osnprg in context.Program on aop.IdParent equals
                                                              osnprg.Id
                                                          //Вид основной рпограммы 
                                                          select new DataSetStateProgPass()
                                                              {
                                                                  Participant = org.Description, //Соисполнители
                                                                  Viewprog =
                                                                      ((osnprg.IdDocType == -1543503843)
                                                                           ? "государственной программы"
                                                                           : ((osnprg.IdDocType == -1543503841)
                                                                                  ? "ведомственной целевой программы"
                                                                                  : ((osnprg.IdDocType == -1543503837)
                                                                                         ? "долгосрочной целевой программы"
                                                                                         : ((osnprg.IdDocType ==
                                                                                             -1543503842)
                                                                                                ? "ПОДПРОГРАММЫ"
                                                                                                : "")
                                                                                    )
                                                                             )
                                                                      ) //Вид программы
                                                              });

                string Str_executer = PassportData().Select(s => s.Executer).SingleOrDefault();

                IEnumerable<DataSetStateProgPass> resume = pres.Union(subProgParticipant).Where(s => s.Participant != Str_executer);

                if (context.Program.Any(s => s.Id == idProgram && (s.IdDocType == -1543503843 || s.IdDocType == -1543503842)))
                    if (!resume.Any())
                        resume = ViewProgramType().ToList();

                return resume;
            }
            else
            {
                DataContext context = IoC.Resolve<DbContext>().Cast<DataContext>();
                var pres1 = (from aop in
                                 context.AttributeOfProgram.Where(
                                     s => s.IdParent == idProgram && s.IdTerminator == null
                                     && s.DateCommit <= DateReport
                                     && s.DateCommit != null
                                     && s.IdPublicLegalFormation == idPublicLegalFormation
                                     && s.IdVersion == idVersion
                                     )
                             //получаем всех детей 
                             join prg in
                                 context.Program.Where(s => (s.IdDocType == -1543503841
                                                            || s.IdDocType == -1543503839)
                                                            && s.IdTerminator == null
                                                            && s.DateCommit <= DateReport
                                                            && s.DateCommit != null
                                                            && s.IdPublicLegalFormation == idPublicLegalFormation
                                                            && s.IdVersion == idVersion
                                                            && (s.IdDocType == -1543503841 || s.IdDocType == -1543503839)
                                                            ) on
                                 aop.IdProgram equals prg.Id
                             //получаем наименование вышестояющей программы если есть и тип «Ведомственная целевая программа» или «Основное мероприятие».
                             join sbp in context.SBP on prg.IdSBP equals sbp.Id
                             join org in context.Organization on sbp.IdOrganization equals
                                 org.Id
                             join osnprg in context.Program on aop.IdParent equals
                                 osnprg.Id
                             //Вид основной рпограммы 
                             select new DataSetStateProgPass()
                             {
                                 Participant = org.Description, //Соисполнители
                                 Viewprog =
                                     ((osnprg.IdDocType == -1543503843)
                                          ? "государственной программы"
                                          : ((osnprg.IdDocType == -1543503841)
                                                 ? "ведомственной целевой программы"
                                                 : ((osnprg.IdDocType == -1543503837)
                                                        ? "долгосрочной целевой программы"
                                                        : ((osnprg.IdDocType ==
                                                            -1543503842)
                                                               ? "ПОДПРОГРАММЫ"
                                                               : "")
                                                   )
                                            )
                                     ) //Вид программы
                             });

                IEnumerable<DataSetStateProgPass> subProgParticipant =
                                                        (from aop in
                                                             context.AttributeOfProgram.Where(
                                                                 s => s.IdParent == idProgram && s.IdTerminator == null
                                                                   && s.IdPublicLegalFormation == idPublicLegalFormation
                                                                   && s.IdVersion == idVersion
                                                                   && s.DateCommit <= DateReport
                                                                   && s.DateCommit != null
                                                                 )
                                                         //получаем всех детей 
                                                         join prg in
                                                             context.Program.Where(s => s.IdDocType == -1543503842
                                                                                         && s.IdTerminator == null
                                                                                         && s.IdPublicLegalFormation == idPublicLegalFormation
                                                                                         && s.IdVersion == idVersion
                                                                                         && s.DateCommit <= DateReport
                                                                                         && s.DateCommit != null
                                                                                         ) on
                                                             aop.IdProgram equals prg.Id
                                                         join atrsubprog in context.AttributeOfProgram.Where(
                                                                 s => s.IdTerminator == null
                                                                   && s.IdPublicLegalFormation == idPublicLegalFormation
                                                                   && s.IdVersion == idVersion
                                                                   && s.DateCommit <= DateReport
                                                                   && s.DateCommit != null
                                                                 ) on prg.Id equals atrsubprog.IdParent
                                                         join subprg in
                                                             context.Program.Where(s => s.IdTerminator == null
                                                                                        && s.IdPublicLegalFormation == idPublicLegalFormation
                                                                                        && s.IdVersion == idVersion
                                                                                        && (s.IdDocType == -1543503841 || s.IdDocType == -1543503839)
                                                                                        && s.DateCommit <= DateReport
                                                                                        && s.DateCommit != null
                                                                                        ) on atrsubprog.IdProgram equals subprg.Id

                                                         //получаем наименование вышестояющей программы если есть и тип «Ведомственная целевая программа» или «Основное мероприятие».
                                                         join sbp in context.SBP on subprg.IdSBP equals sbp.Id
                                                         join org in context.Organization on sbp.IdOrganization equals
                                                             org.Id
                                                         join osnprg in context.Program on aop.IdParent equals
                                                             osnprg.Id
                                                         //Вид основной рпограммы 
                                                         select new DataSetStateProgPass()
                                                         {
                                                             Participant = org.Description, //Соисполнители
                                                             Viewprog =
                                                                 ((osnprg.IdDocType == -1543503843)
                                                                      ? "государственной программы"
                                                                      : ((osnprg.IdDocType == -1543503841)
                                                                             ? "ведомственной целевой программы"
                                                                             : ((osnprg.IdDocType == -1543503837)
                                                                                    ? "долгосрочной целевой программы"
                                                                                    : ((osnprg.IdDocType ==
                                                                                        -1543503842)
                                                                                           ? "ПОДПРОГРАММЫ"
                                                                                           : "")
                                                                               )
                                                                        )
                                                                 ) //Вид программы
                                                         });
                IEnumerable<DataSetStateProgPass> resume = pres1.Union(subProgParticipant);

                if (context.Program.Any(s => s.Id == idProgram && (s.IdDocType == -1543503843 || s.IdDocType == -1543503842)))
                    if (!resume.Any())
                        resume = ViewProgramType().ToList();

                return resume;
            }
        }



        public IEnumerable<DataSetStateProgPass> Goal() //цель -не нужна вычисляеться в PassportData
        {
            if (buildReportApprovedData == false)
            {
                DataContext context = IoC.Resolve<DbContext>().Cast<DataContext>();
                IEnumerable<DataSetStateProgPass> pres =
                    (from aop in context.AttributeOfProgram.Where(s => s.IdProgram == idProgram
                                                                       && s.IdTerminator == null
                                                                       && s.IdPublicLegalFormation == idPublicLegalFormation
                                                                       && s.IdVersion == idVersion
                                                                       )
                     join prg in context.Program.Where(s => s.IdTerminator == null
                                                                 && s.IdPublicLegalFormation == idPublicLegalFormation
                                                                 && s.IdVersion == idVersion  
                                                      ) on aop.IdProgram equals prg.Id
                     join sge in context.SystemGoalElement.Where(s => s.IdTerminator == null
                                                                 && s.IdPublicLegalFormation == idPublicLegalFormation
                                                                 && s.IdVersion == idVersion
                     ) on aop.IdGoalSystemElement
                         equals sge.Id
                     join sg in context.SystemGoal on sge.IdSystemGoal equals sg.Id
                     //Вид программы

                     select new DataSetStateProgPass()
                         {
                             Goal = sg.Caption, //Цель = из атрибутов программы основная цель
                             Viewprog = ((prg.IdDocType == -1543503843)
                                             ? "ГОСУДАРСТВЕННОЙ ПРОГРАММЫ"
                                             : ((prg.IdDocType == -1543503841)
                                                    ? "ВЕДОМСТВЕННОЙ ЦЕЛЕВОЙ ПРОГРАММЫ"
                                                    : ((prg.IdDocType == -1543503837)
                                                           ? "ДОЛГОСРОЧНОЙ ЦЕЛЕВОЙ ПРОГРАММЫ"
                                                           : ((prg.IdDocType == -1543503842)
                                                                  ? "ПОДПРОГРАММЫ"
                                                                  : "")
                                                      )
                                               )
                                        ) //Вид программы
                         });
                return pres;
            }
            else
            {
                DataContext context = IoC.Resolve<DbContext>().Cast<DataContext>();
                var pres1 = (from aop in context.AttributeOfProgram.Where(s => s.IdProgram == idProgram
                                                                       && s.IdTerminator == null
                                                                       && s.DateCommit <= DateReport
                                                                        && s.DateCommit != null
                                                                        && s.IdPublicLegalFormation == idPublicLegalFormation
                                                                        && s.IdVersion == idVersion)
                             join prg in context.Program.Where(s => s.IdTerminator == null
                                                                && s.DateCommit <= DateReport
                                                                && s.DateCommit != null
                                                                && s.IdPublicLegalFormation == idPublicLegalFormation
                                                                && s.IdVersion == idVersion
                                                                 ) on aop.IdProgram equals prg.Id
                             join sge in context.SystemGoalElement.Where(s => s.IdTerminator == null) on aop.IdGoalSystemElement
                                 equals sge.Id
                             join sg in context.SystemGoal on sge.IdSystemGoal equals sg.Id
                             //Вид программы

                             select new DataSetStateProgPass()
                             {
                                 Goal = sg.Caption, //Цель = из атрибутов программы основная цель
                                 Viewprog = ((prg.IdDocType == -1543503843)
                                                 ? "ГОСУДАРСТВЕННОЙ ПРОГРАММЫ"
                                                 : ((prg.IdDocType == -1543503841)
                                                        ? "ВЕДОМСТВЕННОЙ ЦЕЛЕВОЙ ПРОГРАММЫ"
                                                        : ((prg.IdDocType == -1543503837)
                                                               ? "ДОЛГОСРОЧНОЙ ЦЕЛЕВОЙ ПРОГРАММЫ"
                                                               : ((prg.IdDocType == -1543503842)
                                                                      ? "ПОДПРОГРАММЫ"
                                                                      : "")
                                                          )
                                                   )
                                            ) //Вид программы
                             });
                return pres1;
            }
        }



        public IEnumerable<DataSetStateProgPass> Task() //Задачи 
        {
            if (buildReportApprovedData == false)
            {
                DataContext context = IoC.Resolve<DbContext>().Cast<DataContext>();
                //var vs = context.AttributeOfProgram.Where(s => s.IdProgram == idProgram).Select(d => d.IdGoalSystemElement);
                IEnumerable<DataSetStateProgPass> pres = (
                                                             //from sge in context.SystemGoalElement.Where(s => s.IdProgram == idProgram)
                                                             //                                            //Equals(s.IdProgram, vs))
                                                             //join prg in context.Program on sge.IdProgram equals prg.Id
                                                             //join aosge in context.AttributeOfSystemGoalElement.Where(t => Equals(t.IdSystemGoalElement_Parent, vs)) on sge.Id equals aosge.IdSystemGoalElement
                                                             //join tsge in context.SystemGoalElement on aosge.IdSystemGoalElement equals tsge.Id
                                                             //join sg in context.SystemGoal on tsge.IdSystemGoal equals sg.Id //Вид программы

                                                             from prg in context.Program.Where(s => s.Id == idProgram
                                                                                                    &&
                                                                                                    s.IdTerminator ==
                                                                                                    null
                                                                                                    && s.IdPublicLegalFormation == idPublicLegalFormation
                                                                                                    && s.IdVersion == idVersion
                                                                                                    )
                                                             join aop in
                                                                 context.AttributeOfProgram.Where(
                                                                     s => s.IdTerminator == null
                                                                     && s.IdPublicLegalFormation == idPublicLegalFormation
                                                                     && s.IdVersion == idVersion
                                                                 ) on prg.Id equals
                                                                 aop.IdProgram
                                                             join aosge in
                                                                 context.AttributeOfSystemGoalElement.Where(
                                                                     s => s.IdTerminator == null
                                                                     && s.IdPublicLegalFormation == idPublicLegalFormation
                                                                     && s.IdVersion == idVersion
                                                                     ) on
                                                                 aop.IdGoalSystemElement equals
                                                                 aosge.IdSystemGoalElement_Parent
                                                             join sge in
                                                                 context.SystemGoalElement.Where(
                                                                     s => s.IdTerminator == null
                                                                     && s.IdPublicLegalFormation == idPublicLegalFormation
                                                                     && s.IdVersion == idVersion
                                                                     ) on
                                                                 aosge.IdSystemGoalElement equals sge.Id
                                                             join sg in context.SystemGoal on sge.IdSystemGoal equals
                                                                 sg.Id

                                                             select new DataSetStateProgPass()
                                                                 {
                                                                     Task = sg.Caption, //Задачи
                                                                     Viewprog =
                                                                         ((prg.IdDocType == -1543503843)
                                                                              ? "ГОСУДАРСТВЕННОЙ ПРОГРАММЫ"
                                                                              : ((prg.IdDocType == -1543503841)
                                                                                     ? "ВЕДОМСТВЕННОЙ ЦЕЛЕВОЙ ПРОГРАММЫ"
                                                                                     : ((prg.IdDocType == -1543503837)
                                                                                            ? "ДОЛГОСРОЧНОЙ ЦЕЛЕВОЙ ПРОГРАММЫ"
                                                                                            : ((prg.IdDocType ==
                                                                                                -1543503842)
                                                                                                   ? "ПОДПРОГРАММЫ"
                                                                                                   : "")
                                                                                       )
                                                                                )
                                                                         ) //Вид программы
                                                                 });

                return pres;
            }
            else
            {
                DataContext context = IoC.Resolve<DbContext>().Cast<DataContext>();
                var pres1 = (
                    //from sge in context.SystemGoalElement.Where(s => s.IdProgram == idProgram)
                    //                                            //Equals(s.IdProgram, vs))
                    //join prg in context.Program on sge.IdProgram equals prg.Id
                    //join aosge in context.AttributeOfSystemGoalElement.Where(t => Equals(t.IdSystemGoalElement_Parent, vs)) on sge.Id equals aosge.IdSystemGoalElement
                    //join tsge in context.SystemGoalElement on aosge.IdSystemGoalElement equals tsge.Id
                    //join sg in context.SystemGoal on tsge.IdSystemGoal equals sg.Id //Вид программы

                                                             from prg in context.Program.Where(s => s.Id == idProgram
                                                                                                    &&
                                                                                                    s.IdTerminator == null
                                                                                                    && s.DateCommit <= DateReport
                                                                                                    && s.DateCommit != null
                                                                                                    && s.IdPublicLegalFormation == idPublicLegalFormation
                                                                                                    && s.IdVersion == idVersion
                                                                                                    )
                                                             join aop in
                                                                 context.AttributeOfProgram.Where(
                                                                     s => s.IdTerminator == null
                                                                        && s.DateCommit <= DateReport
                                                                        && s.DateCommit != null
                                                                        && s.IdPublicLegalFormation == idPublicLegalFormation
                                                                        && s.IdVersion == idVersion
                                                                        ) on prg.Id equals
                                                                 aop.IdProgram
                                                             join aosge in
                                                                 context.AttributeOfSystemGoalElement.Where(
                                                                     s => s.IdTerminator == null
                                                                     && s.DateCommit <= DateReport
                                                                     && s.DateCommit != null
                                                                     && s.IdPublicLegalFormation == idPublicLegalFormation
                                                                     && s.IdVersion == idVersion
                                                                     ) on
                                                                 aop.IdGoalSystemElement equals
                                                                 aosge.IdSystemGoalElement_Parent
                                                             join sge in
                                                                 context.SystemGoalElement.Where(
                                                                     s => s.IdTerminator == null
                                                                     && s.IdPublicLegalFormation == idPublicLegalFormation
                                                                     && s.IdVersion == idVersion
                                                                     ) on
                                                                 aosge.IdSystemGoalElement equals sge.Id
                                                             join sg in context.SystemGoal on sge.IdSystemGoal equals
                                                                 sg.Id

                                                             select new DataSetStateProgPass()
                                                             {
                                                                 Task = sg.Caption, //Задачи
                                                                 Viewprog =
                                                                     ((prg.IdDocType == -1543503843)
                                                                          ? "ГОСУДАРСТВЕННОЙ ПРОГРАММЫ"
                                                                          : ((prg.IdDocType == -1543503841)
                                                                                 ? "ВЕДОМСТВЕННОЙ ЦЕЛЕВОЙ ПРОГРАММЫ"
                                                                                 : ((prg.IdDocType == -1543503837)
                                                                                        ? "ДОЛГОСРОЧНОЙ ЦЕЛЕВОЙ ПРОГРАММЫ"
                                                                                        : ((prg.IdDocType ==
                                                                                            -1543503842)
                                                                                               ? "ПОДПРОГРАММЫ"
                                                                                               : "")
                                                                                   )
                                                                            )
                                                                     ) //Вид программы
                                                             });
                return pres1;
            }
        }

        public IEnumerable<DataSetStateProgPass> Goalindicator() ////наименование целевых показателей
        {
            if (buildReportApprovedData == false)
            {
                DataContext context = IoC.Resolve<DbContext>().Cast<DataContext>();
                //var vs = context.AttributeOfProgram.Where(s => s.IdProgram == idProgram).Select(d => d.IdGoalSystemElement);
                IEnumerable<DataSetStateProgPass> pres = (
                                                             //from gt in context.GoalTarget.Where(s => Equals(s.IdSystemGoalElement, vs))
                                                             //join gi in context.GoalIndicator on gt.IdGoalIndicator equals gi.Id
                                                             //join sge in context.SystemGoalElement on gt.IdSystemGoalElement equals sge.Id
                                                             //join prg in context.Program on sge.IdProgram equals prg.Id //Вид программы
                                                             //join progview in programview on prg.IdDocType equals progview.Key //Вид программы
                                                             from prg in context.Program.Where(s => s.Id == idProgram)
                                                             join aop in
                                                                 context.AttributeOfProgram.Where(
                                                                     s => s.IdTerminator == null
                                                                     && s.IdPublicLegalFormation == idPublicLegalFormation
                                                                     && s.IdVersion == idVersion
                                                                     ) on prg.Id equals
                                                                 aop.IdProgram
                                                             join gt in
                                                                 context.GoalTarget.Where(s => s.IdTerminator == null
                                                                 && s.IdPublicLegalFormation == idPublicLegalFormation
                                                                 && s.IdVersion == idVersion
                                                                 )
                                                                 on aop.IdGoalSystemElement equals
                                                                 gt.IdSystemGoalElement
                                                             join gi in context.GoalIndicator on gt.IdGoalIndicator
                                                                 equals gi.Id

                                                             select new DataSetStateProgPass()
                                                                 {
                                                                     Goalindicator = gi.Caption,
                                                                     Viewprog =
                                                                         ((prg.IdDocType == -1543503843)
                                                                              ? "ГОСУДАРСТВЕННОЙ ПРОГРАММЫ"
                                                                              : ((prg.IdDocType == -1543503841)
                                                                                     ? "ВЕДОМСТВЕННОЙ ЦЕЛЕВОЙ ПРОГРАММЫ"
                                                                                     : ((prg.IdDocType == -1543503837)
                                                                                            ? "ДОЛГОСРОЧНОЙ ЦЕЛЕВОЙ ПРОГРАММЫ"
                                                                                            : ((prg.IdDocType == -1543503842)
                                                                                                   ? "ПОДПРОГРАММЫ"
                                                                                                   : "")
                                                                                       )
                                                                                )
                                                                         ) //Вид программы

                                                                 });
                return pres;
            }
            else
            {
                DataContext context = IoC.Resolve<DbContext>().Cast<DataContext>();
                var pres1 = (
                    //from gt in context.GoalTarget.Where(s => Equals(s.IdSystemGoalElement, vs))
                    //join gi in context.GoalIndicator on gt.IdGoalIndicator equals gi.Id
                    //join sge in context.SystemGoalElement on gt.IdSystemGoalElement equals sge.Id
                    //join prg in context.Program on sge.IdProgram equals prg.Id //Вид программы
                    //join progview in programview on prg.IdDocType equals progview.Key //Вид программы
                                                             from prg in context.Program.Where(s => s.Id == idProgram
                                                                                                && s.DateCommit <= DateReport
                                                                                                && s.DateCommit != null
                                                                                                && s.IdPublicLegalFormation == idPublicLegalFormation
                                                                                                && s.IdVersion == idVersion
                                                                                                )
                                                             join aop in
                                                                 context.AttributeOfProgram.Where(
                                                                     s => s.IdTerminator == null
                                                                     && s.DateCommit <= DateReport
                                                                     && s.DateCommit != null
                                                                     && s.IdPublicLegalFormation == idPublicLegalFormation
                                                                     && s.IdVersion == idVersion
                                                                     ) on prg.Id equals
                                                                 aop.IdProgram
                                                             join gt in
                                                                 context.GoalTarget.Where(s => s.IdTerminator == null
                                                                                            && s.DateCommit <= DateReport
                                                                                             && s.DateCommit != null
                                                                                             && s.IdPublicLegalFormation == idPublicLegalFormation
                                                                                             && s.IdVersion == idVersion
                                                                                             )
                                                                 on aop.IdGoalSystemElement equals
                                                                 gt.IdSystemGoalElement
                                                             join gi in context.GoalIndicator on gt.IdGoalIndicator
                                                                 equals gi.Id

                                                             select new DataSetStateProgPass()
                                                             {
                                                                 Goalindicator = gi.Caption,
                                                                 Viewprog =
                                                                     ((prg.IdDocType == -1543503843)
                                                                          ? "ГОСУДАРСТВЕННОЙ ПРОГРАММЫ"
                                                                          : ((prg.IdDocType == -1543503841)
                                                                                 ? "ВЕДОМСТВЕННОЙ ЦЕЛЕВОЙ ПРОГРАММЫ"
                                                                                 : ((prg.IdDocType == -1543503837)
                                                                                        ? "ДОЛГОСРОЧНОЙ ЦЕЛЕВОЙ ПРОГРАММЫ"
                                                                                        : ((prg.IdDocType == -1543503842)
                                                                                               ? "ПОДПРОГРАММЫ"
                                                                                               : "")
                                                                                   )
                                                                            )
                                                                     ) //Вид программы

                                                             });
                return pres1;
            }
        }

        public IEnumerable<DataSetStateProgPass> Subproglist() //Подпрограммы и Тип программы = «Подпрограмма ГП» или «Подпрограмма ДЦП»
        {
            DataContext context = IoC.Resolve<DbContext>().Cast<DataContext>();
            var FirstSubProgList = SubProgramList(-1543503842);
            var SecondSubProgList = SubProgramList(-1543503835);
            var pres = FirstSubProgList.Union(SecondSubProgList);
            if (context.Program.Any(s => s.Id == idProgram && (s.IdDocType == -1543503843 || s.IdDocType == -1543503837)))
                if (!pres.Any())
                    pres = ViewProgramType().ToList();
            return pres.OrderBy(s => s.AnalyticCode);

        }

        public IEnumerable<DataSetStateProgPass> Dcpproglist() //Перечень долгосрочных целевых программ и Тип программы = «Долгосрочная целевая программа»-1543503837
        {
            DataContext context = IoC.Resolve<DbContext>().Cast<DataContext>();
            var SubProgList = SubProgramList(-1543503837);
            //if (context.Program.Any(s => s.Id == idProgram && s.IdDocType == -1543503843))
            //    if (!SubProgList.Any())
            //        SubProgList = ViewProgramType().ToList();
            return SubProgList;

            #region OldCode

            //if (buildReportApprovedData == false)
            //{
            //    DataContext context = IoC.Resolve<DbContext>().Cast<DataContext>();
            //    IEnumerable<DataSetStateProgPass> pres = (from aop in
            //                                                  context.AttributeOfProgram.Where(
            //                                                      s => s.IdParent == idProgram
            //                                                        && s.IdPublicLegalFormation == idPublicLegalFormation
            //                                                        && s.IdVersion == idVersion
            //                                                        && s.IdTerminator == null)
            //                                              //получаем всех детей
            //                                              join prg in
            //                                                  context.Program.Where(s => s.IdDocType == -1543503837
            //                                                                        && s.IdTerminator == null 
            //                                                                        && s.IdPublicLegalFormation == idPublicLegalFormation
            //                                                                        && s.IdVersion == idVersion)
            //                                                  //получаем наименование вышестояющей программы если есть и тип «Долгосрочная целевая программа»
            //                                                  on aop.IdProgram equals prg.Id

            //                                              join ancode in context.AnalyticalCodeStateProgram on aop.IdAnalyticalCodeStateProgram equals ancode.Id

            //                                              join vsprg in context.Program on aop.IdParent equals vsprg.Id
            //                                              //Вид программы


            //                                              select new DataSetStateProgPass()
            //                                                  {
            //                                                      Dcpproglist = prg.Caption,
            //                                                      Viewprog =
            //                                                          ((vsprg.IdDocType == -1543503843)
            //                                                               ? "ГОСУДАРСТВЕННОЙ ПРОГРАММЫ"
            //                                                               : ((vsprg.IdDocType == -1543503841)
            //                                                                      ? "ВЕДОМСТВЕННОЙ ЦЕЛЕВОЙ ПРОГРАММЫ"
            //                                                                      : ((vsprg.IdDocType == -1543503837)
            //                                                                             ? "ДОЛГОСРОЧНОЙ ЦЕЛЕВОЙ ПРОГРАММЫ"
            //                                                                             : ((vsprg.IdDocType == -1543503842)
            //                                                                                    ? "ПОДПРОГРАММЫ"
            //                                                                                    : "")
            //                                                                        )
            //                                                                 )
            //                                                          ), //Вид программы
            //                                                      AnalyticCode = ancode.AnalyticalCode
            //                                                  });
            //    return pres.OrderBy(s => s.AnalyticCode);
            //}
            //else
            //{
            //    DataContext context = IoC.Resolve<DbContext>().Cast<DataContext>();
            //    var pres1 = (from aop in
            //                     context.AttributeOfProgram.Where(
            //                         s => s.IdParent == idProgram
            //                         && s.DateCommit <= DateReport
            //                         && s.DateCommit != null
            //                         && s.IdPublicLegalFormation == idPublicLegalFormation
            //                         && s.IdVersion == idVersion
            //                         && s.IdTerminator == null
            //                         )
            //                 //получаем всех детей
            //                 join prg in
            //                     context.Program.Where(s => s.IdDocType == -1543503837
            //                                            && s.DateCommit <= DateReport
            //                                            && s.DateCommit != null
            //                                            && s.IdPublicLegalFormation == idPublicLegalFormation
            //                                            && s.IdVersion == idVersion
            //                                            && s.IdTerminator == null)
            //                         //получаем наименование вышестояющей программы если есть и тип «Долгосрочная целевая программа»
            //                     on aop.IdProgram equals prg.Id

            //                 join ancode in context.AnalyticalCodeStateProgram on aop.IdAnalyticalCodeStateProgram equals ancode.Id

            //                 join vsprg in context.Program on aop.IdParent equals vsprg.Id
            //                 //Вид программы


            //                 select new DataSetStateProgPass()
            //                 {
            //                     Dcpproglist = prg.Caption,
            //                     Viewprog =
            //                         ((vsprg.IdDocType == -1543503843)
            //                              ? "ГОСУДАРСТВЕННОЙ ПРОГРАММЫ"
            //                              : ((vsprg.IdDocType == -1543503841)
            //                                     ? "ВЕДОМСТВЕННОЙ ЦЕЛЕВОЙ ПРОГРАММЫ"
            //                                     : ((vsprg.IdDocType == -1543503837)
            //                                            ? "ДОЛГОСРОЧНОЙ ЦЕЛЕВОЙ ПРОГРАММЫ"
            //                                            : ((vsprg.IdDocType == -1543503842)
            //                                                   ? "ПОДПРОГРАММЫ"
            //                                                   : "")
            //                                       )
            //                                )
            //                         ), //Вид программы
            //                     AnalyticCode = ancode.AnalyticalCode
            //                 });
            //    return pres1.OrderBy(s => s.AnalyticCode);
            //}

            #endregion
        }

        public IEnumerable<DataSetStateProgPass> Vcpproglist() //Перечень ВЦП и Тип программы = «Ведомственная целевая программа» -1543503841
        {
            DataContext context = IoC.Resolve<DbContext>().Cast<DataContext>();
            var SubProgList = SubProgramList(-1543503841);
            if (context.Program.Any(s => s.Id == idProgram && (s.IdDocType == -1543503842)))
                if (!SubProgList.Any())
                    SubProgList = ViewProgramType().ToList();
            return SubProgList;
        }

        public IEnumerable<DataSetStateProgPass> Omproglist() //Перечень основных мероприятий и Тип программы = «Основное мероприятие» -1543503839
        {
            DataContext context = IoC.Resolve<DbContext>().Cast<DataContext>();
            var SubProgList = SubProgramList(-1543503839);
            if (context.Program.Any(s => s.Id == idProgram && (s.IdDocType == -1543503842)))
                if (!SubProgList.Any())
                    SubProgList = ViewProgramType().ToList();
            return SubProgList;
        }

        //Ресурсное обеспечение программы (всего) --------------Добавить итоговую строку с суммой все AmountOfCash
        public IEnumerable<DataSetStateProgPass> ResourceAll() 
        {
            if (buildReportApprovedData == false)
            {
                DataContext context = IoC.Resolve<DbContext>().Cast<DataContext>();
                //основная цель
                var budgetyear =
                                context.Budget.Where(s => s.Id == idBudget)
                               .Select(d => d.Year)
                               .FirstOrDefault();
                var query = "";

                switch (idSourcesDataReports)
                {
                    case 0:
                        query = "SELECT * FROM [sbor].[ProgramPassport16_20] (" + idProgram + "," + "NULL , NULL , NULL, 4 , NULL , " + budgetyear.ToString() + "," + idVersion.ToString() + "," + idPublicLegalFormation.ToString() + " )";
                        break;
                    case 2:
                        query = "SELECT * FROM [sbor].[ProgramPassportSBP16_20] (" + idProgram + "," + "NULL , NULL , NULL,  9 , NULL ," + budgetyear.ToString() + "," + idVersion.ToString() + "," + idPublicLegalFormation.ToString() + " )";
                        break;
                    case 3:
                        query = "SELECT * FROM [sbor].[ProgramPassport16_20] (" + idProgram + "," + "NULL , NULL , NULL,  4 , 10 ," + budgetyear.ToString() + "," + idVersion.ToString() + "," + idPublicLegalFormation.ToString() + " )";
                        break;
                    case 4:
                        query = "SELECT * FROM [sbor].[ProgramPassportSBP16_20] (" + idProgram + "," + "NULL , NULL , NULL,  9 , 11 ," + budgetyear.ToString() + "," + idVersion.ToString() + "," + idPublicLegalFormation.ToString() + " )";
                        break;
                }
                //                                                           (@id int,@idFinanceSource int,@idFinanceSource1 int,@FinanceSourceType int,@ValueType int,@ValueType1 int,@YearBudget int,@idVersion int,@PPO int)
                //var query = "SELECT * FROM [sbor].[ProgramPassport16_20] (" + idProgram + "," + "NULL , NULL , NULL, " + budgetyear.ToString() + "," + idVersion.ToString() + "," + idPublicLegalFormation.ToString() + " )";
                var st = context.Database.SqlQuery<DataSetStateProgPass>(query);
                IEnumerable<DataSetStateProgPass> pres = (from sum in st
                                                          join prog in
                                                              context.Program.Where(
                                                                  s =>
                                                                  s.IdPublicLegalFormation == idPublicLegalFormation
                                                                  && s.IdVersion == idVersion
                                                              ) on sum.IdProgram equals prog.Id

                                                          select new DataSetStateProgPass()
                                                              {
                                                                  UnitDimension = "Общий объем финансирования составляет ",
                                                                  Typeprog = "a",
                                                                  Year = sum.Year,
                                                                  IdProgram = sum.IdProgram,
                                                                  AmountOfCash = sum.AmountOfCash,
                                                                  Viewprog =
                                                                      ((prog.IdDocType == -1543503843)
                                                                           ? "ГОСУДАРСТВЕННОЙ ПРОГРАММЫ"
                                                                           : ((prog.IdDocType == -1543503841)
                                                                                  ? "ВЕДОМСТВЕННОЙ ЦЕЛЕВОЙ ПРОГРАММЫ"
                                                                                  : ((prog.IdDocType == -1543503837)
                                                                                         ? "ДОЛГОСРОЧНОЙ ЦЕЛЕВОЙ ПРОГРАММЫ"
                                                                                         : ((prog.IdDocType ==
                                                                                             -1543503842)
                                                                                                ? "ПОДПРОГРАММЫ"
                                                                                                : "")
                                                                                    )
                                                                             )
                                                                      ) //Вид программы
                                                              });
                IEnumerable<DataSetStateProgPass> tempres = null;
                decimal? str = null;
                if (pres.Any())
                {
                    tempres = pres.GroupBy(x => new { x.IdProgram })
                                                                    .Select(x => new
                                                                                     DataSetStateProgPass
                                                                    {
                                                                        IdProgram = x.Key.IdProgram,
                                                                        AmountOfCash =
                                                                            x.Sum(u => u.AmountOfCash) / 1000
                                                                    });
                    str = tempres.Select(s => s.AmountOfCash).First();
                }

                IEnumerable<DataSetStateProgPass> res =
                    pres.GroupBy(x => new { x.IdProgram, x.Year, x.Viewprog, x.Typeprog, x.UnitDimension }).Select(x => new
                                                                                                                  DataSetStateProgPass
                        {
                            UnitDimension = x.Key.UnitDimension,
                            Typeprog = x.Key.Typeprog,
                            Year = x.Key.Year,
                            IdProgram = x.Key.IdProgram,
                            Viewprog = x.Key.Viewprog,
                            AmountOfCash = x.Sum(u => u.AmountOfCash) / 1000
                        }).ToList();
                foreach (var f in res)
                    f.UnitDimension = f.UnitDimension + str.Value.ToString("### ### ### ### ##0.#") + " тыс. рублей, в том числе:";
                return res;
            }
            else
            {
                DataContext context = IoC.Resolve<DbContext>().Cast<DataContext>();

                var budgetyear =
                                context.Budget.Where(s => s.Id == idBudget)
                               .Select(d => d.Year)
                               .FirstOrDefault();

                var query = "";
                switch (idSourcesDataReports)
                {
                    case 0:
                        query = "SELECT * FROM [sbor].[ProgramPassport16_20_ApprovedData] (" + idProgram + "," + "NULL , NULL , NULL , 4 , NULL , " + budgetyear.ToString() + "," + idVersion.ToString() + "," + idPublicLegalFormation.ToString() + ",'" + DateReport.ToString() + "' )";
                        break;
                    case 2:
                        query = "SELECT * FROM [sbor].[ProgramPassportSBP16_20_ApprovedData] (" + idProgram + "," + "NULL , NULL , NULL,  9 , NULL ," + budgetyear.ToString() + "," + idVersion.ToString() + "," + idPublicLegalFormation.ToString() + ",'" + DateReport.ToString() + "' )";
                        break;
                    case 3:
                        query = "SELECT * FROM [sbor].[ProgramPassport16_20_ApprovedData] (" + idProgram + "," + "NULL , NULL , NULL,  4 , 10 ," + budgetyear.ToString() + "," + idVersion.ToString() + "," + idPublicLegalFormation.ToString() + ",'" + DateReport.ToString() + "' )";
                        break;
                    case 4:
                        query = "SELECT * FROM [sbor].[ProgramPassportSBP16_20_ApprovedData] (" + idProgram + "," + "NULL , NULL , NULL,  9 , 11 ," + budgetyear.ToString() + "," + idVersion.ToString() + "," + idPublicLegalFormation.ToString() + ",'" + DateReport.ToString() + "' )";
                        break;
                }
                //var query = "SELECT * FROM [sbor].[ProgramPassport16_20_ApprovedData] (" + idProgram + "," + "NULL , NULL, NULL, " + budgetyear.ToString() + "," + idVersion.ToString() + "," + idPublicLegalFormation.ToString() + ",'" + DateReport.ToString() + "' )";

                var st = context.Database.SqlQuery<DataSetStateProgPass>(query);
                IEnumerable<DataSetStateProgPass> pres1 = (from sum in st
                                                           join prog in
                                                               context.Program.Where(
                                                                   s =>
                                                                   s.IdPublicLegalFormation == idPublicLegalFormation
                                                                   && s.IdVersion == idVersion
                                                               ) on sum.IdProgram equals prog.Id

                                                           select new DataSetStateProgPass()
                                                           {
                                                               UnitDimension = "Общий объем финансирования составляет ",
                                                               Typeprog = "a",
                                                               Year = sum.Year,
                                                               IdProgram = sum.IdProgram,
                                                               AmountOfCash = sum.AmountOfCash,
                                                               Viewprog =
                                                                   ((prog.IdDocType == -1543503843)
                                                                        ? "ГОСУДАРСТВЕННОЙ ПРОГРАММЫ"
                                                                        : ((prog.IdDocType == -1543503841)
                                                                               ? "ВЕДОМСТВЕННОЙ ЦЕЛЕВОЙ ПРОГРАММЫ"
                                                                               : ((prog.IdDocType == -1543503837)
                                                                                      ? "ДОЛГОСРОЧНОЙ ЦЕЛЕВОЙ ПРОГРАММЫ"
                                                                                      : ((prog.IdDocType ==
                                                                                          -1543503842)
                                                                                             ? "ПОДПРОГРАММЫ"
                                                                                             : "")
                                                                                 )
                                                                          )
                                                                   ) //Вид программы
                                                           });

                IEnumerable<DataSetStateProgPass> tempres = null;
                decimal? str = null;
                if (pres1.Any())
                {
                    tempres = pres1.GroupBy(x => new { x.IdProgram })
                                                                    .Select(x => new
                                                                                     DataSetStateProgPass
                                                                    {
                                                                        IdProgram = x.Key.IdProgram,
                                                                        AmountOfCash =
                                                                            x.Sum(u => u.AmountOfCash) / 1000
                                                                    });
                    str = tempres.Select(s => s.AmountOfCash).First();
                }

                IEnumerable<DataSetStateProgPass> res1 =
                    pres1.GroupBy(x => new { x.IdProgram, x.Year, x.Viewprog, x.Typeprog, x.UnitDimension }).Select(x => new
                                                                                                                  DataSetStateProgPass
                    {
                        UnitDimension = x.Key.UnitDimension,
                        Typeprog = x.Key.Typeprog,
                        Year = x.Key.Year,
                        IdProgram = x.Key.IdProgram,
                        Viewprog = x.Key.Viewprog,
                        AmountOfCash = x.Sum(u => u.AmountOfCash) / 1000
                    }).ToList();
                foreach (var f in res1)
                    f.UnitDimension = f.UnitDimension + str.Value.ToString("### ### ### ### ##0.#") + " тыс. рублей, в том числе:";
                return res1;
            }
        }

        //Ресурсное обеспечение программы (Сумма средств фед. бюджета ) --------Добавить года для вывода 
        public IEnumerable<DataSetStateProgPass> ResourceFederal() 
        {
            if (buildReportApprovedData == false)
            {
                DataContext context = IoC.Resolve<DbContext>().Cast<DataContext>();
                //основная цель
                //var vs = context.AttributeOfProgram.Where(s => s.IdProgram == idProgram).Select(d => d.IdGoalSystemElement).FirstOrDefault();

                var budgetyear =
                    context.Budget.Where(s => s.Id == idBudget)
                           .Select(d => d.Year)
                           .FirstOrDefault();

                var query = "";
                switch (idSourcesDataReports)
                {
                    case 0:
                        query = "SELECT * FROM [sbor].[ProgramPassport16_20] (" + idProgram + "," + "-1543503849 , 3 , NULL, 4 , NULL , " + budgetyear.ToString() + "," + idVersion.ToString() + "," + idPublicLegalFormation.ToString() + " )";
                        break;
                    case 2:
                        query = "SELECT * FROM [sbor].[ProgramPassportSBP16_20] (" + idProgram + "," + "-1543503849 , 3 , NULL,  9 , NULL ," + budgetyear.ToString() + "," + idVersion.ToString() + "," + idPublicLegalFormation.ToString() + " )";
                        break;
                    case 3:
                        query = "SELECT * FROM [sbor].[ProgramPassport16_20] (" + idProgram + "," + "-1543503849 , 3 , NULL,  4 , 10 ," + budgetyear.ToString() + "," + idVersion.ToString() + "," + idPublicLegalFormation.ToString() + " )";
                        break;
                    case 4:
                        query = "SELECT * FROM [sbor].[ProgramPassportSBP16_20] (" + idProgram + "," + "-1543503849 , 3 , NULL,  9 , 11 ," + budgetyear.ToString() + "," + idVersion.ToString() + "," + idPublicLegalFormation.ToString() + " )";
                        break;
                }

                //var st = context.Database.SqlQuery<DataSetStateProgPass>("SELECT * FROM [sbor].[ProgramPassport16_20] (" +
                //                                                    idProgram + "," + "-1543503849 , 3 , NULL, " +
                //                                                    budgetyear.ToString() + "," + idVersion.ToString() +
                //                                                    "," + idPublicLegalFormation.ToString() + " )");
                    
                var st = context.Database.SqlQuery<DataSetStateProgPass>(query);
                IEnumerable<DataSetStateProgPass> pres = (from sum in st
                                                          join prog in
                                                              context.Program.Where(
                                                                  s =>
                                                                  s.IdPublicLegalFormation == idPublicLegalFormation
                                                                  && s.IdVersion == idVersion
                                                              ) on sum.IdProgram equals prog.Id

                                                          select new DataSetStateProgPass()
                                                              {
                                                                  UnitDimension =
                                                                      "Объем финансирования за счет средств федерального бюджета составляет ",
                                                                  Typeprog = "b",
                                                                  Year = sum.Year,
                                                                  AmountOfCash = sum.AmountOfCash,
                                                                  Viewprog =
                                                                      ((prog.IdDocType == -1543503843)
                                                                           ? "ГОСУДАРСТВЕННОЙ ПРОГРАММЫ"
                                                                           : ((prog.IdDocType == -1543503841)
                                                                                  ? "ВЕДОМСТВЕННОЙ ЦЕЛЕВОЙ ПРОГРАММЫ"
                                                                                  : ((prog.IdDocType == -1543503837)
                                                                                         ? "ДОЛГОСРОЧНОЙ ЦЕЛЕВОЙ ПРОГРАММЫ"
                                                                                         : ((prog.IdDocType == -1543503842)
                                                                                                ? "ПОДПРОГРАММЫ"
                                                                                                : "")
                                                                                    )
                                                                             )
                                                                      ) //Вид программы
                                                              });
                IEnumerable<DataSetStateProgPass> tempres = null;
                decimal? str = null;
                if (pres.Any())
                {
                    tempres = pres.GroupBy(x => new {x.IdProgram})
                                                                    .Select(x => new
                                                                                     DataSetStateProgPass
                                                                        {
                                                                            IdProgram = x.Key.IdProgram,
                                                                            AmountOfCash =
                                                                                x.Sum(u => u.AmountOfCash)/1000
                                                                        });
                    str = tempres.Select(s => s.AmountOfCash).First();
                }

                

                IEnumerable<DataSetStateProgPass> res =
                    pres.GroupBy(x => new {x.IdProgram, x.Viewprog, x.Year, x.Typeprog, x.UnitDimension})
                        .Select(x => new
                                         DataSetStateProgPass
                            {
                                UnitDimension = x.Key.UnitDimension,
                                Typeprog = x.Key.Typeprog,
                                Year = x.Key.Year,
                                IdProgram = x.Key.IdProgram,
                                Viewprog = x.Key.Viewprog,
                                AmountOfCash = x.Sum(u => u.AmountOfCash) / 1000
                            }).ToList();
                foreach (var f in res)
                    f.UnitDimension = f.UnitDimension + str.Value.ToString("### ### ### ### ##0.#") + " тыс. рублей, в том числе:";
                return res;
            }
            else
            {
                DataContext context = IoC.Resolve<DbContext>().Cast<DataContext>();
               
                var budgetyear =
                    context.Budget.Where(s => s.Id == idBudget)
                           .Select(d => d.Year)
                           .FirstOrDefault();

                var query = "";
                switch (idSourcesDataReports)
                {
                    case 0:
                        query = "SELECT * FROM [sbor].[ProgramPassport16_20_ApprovedData] (" + idProgram + "," + "-1543503849 , 3 , NULL, 4 , NULL , " + budgetyear.ToString() + "," + idVersion.ToString() + "," + idPublicLegalFormation.ToString() + ",'" + DateReport.ToString() + "' )";
                        break;
                    case 2:
                        query = "SELECT * FROM [sbor].[ProgramPassportSBP16_20_ApprovedData] (" + idProgram + "," + "-1543503849 , 3 , NULL,  9 , NULL ," + budgetyear.ToString() + "," + idVersion.ToString() + "," + idPublicLegalFormation.ToString() + ",'" + DateReport.ToString() + "' )";
                        break;
                    case 3:
                        query = "SELECT * FROM [sbor].[ProgramPassport16_20_ApprovedData] (" + idProgram + "," + "-1543503849 , 3 , NULL,  4 , 10 ," + budgetyear.ToString() + "," + idVersion.ToString() + "," + idPublicLegalFormation.ToString() + ",'" + DateReport.ToString() + "' )";
                        break;
                    case 4:
                        query = "SELECT * FROM [sbor].[ProgramPassportSBP16_20_ApprovedData] (" + idProgram + "," + "-1543503849 , 3 , NULL,  9 , 11 ," + budgetyear.ToString() + "," + idVersion.ToString() + "," + idPublicLegalFormation.ToString() + ",'" + DateReport.ToString() + "' )";
                        break;
                }

                var st = context.Database.SqlQuery<DataSetStateProgPass>(query);

                //var st = context.Database.SqlQuery<DataSetStateProgPass>("SELECT * FROM [sbor].[ProgramPassport16_20_ApprovedData] (" +
                //                                                    idProgram + "," + "-1543503849 , 3 , NULL, " +
                //                                                    budgetyear.ToString() + "," + idVersion.ToString() +
                //                                                    "," + idPublicLegalFormation.ToString() + ",'" + DateReport.ToString() + "' )");
                
                IEnumerable<DataSetStateProgPass> pres = (from sum in st
                                                          join prog in
                                                              context.Program.Where(
                                                                  s =>
                                                                  s.IdPublicLegalFormation == idPublicLegalFormation
                                                                  && s.IdVersion == idVersion
                                                              ) on sum.IdProgram equals prog.Id

                                                          select new DataSetStateProgPass()
                                                          {
                                                              UnitDimension =
                                                                  "Объем финансирования за счет средств федерального бюджета составляет ",
                                                              Typeprog = "b",
                                                              Year = sum.Year,
                                                              AmountOfCash = sum.AmountOfCash,
                                                              Viewprog =
                                                                  ((prog.IdDocType == -1543503843)
                                                                       ? "ГОСУДАРСТВЕННОЙ ПРОГРАММЫ"
                                                                       : ((prog.IdDocType == -1543503841)
                                                                              ? "ВЕДОМСТВЕННОЙ ЦЕЛЕВОЙ ПРОГРАММЫ"
                                                                              : ((prog.IdDocType == -1543503837)
                                                                                     ? "ДОЛГОСРОЧНОЙ ЦЕЛЕВОЙ ПРОГРАММЫ"
                                                                                     : ((prog.IdDocType == -1543503842)
                                                                                            ? "ПОДПРОГРАММЫ"
                                                                                            : "")
                                                                                )
                                                                         )
                                                                  ) //Вид программы
                                                          });

                IEnumerable<DataSetStateProgPass> tempres = null;
                decimal? str = null;
                if (pres.Any())
                {
                    tempres = pres.GroupBy(x => new { x.IdProgram })
                                                                    .Select(x => new
                                                                                     DataSetStateProgPass
                                                                    {
                                                                        IdProgram = x.Key.IdProgram,
                                                                        AmountOfCash =
                                                                            x.Sum(u => u.AmountOfCash) / 1000
                                                                    });
                    str = tempres.Select(s => s.AmountOfCash).First();
                }

                IEnumerable<DataSetStateProgPass> res =
                    pres.GroupBy(x => new { x.IdProgram, x.Viewprog, x.Year, x.Typeprog, x.UnitDimension })
                        .Select(x => new
                                         DataSetStateProgPass
                        {
                            UnitDimension = x.Key.UnitDimension,
                            Typeprog = x.Key.Typeprog,
                            Year = x.Key.Year,
                            IdProgram = x.Key.IdProgram,
                            Viewprog = x.Key.Viewprog,
                            AmountOfCash = x.Sum(u => u.AmountOfCash) / 1000
                        }).ToList();

                foreach (var f in res)
                    f.UnitDimension = f.UnitDimension + str.Value.ToString("### ### ### ### ##0.#") + " тыс. рублей, в том числе:";
                                      //(f.Value.HasValue ? " - " + f.Value.Value.ToString("0.#####") : "");
                //res.ForEach(f => f.UnitDimension = f.UnitDimension + " - " + str.Value.ToString("### ### ### ### ###.#"));
                
                return res;
            }
        }

        //Ресурсное обеспечение программы (Сумма средств областного бюджета )
        public IEnumerable<DataSetStateProgPass> ResourceRegional()
        {
            if (buildReportApprovedData == false)
            {
                DataContext context = IoC.Resolve<DbContext>().Cast<DataContext>();
                //основная цель
                //var vs = context.AttributeOfProgram.Where(s => s.IdProgram == idProgram).Select(d => d.IdGoalSystemElement).FirstOrDefault();

                var budgetyear =
                    context.Budget.Where(s => s.Id == idBudget)
                           .Select(d => d.Year)
                           .FirstOrDefault();

                var query = "";

                switch (idSourcesDataReports)
                {
                    case 0:
                        query = "SELECT * FROM [sbor].[ProgramPassport16_20] (" + idProgram + "," + "-1543503848 , NULL , NULL, 4 , NULL , " + budgetyear.ToString() + "," + idVersion.ToString() + "," + idPublicLegalFormation.ToString() + " )";
                        break;
                    case 2:
                        query = "SELECT * FROM [sbor].[ProgramPassportSBP16_20] (" + idProgram + "," + "-1543503848 , NULL , NULL,  9 , NULL ," + budgetyear.ToString() + "," + idVersion.ToString() + "," + idPublicLegalFormation.ToString() + " )";
                        break;
                    case 3:
                        query = "SELECT * FROM [sbor].[ProgramPassport16_20] (" + idProgram + "," + "-1543503848 , NULL , NULL,  4 , 10 ," + budgetyear.ToString() + "," + idVersion.ToString() + "," + idPublicLegalFormation.ToString() + " )";
                        break;
                    case 4:
                        query = "SELECT * FROM [sbor].[ProgramPassportSBP16_20] (" + idProgram + "," + "-1543503848 , NULL , NULL,  9 , 11 ," + budgetyear.ToString() + "," + idVersion.ToString() + "," + idPublicLegalFormation.ToString() + " )";
                        break;
                }

                var st = context.Database.SqlQuery<DataSetStateProgPass>(query);
                //var st =
                //    context.Database.SqlQuery<DataSetStateProgPass>("SELECT * FROM [sbor].[ProgramPassport16_20] (" +
                //                                                    idProgram + "," + "-1543503848 , NULL, NULL," +
                //                                                    budgetyear.ToString() + "," + idVersion.ToString() +
                //                                                    "," + idPublicLegalFormation.ToString() + " )");
                    
                IEnumerable<DataSetStateProgPass> pres = (from sum in st
                                                          join prog in
                                                              context.Program.Where(
                                                                  s =>
                                                                  s.IdPublicLegalFormation == idPublicLegalFormation
                                                                  && s.IdVersion == idVersion
                                                              ) on sum.IdProgram equals prog.Id

                                                          select new DataSetStateProgPass()
                                                              {
                                                                  UnitDimension =
                                                                      "Объем финансирования за счет средств областного бюджета составляет ",
                                                                  Typeprog = "c",
                                                                  Year = sum.Year,
                                                                  AmountOfCash = sum.AmountOfCash,
                                                                  Viewprog =
                                                                      ((prog.IdDocType == -1543503843)
                                                                           ? "ГОСУДАРСТВЕННОЙ ПРОГРАММЫ"
                                                                           : ((prog.IdDocType == -1543503841)
                                                                                  ? "ВЕДОМСТВЕННОЙ ЦЕЛЕВОЙ ПРОГРАММЫ"
                                                                                  : ((prog.IdDocType == -1543503837)
                                                                                         ? "ДОЛГОСРОЧНОЙ ЦЕЛЕВОЙ ПРОГРАММЫ"
                                                                                         : ((prog.IdDocType == -1543503842)
                                                                                                ? "ПОДПРОГРАММЫ"
                                                                                                : "")
                                                                                    )
                                                                             )
                                                                      ) //Вид программы
                                                              });

                IEnumerable<DataSetStateProgPass> tempres = null;
                decimal? str = null;
                if (pres.Any())
                {
                    tempres = pres.GroupBy(x => new { x.IdProgram })
                                                                    .Select(x => new
                                                                                     DataSetStateProgPass
                                                                    {
                                                                        IdProgram = x.Key.IdProgram,
                                                                        AmountOfCash =
                                                                            x.Sum(u => u.AmountOfCash) / 1000
                                                                    });
                    str = tempres.Select(s => s.AmountOfCash).First();
                }

                IEnumerable<DataSetStateProgPass> res =
                    pres.GroupBy(x => new {x.IdProgram, x.Viewprog, x.Year, x.Typeprog, x.UnitDimension})
                        .Select(x => new
                                         DataSetStateProgPass
                            {
                                UnitDimension = x.Key.UnitDimension,
                                Typeprog = x.Key.Typeprog,
                                Year = x.Key.Year,
                                IdProgram = x.Key.IdProgram,
                                Viewprog = x.Key.Viewprog,
                                AmountOfCash = x.Sum(u => u.AmountOfCash) / 1000
                            }).ToList();
                foreach (var f in res)
                    f.UnitDimension = f.UnitDimension + str.Value.ToString("### ### ### ### ##0.#") + " тыс. рублей, в том числе:";

                return res;
            }
            else
            {
                DataContext context = IoC.Resolve<DbContext>().Cast<DataContext>();
                //основная цель
                //var vs = context.AttributeOfProgram.Where(s => s.IdProgram == idProgram).Select(d => d.IdGoalSystemElement).FirstOrDefault();

                var budgetyear =
                    context.Budget.Where(s => s.Id == idBudget)
                           .Select(d => d.Year)
                           .FirstOrDefault();

                var query = "";

                switch (idSourcesDataReports)
                {
                    case 0:
                        query = "SELECT * FROM [sbor].[ProgramPassport16_20_ApprovedData] (" + idProgram + "," + "-1543503848 , NULL , NULL, 4 , NULL , " + budgetyear.ToString() + "," + idVersion.ToString() + "," + idPublicLegalFormation.ToString() + ",'" + DateReport.ToString() + "' )";
                        break;
                    case 2:
                        query = "SELECT * FROM [sbor].[ProgramPassportSBP16_20_ApprovedData] (" + idProgram + "," + "-1543503848 , NULL , NULL,  9 , NULL ," + budgetyear.ToString() + "," + idVersion.ToString() + "," + idPublicLegalFormation.ToString() + ",'" + DateReport.ToString() + "' )";
                        break;
                    case 3:
                        query = "SELECT * FROM [sbor].[ProgramPassport16_20_ApprovedData] (" + idProgram + "," + "-1543503848 , NULL , NULL,  4 , 10 ," + budgetyear.ToString() + "," + idVersion.ToString() + "," + idPublicLegalFormation.ToString() + ",'" + DateReport.ToString() + "' )";
                        break;
                    case 4:
                        query = "SELECT * FROM [sbor].[ProgramPassportSBP16_20_ApprovedData] (" + idProgram + "," + "-1543503848 , NULL , NULL,  9 , 11 ," + budgetyear.ToString() + "," + idVersion.ToString() + "," + idPublicLegalFormation.ToString() + ",'" + DateReport.ToString() + "' )";
                        break;
                }

                var st = context.Database.SqlQuery<DataSetStateProgPass>(query);

                //var st =
                //    context.Database.SqlQuery<DataSetStateProgPass>("SELECT * FROM [sbor].[ProgramPassport16_20_ApprovedData] (" +
                //                                                    idProgram + "," + "-1543503848 , NULL, NULL, " +
                //                                                    budgetyear.ToString() + "," + idVersion.ToString() +
                //                                                    "," + idPublicLegalFormation.ToString() + ",'" + DateReport.ToString() + "' )");
                //(" + vs.ToString() + "," + idProgram + "," + "-1543503848 )");
                IEnumerable<DataSetStateProgPass> pres = (from sum in st
                                                          join prog in
                                                              context.Program.Where(
                                                                  s =>
                                                                  s.IdPublicLegalFormation == idPublicLegalFormation
                                                                  && s.IdVersion == idVersion
                                                              ) on sum.IdProgram equals prog.Id

                                                          select new DataSetStateProgPass()
                                                          {
                                                              UnitDimension =
                                                                  "Объем финансирования за счет средств областного бюджета составляет ",
                                                              Typeprog = "c",
                                                              Year = sum.Year,
                                                              AmountOfCash = sum.AmountOfCash,
                                                              Viewprog =
                                                                  ((prog.IdDocType == -1543503843)
                                                                       ? "ГОСУДАРСТВЕННОЙ ПРОГРАММЫ"
                                                                       : ((prog.IdDocType == -1543503841)
                                                                              ? "ВЕДОМСТВЕННОЙ ЦЕЛЕВОЙ ПРОГРАММЫ"
                                                                              : ((prog.IdDocType == -1543503837)
                                                                                     ? "ДОЛГОСРОЧНОЙ ЦЕЛЕВОЙ ПРОГРАММЫ"
                                                                                     : ((prog.IdDocType == -1543503842)
                                                                                            ? "ПОДПРОГРАММЫ"
                                                                                            : "")
                                                                                )
                                                                         )
                                                                  ) //Вид программы
                                                          });

                IEnumerable<DataSetStateProgPass> tempres = null;
                decimal? str = null;
                if (pres.Any())
                {
                    tempres = pres.GroupBy(x => new { x.IdProgram })
                                                                    .Select(x => new
                                                                                     DataSetStateProgPass
                                                                    {
                                                                        IdProgram = x.Key.IdProgram,
                                                                        AmountOfCash =
                                                                            x.Sum(u => u.AmountOfCash) / 1000
                                                                    });
                    str = tempres.Select(s => s.AmountOfCash).First();
                }

                IEnumerable<DataSetStateProgPass> res =
                    pres.GroupBy(x => new { x.IdProgram, x.Viewprog, x.Year, x.Typeprog, x.UnitDimension })
                        .Select(x => new
                                         DataSetStateProgPass
                        {
                            UnitDimension = x.Key.UnitDimension,
                            Typeprog = x.Key.Typeprog,
                            Year = x.Key.Year,
                            IdProgram = x.Key.IdProgram,
                            Viewprog = x.Key.Viewprog,
                            AmountOfCash = x.Sum(u => u.AmountOfCash) / 1000
                        }).ToList();
                foreach (var f in res)
                    f.UnitDimension = f.UnitDimension + str.Value.ToString("### ### ### ### ##0.#") + " тыс. рублей, в том числе:";

                return res;
            }
        }

        //Ресурсное обеспечение программы (Сумма средств местного бюджета)
        public IEnumerable<DataSetStateProgPass> ResourceLocal()
        {
            if (buildReportApprovedData == false)
            {
                DataContext context = IoC.Resolve<DbContext>().Cast<DataContext>();
                //основная цель
                //var vs = context.AttributeOfProgram.Where(s => s.IdProgram == idProgram).Select(d => d.IdGoalSystemElement).FirstOrDefault();

                var budgetyear =
                    context.Budget.Where(s => s.Id == idBudget)
                           .Select(d => d.Year)
                           .FirstOrDefault();

                var query = "";
                switch (idSourcesDataReports)
                {
                    case 0:
                        query = "SELECT * FROM [sbor].[ProgramPassport16_20] (" + idProgram + "," + "-1543503847 , NULL , NULL, 4 , NULL , " + budgetyear.ToString() + "," + idVersion.ToString() + "," + idPublicLegalFormation.ToString() + " )";
                        break;
                    case 2:
                        query = "SELECT * FROM [sbor].[ProgramPassportSBP16_20] (" + idProgram + "," + "-1543503847 , NULL , NULL,  9 , NULL ," + budgetyear.ToString() + "," + idVersion.ToString() + "," + idPublicLegalFormation.ToString() + " )";
                        break;
                    case 3:
                        query = "SELECT * FROM [sbor].[ProgramPassport16_20] (" + idProgram + "," + "-1543503847 , NULL , NULL,  4 , 10 ," + budgetyear.ToString() + "," + idVersion.ToString() + "," + idPublicLegalFormation.ToString() + " )";
                        break;
                    case 4:
                        query = "SELECT * FROM [sbor].[ProgramPassportSBP16_20] (" + idProgram + "," + "-1543503847 , NULL , NULL,  9 , 11 ," + budgetyear.ToString() + "," + idVersion.ToString() + "," + idPublicLegalFormation.ToString() + " )";
                        break;
                }

                var st = context.Database.SqlQuery<DataSetStateProgPass>(query);

                //var st =
                //    context.Database.SqlQuery<DataSetStateProgPass>("SELECT * FROM [sbor].[ProgramPassport16_20] (" +
                //                                                    idProgram + "," + "-1543503847 , NULL, NULL, " +
                //                                                    budgetyear.ToString() + "," + idVersion.ToString() +
                //                                                    "," + idPublicLegalFormation.ToString() + " )");
                    //(" + vs.ToString() + "," + idProgram + "," + "-1543503847 )");
                IEnumerable<DataSetStateProgPass> pres = (from sum in st
                                                          join prog in
                                                              context.Program.Where(
                                                                  s =>
                                                                  s.IdPublicLegalFormation == idPublicLegalFormation
                                                                  && s.IdVersion == idVersion
                                                              ) on sum.IdProgram equals prog.Id

                                                          select new DataSetStateProgPass()
                                                              {
                                                                  UnitDimension = "Объем финансирования за счет средств местного бюджета составляет ",
                                                                  Typeprog = "d",
                                                                  Year = sum.Year,
                                                                  AmountOfCash = sum.AmountOfCash,
                                                                  Viewprog =
                                                                      ((prog.IdDocType == -1543503843)
                                                                           ? "ГОСУДАРСТВЕННОЙ ПРОГРАММЫ"
                                                                           : ((prog.IdDocType == -1543503841)
                                                                                  ? "ВЕДОМСТВЕННОЙ ЦЕЛЕВОЙ ПРОГРАММЫ"
                                                                                  : ((prog.IdDocType == -1543503837)
                                                                                         ? "ДОЛГОСРОЧНОЙ ЦЕЛЕВОЙ ПРОГРАММЫ"
                                                                                         : ((prog.IdDocType == -1543503842)
                                                                                                ? "ПОДПРОГРАММЫ"
                                                                                                : "")
                                                                                    )
                                                                             )
                                                                      ) //Вид программы
                                                              });

                IEnumerable<DataSetStateProgPass> tempres = null;
                decimal? str = null;
                if (pres.Any())
                {
                    tempres = pres.GroupBy(x => new { x.IdProgram })
                                                                    .Select(x => new
                                                                                     DataSetStateProgPass
                                                                    {
                                                                        IdProgram = x.Key.IdProgram,
                                                                        AmountOfCash =
                                                                            x.Sum(u => u.AmountOfCash) / 1000
                                                                    });
                    str = tempres.Select(s => s.AmountOfCash).First();
                }
                
                IEnumerable<DataSetStateProgPass> res =
                    pres.GroupBy(x => new {x.IdProgram, x.Viewprog, x.Year, x.Typeprog, x.UnitDimension})
                        .Select(x => new
                                         DataSetStateProgPass
                            {
                                UnitDimension = x.Key.UnitDimension,
                                Typeprog = x.Key.Typeprog,
                                Year = x.Key.Year,
                                IdProgram = x.Key.IdProgram,
                                Viewprog = x.Key.Viewprog,
                                AmountOfCash = x.Sum(u => u.AmountOfCash) / 1000
                            }).ToList();
                foreach (var f in res)
                    f.UnitDimension = f.UnitDimension + str.Value.ToString("### ### ### ### ##0.#") + " тыс. рублей, в том числе:";

                return res;
            }
            else
            {
                DataContext context = IoC.Resolve<DbContext>().Cast<DataContext>();
                //основная цель
                //var vs = context.AttributeOfProgram.Where(s => s.IdProgram == idProgram).Select(d => d.IdGoalSystemElement).FirstOrDefault();

                var budgetyear =
                    context.Budget.Where(s => s.Id == idBudget)
                           .Select(d => d.Year)
                           .FirstOrDefault();

                var query = "";
                switch (idSourcesDataReports)
                {
                    case 0:
                        query = "SELECT * FROM [sbor].[ProgramPassport16_20_ApprovedData] (" + idProgram + "," + "-1543503847 , NULL , NULL, 4 , NULL , " + budgetyear.ToString() + "," + idVersion.ToString() + "," + idPublicLegalFormation.ToString() + ",'" + DateReport.ToString() + "' )";
                        break;
                    case 2:
                        query = "SELECT * FROM [sbor].[ProgramPassportSBP16_20_ApprovedData] (" + idProgram + "," + "-1543503847 , NULL , NULL,  9 , NULL ," + budgetyear.ToString() + "," + idVersion.ToString() + "," + idPublicLegalFormation.ToString() + ",'" + DateReport.ToString() + "' )";
                        break;
                    case 3:
                        query = "SELECT * FROM [sbor].[ProgramPassport16_20_ApprovedData] (" + idProgram + "," + "-1543503847 , NULL , NULL,  4 , 10 ," + budgetyear.ToString() + "," + idVersion.ToString() + "," + idPublicLegalFormation.ToString() + ",'" + DateReport.ToString() + "' )";
                        break;
                    case 4:
                        query = "SELECT * FROM [sbor].[ProgramPassportSBP16_20_ApprovedData] (" + idProgram + "," + "-1543503847 , NULL , NULL,  9 , 11 ," + budgetyear.ToString() + "," + idVersion.ToString() + "," + idPublicLegalFormation.ToString() + ",'" + DateReport.ToString() + "' )";
                        break;
                }

                var st = context.Database.SqlQuery<DataSetStateProgPass>(query);

                //var st =
                //   context.Database.SqlQuery<DataSetStateProgPass>("SELECT * FROM [sbor].[ProgramPassport16_20_ApprovedData] (" +
                //                                                   idProgram + "," + "-1543503847 , NULL, NULL, " +
                //                                                   budgetyear.ToString() + "," + idVersion.ToString() +
                //                                                   "," + idPublicLegalFormation.ToString() + ",'" + DateReport.ToString() + "' )");
                //(" + vs.ToString() + "," + idProgram + "," + "-1543503847 )");
                IEnumerable<DataSetStateProgPass> pres = (from sum in st
                                                          join prog in
                                                              context.Program.Where(
                                                                  s =>
                                                                  s.IdPublicLegalFormation == idPublicLegalFormation
                                                                  && s.IdVersion == idVersion
                                                              ) on sum.IdProgram equals prog.Id

                                                          select new DataSetStateProgPass()
                                                          {
                                                              UnitDimension = "Объем финансирования за счет средств местного бюджета составляет ",
                                                              Typeprog = "d",
                                                              Year = sum.Year,
                                                              AmountOfCash = sum.AmountOfCash,
                                                              Viewprog =
                                                                  ((prog.IdDocType == -1543503843)
                                                                       ? "ГОСУДАРСТВЕННОЙ ПРОГРАММЫ"
                                                                       : ((prog.IdDocType == -1543503841)
                                                                              ? "ВЕДОМСТВЕННОЙ ЦЕЛЕВОЙ ПРОГРАММЫ"
                                                                              : ((prog.IdDocType == -1543503837)
                                                                                     ? "ДОЛГОСРОЧНОЙ ЦЕЛЕВОЙ ПРОГРАММЫ"
                                                                                     : ((prog.IdDocType == -1543503842)
                                                                                            ? "ПОДПРОГРАММЫ"
                                                                                            : "")
                                                                                )
                                                                         )
                                                                  ) //Вид программы
                                                          });

                IEnumerable<DataSetStateProgPass> tempres = null;
                decimal? str = null;
                if (pres.Any())
                {
                    tempres = pres.GroupBy(x => new { x.IdProgram })
                                                                    .Select(x => new
                                                                                     DataSetStateProgPass
                                                                    {
                                                                        IdProgram = x.Key.IdProgram,
                                                                        AmountOfCash =
                                                                            x.Sum(u => u.AmountOfCash) / 1000
                                                                    });
                    str = tempres.Select(s => s.AmountOfCash).First();
                }
                
                IEnumerable<DataSetStateProgPass> res =
                    pres.GroupBy(x => new { x.IdProgram, x.Viewprog, x.Year, x.Typeprog, x.UnitDimension })
                        .Select(x => new
                                         DataSetStateProgPass
                        {
                            UnitDimension = x.Key.UnitDimension,
                            Typeprog = x.Key.Typeprog,
                            Year = x.Key.Year,
                            IdProgram = x.Key.IdProgram,
                            Viewprog = x.Key.Viewprog,
                            AmountOfCash = x.Sum(u => u.AmountOfCash) / 1000
                        }).ToList();
                foreach (var f in res)
                    f.UnitDimension = f.UnitDimension + str.Value.ToString("### ### ### ### ##0.#") + " тыс. рублей, в том числе:";

                return res;
            }
        }


        //Ресурсное обеспечение программы (Сумма иных источников бюджета)
        public IEnumerable<DataSetStateProgPass> AnotherResource()
        {
            if (buildReportApprovedData == false)
            {
                DataContext context = IoC.Resolve<DbContext>().Cast<DataContext>();
                //основная цель
                //var vs = context.AttributeOfProgram.Where(s => s.IdProgram == idProgram).Select(d => d.IdGoalSystemElement).FirstOrDefault();

                var budgetyear =
                    context.Budget.Where(s => s.Id == idBudget)
                           .Select(d => d.Year)
                           .FirstOrDefault();

                var query = "";
                switch (idSourcesDataReports)
                {
                    case 0:
                        query = "SELECT * FROM [sbor].[ProgramPassport16_20] (" + idProgram + "," + "NULL , NULL , 9, 4 , NULL , " + budgetyear.ToString() + "," + idVersion.ToString() + "," + idPublicLegalFormation.ToString() + " )";
                        break;
                    case 2:
                        query = "SELECT * FROM [sbor].[ProgramPassportSBP16_20] (" + idProgram + "," + "NULL , NULL , 9,  9 , NULL ," + budgetyear.ToString() + "," + idVersion.ToString() + "," + idPublicLegalFormation.ToString() + " )";
                        break;
                    case 3:
                        query = "SELECT * FROM [sbor].[ProgramPassport16_20] (" + idProgram + "," + "NULL , NULL , 9,  4 , 10 ," + budgetyear.ToString() + "," + idVersion.ToString() + "," + idPublicLegalFormation.ToString() + " )";
                        break;
                    case 4:
                        query = "SELECT * FROM [sbor].[ProgramPassportSBP16_20] (" + idProgram + "," + "NULL , NULL , 9,  9 , 11 ," + budgetyear.ToString() + "," + idVersion.ToString() + "," + idPublicLegalFormation.ToString() + " )";
                        break;
                }

                var st = context.Database.SqlQuery<DataSetStateProgPass>(query);

                //var st =
                //    context.Database.SqlQuery<DataSetStateProgPass>("SELECT * FROM [sbor].[ProgramPassport16_20] (" +
                //                                                    idProgram + "," + "NULL , NULL,  9, " +
                //                                                    budgetyear.ToString() + "," + idVersion.ToString() +
                //                                                    "," + idPublicLegalFormation.ToString() + " )");
                //(" + vs.ToString() + "," + idProgram + "," + "-1543503847 )");
                IEnumerable<DataSetStateProgPass> pres = (from sum in st
                                                          join prog in
                                                              context.Program.Where(
                                                                  s =>
                                                                  s.IdPublicLegalFormation == idPublicLegalFormation
                                                                  && s.IdVersion == idVersion
                                                              ) on sum.IdProgram equals prog.Id

                                                          select new DataSetStateProgPass()
                                                          {
                                                              UnitDimension = "Объем финансирования за счет иных источников составляет  ",
                                                              Typeprog = "e",
                                                              Year = sum.Year,
                                                              AmountOfCash = sum.AmountOfCash,
                                                              Viewprog =
                                                                  ((prog.IdDocType == -1543503843)
                                                                       ? "ГОСУДАРСТВЕННОЙ ПРОГРАММЫ"
                                                                       : ((prog.IdDocType == -1543503841)
                                                                              ? "ВЕДОМСТВЕННОЙ ЦЕЛЕВОЙ ПРОГРАММЫ"
                                                                              : ((prog.IdDocType == -1543503837)
                                                                                     ? "ДОЛГОСРОЧНОЙ ЦЕЛЕВОЙ ПРОГРАММЫ"
                                                                                     : ((prog.IdDocType == -1543503842)
                                                                                            ? "ПОДПРОГРАММЫ"
                                                                                            : "")
                                                                                )
                                                                         )
                                                                  ) //Вид программы
                                                          });

                IEnumerable<DataSetStateProgPass> tempres = null;
                decimal? str = null;
                if (pres.Any())
                {
                    tempres = pres.GroupBy(x => new { x.IdProgram })
                                                                    .Select(x => new
                                                                                     DataSetStateProgPass
                                                                    {
                                                                        IdProgram = x.Key.IdProgram,
                                                                        AmountOfCash =
                                                                            x.Sum(u => u.AmountOfCash) / 1000
                                                                    });
                    str = tempres.Select(s => s.AmountOfCash).First();
                }

                IEnumerable<DataSetStateProgPass> res =
                    pres.GroupBy(x => new { x.IdProgram, x.Viewprog, x.Year, x.Typeprog, x.UnitDimension })
                        .Select(x => new
                                         DataSetStateProgPass
                        {
                            UnitDimension = x.Key.UnitDimension,
                            Typeprog = x.Key.Typeprog,
                            Year = x.Key.Year,
                            IdProgram = x.Key.IdProgram,
                            Viewprog = x.Key.Viewprog,
                            AmountOfCash = x.Sum(u => u.AmountOfCash) / 1000
                        }).ToList();
                foreach (var f in res)
                    f.UnitDimension = f.UnitDimension + str.Value.ToString("### ### ### ### ##0.#") + " тыс. рублей, в том числе:";

                return res;
            }
            else
            {
                DataContext context = IoC.Resolve<DbContext>().Cast<DataContext>();
                //основная цель
                //var vs = context.AttributeOfProgram.Where(s => s.IdProgram == idProgram).Select(d => d.IdGoalSystemElement).FirstOrDefault();

                var budgetyear =
                    context.Budget.Where(s => s.Id == idBudget)
                           .Select(d => d.Year)
                           .FirstOrDefault();

                var query = "";
                switch (idSourcesDataReports)
                {
                    case 0:
                        query = "SELECT * FROM [sbor].[ProgramPassport16_20_ApprovedData] (" + idProgram + "," + "NULL , NULL , 9, 4 , NULL , " + budgetyear.ToString() + "," + idVersion.ToString() + "," + idPublicLegalFormation.ToString() + ",'" + DateReport.ToString() + "' )";
                        break;
                    case 2:
                        query = "SELECT * FROM [sbor].[ProgramPassportSBP16_20_ApprovedData] (" + idProgram + "," + "NULL , NULL , 9,  9 , NULL ," + budgetyear.ToString() + "," + idVersion.ToString() + "," + idPublicLegalFormation.ToString() + ",'" + DateReport.ToString() + "' )";
                        break;
                    case 3:
                        query = "SELECT * FROM [sbor].[ProgramPassport16_20_ApprovedData] (" + idProgram + "," + "NULL , NULL , 9,  4 , 10 ," + budgetyear.ToString() + "," + idVersion.ToString() + "," + idPublicLegalFormation.ToString() + ",'" + DateReport.ToString() + "' )";
                        break;
                    case 4:
                        query = "SELECT * FROM [sbor].[ProgramPassportSBP16_20_ApprovedData] (" + idProgram + "," + "NULL , NULL , 9,  9 , 11 ," + budgetyear.ToString() + "," + idVersion.ToString() + "," + idPublicLegalFormation.ToString() + ",'" + DateReport.ToString() + "' )";
                        break;
                }

                var st = context.Database.SqlQuery<DataSetStateProgPass>(query);

                //var st =
                //   context.Database.SqlQuery<DataSetStateProgPass>("SELECT * FROM [sbor].[ProgramPassport16_20_ApprovedData] (" +
                //                                                   idProgram + "," + "NULL , NULL, 9, " +
                //                                                   budgetyear.ToString() + "," + idVersion.ToString() +
                //                                                   "," + idPublicLegalFormation.ToString() + ",'" + DateReport.ToString() + "' )");
                //(" + vs.ToString() + "," + idProgram + "," + "-1543503847 )");
                IEnumerable<DataSetStateProgPass> pres = (from sum in st
                                                          join prog in
                                                              context.Program.Where(
                                                                  s =>
                                                                  s.IdPublicLegalFormation == idPublicLegalFormation
                                                                  && s.IdVersion == idVersion
                                                              ) on sum.IdProgram equals prog.Id

                                                          select new DataSetStateProgPass()
                                                          {
                                                              UnitDimension = "Объем финансирования за счет иных источников составляет ",
                                                              Typeprog = "e",
                                                              Year = sum.Year,
                                                              AmountOfCash = sum.AmountOfCash,
                                                              Viewprog =
                                                                  ((prog.IdDocType == -1543503843)
                                                                       ? "ГОСУДАРСТВЕННОЙ ПРОГРАММЫ"
                                                                       : ((prog.IdDocType == -1543503841)
                                                                              ? "ВЕДОМСТВЕННОЙ ЦЕЛЕВОЙ ПРОГРАММЫ"
                                                                              : ((prog.IdDocType == -1543503837)
                                                                                     ? "ДОЛГОСРОЧНОЙ ЦЕЛЕВОЙ ПРОГРАММЫ"
                                                                                     : ((prog.IdDocType == -1543503842)
                                                                                            ? "ПОДПРОГРАММЫ"
                                                                                            : "")
                                                                                )
                                                                         )
                                                                  ) //Вид программы
                                                          });

                IEnumerable<DataSetStateProgPass> tempres = null;
                decimal? str = null;
                if (pres.Any())
                {
                    tempres = pres.GroupBy(x => new { x.IdProgram })
                                                                    .Select(x => new
                                                                                     DataSetStateProgPass
                                                                    {
                                                                        IdProgram = x.Key.IdProgram,
                                                                        AmountOfCash =
                                                                            x.Sum(u => u.AmountOfCash) / 1000
                                                                    });
                    str = tempres.Select(s => s.AmountOfCash).First();
                }

                IEnumerable<DataSetStateProgPass> res =
                    pres.GroupBy(x => new { x.IdProgram, x.Viewprog, x.Year, x.Typeprog, x.UnitDimension })
                        .Select(x => new
                                         DataSetStateProgPass
                        {
                            UnitDimension = x.Key.UnitDimension,
                            Typeprog = x.Key.Typeprog,
                            Year = x.Key.Year,
                            IdProgram = x.Key.IdProgram,
                            Viewprog = x.Key.Viewprog,
                            AmountOfCash = x.Sum(u => u.AmountOfCash) / 1000
                        }).ToList();
                foreach (var f in res)
                    f.UnitDimension = f.UnitDimension + str.Value.ToString("### ### ### ### ##0.#") + " тыс. рублей, в том числе:";

                return res;
            }
        }


        public IEnumerable<DataSetStateProgPass> Resource()//все ресурсы по типу бюджетному (в отчете сделать групировку родительскую по виду программы и потомок по полю тайппрог и вывести детаилс по нему сумма плюс год)
        {
            var res1 = ResourceAll().ToList();
            var res2 = ResourceFederal().ToList();
            var res3 = ResourceRegional().ToList();
            var res4 = ResourceLocal().ToList();
            var res5 = AnotherResource().ToList();

            IEnumerable<DataSetStateProgPass> AllBudget =
            res1.GroupBy(x => new { x.IdProgram })
                       .Select(x => new
                                        DataSetStateProgPass
                       {
                           IdProgram = x.Key.IdProgram,
                           AmountOfCash = x.Sum(u => u.AmountOfCash) / 1000
                       }).ToList();

            IEnumerable<DataSetStateProgPass> RegionalBudget =
            res3.GroupBy(x => new { x.IdProgram })
                       .Select(x => new
                                        DataSetStateProgPass
                       {
                           IdProgram = x.Key.IdProgram,
                           AmountOfCash = x.Sum(u => u.AmountOfCash) / 1000
                       }).ToList();



            //var res = res1.Union(res2).Union(res3).Union(res4).Union(res5).ToList();
            var res = res1.ToList();
            if (AllBudget.Select(s => s.AmountOfCash).SingleOrDefault() ==
                RegionalBudget.Select(s => s.AmountOfCash).SingleOrDefault())
            {
                res = res3.ToList();
            }
            else
            {
                res = res1.Union(res2).Union(res3).Union(res4).Union(res5).ToList();
            }
            
            foreach (var f in res)
                f.AmountOfCashView = (f.AmountOfCash.HasValue ? " год - " + f.AmountOfCash.Value.ToString("### ### ### ### ##0.#") + " тыс. рублей" : "");
            if (!res.Any())
                res = ViewProgramType().ToList();

            return res;
        }

        //Ожидаемые конечные  результаты реализации программы
        public IEnumerable<DataSetStateProgPass> ExpectedOutComes()
        {
            if (buildReportApprovedData == false)
            {
                DataContext context = IoC.Resolve<DbContext>().Cast<DataContext>();
                var resume = context.Database.SqlQuery<DataSetStateProgPass>("SELECT * FROM [sbor].[ExpectedOutComes] (" + idProgram.ToString() + "," + idVersion.ToString() + ","+idPublicLegalFormation.ToString() + ")").ToList();//@idProgram int, @idVersion int, @idPublicLegalFormation int
                foreach (var f in resume)
                    f.Goalindicator = f.Goalindicator +
                                      (f.Value.HasValue ? " - " + f.Value.Value.ToString("0.#####") : "");
                if (!resume.Any())
                    resume = ViewProgramType().ToList();

                return resume;
            }
            else
            {
                DataContext context = IoC.Resolve<DbContext>().Cast<DataContext>();
                
                var resume = context.Database.SqlQuery<DataSetStateProgPass>("SELECT * FROM [sbor].[ExpectedOutComes_ApprovedData] (" + idProgram.ToString() + ",'" + DateReport.ToString() + "'," + idVersion.ToString() + "," + idPublicLegalFormation.ToString() + ")").ToList();//@idProgram int, @idVersion int, @idPublicLegalFormation int
                foreach (var f in resume)
                    f.Goalindicator = f.Goalindicator +
                                      (f.Value.HasValue ? " - " + f.Value.Value.ToString("0.#####") : "");
                if (!resume.Any())
                    resume = ViewProgramType().ToList();

                return resume;
            }

        }

        public IEnumerable<DataSetStateProgPass> DateReportConstruct()
        {
            if (DateReport.ToString() == "01.01.0001 0:00:00")
            {
                DataContext context = IoC.Resolve<DbContext>().Cast<DataContext>();
                var res1 = (from prg in
                                context.Program.Where(s => s.Id == idProgram
                                )
                            select new DataSetStateProgPass()
                                {
                                    Datestart = DateTime.Now
                                }).ToList();


                return res1;
            }
            else
            {
                DataContext context = IoC.Resolve<DbContext>().Cast<DataContext>();
                var pres = (from prg in
                                context.Program.Where(s => s.Id == idProgram
                                )
                            select new DataSetStateProgPass()
                            {
                                Datestart = DateReport  
                            }).ToList();


                return pres;
            }
            
        }

        private IEnumerable<DataSetStateProgPass> SubProgramList(int DocType) //получение списка подпрограмм различного типа
        {
            if (buildReportApprovedData == false)
            {
                DataContext context = IoC.Resolve<DbContext>().Cast<DataContext>();
                IEnumerable<DataSetStateProgPass> pres = (from aop in
                                                              context.AttributeOfProgram.Where(
                                                                  s => s.IdParent == idProgram
                                                                  && s.IdPublicLegalFormation == idPublicLegalFormation
                                                                  && s.IdVersion == idVersion
                                                                  && s.IdTerminator == null
                                                                  )
                                                          //получаем всех детей
                                                          join prg in
                                                              context.Program.Where(s => s.IdDocType == DocType
                                                                                         && s.IdPublicLegalFormation == idPublicLegalFormation
                                                                                         && s.IdVersion == idVersion
                                                                                         && s.IdTerminator == null)
                                                                  //получаем наименовани потомка программы если есть и тип «Подпрограмма ГП» или «Подпрограмма ДЦП»
                                                              on aop.IdProgram equals prg.Id
                                                          join ancode in context.AnalyticalCodeStateProgram on aop.IdAnalyticalCodeStateProgram equals ancode.Id

                                                          //LEFT JOIN [ref].[AnalyticalCodeStateProgram] AS VCPAnCode
                                                          //ON VCPAnCode.id = vcpom.idAnalyticalCodeStateProgram
                                                          join vsprg in context.Program on aop.IdParent equals vsprg.Id
                                                          //Вид программы

                                                          select new DataSetStateProgPass()
                                                          {
                                                              Subproglist = prg.Caption,
                                                              Viewprog =
                                                                  ((vsprg.IdDocType == -1543503843)
                                                                       ? "ГОСУДАРСТВЕННОЙ ПРОГРАММЫ"
                                                                       : ((vsprg.IdDocType == -1543503841)
                                                                              ? "ВЕДОМСТВЕННОЙ ЦЕЛЕВОЙ ПРОГРАММЫ"
                                                                              : ((vsprg.IdDocType == -1543503837)
                                                                                     ? "ДОЛГОСРОЧНОЙ ЦЕЛЕВОЙ ПРОГРАММЫ"
                                                                                     : ((vsprg.IdDocType ==
                                                                                         -1543503842)
                                                                                            ? "ПОДПРОГРАММЫ"
                                                                                            : "")
                                                                                )
                                                                         )
                                                                  ), //Вид программы
                                                              AnalyticCode = ancode.AnalyticalCode

                                                          });
                return pres.OrderBy(s => s.AnalyticCode);
            }
            else
            {
                DataContext context = IoC.Resolve<DbContext>().Cast<DataContext>();
                var pres1 = (from aop in
                                 context.AttributeOfProgram.Where(
                                     s => s.IdParent == idProgram
                                     && s.DateCommit <= DateReport
                                     && s.DateCommit != null
                                     && s.IdPublicLegalFormation == idPublicLegalFormation
                                     && s.IdVersion == idVersion
                                     && s.IdTerminator == null)
                             //получаем всех детей
                             join prg in
                                 context.Program.Where(s => s.IdDocType == DocType
                                                            && s.DateCommit <= DateReport
                                                            && s.DateCommit != null
                                                            && s.IdPublicLegalFormation == idPublicLegalFormation
                                                            && s.IdVersion == idVersion
                                                            && s.IdTerminator == null)
                                     //получаем наименовани потомка программы если есть и тип «Подпрограмма ГП» или «Подпрограмма ДЦП»
                                 on aop.IdProgram equals prg.Id

                             join ancode in context.AnalyticalCodeStateProgram on aop.IdAnalyticalCodeStateProgram equals ancode.Id

                             join vsprg in context.Program on aop.IdParent equals vsprg.Id
                             //Вид программы

                             select new DataSetStateProgPass()
                             {
                                 Subproglist = prg.Caption,
                                 Viewprog =
                                     ((vsprg.IdDocType == -1543503843)
                                          ? "ГОСУДАРСТВЕННОЙ ПРОГРАММЫ"
                                          : ((vsprg.IdDocType == -1543503841)
                                                 ? "ВЕДОМСТВЕННОЙ ЦЕЛЕВОЙ ПРОГРАММЫ"
                                                 : ((vsprg.IdDocType == -1543503837)
                                                        ? "ДОЛГОСРОЧНОЙ ЦЕЛЕВОЙ ПРОГРАММЫ"
                                                        : ((vsprg.IdDocType ==
                                                            -1543503842)
                                                               ? "ПОДПРОГРАММЫ"
                                                               : "")
                                                   )
                                            )
                                     ), //Вид программы
                                 AnalyticCode = ancode.AnalyticalCode

                             });
                return pres1.OrderBy(s => s.AnalyticCode);
            }
        }

        private IEnumerable<DataSetStateProgPass> ViewProgramType()//Получаем датасет с видом программы для заплонения пустыми полями отчета
        {
            DataContext context = IoC.Resolve<DbContext>().Cast<DataContext>();
            var resume = (from prg in context.Program.Where(s => s.Id == idProgram)
                          select new DataSetStateProgPass()
                              {
                                  Viewprog =
                                      ((prg.IdDocType == -1543503843)
                                           ? "ГОСУДАРСТВЕННОЙ ПРОГРАММЫ"
                                           : ((prg.IdDocType == -1543503841)
                                                  ? "ВЕДОМСТВЕННОЙ ЦЕЛЕВОЙ ПРОГРАММЫ"
                                                  : ((prg.IdDocType == -1543503837)
                                                         ? "ДОЛГОСРОЧНОЙ ЦЕЛЕВОЙ ПРОГРАММЫ"
                                                         : ((prg.IdDocType ==
                                                             -1543503842)
                                                                ? "ПОДПРОГРАММЫ"
                                                                : "")
                                                    )
                                             )
                                      )
                              });
            return resume;
        }


    }
}
