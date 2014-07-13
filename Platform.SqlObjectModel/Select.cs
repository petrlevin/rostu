using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Data.Schema.ScriptDom.Sql;

namespace Platform.SqlObjectModel
{
	/// <summary>
	/// Класс реализующий построение простого запроса вида SELECT [alias].[field1],..,[alias].[fieldN] FROM [schema].[table] [alias]
	/// </summary>
	public class Select : BaseSelect
	{
		/// <summary>
		/// Конструктор с заполнением минимально необходимого набора данных для формирования запроса SELECT
		/// </summary>
		/// <param name="fieldsName">Имена полей результирующего набора</param>
		/// <param name="schemaName">Имя схемы, которой принадлежит таблица</param>
		/// <param name="tableName">Имя таблицы</param>
		/// <param name="aliasName">Алиас присваиваемый таблице</param>
		public Select(IEnumerable<string> fieldsName, string schemaName, string tableName, string aliasName=null)
		{
			if (fieldsName==null )
				throw new ArgumentNullException("fieldsName", "Select: передан пустой fieldsName");
			if (string.IsNullOrWhiteSpace(tableName))
				throw new ArgumentNullException("tableName", "Select: передан пустой tableName");

			FieldsName = fieldsName;
			TableName = tableName;
			SchemaName = schemaName;
			AliasName = aliasName;
		}


        /// <summary>
        /// Конструктор
        /// </summary>
        /// <param name="fieldsName">Список полей, разделенных запятой</param>
		/// <param name="schemaName">Имя схемы</param>
		/// <param name="tableName">Имя таблицы</param>
		/// <param name="aliasName">Алиас присваиваемый таблице</param>
		public Select(string fieldsName, string schemaName, string tableName, string aliasName=null)
            : this(String.IsNullOrWhiteSpace(fieldsName)?new List<string>() : fieldsName.Split(',').ToList(),schemaName,tableName,aliasName)
        {
        }

		/// <summary>
		/// Метод реализующий построение конструкции SELECT
		/// </summary>
		/// <returns>SelectStatement</returns>
		public override SelectStatement GetQuery()
		{
			return new SelectStatement {QueryExpression = GetQuerySpecification()};
		}

        /// <summary>
        /// Метод реализующий построение конструкции SELECT
        /// </summary>
        /// <returns>QuerySpecification</returns>
        public QuerySpecification GetQuerySpecification()
        {
            List<TSqlFragment> fields = FieldsName.Select(s => Helper.CreateColumn(AliasName, s)).Cast<TSqlFragment>().ToList();
            QuerySpecification queryExpression = Helper.CreateQuerySpecification(
                new TableSource[] { Helper.CreateSchemaObjectTableSource(SchemaName, TableName, AliasName) }, fields, null, null,
                null, null);
            return queryExpression;
        }

	}
}
