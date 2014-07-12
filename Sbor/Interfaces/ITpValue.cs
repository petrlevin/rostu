using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Platform.BusinessLogic.Reference;
using Platform.PrimaryEntities.Common.Interfaces;

namespace Sbor.Logic
{
    interface ITpValue : ITpWithHierarchyPeriod
    {
        /// <summary>
        /// Значение
        /// </summary>
        decimal Value { get; set; }
    }

    interface ITpValueNullable : ITpWithHierarchyPeriod
    {
        /// <summary>
        /// Значение
        /// </summary>
        decimal? Value { get; set; }
    }
}
