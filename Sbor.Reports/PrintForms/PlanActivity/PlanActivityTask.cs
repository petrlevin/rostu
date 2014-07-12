using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Platform.BusinessLogic;
using Platform.BusinessLogic.ReportingServices.PrintForms;
using Platform.Common;

namespace Sbor.Reports.PrintForms.PlanActivity
{
    [PrintForm(Caption = "Государственное задание")]
    class PlanActivityTask : PrintFormBase
    {
        private readonly DataContext context;

        public PlanActivityTask(int docId) : base(docId)
        {
            context = IoC.Resolve<DbContext>().Cast<DataContext>();
        }

        public IEnumerable<PlanActivityInfo> PlanActivityInfo()
        {
            IEnumerable<PlanActivityInfo> header = context.PlanActivity.Where(s => s.Id == DocId).Select(s =>
                    new PlanActivityInfo
                    {
                        Id = s.Id,
                        SBP = s.SBP.Organization.Description,
                        Year = s.Budget.Year
                    });
            return header;
        }

        public List<BudgetLevel> BudgetLevel()
        {
            var doc = context.PlanActivity.FirstOrDefault(s => s.Id == DocId);

            var table = new List<BudgetLevel>();

            if (doc.PublicLegalFormation.BudgetLevel.Id == -1879048164 || doc.PublicLegalFormation.BudgetLevel.Id == -1879048162)

            {

                BudgetLevel BL = new BudgetLevel
                                       {
                                           BB = "ГОСУДАРСТВЕНН",
                                           ZB = "Государственн",
                                           MB = "государственн"
                                       };
               table.Add(BL);
            }

            else

            {
                BudgetLevel BL = new BudgetLevel
                                     {
                                         BB = "МУНИЦИПАЛЬН",
                                         ZB = "Муниципальн",
                                         MB = "муниципальн"
                                     };

                table.Add(BL);
            }

            return table;
        }

        public IEnumerable<PlanActivity_Activity> PlanActivity_Service()
        {

            var ReportQuery = new StringBuilder();

            ReportQuery.AppendFormat(@"select 
                    ROW_NUMBER() OVER ( ORDER BY idActivity
                        ) AS RowNumber, 
                    PAA.id,
                    PAA.idOwner,
                    PAA.idContingent, 
                    PAA.idIndicatorActivity, 
                    PAA.idActivity 
                from tp.PlanActivity_Activity as PAA
                inner join ref.Activity on ref.Activity.id = PAA.idActivity
                where idActivityType = 0 and idOwner = {0}", DocId);

            var dc = IoC.Resolve<DbContext>().Cast<DataContext>();

            return dc.Database.SqlQuery<PlanActivity_Activity>(ReportQuery.ToString()).ToList();
        }

        public IEnumerable<PlanActivity_Activity> PlanActivity_Work()
        {

            var ReportQuery = new StringBuilder();

            ReportQuery.AppendFormat(@"select 
                    ROW_NUMBER() OVER ( ORDER BY idActivity
                        ) AS RowNumber,
                    PAA.id ,
                    PAA.idOwner,
                    PAA.idContingent, 
                    PAA.idIndicatorActivity, 
                    PAA.idActivity 
                from tp.PlanActivity_Activity as PAA
                inner join ref.Activity on ref.Activity.id = PAA.idActivity
                where idActivityType = 1 and idOwner = {0}", DocId);

            var dc = IoC.Resolve<DbContext>().Cast<DataContext>();

            return dc.Database.SqlQuery<PlanActivity_Activity>(ReportQuery.ToString()).ToList();
        }

    }
}
