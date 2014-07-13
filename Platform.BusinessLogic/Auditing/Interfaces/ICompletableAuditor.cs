using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Platform.BusinessLogic.Auditing.Interfaces
{
    public interface ICompletableAuditor
    {
        void Complete(int elapsedMiliseconds);
    }
}
