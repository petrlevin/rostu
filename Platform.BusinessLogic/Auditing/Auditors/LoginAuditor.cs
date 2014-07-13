using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Platform.BusinessLogic.Auditing.Auditors.Abstract;
using Platform.BusinessLogic.Auditing.Interfaces;
using Platform.Common;

namespace Platform.BusinessLogic.Auditing.Auditors
{
    public class LoginAuditor: AuditorBase, ISingleactionAuditor
    {
        public LoginAuditor()
        {
        }

        public LoginAuditor(string sessionId)
        {
            SessionId = sessionId;
        }

        public string SessionId { get; set; }

        public void Start(DateTime startedAt)
        {
            logger.Login(SessionId, startedAt);
        }
    }
}
