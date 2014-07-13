using BaseApp.Rights.Functional;
using Platform.BusinessLogic.Activity.Operations;
using Platform.BusinessLogic.DataAccess;
using Platform.PrimaryEntities.Factoring;
using Platform.PrimaryEntities.Reference;

namespace BaseApp.Service.Common
{
    public abstract class DataAccessService : NeedRightsService
    {
		protected DataManager GetDataManager(Entity source)
		{
			return DataManagerFactory.Create(source);
		}

        protected DataManager GetDataManager(Entity source, Form form)
        {
            return DataManagerFactory.Create(source, form);
        }

		protected TDataManager GetDataManager<TDataManager>(Entity source) where TDataManager : DataManager
        {
            return DataManagerFactory.Create<TDataManager>(source);
		}

        protected DataManager GetDataManager(int entityId)
        {
            return GetDataManager(Objects.ById<Entity>(entityId));
        }

        protected DataManager GetDataManager(int entityId, Form form)
        {
            return GetDataManager(Objects.ById<Entity>(entityId), form);
        }

        protected DataManager GetDataManager(string entityName)
        {
            return GetDataManager(Objects.ByName<Entity>(entityName));
        }

        protected TDataManager GetDataManager<TDataManager>(int entityId) where TDataManager : DataManager
        {
            return GetDataManager<TDataManager>(Objects.ById<Entity>(entityId));
        }

        #region Проверка прав

        protected void ValidateExecuteFunctional(int entityId, int? entityOperationId)
        {
            var rm = GetRightsManager(entityId);
            var rh = RightsHolder.Define(entityId, null);
            if (entityOperationId.HasValue)
                rm.ValidateExecute(rh, entityOperationId.Value);
            else
            {
                Operations.BeforeCancelComplete += (op, doc) => rm.ValidateExecute(rh, op);
            }

        }

        protected void ValidateWriteFunctional(int entityId, int? ownerEntityId, int? itemId = null, DataManager dataManager = null)
        {

            var rh = RightsHolder.Define(entityId, ownerEntityId);
            var rm = GetRightsManager(entityId);
            rm.ValidateWrite(rh, itemId);
            if (dataManager != null)
                dataManager.CreateUpdate += (s, o, id) => rm.ValidateWrite(rh, id);
        }

        protected void ValidateWriteFunctional(int entityId, int? ownerEntityId, int[] itemIds, DataManager dataManager = null)
        {

            var rh = RightsHolder.Define(entityId, ownerEntityId);
            var rm = GetRightsManager(entityId);
            rm.ValidateWrite(rh, itemIds);
            if (dataManager != null)
                dataManager.CreateUpdate += (s, o, id) => rm.ValidateWrite(rh, id);
        }

        protected void ValidateReadFunctional(GridParams param)
        {
            GetRightsManager(param.EntityId).ValidateRead(RightsHolder.Define(param));
        }

        #endregion
    }
}