using Platform.BusinessLogic.AppServices;
using Platform.Web.Services.DataProcessingService;

namespace Sbor.Services
{
    /// <summary>
    /// Сервис для обращения к обработкам. См. пространство имен Sbor.DataProcessors
    /// </summary>
    [AppService]
    public class DataProcessingService
    {
        /// <summary>
        /// Заполнение регистра «Объемы финансовых средств» по данным документа Деятельность ведомства 
        /// </summary>
        public void CreationConductingsLimitVolumeAppropriations()
        {
            var processor = new DataProcessors.ProcessingsActivityOfSBP();
            processor.CreationConductingsLimitVolumeAppropriations();
        }

        /// <summary>
        /// Округление документа «План финансово-хозяйственной деятельности»
        /// </summary>
        public void RoundFinancialAndBusinessActivities(int topDocs = -1, int idSbp = 0)
        {
            var processor = new RoundDocs();
            processor.RoundFinancialAndBusinessActivities(topDocs, idSbp);
        }

        /// <summary>
        /// Округление документа «Смета казенного учреждения»
        /// </summary>
        public void RoundPublicInstitutionEstimate(int topDocs = -1, int idSbp = 0)
        {
            var processor = new RoundDocs();
            processor.RoundPublicInstitutionEstimate(topDocs, idSbp);
        }

        /// <summary>
        /// Округление документа «План деятельности»
        /// </summary>
        public void RoundPlanActivity(int topDocs = -1, int idSbp = 0)
        {
            var processor = new RoundDocs();
            processor.RoundPlanActivity(topDocs, idSbp);
        }

        /// <summary>
        /// Округление документа «Предельные объемы бюджетных ассигнований»
        /// </summary>
        public void RoundLimitBudgetAllocations(int topDocs = -1, int idSbp = 0)
        {
            var processor = new RoundDocs();
            processor.RoundLimitBudgetAllocations(topDocs, idSbp);
        }

        /// <summary>
        /// Округление документа «Деятельность ведомства»
        /// </summary>
        public void RoundActivityOfSBP(int topDocs, int idSbp)
        {
            var processor = new RoundDocs();
            processor.RoundActivityOfSBP(topDocs, idSbp);
        }

        /// <summary>
        /// Округление документа «Деятельность ведомства»
        /// </summary>
        public string Test(int a, int b)
        {
            return "result";
        }
    }
}
