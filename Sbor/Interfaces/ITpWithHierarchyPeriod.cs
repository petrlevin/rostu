using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Platform.BusinessLogic.Reference;
using Platform.PrimaryEntities.Common.Interfaces;

namespace Sbor.Logic
{
    interface ITpWithHierarchyPeriod : IIdentitied
    {
        /// <summary>
        /// Ссылка на владельца
        /// </summary>
        int IdOwner { get; set; }

        /// <summary>
        /// Ссылка на главную ТЧ
        /// </summary>
        int IdMaster { get; set; }

        /// <summary>
        /// Год
        /// </summary>
        int IdHierarchyPeriod { get; set; }

        /// <summary>
        /// Период
        /// </summary>
        HierarchyPeriod HierarchyPeriod { get; set; }
    }
}
