using System;
using System.Data.SqlClient;
using Microsoft.SqlServer.Server;
using Platform.DbClr;
using Platform.DbCmd;
using Platform.PrimaryEntities.Common.DbEnums;
using Platform.PrimaryEntities.DbEnums;
using Platform.PrimaryEntities.Reference;

namespace BaseApp.DbClr.Procedures
{
	/// <summary>
	/// Класс описывающий процедуру создания или обновления индекса
	/// </summary>
	public class CreateOrAlterIndex
	{
		/// <summary>
		/// Экземпляр SqlCmd для выполненения команд
		/// </summary>
		private static readonly SqlCmd _sqlCmd = new SqlCmd(new SqlConnection("context connection = true"), ConnectionType.ConnectionPerCommand);

		/// <summary>
		/// Реализация процедуры
		/// </summary>
		/// <param name="idIndex"></param>
		public static void Exec(int idIndex)
		{
			Index index;
			try
			{
				index = Objects.ById<Index>(idIndex);

			}
			catch
			{
				SqlContext.Pipe.Send(string.Format("Ошибка при получении индекса с идентифкатором '{0}'", idIndex));
				throw new Exception("Ошибка");
			}
			if (index.Status == RefStatus.Work)
			{
				_dropIndex(index);
				_createIndex(index);
			}
		}

		/// <summary>
		/// Удаление индекса
		/// </summary>
		private static void _dropIndex(Index index)
		{
			_sqlCmd.ExecuteNonQuery(
				string.Format(
					"IF EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[{1}].[{2}]') AND name = N'{0}') DROP INDEX [{0}] ON [{1}].[{2}] WITH ( ONLINE = OFF )",
					index.Name, index.Entity.Schema,
					index.Entity.Name));
		}

		/// <summary>
		/// Создание индекса
		/// </summary>
		private static void _createIndex(Index index)
		{
			if ((index.IndexType != IndexType.NonuniqueIndex && index.IndexType != IndexType.UniqueIndex) || (index.IndexType == IndexType.UniqueIndex && index.Entity.IsVersioning))
				return;
			string isUnique = index.IndexType == IndexType.UniqueIndex ? "UNIQUE" : "";
			string isClustered = index.IsClustered ? "CLUSTERED" : "NONCLUSTERED";
			string whereCondition = "";
			if (!string.IsNullOrEmpty(index.Filter))
				whereCondition = string.Format("WHERE ({0})", index.Filter);
			
			string fieldsName = string.Join(",", _sqlCmd.SelectOneColumn<string>(
				"select '['+b.Name+'] ASC' from ml.Index_EntityField_Indexable a inner join ref.EntityField b on b.id=a.idEntityField WHERE idIndex={0} order by a.idEntityFieldOrder",
				index.Id).ToArray());
			
			string includedFieldsName = string.Join(",", _sqlCmd.SelectOneColumn<string>(
				"select '['+b.Name+']' from ml.Index_EntityField_Included a inner join ref.EntityField b on b.id=a.idEntityField WHERE idIndex={0}",
				index.Id).ToArray());

			string included = string.IsNullOrEmpty(includedFieldsName) ? "" : string.Format(" INCLUDE ({0}) ", includedFieldsName);

			if (!string.IsNullOrEmpty(fieldsName))
			{
				_sqlCmd.ExecuteNonQuery(string.Format("CREATE {0} {1} INDEX [{2}] ON [{3}].[{4}] ({5}) {6} {7}",
													 isUnique, isClustered, index.Name, index.Entity.Schema, index.Entity.Name,
													 fieldsName, included, whereCondition));
			}
		}

	}
}
