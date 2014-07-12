using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Platform.BusinessLogic;
using Platform.BusinessLogic.ReportingServices.PrintForms;
using Platform.Common;
using Sbor.Reports.PrintForms.PlanActivity;

namespace Sbor.Reports.PrintForms.PlanActivity
{
    class PlanActivityTask_Service
    {
        private readonly DataContext context;
        private readonly int IdDoc;

        public PlanActivityTask_Service(int id, int idOwner, int idContingent, int idIndicatorActivity, int idActivity)
        {
            context = IoC.Resolve<DbContext>().Cast<DataContext>();
            IdDoc = idOwner;
        }

        public List<BudgetLevel> BudgetLevel()
                {
                    var doc = context.PlanActivity.FirstOrDefault(s => s.Id == IdDoc);

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
    }
}
