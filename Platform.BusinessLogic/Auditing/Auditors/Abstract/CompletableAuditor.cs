using System;
using Platform.BusinessLogic.Auditing.Interfaces;

namespace Platform.BusinessLogic.Auditing.Auditors.Abstract
{
    public class CompletableAuditor: AuditorBase, ICompletableAuditor
    {
        protected event Action<int> complete;

        public void Complete(int elapsedMiliseconds)
        {
            if (complete != null)
                complete(elapsedMiliseconds);
        }
    }
}
