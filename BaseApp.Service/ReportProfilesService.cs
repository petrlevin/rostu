using Platform.BusinessLogic.AppServices;
using Platform.BusinessLogic.DataAccess;

namespace BaseApp.Service
{
    /// <summary>
    /// ������ ��� ������ � ��������� �������
    /// </summary>
    [AppService]
    public class ReportProfilesService : DataService
    {
        /// <summary>
        /// �������� ������ ��������
        /// </summary>
        /// <param name="param">��������� ������</param>
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