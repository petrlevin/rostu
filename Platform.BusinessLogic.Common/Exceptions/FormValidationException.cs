using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Platform.Common.Exceptions;
using Platform.Common.Interfaces;

namespace Platform.BusinessLogic.Common.Exceptions
{
    public class FormValidationException: PlatformException , IUserFriendlyException
    {
        private readonly string _exceptionTypeDescription;

        public FormValidationException(string exceptionTypeDescription , string message) :base (message)
        {
            _exceptionTypeDescription = exceptionTypeDescription;
        }

        public string OperationDescription
        {
            get { return  "Валидация формы"; }
        }

        public string ExceptionTypeDescription
        {
            get { return _exceptionTypeDescription; }
        }
    }
}
