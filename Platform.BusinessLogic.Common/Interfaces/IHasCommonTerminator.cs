using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Platform.BusinessLogic.Common.Interfaces;

namespace Platform.BusinessLogic.Common.Interfaces
{
    public interface IHasCommonTerminator : IHasTerminator
    {
        int? IdTerminatorEntity { get; set; }
        int? IdTerminateOperation { get; set; }

        /// <summary>
        /// Дата аннулирования
        /// </summary>
        DateTime? DateTerminate { get; set; }
    }
}
