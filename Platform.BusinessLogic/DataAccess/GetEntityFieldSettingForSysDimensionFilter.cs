using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Platform.Common;
using Platform.Log;
using Platform.PrimaryEntities.Factoring;
using Platform.PrimaryEntities.Reference;

namespace Platform.BusinessLogic.DataAccess
{
	public static class GetEntityFieldSettingForSysDimensionFilter
	{
		/// <summary>
		/// Соединение с БД
		/// </summary>
		private static readonly SqlConnection _connection = IoC.Resolve<SqlConnection>("DbConnection");

		/// <summary>
		/// Сущность Filter
		/// </summary>
		private static readonly Entity _entity = Objects.ByName<Entity>("EntityFieldSetting");

		private const string _textCommand = "select COUNT(1) from ref.EntityFieldSetting WHERE idEntityField={0} and IgnoreFilterBy{1}=1";

		public static bool Get(int idEntityField, string sysDimensionName)
		{
			bool result = false;
			using (SqlCommand command = new SqlCommand(string.Format(_textCommand, idEntityField, sysDimensionName), _connection))
			{
				if (_connection.State!=ConnectionState.Open)
				{
					_connection.Open();
				}
				object res = command.ExecuteScalarLog();
				result = Convert.ToBoolean(res);
			}
			return result;
		}
	}
}
