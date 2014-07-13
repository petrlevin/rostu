using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Platform.Common.Exceptions;
using Platform.Common.Interfaces;

namespace Platform.BusinessLogic.Common.Exceptions
{
    public class ControlAccessException: PlatformException , IUserFriendlyException 
    {
        public ControlAccessException(string message) :base(message)
        {

        }

        public string OperationDescription
        {
            get { return "Редактирование контроля"; }
        }

        public string ExceptionTypeDescription
        {
            get { return ""; }
        }
    }
}
