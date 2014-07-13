using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Platform.BusinessLogic.Common.Interfaces;

namespace Platform.BusinessLogic.Common.Interfaces
{
    public interface IAddRegister : IValueRegister
    {
        /// <summary>
        /// Доп. потребность
        /// </summary>
        bool IsAdditionalNeed { get; set; }
    }
}
