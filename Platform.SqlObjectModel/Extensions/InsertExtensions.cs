using System;
using System.Collections.Generic;
using Platform.Utils.Collections;
using Microsoft.Data.Schema.ScriptDom.Sql;

namespace Platform.SqlObjectModel.Extensions
{
	/// <summary>
	/// Расширения для объекта InsertStatement
	/// </summary>
	public static class InsertExtensions
	{
		/// <summary>
		/// Реализация InsertSource как список параметров вида @fieldName
		/// </summary>
		/// <param name="insertStatement">Расширяемое выражение</param>
		/// <returns>InsertStatement</returns>
		public static InsertStatement SourceAsParameters(this InsertStatement insertStatement)
		{
			if (insertStatement==null)
				throw new Exception("SourceAsParameters: не указан insertStatement");

			RowValue rowValue = new RowValue();
			foreach (Column column in insertStatement.Columns)
			{
				rowValue.ColumnValues.Add(("@"+column.Identifiers[0].Value).ToLiteral(LiteralType.Variable));
			}
			ValuesInsertSource valuesInsertSource=new ValuesInsertSource();
			valuesInsertSource.RowValues.Add(rowValue);
			insertStatement.InsertSource = valuesInsertSource;
			return insertStatement;
		}

		/// <summary>
		/// Реализация InsertSource как список значений
		/// </summary>
		/// <param name="insertStatement">Расширяемое выражение</param>
		/// <param name="values">Словарь из пары "имя поля","значение"</param>
		/// <returns>InsertStatement</returns>
		public static InsertStatement SourceAsValues(this InsertStatement insertStatement, Dictionary<String,object> values)
		{
			if (insertStatement == null)
				throw new Exception("SourceAsValues: не указан insertStatement");
			if (values == null || values.Count == 0)
				throw new Exception("SourceAsValues: не указан values");

			RowValue rowValue = new RowValue();
			foreach (Column column in insertStatement.Columns)
			{
				object value;
				if (!values.TryGetValue(column.Identifiers[0].Value, out value))
				{
					throw new Exception("Не передано значение для поля " + column.Identifiers[0].Value);
				}
				rowValue.ColumnValues.Add(value.ToLiteral());
			}
			ValuesInsertSource valuesInsertSource = new ValuesInsertSource();
			valuesInsertSource.RowValues.Add(rowValue);
			insertStatement.InsertSource = valuesInsertSource;
			return insertStatement;
		}

	}
}
