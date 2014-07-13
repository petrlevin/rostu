using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Platform.BusinessLogic.Auditing
{
    /// <summary>
    /// Когда проводить аудит (относительно транзакции)
    /// </summary>
    public enum AuditTime
    {
        /// <summary>
        /// Сейчас!
        /// </summary>
        Now,

        /// <summary>
        /// После завершения транзакции
        /// </summary>
        AfterTransaction,

        /// <summary>
        /// Стартовая часть - сейчас, окончательная - после завершения транзации
        /// </summary>
        NowWithCompleteAfterTransaction
    }
}
