using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Platform.Common.Exceptions;
using Platform.Common.Interfaces;

namespace Platform.BusinessLogic.Common.Exceptions
{
    public class SystemUFException: PlatformException , IHandledException
    {
        public SystemUFException(string message)
            : base(message)
        {
            
        }

        public string ClientHandler
        {
            get { return "System"; }
        }


    }
}
