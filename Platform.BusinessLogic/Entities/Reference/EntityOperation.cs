using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Platform.PrimaryEntities.Reference;

namespace Platform.BusinessLogic.Reference
{
    public partial class EntityOperation
    {
        public override string ToString()
        {
            if (Operation != null)
                return String.IsNullOrWhiteSpace(Operation.Caption) ? Operation.Name : Operation.Caption;
            else
                return String.Format("Операция id - {0} для {1} ", Id, Entity);
        }

    }
}
