using System;
using Platform.Common.Exceptions;
using Platform.Common.Interfaces;

namespace BaseApp.Common.Exceptions
{
    /// <summary>
    /// Исключение нарушения организационных прав
    /// </summary>
    public class OranizationalRightsException : PlatformException, IUserFriendlyException
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        public OranizationalRightsException(string message)
            : base(message)
        {

        }

       /// <summary>
       /// 
       /// </summary>
       /// <param name="message"></param>
       /// <param name="innerException"></param>
        public OranizationalRightsException(string message, Exception innerException)
            : base(message, innerException)
        {

        }


        /// <summary>
        /// Описание совершаемой операции
        /// </summary>
        public string OperationDescription
        {
            get { return "Организационные права"; }
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
