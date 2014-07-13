using System.Data.Entity;
using BaseApp.Common.Interfaces;
using BaseApp.Rights.Functional;
using Platform.Common;
using Platform.PrimaryEntities.Factoring;
using Platform.PrimaryEntities.Reference;

namespace BaseApp.Service.Common
{
    public abstract class NeedRightsService
    {
        protected RightsManager GetRightsManager(int entityId)
        {
            return new RightsManager(IoC.Resolve<IUser>("CurrentUser"), IoC.Resolve<DbContext>(), Objects.ById<Entity>(entityId));
        }
    }
}