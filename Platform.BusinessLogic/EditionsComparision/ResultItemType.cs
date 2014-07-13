using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Platform.BusinessLogic.EditionsComparision
{
    public enum ResultItemType
    {
        Added,
        Deleted,
        Changed,
        Unchanged,
        
        /// <summary>
        /// Строки различны (может быть это удаленная в наборе А строка с одной стороны и добавленная в набор Б с другой).
        /// </summary>
        Different
    }
}
