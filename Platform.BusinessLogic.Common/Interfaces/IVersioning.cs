using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Platform.PrimaryEntities.Common.Interfaces;

namespace Platform.BusinessLogic.Common.Interfaces
{
    public interface IVersioning : IIdentitied
    {
        /// <summary>
        /// Дата начала действия
        /// </summary>
        DateTime? ValidityFrom { get; set; }

        /// <summary>
        /// Дата окончания действия
        /// </summary>
        DateTime? ValidityTo { get; set; }

        /// <summary>
        /// Cсылка на корень
        /// </summary>
        int? IdRoot { get; set; }

    }
}
