using System.Data.Entity;
using Platform.Common;
using Platform.PrimaryEntities.Common.Interfaces;
using Platform.Utils;

namespace Platform.BusinessLogic.Activity.Operations
{
    public class OperationWideContext : Stacked<OperationWideContext>
    {
        public OperationWideContext(IBaseEntity element)
        {
            OriginalTarget = (IBaseEntity)IoC.Resolve<DbContext>().Entry(element).OriginalValues.ToObject();
        }

        public IBaseEntity OriginalTarget { get; private set; }
    }
}