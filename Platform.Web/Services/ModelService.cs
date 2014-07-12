using BaseApp.Rights.Functional;
using BaseApp.Service.Common;
using Platform.BusinessLogic.DataAccess;

namespace Platform.Web.Services
{
    /// <summary>
    /// Методы для получения информации о модели сущности
    /// </summary>
    public class ModelService : NeedRightsService
	{
		/// <summary>
		/// Получить модель для сущности
		/// </summary>
		/// <param name="entityId">id сущности, модель которой запросили</param>
		/// <param name="ownerEntityId">id сущности, в которой находится поле, для которого пошел запрос</param>
        /// <returns><see cref="ModelAppResponse"/></returns>
		public object GetEntityModel(int entityId, int? ownerEntityId)
		{
            var manager = new ModelManager();
			ModelAppResponse result =  manager.GetEntityModel(entityId, ownerEntityId);
            result.ReadOnly = !GetRightsManager(entityId).AllowedEdit(RightsHolder.Define(entityId, ownerEntityId));
		    return result;
		}
	}
}