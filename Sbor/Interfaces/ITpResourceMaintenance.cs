using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Platform.PrimaryEntities.Common.Interfaces;

namespace Sbor.Logic
{
    interface ITpResourceMaintenance : IIdentitied
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
        /// Источник
        /// </summary>
        int? IdFinanceSource { get; set; }
    }
}
