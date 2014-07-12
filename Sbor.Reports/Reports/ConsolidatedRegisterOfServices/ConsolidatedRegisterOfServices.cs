using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BaseApp.Reference;
using Platform.BusinessLogic.Common.Interfaces;
using Platform.BusinessLogic;
using Platform.BusinessLogic.ReportingServices.Reports;
using Platform.Common;
using System.Globalization;
using Sbor.Document;
using Sbor.Reference;
using Sbor.Registry;
using Sbor.Reports.ConsolidatedRegisterOfServices;


namespace Sbor.Reports.Report
{
    [Report]
    public class ConsolidatedRegisterOfServices
    {
        public int id { get; set; }
        public string Caption { get; set; }
        public int idPublicLegalFormation { get; set; } //ППО
        public DateTime DateReport { get; set; } //Дата отчета
        public bool RepeatTableHeader { get; set; } //Повторять заголовки  таблиц на каждой странице

        public List<PrimeDataSet> MainData()
        {
            DataContext context = IoC.Resolve<DbContext>().Cast<DataContext>();

            var res = new List<PrimeDataSet>();
            var CaptionReport = string.Empty;

            var PPO = context.PublicLegalFormation.Single(r => r.Id == idPublicLegalFormation);

            res.Add(new PrimeDataSet()
            {
                level = PPO.BudgetLevel.Id == -1879048162 ? "государственной" : "муниципальной",
                curDate = DateTime.Now.ToShortDateString()
            });


            return res;
        }

        public List<TableSet> TableData()
        {
            var res = new List<TableSet>();

            DateReport = new DateTime(DateReport.Year, DateReport.Month, DateReport.Day, 23, 59, 59);

            DataContext context = IoC.Resolve<DbContext>().Cast<DataContext>();
  
            // Находим первые документы в цепочке удовлетворяющие условиям
            var pRA = context.RegisterActivity.Where(r => r.IdPublicLegalFormation == idPublicLegalFormation &&
                                                                            r.IdDocType == -1543503849 && 
                                                                            //r.IsApproved == true && 
                                                                            !r.IdParent.HasValue &&
                                                                            r.Date <= DateReport ).ToList();
            TableSet newTableSet;

            foreach (var RAp in pRA)
            {
                // пытаемся найти последний документ потомок удовлетворяющий условиям

                var doc = context.RegisterActivity.FirstOrDefault(i => i.Id == RAp.Id);

                RegisterActivity children = null;
                if (doc.IsApproved == true && doc.Date <= DateReport)
                {
                    children = doc;
                }

                do
                {
                    var child = context.RegisterActivity.Where(c => c.IdParent == doc.Id).ToList();
                    if (child.Any())
                    {
                        doc = child.FirstOrDefault();
                        if (doc.IsApproved == true && doc.Date <= DateReport)
                        {
                            children = doc;
                        }
                    }
                    else
                    {
                        break;
                    }
                } while (true);

                if (children==null)
                {
                    continue;
                };


                var vSBP = context.SBP.SingleOrDefault(s => s.Id == children.IdSBP);
                var vOrganization = vSBP.Organization.Description;

                var tActivity = context.RegisterActivity_Activity.Where(w => w.IdOwner == children.Id).ToList();

                foreach (var rActivity in tActivity)
                {
                    // Перечень мероприятий

                    var sActivity = context.Activity.SingleOrDefault(A => A.Id == rActivity.IdActivity);

                    // Исполнители - Организация
                    var tPerformers =
                        context.RegisterActivity_Performers.Where(wp => wp.IdOwner == children.Id && wp.IdMaster == rActivity.Id).ToList();

                    var vPerformers  = "";

                    foreach (var rPerformerse in tPerformers)
                    {
                        var sPerformerse = context.Organization.SingleOrDefault(wo => wo.Id == rPerformerse.Performers);
                        vPerformers = vPerformers + sPerformerse.Description + ";\n";
                    }

                    // НПА,
                    var vRegulatoryAct = "";

                    var tActivity_RegulatoryAct = context.Activity_RegulatoryAct.Where(w => w.IdOwner == rActivity.IdActivity && w.IsBasis == true && w.IdSBP == children.IdSBP).ToList();

                    foreach (var rRegulatoryAct in tActivity_RegulatoryAct)
                    {
                        var sRegulatoryAct =
                            context.RegulatoryAct.SingleOrDefault(sRA => sRA.Id == rRegulatoryAct.IdRegulatoryAct);
                        var sTypeRegulatoryAct =
                            context.TypeRegulatoryAct.SingleOrDefault(tra => tra.Id == sRegulatoryAct.IdTypeRegulatoryAct);
                        vRegulatoryAct = vRegulatoryAct + sTypeRegulatoryAct.Caption + " № " + sRegulatoryAct.Number +
                                            " от " + sRegulatoryAct.Date.ToShortDateString() + " \"" + sRegulatoryAct.Caption + "\" ;\n";
                    }

 
                    // Контингент

                    var rContingent = context.Contingent.SingleOrDefault(sCon => sCon.Id == rActivity.IdContingent);

                    newTableSet = new TableSet()
                    {
                        sn3 = vOrganization,
                        sn4 = sActivity == null ? "" : sActivity.FullCaption,
                        sn5 = vPerformers,
                        sn6 = vRegulatoryAct,
                        sn7 = rContingent== null ? "" :rContingent.Caption
                    };
                    res.Add(newTableSet);
                    
                }
            }

            return res;
        }

    }
}
