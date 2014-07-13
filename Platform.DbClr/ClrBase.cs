using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Text;
using Platform.DbCmd;

namespace Platform.DbClr
{
	/// <summary>
	/// Базовый класс для SP, UDF и триггеров.
	/// Содержит helper-методы
	/// </summary>
	public abstract class ClrBase
	{
		public ClrBase()
		{
			sqlCmd = new SqlCmd(SqlCmd.ContextConnection, ConnectionType.ConnectionPerCommand);
		}

		/// <summary>
		/// Объект для выполнения команд на SQLServer'е
		/// </summary>
		protected SqlCmd sqlCmd;
	}
}
