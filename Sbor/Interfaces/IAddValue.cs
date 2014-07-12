using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sbor.Interfaces
{
    interface IAddValue
    {
        /// <summary>
        /// Доп. потребность
        /// </summary>
        decimal? AdditionalValue { get; set; }
    }
}
