using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Platform.BusinessLogic.Common.Enums;
using Platform.PrimaryEntities.Common.Interfaces;
using Platform.Unity.Common;

namespace Platform.BusinessLogic.Common.Interfaces
{
    [AutoRegistration]
    public interface IBeforeEntityCUD
    {
        void OnInsert(IEntity entity, Dictionary<string, object> values);
        void OnUpdate(IEntity entity, int itemsId, Dictionary<string, object> values);
        void OnDelete(IEntity entity, IEnumerable<Int32> itemsIds);
    }
}
