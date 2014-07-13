using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Data.Schema.ScriptDom.Sql;
using Platform.SqlObjectModel.Extensions;

namespace Platform.SqlObjectModel
{
	/// <summary>
	/// Класс реализующий построение выражения INSERT INTO [schema].[table]
	/// </summary>
	public class Insert : BaseInsert
	{
		/// <summary>
		/// Конструктор с заполнением минимально необходимого набора данных для формирования INSERT
		/// </summary>
		/// <param name="fieldsName">Имена полей результирующего набора</param>
		/// <param name="schemaName">Имя схемы, которой принадлежит таблица</param>
		/// <param name="tableName">Имя таблицы</param>
		public Insert(List<string> fieldsName, string schemaName, string tableName)
		{
			if (fieldsName == null || !fieldsName.Any())
				throw new ArgumentNullException("fieldsName", "Передан пустой fieldsName");
			if (string.IsNullOrWhiteSpace(tableName))
				throw new ArgumentNullException("tableName", "Передан пустой tableName");
			
			FieldsName = fieldsName;
			SchemaName = schemaName;
			TableName = tableName;
		}

		/// <summary>
		/// Метод реализующий построение конструкции INSERT
		/// </summary>
		/// <returns>InsertStatement</returns>
		public override InsertStatement GetQuery()
		{
			InsertStatement result = new InsertStatement
				{
					Target = Helper.CreateSchemaObjectName(SchemaName, TableName).ToSchemaObjectDataModificationTarget(),
					InsertOption = InsertOption.Into
				};
			foreach (string field in FieldsName)
			{
				result.Columns.Add(field.ToColumn());
			}
			return result;
		}
	}
}
