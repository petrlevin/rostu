using System;
using Microsoft.Data.Schema.ScriptDom.Sql;
using Platform.SqlObjectModel.Extensions;

namespace Platform.SqlObjectModel
{
	/// <summary>
	/// Класс реализующий построение выражения вида UPDATE [schema].[table] [alias]
	/// </summary>
	public class Update : BaseUpdate
	{
		/// <summary>
		/// Конструктор для построения выражения UPDATE
		/// </summary>
		/// <param name="schemaName">Имя схемы таблицы</param>
		/// <param name="tableName">Имя таблицы</param>
		/// <param name="aliasName">Алиас таблицы </param>
		public Update(string schemaName, string tableName, string aliasName="")
		{
			if (string.IsNullOrWhiteSpace(tableName))
				throw new ArgumentNullException("tableName", "Передан пустой tableName");
			
			SchemaName = schemaName;
			TableName = tableName;
			AliasName = aliasName;
		}

		/// <summary>
		/// Метод реализующий построение конструкции UPDATE
		/// </summary>
		/// <returns>UpdateStatement</returns>
		public override UpdateStatement GetQuery()
		{
			UpdateStatement result;
			if (string.IsNullOrWhiteSpace(AliasName))
			{
				result = new UpdateStatement
					{
						Target =
							Helper.CreateSchemaObjectName(SchemaName, TableName).ToSchemaObjectDataModificationTarget()
					};
			} else
			{
				result = new UpdateStatement
					{
						Target =
							Helper.CreateSchemaObjectName(new string[] {AliasName}).ToSchemaObjectDataModificationTarget
								()
					};
				result.FromClauses.Add(Helper.CreateSchemaObjectTableSource(SchemaName, TableName, AliasName));
			}
			return result;
		}
	}
}
