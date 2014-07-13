using Platform.BusinessLogic.Interfaces;
using Platform.BusinessLogic.Reference;
using Platform.PrimaryEntities.Common.Interfaces;

namespace BaseApp.Rights.Functional
{
    public abstract class StrategyBase
    {
        public abstract string AllowedRead(IEntity rightHolder,IEntity entity);

        public abstract string AllowedEdit(IEntity rightHolder, IEntity entity, int[] itemIds=null);

        public abstract string AllowedExecute(IEntity rightHolder, int entityOperationId );

        public abstract string AllowedExecute(IEntity rightHolder, EntityOperation entityOperation);
    }
}
