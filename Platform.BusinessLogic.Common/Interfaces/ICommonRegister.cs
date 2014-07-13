using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Platform.BusinessLogic.Common.Interfaces;

namespace Platform.BusinessLogic.Common.Interfaces
{
    public interface ICommonRegister : IHasCommonRegistrator ,IHasCommonTerminator
    {

        /// <summary>
        /// Утверждающий документ
        /// </summary>
        int? IdApproved { get; set; }

        /// <summary>
        /// Утверждающий документ: тип документа
        /// </summary>
        int? IdApprovedEntity { get; set; }

        /// <summary>
        /// Дата утверждения
        /// </summary>
        DateTime? DateCommit { get; set; }

      
    }
}
