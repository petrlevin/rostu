using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BaseApp.Reference;
using Platform.BusinessLogic;
using Platform.BusinessLogic.ReportingServices;
using Platform.BusinessLogic.ReportingServices.PrintForms;
using Platform.Common;
using Sbor.Reference;
using Sbor.Tablepart;

namespace Sbor.Reports.PrintForms.RegisterActivity
{
    public class DataSetDoc
    {
        public string fullName { get; set; }
        public string s1 { get; set; }
    }

    public class DataSetMain
    {
        public string CapActivity { get; set; }
        public string CapContingent { get; set; }
        public string CapIndicatorVolume { get; set; }
        public string CapUnit { get; set; }
        public string RegulatoryActs { get; set; }
        public string Performers { get; set; }
    }

    /// <summary>
    /// Для просмотра: http://localhost/platform3/Services/PrintForm.aspx?entityName=RegisterActivity&printFormClassName=RegisterActivityPf&docId=-1811939300
    /// </summary>
    [PrintForm(Caption = "Перечень публичных обязательств")]
    public class RegisterActivityPf : PrintFormBase
    {
        public RegisterActivityPf(int docId) : base(docId) { }

        public List<DataSetDoc> DataSetDoc()
        {
            DataContext context = IoC.Resolve<DbContext>().Cast<DataContext>();

            return context.RegisterActivity.Where(s => s.Id == DocId).Select(s => new DataSetDoc {
                fullName = (s.SBP == null ? "" : (s.SBP.Organization.Description ?? s.SBP.Organization.Caption)),
                s1 = (s.PublicLegalFormation.IdBudgetLevel == BudgetLevel.SubjectRF ? "государственных" : "муниципальных")
            }).ToList();
        }

        public List<DataSetMain> DataSetMain()
        {
            DataContext context = IoC.Resolve<DbContext>().Cast<DataContext>();

            var doc = context.RegisterActivity.Single(s => s.Id == DocId);

            List<DataSetMain> res = new List<DataSetMain>();

            res.AddRange(
                context.RegisterActivity_Activity.Where(w => w.IdOwner == DocId).ToList()
                       .Select(s => new
                           {
                               s.Activity,
                               s.Contingent,
                               s.IndicatorActivity_Volume,
                               RegulatoryActs =
                                        s.Activity.RegulatoryAct.Where(w => w.IdSBP == doc.IdSBP && (w.IsBasis ?? false))
                                         .Select(a => 
                                             a.RegulatoryAct.TypeRegulatoryAct.Caption +
                                             " от " + a.RegulatoryAct.Date.ToString("dd.MM.yyy") + " №" + a.RegulatoryAct.Number +
                                             " «" + a.RegulatoryAct.Caption + "»")
                                         .ToList(),
                               Performers =
                                        s.RegisterActivity_Performers.Select(a => a.Rformers.Caption)
                                         .Distinct()
                                         .OrderBy(o => o)
                                         .ToList()
                           })
                       .ToList()
                       .Select(s => new DataSetMain
                           {
                               CapActivity = s.Activity.Caption,
                               CapContingent = (s.Contingent == null ? "" : s.Contingent.Caption),
                               CapIndicatorVolume = s.IndicatorActivity_Volume.Caption,
                               CapUnit = s.IndicatorActivity_Volume.UnitDimension.Caption,
                               RegulatoryActs = string.Join("; \r\n", s.RegulatoryActs),
                               Performers = string.Join("; \r\n", s.Performers)
                           })
                       .ToList());

            return res;
        }

    }
}
