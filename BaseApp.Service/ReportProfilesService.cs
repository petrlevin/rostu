using Platform.BusinessLogic.AppServices;
using Platform.BusinessLogic.DataAccess;

namespace BaseApp.Service
{
    /// <summary>
    /// Сервис для работы с профилями отчетов
    /// </summary>
    [AppService]
    public class ReportProfilesService : DataService
    {
        /// <summary>
        /// Получить список профилей
        /// </summary>
        /// <param name="param">Параметры списка</param>
        /// <returns></returns>
        public GridResult GetReportProfiles(GridParams param)
        {
            var result = new GridResult();
            
            var dataManager = GetDataManager(param.EntityId);
            AppResponse entityEntries = dataManager.GetEntityEntries(param, getFormFields(param) );
            
            result.Rows = entityEntries.Result;
            result.Count = entityEntries.Count;
            result.Aggregates = dataManager.GetAggregates(param, false);

            return result;
        }

        
    }
}