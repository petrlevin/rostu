using Platform.Common.Interfaces;
using Platform.Common.Exceptions;

namespace Platform.BusinessLogic.Common.Exceptions
{
	/// <summary>
	/// Класс реализующий исключения
	/// </summary>
	public class DalSqlException : PlatformException, IUserFriendlyException
	{
		#region Implementation of IUserFriendlyException


		public string OperationDescription { get; private set; }
		public string ExceptionTypeDescription { get; private set; }

		#endregion

		private int _errorCode;

		private string _errorMessage;

		/// <summary>
		/// Конструктор
		/// </summary>
		/// <param name="exceptionTypeDescription">Описание типа ошибки</param>
		/// <param name="message">Сообщение</param>
		/// <param name="operationDescription">Описание совершаемой операции</param>
		public DalSqlException(string operationDescription, string exceptionTypeDescription, string message)
			: base(message)
		{

			OperationDescription = operationDescription;
			ExceptionTypeDescription = exceptionTypeDescription;
		}
	}
}
