using System;
using Platform.Common.Exceptions;
using Platform.Common.Interfaces;

namespace BaseApp.Common.Exceptions
{
    /// <summary>
    /// Исключение нарушения функциональных прав
    /// </summary>
    public class FunctionalRightsException: PlatformException , IUserFriendlyException
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        public FunctionalRightsException(string message)
            : base(message)
        {

        }

       /// <summary>
       /// 
       /// </summary>
       /// <param name="message"></param>
       /// <param name="innerException"></param>
       public FunctionalRightsException(string message, Exception innerException)
            : base(message, innerException)
        {

        }

        /// <summary>
        /// Описание совершаемой операции
        /// </summary>
        public string OperationDescription
        {
            get { return "Функциональные права"; }
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
