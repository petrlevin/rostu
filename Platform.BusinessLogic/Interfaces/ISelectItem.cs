using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Platform.PrimaryEntities.Reference;

namespace Platform.BusinessLogic.Interfaces
{
    public interface ISelectItem
    {
        Entity Entity { get; }
        String Sql{ get; }
    }
}
