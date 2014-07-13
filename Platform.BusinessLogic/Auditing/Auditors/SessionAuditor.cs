using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Platform.BusinessLogic.Auditing.Auditors.Abstract;
using Platform.BusinessLogic.Common.Interfaces;
using Platform.Common;
using IAuditLogger = Platform.BusinessLogic.Auditing.Interfaces.IAuditLogger;

namespace Platform.BusinessLogic.Auditing.Auditors
{
    public class SessionAuditor: AuditorBase
    {
        public override void Init()
        {
            logger = IoC.Resolve<IAuditLogger>("UserlessLogger");
            configuration = IoC.Resolve<AuditConfiguration>();
        }

        public void SessionStart(string sessionId, DateTime start)
        {
            logger.SessionStart(sessionId, start);
        }

        public void SessionEnd(string sessionId, DateTime end)
        {
            logger.SessionEnd(sessionId, end);
        }
    }
}
