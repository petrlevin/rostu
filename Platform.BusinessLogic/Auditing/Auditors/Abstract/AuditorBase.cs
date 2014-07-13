using System;
using Platform.BusinessLogic.Auditing.Interfaces;
using Platform.BusinessLogic.Common.Interfaces;
using Platform.Common;
using IAuditLogger = Platform.BusinessLogic.Auditing.Interfaces.IAuditLogger;

namespace Platform.BusinessLogic.Auditing.Auditors.Abstract
{
    public abstract class AuditorBase : IAuditor
    {
        protected IAuditLogger logger { get; set; }

        protected AuditConfiguration configuration { get; set; }

        public virtual void Init()
        {
            logger = IoC.Resolve<IAuditLogger>();
            configuration = IoC.Resolve<AuditConfiguration>();
        }
    }
}