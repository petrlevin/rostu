using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Platform.BusinessLogic.DataAccess;

namespace Platform.BusinessLogic.Interfaces
{
    public interface ICreateUpdateObservable
    {
        event CreateUpdateHandler OnCreateUpdate;
    }
}
