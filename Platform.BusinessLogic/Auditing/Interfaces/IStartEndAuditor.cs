using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Platform.BusinessLogic.Auditing.Interfaces
{
    public interface IStartEndAuditor : ISingleactionAuditor
    {
        /// <summary>
        /// Завершение операции
        /// </summary>
        /// <param name="elapsedMilliseconds"></param>
        void End(int elapsedMilliseconds);
    }
}
