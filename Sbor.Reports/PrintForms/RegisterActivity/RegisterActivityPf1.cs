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
using Platform.Common.Extensions;
using Sbor.DbEnums;
using Sbor.Reference;
using Sbor.Tablepart;

namespace Sbor.Reports.PrintForms.RegisterActivity
{
    public class DataSetDoc1
    {
        public string fullName { get; set; }
        public string s1 { get; set; }
        public string s2 { get; set; }
        public string s3 { get; set; }
        public string s4 { get; set; }
    }

    public class DataSetMain1
    {
        public int Id { get; set; }
        public string CapType { get; set; }
        public string CapActivity { get; set; }
        public string CapContingent { get; set; }
        public string CapPaidType { get; set; }
        public string CapIndicatorVolume { get; set; }
        public string CapVolumeUnit { get; set; }
        public string CapIndicator { get; set; }
        public string CapUnit { get; set; }
        public string Performers { get; set; }
    }

    /// <summary>
    /// Для просмотра: http://localhost/platform3/Services/PrintForm.aspx?entityName=RegisterActivity&printFormClassName=RegisterActivityPf1&docId=-1811939300
    /// </summary>
    [PrintForm(Caption = "Ведомственный реестр услуг (работ)")]
    public class RegisterActivityPf1 : PrintFormBase
    {
        public RegisterActivityPf1(int docId) : base(docId) { }

        public List<DataSetDoc1> DataSetDoc()
        {
            DataContext context = IoC.Resolve<DbContext>().Cast<DataContext>();

            return context.RegisterActivity.Where(s => s.Id == DocId).Select(s => new DataSetDoc1 {
                fullName = (s.SBP == null ? "" : (s.SBP.Organization.Description ?? s.SBP.Organization.Caption)),
                s1 = (s.PublicLegalFormation.IdBudgetLevel == BudgetLevel.SubjectRF ? "государственных" : "муниципальных"),
                s2 = (s.PublicLegalFormation.IdBudgetLevel == BudgetLevel.SubjectRF ? "государственной" : "муниципальной"),
                s3 = (s.PublicLegalFormation.IdBudgetLevel == BudgetLevel.SubjectRF ? "государственную" : "муниципальную"),
                s4 = (s.PublicLegalFormation.IdBudgetLevel == BudgetLevel.SubjectRF ? "Государственные" : "Муниципальные")
            }).ToList();
        }

        public List<DataSetMain1> DataSetMain()
        {
            DataContext context = IoC.Resolve<DbContext>().Cast<DataContext>();

            var doc = context.RegisterActivity.Single(s => s.Id == DocId);

            string s4 = (doc.PublicLegalFormation.IdBudgetLevel == BudgetLevel.SubjectRF
                             ? "Государственные"
                             : "Муниципальные");

            List<DataSetMain1> res = new List<DataSetMain1>();

            var q1 =
                context.RegisterActivity_Activity.Where(w =>
                    w.IdOwner == DocId &&
                    (w.Activity.IdActivityType == (byte) ActivityType.Service ||
                     w.Activity.IdActivityType == (byte) ActivityType.Work))
                       .Select(s => new
                           {
                               s.Id,
                               s.Activity,
                               s.Contingent,
                               s.IndicatorActivity_Volume,
                               s.Activity.IdActivityType
                           })
                       .ToList()
                       .Select(s => new
                           {
                               Id = s.Id,
                               CapType = (s.IdActivityType == (byte)ActivityType.Service ? "I.	" + s4 + " услуги" : "II.	" + s4 + " работы"),
                               CapActivity = s.Activity.Caption,
                               CapContingent = (s.Contingent == null ? "" : s.Contingent.Caption),
                               CapIndicatorVolume = s.IndicatorActivity_Volume.Caption,
                               CapVolumeUnit = s.IndicatorActivity_Volume.UnitDimension.Caption,
                               CapPaidType = s.Activity.PaidType.Caption()
                           })
                       .ToList();

            var q2 =
                context.RegisterActivity_Performers.Where(w => w.IdOwner == DocId)
                       .Select(s => new {s.IdMaster, s.Rformers.Caption})
                       .Distinct()
                       .ToList()
                       .GroupBy(g => g.IdMaster)
                       .Select(a => new
                           {
                               Id = a.Key ?? 0,
                               Performers = a.Select(k => k.Caption).OrderBy(o => o).Aggregate((c, d) => c + "; \r\n" + d)
                           }).ToList();
            q2.AddRange(
                q1.Select(s => s.Id)
                  .Distinct()
                  .Where(w => !q2.Any(a => a.Id == w))
                  .Select(k => new {Id = k, Performers = (string) null}));

            var q3 =
                context.RegisterActivity_IndicatorActivity.Where(r => r.IdOwner == DocId)
                       .Select(s => new
                            {
                                s.Id,
                                CapIndicator = s.DicatorActivity.Caption,
                                CapUnit = s.DicatorActivity.UnitDimension.Caption
                            })
                       .ToList();
            q3.AddRange(
                q1.Select(s => s.Id)
                  .Distinct()
                  .Where(w => !q3.Any(a => a.Id == w))
                  .Select(k => new { Id = k, CapIndicator = (string)null, CapUnit = (string)null }));


            res = q1.Join(q2, a => a.Id, b => b.Id, (a, b) => new {a, b})
                    .Join(q3, a => a.a.Id, b => b.Id,
                          (a, b) => new DataSetMain1
                              {
                                  Id = a.a.Id,
                                  CapType = a.a.CapType,
                                  CapActivity = a.a.CapActivity,
                                  CapContingent = a.a.CapContingent,
                                  CapPaidType = a.a.CapPaidType,
                                  CapIndicatorVolume = a.a.CapIndicatorVolume,
                                  CapVolumeUnit = a.a.CapVolumeUnit,
                                  CapIndicator = b.CapIndicator,
                                  CapUnit = b.CapUnit,
                                  Performers = a.b.Performers
                              }).ToList();


            return res;
        }

    }
}
