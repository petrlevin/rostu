using System;
using Microsoft.Data.Schema.ScriptDom.Sql;
using Platform.SqlObjectModel.Extensions;

namespace Platform.SqlObjectModel
{
	/// <summary>
	/// Класс реализующий построение выражения DELETE FROM [schema].[table] [alias]
	/// </summary>
	public class Delete : BaseDelete
	{
		/// <summary>
		/// Конструктор для построения выражения DELETE
		/// </summary>
		/// <param name="schemaName">Имя схемы, которой принадлежит таблица</param>
		/// <param name="tableName">Имя таблицы</param>
		/// <param name="aliasName">Алиас таблицы</param>
		public Delete(string schemaName, string tableName, string aliasName="")
		{
			if (string.IsNullOrEmpty(tableName))
				throw new ArgumentNullException("tableName", "Передан пустой tableName");

			SchemaName = schemaName;
			TableName = tableName;
			AliasName = aliasName;
		}

		/// <summary>
		/// Метод реализующий построение конструкции SELECT
		/// </summary>
		/// <returns>DeleteStatement</returns>
		public override DeleteStatement GetQuery()
		{
			DeleteStatement result;
			if (string.IsNullOrWhiteSpace(AliasName))
			{
				result = new DeleteStatement
				{
					Target =
						Helper.CreateSchemaObjectName(SchemaName, TableName).ToSchemaObjectDataModificationTarget()
				};
			}
			else
			{
				result = new DeleteStatement
				{
					Target =
						Helper.CreateSchemaObjectName(new string[] { AliasName }).ToSchemaObjectDataModificationTarget
							()
				};
				result.FromClauses.Add(Helper.CreateSchemaObjectTableSource(SchemaName, TableName, AliasName));
			}
			return result;
		}
	}
}
