using Platform.BusinessLogic.Interfaces;
using Platform.BusinessLogic.Reference;
using Platform.PrimaryEntities.Common.Interfaces;

namespace BaseApp.Rights.Functional
{
    class SuperUserStrategy :StrategyBase
    {
        public override string AllowedRead(IEntity rightHolder,IEntity entity)
        {
            return null;
        }

        public override string AllowedEdit(IEntity rightHolder, IEntity entity, int[] itemIds=null)
        {
            return null;
        }

        public override string AllowedExecute(IEntity rightHolder, int entityOperationId)
        {
            return null;
        }

        public override string AllowedExecute(IEntity rightHolder, EntityOperation entityOperation)
        {
            return null;
        }
    }
}
