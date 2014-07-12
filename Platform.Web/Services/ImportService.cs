using System;
using System.Data.Entity;
using System.Linq;
using BaseApp.Service.Common;
using Newtonsoft.Json;
using Platform.BusinessLogic;
using Platform.BusinessLogic.Activity.Controls;
using Platform.BusinessLogic.Activity.Controls.DispatcherStrategies;
using Platform.BusinessLogic.Common.Interfaces;
using Platform.BusinessLogic.ServerFilters;
using Platform.Common;
using Platform.PrimaryEntities.Common.Interfaces;

namespace Platform.Web.Services
{
	public class ServerResponse
	{
		public bool success;
		public string msg;
	}

	public class ImportService : DataAccessService
	{
        public string Import(byte[] file, long templateId, int? idOwner, string dependenciesStr, int? ignor, string fileType)
		{
            var db = IoC.Resolve<DbContext>().Cast<DataContext>();
			var template = db.TemplateImportXLS.First(f => f.Id == templateId);
			if (template == null)
				throw new Exception("Ошибка импорта! Отсутствует шаблон импорта");

            var dependencies = FieldValues.FromString(dependenciesStr);

		    using (new ControlScope<SkipSkippableStrategy>(()=>template.IsIgnoreSoftControl , ScopeOptions.ApplyOnlyDispatching))
		    {
		        try
		        {

		            var dataManager = this.GetDataManager(template.IdEntity);
                    var import = new BaseApp.Import.Import(file, template, idOwner, dependencies, ignor ?? 0, dataManager, fileType);
		            string resultMsg = import.Process();
		            var result = new ServerResponse
		                             {
		                                 success = true,
		                                 msg = resultMsg
		                             };

		            return JsonConvert.SerializeObject(result);
		        }
		        catch (Exception ex)
		        {
		            var resultMsg = "&lt;b&gt;Ошибка!&lt;/b&gt; Произошла ошибка импорта :" + ex.Message;
		            var result = new ServerResponse
		                             {
		                                 success = false,
		                                 msg = resultMsg
		                             };
		            return JsonConvert.SerializeObject(result);
		        }
		    }
		}

	}
}