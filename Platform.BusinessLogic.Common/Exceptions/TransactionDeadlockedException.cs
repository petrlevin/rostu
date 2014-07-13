using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Platform.Common.Exceptions;
using Platform.Common.Interfaces;

namespace Platform.BusinessLogic.Common.Exceptions
{
    public class TransactionDeadlockedException : PlatformException, IHandledException
    {
        public string ClientHandler
        {
            get { return "System"; }
        }

        public TransactionDeadlockedException(string message,Exception inner) :base(message,inner)
        {
            
        }

        public TransactionDeadlockedException(string message)
            : base(message)
        {

        }


    }
}
