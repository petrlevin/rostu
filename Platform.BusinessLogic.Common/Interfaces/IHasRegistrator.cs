using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Platform.PrimaryEntities.Common.Interfaces;

namespace Platform.BusinessLogic.Common.Interfaces
{
    public interface IHasRegistrator :IIdentitied
    {
        int IdRegistrator { get; set; }
    }
}
