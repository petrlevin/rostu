using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Platform.Common.Interfaces
{
    public interface IUserFriendlyException : IHandledException
	{

		/// <summary>
		/// Описание совершаемой операции
		/// </summary>
		string OperationDescription { get; }

		/// <summary>
		/// Описание типа ошибки
		/// </summary>
		string ExceptionTypeDescription { get; }
	}
}
