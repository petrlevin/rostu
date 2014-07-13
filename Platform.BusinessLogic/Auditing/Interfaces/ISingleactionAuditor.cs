using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Platform.BusinessLogic.Auditing.Interfaces
{
    public interface ISingleactionAuditor : IAuditor
    {
        /// <summary>
        /// Старт операции
        /// </summary>
        /// <param name="startedAt"></param>
        void Start(DateTime startedAt);
    }
}
