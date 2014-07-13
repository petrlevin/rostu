using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Platform.BusinessLogic.Common.Interfaces;

namespace Platform.BusinessLogic.Common.Interfaces
{
    public interface IHasCommonRegistrator :IHasRegistrator
    {
        int IdRegistratorEntity { get; set; }
    }
}
