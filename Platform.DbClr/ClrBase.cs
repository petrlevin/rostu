using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Text;
using Platform.DbCmd;

namespace Platform.DbClr
{
	/// <summary>
	/// ������� ����� ��� SP, UDF � ���������.
	/// �������� helper-������
	/// </summary>
	public abstract class ClrBase
	{
		public ClrBase()
		{
			sqlCmd = new SqlCmd(SqlCmd.ContextConnection, ConnectionType.ConnectionPerCommand);
		}

		/// <summary>
		/// ������ ��� ���������� ������ �� SQLServer'�
		/// </summary>
		protected SqlCmd sqlCmd;
	}
}
