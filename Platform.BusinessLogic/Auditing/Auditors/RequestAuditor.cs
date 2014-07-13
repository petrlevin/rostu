using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Platform.BusinessLogic.Auditing.Auditors.Abstract;
using Platform.BusinessLogic.Auditing.Interfaces;
using Platform.BusinessLogic.Common.Interfaces;
using Platform.Common;

namespace Platform.BusinessLogic.Auditing.Auditors
{
    public class RequestAuditor: AuditorBase, IStartEndAuditor
    {
        public string MethodName { get; set; }
        public string JsonData { get; set; }
        public bool Disabled { get; set; }

        public override void Init()
        {
        }

        public void Start(DateTime startedAt)
        {
        }

        public void End(int elapsedMilliseconds)
        {
            if (Disabled) return;
            base.Init();
            if (!logger.IsRequestsEnabled) return;

            logger.Log(MethodName, elapsedMilliseconds, JsonData);
        }
    }
}
