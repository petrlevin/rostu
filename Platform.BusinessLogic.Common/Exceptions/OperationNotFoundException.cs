using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Platform.Common.Exceptions;

namespace Platform.BusinessLogic.Common.Exceptions
{
    public class OperationNotFoundException :PlatformException
    {
        public OperationNotFoundException(string message) : base(message)
        {
            
        }
    }
}
