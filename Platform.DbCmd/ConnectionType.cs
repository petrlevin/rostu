using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Platform.DbCmd
{
	/// <summary>
	/// Тип соединения
	/// </summary>
	public enum ConnectionType
	{
		/// <summary>
		/// Для каждой исполняемой команды открывать отдельное соединение. После выполнения команды - закрывать.
		/// </summary>
		ConnectionPerCommand,

		/// <summary>
		/// Использовать одно соединение, пока существует объект <see cref="SqlCmd"/>.
		/// </summary>
		SingleConnection
	}
}
