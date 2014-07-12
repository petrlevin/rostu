using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Platform.BusinessLogic;
using Platform.BusinessLogic.AppServices;
using Platform.ClientInteraction;
using Platform.ClientInteraction.Scopes;
using Platform.Common;
using Sbor.DbEnums;
using Sbor.Logic;
using Sbor.Tablepart;

namespace Sbor.Services
{
    /// <summary>
    /// Сервисы ЭД План финансово - хозяйственной деятельности
    /// </summary>
    [AppService]
    public class SborFinancialAndBusinessActivitiesService
    {
        public void fillData_FBA_UpdateIndirect(int id, int[] rows)
        {
            DataContext context = IoC.Resolve<DbContext>().Cast<DataContext>();

            var doc = context.FinancialAndBusinessActivities.Single(s => s.Id == id);

            doc.UpdateIndirect(context, rows);

            context.SaveChanges();
        }

        /// <summary>
        /// Кнопка заполнить на ТЧ "Мероприятия"
        /// </summary>
        /// <param name="id">id текущего документа</param>
        public void fillData_FBA_Activity(CommunicationContext communicationContext, int id)
        {
            using (new CommunicationContextScope(communicationContext))
            {
                DataContext context = IoC.Resolve<DbContext>().Cast<DataContext>();

                var doc = context.FinancialAndBusinessActivities.Single(s => s.Id == id);

                doc.FilltpActivity(context, true);

                context.SaveChanges();
            }
        }

        /// <summary>
        /// Получить по бланку СБП для текущего документа список скрываемых полей в гриде и форме сметной строки
        /// </summary>
        public Dictionary<string, IEnumerable<string>> get_Sbpblank(int idSbp, int idBudget)
        {
            DataContext context = IoC.Resolve<DbContext>().Cast<DataContext>();

            var sbpBlank = new SBP_Blank();

            var sbp = context.SBP.FirstOrDefault(s => s.Id == idSbp);
            if (sbp == null)
                return null;

            var parent = sbp.Parent;

            if (parent != null)
                sbpBlank = parent.SBP_Blank.FirstOrDefault(b => b.BlankType == BlankType.FormationAUBU && b.IdBudget == idBudget);

            return sbpBlank.GetBlankCostProperties();
        }

        /// <summary>
        /// Получить по бланку СБП для текущего документа список скрываемых полей в гриде и форме сметной строки
        /// </summary>
        public Dictionary<string, IEnumerable<string>> get_SbpblankByDocument(int idDocument)
        {
            DataContext context = IoC.Resolve<DbContext>().Cast<DataContext>();

            var doc = context.FinancialAndBusinessActivities.FirstOrDefault(l => l.Id == idDocument);
            if (doc == null)
                return null;

            var sbpBlank = context.SBP_BlankHistory.FirstOrDefault(r => r.Id == doc.IdSBP_BlankActual);
            if (sbpBlank == null)
            {
                return null;
            }

            return sbpBlank.GetBlankCostProperties();
        }

        public void fillData_FBA_FinancialIndicatorsInstitutions(int id)
        {
            DataContext context = IoC.Resolve<DbContext>().Cast<DataContext>();

            var doc = context.FinancialAndBusinessActivities.Single(s => s.Id == id);

            doc.FilltpFBAFinancialIndicatorsInstitutions(context);

            context.SaveChanges();
        }
    }
}
