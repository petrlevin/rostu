using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Platform.BusinessLogic.Exceptions
{
    /// <summary>
    /// Общее исключение
    /// </summary>
    public class CommonUserFrendlyException : Platform.Common.Exceptions.PlatformException, Platform.Common.Interfaces.IUserFriendlyException
    {
        public CommonUserFrendlyException(string message)
            : base(message)
        {

        }

        /// <summary>
        /// Описание совершаемой операции
        /// </summary>
        public string OperationDescription
        {
            get { return "Ошибка"; }
        }

        /// <summary>
        /// Описание типа ошибки
        /// </summary>
        public string ExceptionTypeDescription
        {
            get { return ""; }
        }
    }
}
