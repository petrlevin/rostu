using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Platform.BusinessLogic;
using Platform.BusinessLogic.ReportingServices.PrintForms;
using Platform.Common;

namespace Sbor.Reports.PrintForms.PublicInstitutionEstimate
{

    /// <summary>
    /// Для просмотра: http://localhost/platform3/Services/PrintForm.aspx?entityName=StateProgram&printFormClassName=StateProgramPF&docId=-1275068397
    /// </summary>
    [PrintForm(Caption = "Смета казенного учреждения")]
    public class PublicInstitutionEstimatePF : PrintFormBase 
    {
        public PublicInstitutionEstimatePF(int docId) : base(docId)
        {
            //context = IoC.Resolve<DbContext>().Cast<DataContext>();
        }

        public List< DataSetPIE > ActJust()
        {
            DataContext context = IoC.Resolve<DbContext>().Cast<DataContext>();
            List<DataSetPIE> pres = (
                                        from pie in context.PublicInstitutionEstimate.Where(s => s.Id == DocId)//документ сметы
                                        join piea in context.PublicInstitutionEstimate_Activity on pie.Id equals piea.IdOwner //тч мероприятие
                                        join act in context.Activity on piea.IdActivity equals act.Id //справочник мероприятия

                                        join jpieexp in context.PublicInstitutionEstimate_Expense on piea.Id equals jpieexp.IdMaster into tmjpieexp//тч рассходы
                                        from pieexp in tmjpieexp.DefaultIfEmpty()
                                        
                                        join budget in context.Budget on pie.IdBudget equals budget.Id 
                                        join sbp in context.SBP on pie.IdSBP equals sbp.Id 
                                        join org in context.Organization on sbp.IdOrganization equals org.Id 

                                        join jpieexpkos in context.PublicInstitutionEstimate_Expense.Where(s => s.IsIndirectCosts == true) on piea.Id equals jpieexpkos.Id
                                        into tmjpieexpkos
                                        from pieexpkos in tmjpieexpkos.DefaultIfEmpty()//Косвенный ОФГ

                                        join jpieexpstr in context.PublicInstitutionEstimate_Expense.Where(s => s.IsIndirectCosts == false) on piea.Id equals jpieexpstr.Id
                                        into tmjpieexpstr
                                        from pieexpstr in tmjpieexpstr.DefaultIfEmpty()//не косвенный ОФГ

                                        join jkosgu in context.KOSGU on pieexp.IdKOSGU equals jkosgu.Id into tmjkosgu
                                        from kosgu in tmjkosgu.DefaultIfEmpty()//справочник КОСГУ
                                        
                                        join jkvsr in context.KVSR on pieexp.IdKVSR equals jkvsr.Id into tmjkvsr
                                        from kvsr in tmjkvsr.DefaultIfEmpty()

                                        join jrzpr in context.RZPR on pieexp.IdRZPR equals jrzpr.Id into tmjrzpr
                                        from rzpr in tmjrzpr.DefaultIfEmpty()

                                        join jkcsr in context.KCSR on pieexp.IdKCSR equals jkcsr.Id into tmjkcsr
                                        from kcsr in tmjkcsr.DefaultIfEmpty()
 
                                        join jkvr in context.KVR on pieexp.IdKVR equals jkvr.Id into tmjkvr
                                        from kvr in tmjkvr.DefaultIfEmpty()

                                        //-------------------------

                                        join jkfo in context.KFO on pieexp.IdKFO equals jkfo.Id into tmjkfo
                                        from kfo in tmjkfo.DefaultIfEmpty()

                                        join jdkr in context.DKR on pieexp.IdDKR equals jdkr.Id into tmjdkr
                                        from dkr in tmjdkr.DefaultIfEmpty()

                                        join jdfk in context.DFK on pieexp.IdDFK equals jdfk.Id into tmjdfk
                                        from dfk in tmjdfk.DefaultIfEmpty()

                                        join jdek in context.DEK on pieexp.IdDEK equals jdek.Id into tmjdek
                                        from dek in tmjdek.DefaultIfEmpty()

                                        join jcodesybs in context.CodeSubsidy on pieexp.IdCodeSubsidy equals jcodesybs.Id into tmjcodesybs
                                        from codesybs in tmjcodesybs.DefaultIfEmpty()

                                        join jbranchcode in context.BranchCode on pieexp.IdCodeSubsidy equals jbranchcode.Id into tmjbranchcode
                                        from branchcode in tmjbranchcode.DefaultIfEmpty()


                                        select new DataSetPIE()
                                            {
        Id = pie.Id,//индетификатор документа
        Activity = act.Caption, //Наименование мероприятия
        Expense = kosgu.Caption, //Наименование расходов
        KVSR = kvsr.Caption,//КВСР
        RZPR = rzpr.Code,//РзПР
        KCSR =  kcsr.Code,//КЦСР
        KVR = kvr.Code, //КВР
        KOSGU = kosgu.Code, //КОСГУ
        KFO = kfo.Code, //КФО
        DKR = dkr.Code,  //ДКР
        DFK = dfk.Code, //ДФК
        DEK = dek.Code, //ДЭК
        Codesybs = codesybs.Code,  //Код субсидии
        Branchcode = branchcode.Code, //Отраслевой код
        Budgetyear = budget.Year, //год бюджета
        Numbersmet = pie.Number,  //Номер сметы из одкумента
        Datesmet = pie.Date, //дата сметы из одкумента
        institution = org.Caption, //Получатель бюджетных средств (учреждение сметы из одкумента)

        //OfgbvAll = (
        //(pieexp.IsIndirectCosts.HasValue && pieexp.IsIndirectCosts.Value) ? 0 : pieexp.OFG
        //            ), //OFGBVAll Очередной финансовый год - Базовый вариант - Всего
        OfgbvAll = pieexp.OFG,

        OfgbvIndirectCosts = (
                                (pieexp.IsIndirectCosts.HasValue && pieexp.IsIndirectCosts.Value) ? pieexp.OFG : 0
                             ), //OFGBVIndirectCosts Очередной финансовый год - Базовый вариант - В т.ч. косвенные расходы
        //OfgdpAll = (
        //            (pieexp.IsIndirectCosts.HasValue && pieexp.IsIndirectCosts.Value) ? 0 : pieexp.AdditionalOFG
        //            ), //OFGDPAll Очередной финансовый год - Доп. потребность - Всего 
        OfgdpAll = pieexp.AdditionalOFG,
        
        OfgdpIndirectCosts = (
                                (pieexp.IsIndirectCosts.HasValue && pieexp.IsIndirectCosts.Value) ? pieexp.AdditionalOFG : 0
                                ), //OFGDPIndirectCosts Очередной финансовый год - Доп. потребность - В т.ч. косвенные расходы
        //Ofg1BvAll = (
        //            (pieexp.IsIndirectCosts.HasValue && pieexp.IsIndirectCosts.Value) ? 0 : pieexp.PFG1
        //            ), // Плановый период  - Базовый вариант - Всего 
        Ofg1BvAll = pieexp.PFG1,

        Ofg1BvIndirectCosts = (
                                (pieexp.IsIndirectCosts.HasValue && pieexp.IsIndirectCosts.Value) ? pieexp.PFG1 : 0
                                ), // Плановый период  - Базовый вариант - В т.ч. косвенные расходы
        //Ofg1DpAll = (
        //            (pieexp.IsIndirectCosts.HasValue && pieexp.IsIndirectCosts.Value) ? 0 : pieexp.AdditionalPFG1
        //            ), // Плановый период  - Доп. потребность - Всего 
        Ofg1DpAll = pieexp.AdditionalPFG1,

        Ofg1DpIndirectCosts = (
                                (pieexp.IsIndirectCosts.HasValue && pieexp.IsIndirectCosts.Value) ? pieexp.AdditionalPFG1 : 0
                                ), // Плановый период  - Доп. потребность - В т.ч. косвенные расходы
        //Ofg2BvAll = (
        //            (pieexp.IsIndirectCosts.HasValue && pieexp.IsIndirectCosts.Value) ? 0 : pieexp.PFG2
        //            ),// Плановый период  - Базовый вариант - Всего 
        Ofg2BvAll = pieexp.PFG2,

        Ofg2BvIndirectCosts = (
                                (pieexp.IsIndirectCosts.HasValue && pieexp.IsIndirectCosts.Value) ? pieexp.PFG2 : 0
                                ), // Плановый период  - Базовый вариант - В т.ч. косвенные расходы
        //Ofg2DpAll = (
        //            (pieexp.IsIndirectCosts.HasValue && pieexp.IsIndirectCosts.Value) ? 0 : pieexp.AdditionalPFG2
        //            ),// Плановый период  - Доп. потребность - Всего 
        Ofg2DpAll = pieexp.AdditionalPFG2,

        Ofg2DpIndirectCosts = (
                                (pieexp.IsIndirectCosts.HasValue && pieexp.IsIndirectCosts.Value) ? 0 : pieexp.AdditionalPFG2
                                )  // Плановый период  - Доп. потребность - В т.ч. косвенные расходы
        //косвенные расходы isIndirectCosts
                                            }
                                    ).ToList();


            List<DataSetPIE> res = pres.GroupBy(x => new {x.Id, x.Activity, x.Expense, x.KVSR, x.RZPR, x.KCSR, x.KVR, x.KOSGU, x.KFO, x.DKR, x.DFK, x.DEK, x.Codesybs, x.Branchcode, x.Budgetyear, x.Numbersmet, x.Datesmet, x.institution }).Select(x => new 
                DataSetPIE
            {
                Id = x.Key.Id,//индетификатор сметы
                Activity = x.Key.Activity , //Наименование мероприятия
                Expense = x.Key.Expense, //Наименование расходов
                KVSR = x.Key.KVSR,//КВСР
                RZPR = x.Key.RZPR,//РзПР
                KCSR = x.Key.KCSR,//КЦСР
                KVR = x.Key.KVR, //КВР
                KOSGU = x.Key.KOSGU, //КОСГУ
                KFO = x.Key.KFO, //КФО
                DKR = x.Key.DKR,  //ДКР
                DFK = x.Key.DFK, //ДФК
                DEK = x.Key.DEK, //ДЭК
                Codesybs = x.Key.Codesybs,  //Код субсидии
                Branchcode = x.Key.Branchcode, //Отраслевой код
                Budgetyear = x.Key.Budgetyear, //год бюджета
                Numbersmet = x.Key.Numbersmet,  //Номер сметы из одкумента
                Datesmet = x.Key.Datesmet, //дата сметы из одкумента
                institution = x.Key.institution, //Получатель бюджетных средств (учреждение сметы из одкумента)
                OfgbvAll = x.Sum(u => u.OfgbvAll),
                OfgbvIndirectCosts = x.Sum(t => t.OfgbvIndirectCosts),
                OfgdpAll = x.Sum(t => t.OfgdpAll),
                OfgdpIndirectCosts = x.Sum(t => t.OfgdpIndirectCosts),
                Ofg1BvAll = x.Sum(t => t.Ofg1BvAll),
                Ofg1BvIndirectCosts = x.Sum(t => t.Ofg1BvIndirectCosts),
                Ofg1DpAll = x.Sum(t => t.Ofg1DpAll),
                Ofg1DpIndirectCosts = x.Sum(t => t.Ofg1DpIndirectCosts),
                Ofg2BvAll = x.Sum(t => t.Ofg2BvAll),
                Ofg2BvIndirectCosts = x.Sum(t => t.Ofg2BvIndirectCosts),
                Ofg2DpAll = x.Sum(t => t.Ofg2DpAll),
                Ofg2DpIndirectCosts = x.Sum(t => t.Ofg2DpIndirectCosts)
            }
                 ).ToList();
            return res;
        }

        public List<DataSetPIE> ActAuBu()
        {
            DataContext context = IoC.Resolve<DbContext>().Cast<DataContext>();
            List<DataSetPIE> pres = (
                                        from pie in context.PublicInstitutionEstimate.Where(s => s.Id == DocId)
                                        join piea in context.PublicInstitutionEstimate_ActivityAUBU on pie.Id equals piea.IdOwner //тч мероприятие АУ БУ
                                        join act in context.Activity on piea.IdActivity equals act.Id //справочник мероприятия
                                        
                                        join jpieexp in context.PublicInstitutionEstimate_FounderAUBUExpense on piea.Id equals jpieexp.IdMaster into tmjpieexp//ТЧ «Расходы учредителя по мероприятиям АУ/БУ»
                                        from pieexp in tmjpieexp.DefaultIfEmpty()


                                        join jkosgu in context.KOSGU on pieexp.IdKOSGU equals jkosgu.Id into tmjkosgu
                                        from kosgu in tmjkosgu.DefaultIfEmpty()//справочник КОСГУ

                                        join jkvsr in context.KVSR on pieexp.IdKVSR equals jkvsr.Id into tmjkvsr
                                        from kvsr in tmjkvsr.DefaultIfEmpty()

                                        join jrzpr in context.RZPR on pieexp.IdRZPR equals jrzpr.Id into tmjrzpr
                                        from rzpr in tmjrzpr.DefaultIfEmpty()

                                        join jkcsr in context.KCSR on pieexp.IdKCSR equals jkcsr.Id into tmjkcsr
                                        from kcsr in tmjkcsr.DefaultIfEmpty()

                                        join jkvr in context.KVR on pieexp.IdKVR equals jkvr.Id into tmjkvr
                                        from kvr in tmjkvr.DefaultIfEmpty()

                                        //-------------------------

                                        join jkfo in context.KFO on pieexp.IdKFO equals jkfo.Id into tmjkfo
                                        from kfo in tmjkfo.DefaultIfEmpty()

                                        join jdkr in context.DKR on pieexp.IdDKR equals jdkr.Id into tmjdkr
                                        from dkr in tmjdkr.DefaultIfEmpty()

                                        join jdfk in context.DFK on pieexp.IdDFK equals jdfk.Id into tmjdfk
                                        from dfk in tmjdfk.DefaultIfEmpty()

                                        join jdek in context.DEK on pieexp.IdDEK equals jdek.Id into tmjdek
                                        from dek in tmjdek.DefaultIfEmpty()

                                        join jcodesybs in context.CodeSubsidy on pieexp.IdCodeSubsidy equals jcodesybs.Id into tmjcodesybs
                                        from codesybs in tmjcodesybs.DefaultIfEmpty()

                                        join jbranchcode in context.BranchCode on pieexp.IdCodeSubsidy equals jbranchcode.Id into tmjbranchcode
                                        from branchcode in tmjbranchcode.DefaultIfEmpty()


                                        select new DataSetPIE()
                                        {
                                            Id = pie.Id,//индетификатор документа
                                            Activity = act.Caption, //Наименование мероприятия
                                            Expense = kosgu.Caption, //Наименование расходов
                                            KVSR = kvsr.Caption,//КВСР
                                            RZPR = rzpr.Code,//РзПР
                                            KCSR = kcsr.Code,//КЦСР
                                            KVR = kvr.Code, //КВР
                                            KOSGU = kosgu.Code, //КОСГУ
                                            KFO = kfo.Code, //КФО
                                            DKR = dkr.Code,  //ДКР
                                            DFK = dfk.Code, //ДФК
                                            DEK = dek.Code, //ДЭК
                                            Codesybs = codesybs.Code,  //Код субсидии
                                            Branchcode = branchcode.Code, //Отраслевой код
                                            OfgbvAll = pieexp.OFG, //OFGBVAll Очередной финансовый год - Базовый вариант - Всего 
                                            //нету косвенных OfgbvIndirectCosts = pieexpkos.OFG, //OFGBVIndirectCosts Очередной финансовый год - Базовый вариант - В т.ч. косвенные расходы
                                            OfgdpAll = pieexp.AdditionalOFG, //OFGDPAll Очередной финансовый год - Доп. потребность - Всего 
                                            //нету косвенных OfgdpIndirectCosts = pieexpkos.AdditionalOFG, //OFGDPIndirectCosts Очередной финансовый год - Доп. потребность - В т.ч. косвенные расходы
                                            Ofg1BvAll = pieexp.PFG1, // Плановый период  - Базовый вариант - Всего 
                                            //нету косвенных Ofg1BvIndirectCosts = pieexpkos.PFG1, // Плановый период  - Базовый вариант - В т.ч. косвенные расходы
                                            Ofg1DpAll = pieexp.AdditionalPFG1, // Плановый период  - Доп. потребность - Всего 
                                            //нету косвенных Ofg1DpIndirectCosts = pieexpkos.AdditionalPFG1, // Плановый период  - Доп. потребность - В т.ч. косвенные расходы
                                            Ofg2BvAll = pieexp.PFG2, // Плановый период  - Базовый вариант - Всего 
                                            //нету косвенных Ofg2BvIndirectCosts = pieexpkos.PFG2, // Плановый период  - Базовый вариант - В т.ч. косвенные расходы
                                            Ofg2DpAll = pieexp.AdditionalPFG2 // Плановый период  - Доп. потребность - Всего 
                                            //нету косвенных Ofg2DpIndirectCosts = pieexpkos.AdditionalPFG2  // Плановый период  - Доп. потребность - В т.ч. косвенные расходы
                                            //косвенных расходы isIndirectCosts
                                        }
                                    ).ToList();
            return pres;
        }

        public List<DataSetPIE> SumAllAct()
        {
            List<DataSetPIE> pres = ActAuBu();
            List<DataSetPIE> pres1 = ActJust();
            var presgrbst = pres.Union(pres1).GroupBy(x => x.Id).Select(x => new
                DataSetPIE
                {
                    OfgbvAll = x.Sum(u => u.OfgbvAll),
                    OfgbvIndirectCosts = x.Sum(t => t.OfgbvIndirectCosts),
                    OfgdpAll = x.Sum(t=>t.OfgdpAll),
                    OfgdpIndirectCosts = x.Sum(t=>t.OfgdpIndirectCosts),
                    Ofg1BvAll = x.Sum(t=>t.Ofg1BvAll),
                    Ofg1BvIndirectCosts = x.Sum(t=>t.Ofg1BvIndirectCosts),
                    Ofg1DpAll = x.Sum(t=>t.Ofg1DpAll),
                    Ofg1DpIndirectCosts = x.Sum(t=>t.Ofg1DpIndirectCosts),
                    Ofg2BvAll = x.Sum(t=>t.Ofg2BvAll),
                    Ofg2BvIndirectCosts = x.Sum(t=>t.Ofg2BvIndirectCosts),
                    Ofg2DpAll = x.Sum(t=>t.Ofg2DpAll),
                    Ofg2DpIndirectCosts = x.Sum(t=>t.Ofg2DpIndirectCosts)
                 }
                 ).ToList();
            return presgrbst;
        }

        //Mainbk { get; set; }// Глава по БК
        //public string Grbs
        public List<DataSetPIE> MainbkGrbs()
        {
            DataContext context = IoC.Resolve<DbContext>().Cast<DataContext>();
            //var tsbp =
            //    context.SBP.Where(s => context.PublicInstitutionEstimate != null ? s.Id == context.PublicInstitutionEstimate.Single(t => t.Id == DocId) : false);
            List<DataSetPIE> pres = (
                                        from pie in context.PublicInstitutionEstimate.Where(s => s.Id == DocId)
                                        join sbp in context.SBP on pie.IdSBP equals sbp.Id
                                        join kvsr in context.KVSR on sbp.IdKVSR equals kvsr.Id
                                        join org in context.Organization on sbp.IdOrganization equals org.Id 
                                        select new DataSetPIE()
                                            {
                                                Budgetyear = sbp.IdSBPType,
                                                Grbs = org.Caption, //возможно грбс если IdSBPType = 1
                                                Mainbk = kvsr.Caption, //Глава по БК  если IdSBPType = 1
                                                Idsbpparent = sbp.IdParent  
                                            }
                                            ).ToList();
            foreach (var t in pres)
            {
                if (t.Budgetyear != 1)
                {
                    List<DataSetPIE> presp = (
                                        from sbp in context.SBP.Where(z=>z.Id == t.Idsbpparent) 
                                        join kvsr in context.KVSR on sbp.IdKVSR equals kvsr.Id
                                        join org in context.Organization on sbp.IdOrganization equals org.Id
                                        select new DataSetPIE()
                                        {
                                            Budgetyear = sbp.IdSBPType,
                                            Grbs = org.Caption, //возможно грбс если IdSBPType = 1
                                            Mainbk = kvsr.Caption, //Глава по БК  если IdSBPType = 1
                                            Idsbpparent = sbp.IdParent
                                        }
                                            ).ToList();
                    return presp;
                }
            }
            return pres;
        }


    }
}
