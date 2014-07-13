using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Platform.Application.Common;

namespace Platform.BusinessLogic.StartupScripts
{
    /// <summary>
    /// http://stackoverflow.com/questions/12055511/transaction-scope-timeout-on-10-minutes
    /// http://blogs.msdn.com/b/ajit/archive/2008/06/18/override-the-system-transactions-default-timeout-of-10-minutes-in-the-code.aspx
    /// </summary>
    public class OverrideTransactionScopeMaximumTimeout : IBeforeAplicationStart
    {
        public void Execute()
        {
            // 1. create a object of the type specified by the fully qualified name
            Type oSystemType = typeof(global::System.Transactions.TransactionManager);
            System.Reflection.FieldInfo oCachedMaxTimeout = oSystemType.GetField("_cachedMaxTimeout", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
            System.Reflection.FieldInfo oMaximumTimeout = oSystemType.GetField("_maximumTimeout", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
            oCachedMaxTimeout.SetValue(null, true);
            oMaximumTimeout.SetValue(null, TimeSpan.FromHours(12));
        }
    }
}
