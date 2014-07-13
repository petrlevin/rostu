using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;
using Microsoft.Data.Schema.ScriptDom;
using Microsoft.Data.Schema.ScriptDom.Sql;

namespace Platform.SqlObjectModel.Extensions
{
	/// <summary>
	/// Расширения для создания объектов из пространства Microsoft.Data.Schema.ScriptDom.Sql
	/// </summary>
	public static class StringExtensions
	{
		/// <summary>
		/// Преобразует строку в объект Identifier
		/// </summary>
		/// <param name="value">Преобразуемая строка</param>
		/// <returns>Identifier</returns>
		public static Identifier ToIdentifier(this string value)
		{
			if (String.IsNullOrWhiteSpace(value))
				throw new Exception("ToIdentifier: Пустая строка не может быть преобразована");
			return new Identifier { QuoteType = QuoteType.SquareBracket, Value = value };
		}

		/// <summary>
		/// Преобразует строку в объект Identifier
		/// </summary>
		/// <param name="value">Преобразуемая строка</param>
		/// <returns>Identifier</returns>
		public static Identifier ToIdentifierWithoutQuote(this string value)
		{
			if (String.IsNullOrWhiteSpace(value))
				throw new Exception("ToIdentifierWithoutQuote: Пустая строка не может быть преобразована");
			return new Identifier { QuoteType = QuoteType.NotQuoted, Value = value };
		}

		/// <summary>
		/// Преобразует строку в объект Literal с указаным типом
		/// </summary>
		/// <param name="value">Преобразуемая строка</param>
		/// <param name="type">Тип</param>
		/// <returns>Literal</returns>
		public static Literal ToLiteral(this string value, LiteralType type = LiteralType.AsciiStringLiteral)
		{
			if (value == null)
				throw new ArgumentNullException("value", "Пустая строка не может быть преобразована");
			return new Literal {Value = value, LiteralType = type};
		}

		/// <summary>
		/// Преобразует int,guid,decimal,string в объект Literal
		/// </summary>
		/// <param name="value">Преобразуемое значение</param>
		/// <returns>Literal</returns>
		public static Literal ToLiteral(this object value)
		{
			Literal result = new Literal();

			if (value == null || (value is DBNull))
			{
				result.Value = "NULL";
				result.LiteralType = LiteralType.Null;
			}
			else if (value is int || value is Int64 || value is byte)
			{
				result.Value = value.ToString();
				result.LiteralType = LiteralType.Integer;
			}
			else if (value is Guid)
			{
				result.Value = value.ToString();
				result.LiteralType = LiteralType.AsciiStringLiteral;
			}
			else if (value is decimal || value is double)
			{
				result.Value = Convert.ToDecimal(value).ToString(CultureInfo.CreateSpecificCulture("en-US"));
				result.LiteralType = LiteralType.Real;
			}
			else if (value is string)
			{
				result.Value = value.ToString();
				result.LiteralType = LiteralType.AsciiStringLiteral;
			} 
			else if (value is bool)
			{
				result.Value = ((bool) value) ? "1" : "0";
				result.LiteralType = LiteralType.Integer;
			}
			else if (value is DateTime)
			{
				result.Value = "CONVERT(datetime, '"+value.ToString()+"', 104)";
				result.LiteralType = LiteralType.Variable;

			}
			else throw new Exception("ToLiteral: Не реализовано для " + value.GetType().ToString());

			return result;
		}

		/// <summary>
		/// Преобразует строку формата schema.table или table в объект SchemaObjectName
		/// </summary>
		/// <param name="value">Преобразуемая строка</param>
		/// <returns>SchemaObjectName</returns>
		public static SchemaObjectName ToSchemaObjectName(this string value)
		{
			if (string.IsNullOrWhiteSpace(value))
				throw new Exception("ToSchemaObjectName: Пустая строка не может быть преобразована");
			value = value.Replace("[", "").Replace("]", "");
			string[] values = value.Split(new char[] { '.' });

			return values.Length == 1 ? Helper.CreateSchemaObjectName("", value) : Helper.CreateSchemaObjectName(values[0], values[1]);
		}

		/// <summary>
		/// Преобразует строку формата schema.table alias или table alias в объект SchemaObjectTableSource
		/// </summary>
		/// <param name="value">Преобразуемая строка</param>
		/// <returns></returns>
		public static SchemaObjectTableSource ToSchemaObjectTableSource(this string value)
		{
			if (string.IsNullOrWhiteSpace(value))
				throw new Exception("ToCreateSchemaObjectTableSource: Пустая строка не может быть преобразована");
			value = value.Replace("[", "").Replace("]", "");
			string[] values = value.Split(new char[] { '.', ' ' });
			if (values.Length != 2 && values.Length != 3)
				throw new Exception("ToCreateSchemaObjectTableSource: Не верный формат строки");
			SchemaObjectTableSource result = new SchemaObjectTableSource();
			if (values.Length == 3)
			{
				result = Helper.CreateSchemaObjectTableSource(values[0], values[1], values[2]);
			}
			if (values.Length == 2)
			{
				result = Helper.CreateSchemaObjectTableSource("", values[0], values[1]);
			}
			return result;
		}

		/// <summary>
		/// Преобразует строку формата tableAlias.fieldName в объект Column
		/// </summary>
		/// <param name="value">Преобразуемая строка</param>
		/// <returns></returns>
		public static Column ToColumn(this string value)
		{
			if (string.IsNullOrWhiteSpace(value))
				throw new Exception("ToColumn: Пустая строка не может быть преобразована");
			value = value.Replace("[", "").Replace("]", "");
			string[] values = value.Split(new char[] { '.'});
			
			Column result = values.Length >= 2
								? Helper.CreateColumn(values[0], values[1])
								: Helper.CreateColumn("", values[0]);

			return result;
		}

		/// <summary>
		/// Преобразует строку формата tableAlias.fieldName fieldAliasName в объект SelectColumn
		/// </summary>
		/// <param name="value">Преобразуемая строка</param>
		/// <returns></returns>
		public static SelectColumn ToSelectColumn(this string value , bool splitByWhiteSpace = true)
		{
			if (string.IsNullOrWhiteSpace(value))
				throw new Exception("ToSelectColumn: Пустая строка не может быть преобразована");
			value = value.Replace("[", "").Replace("]", "");
            string[] values = splitByWhiteSpace ? value.Split(new char[] { '.', ' ' }) : value.Split(new char[] { '.'});
			if (values.Length < 3)
				throw new Exception("ToSelectColumn: Не верный формат строки");

			return Helper.CreateColumn(values[0], values[1], values[2]);
		}

		/// <summary>
		/// Проверяет, что строка содержит символы [a-z]
		/// </summary>
		/// <param name="value">Проверяемая строка</param>
		/// <returns>bool</returns>
		public static bool IsValidString(this string value)
		{
			Regex regex = new Regex("^[a-z]+$");
			return regex.IsMatch(value.ToLower());
		}

		/// <summary>
		/// Проверяет, что строка состоит только из букв 'z'
		/// </summary>
		/// <param name="value">Проверяемая строка</param>
		/// <returns>bool</returns>
		public static bool IsLast(this string value)
		{
			Regex regex = new Regex("^[z]+$");
			return regex.IsMatch(value.ToLower());
		}

		/// <summary>
		/// Возвращает строку используемую в качестве алиаса таблицы
		/// </summary>
		/// <param name="value">Обрабатываемая строка</param>
		/// <returns>string</returns>
		public static string GetNextAlias(this string value)
		{
			string result = "";
			if (value == string.Empty)
				result = "a";
			else
			{
				if (value.IsValidString())
				{
					if (value[value.Length - 1] < 'z')
					{
						char lastChar = value[value.Length - 1];
						lastChar++;
						result = value.Remove(value.Length - 1) + lastChar;
					}
					else
					{
						if (value.IsLast())
							result = value.Replace("z", "a") + "a";
						else
						{
							result = GetNextAlias(value.Remove(value.Length - 1)) + "z";
						}
					}
				}
			}
			return result;
		}

		/// <summary>
		/// Преобразование строки к объектному виду
		/// </summary>
		/// <param name="value">Преобразуемая строка</param>
		/// <returns>TSqlStatement</returns>
		public static SelectStatement ToSelectStatement(this string value)
		{
			if (string.IsNullOrWhiteSpace(value))
				throw new Exception("ToSelectStatement: не указан value");

			TSql100Parser parser = new TSql100Parser(false);
			IList<ParseError> errors;
			IScriptFragment result = parser.Parse(new StringReader(value), out errors);
			if (errors.Count > 0)
			{
				throw new Exception(string.Format(@"Errors encountered: ""{0}""", errors[0].Message));
			}
			TSqlStatement statement = ((TSqlScript)result).Batches[0].Statements[0];
			
			if (!(statement is SelectStatement))
				throw new Exception("Перобразовано к типу " + statement.GetType() + " вместо SelectStatement");
			
			return (statement as SelectStatement);
		}

		/// <summary>
		/// Преобразует строку в TSqlStatement
		/// </summary>
		/// <param name="value">Преобразуемая строка</param>
		/// <returns>TSqlStatement</returns>
		public static TSqlStatement ToTSqlStatement(this string value)
		{
			if (string.IsNullOrWhiteSpace(value))
				throw new Exception("ToSelectStatement: не указан value");
			TSql100Parser parser = new TSql100Parser(false);
			IList<ParseError> errors;
			IScriptFragment result = parser.Parse(new StringReader(value), out errors);
			if (errors.Count > 0)
			{
				throw new Exception(string.Format(@"Errors encountered: ""{0}""", errors[0].Message));
			}
			return ((TSqlScript)result).Batches[0].Statements[0];
		}

		/// <summary>
		/// Преобразует выражение для SQL
		/// </summary>
		/// <param name="value">Преобразуемое выражение</param>
		/// <returns></returns>
		public static string ToSqlValue(this object value)
		{
			string result;
			if (value == null || (value is DBNull))
			{
				result = "NULL";
			}
			else if (value is int || value is Int64 || value is byte)
			{
				result = value.ToString();
			}
			else if (value is Guid)
			{
				result = string.Format("'{0}'",value);
			}
			else if (value is decimal)
			{
				result = Convert.ToDecimal(value).ToString(CultureInfo.CreateSpecificCulture("en-US"));
			}
			else if (value is string)
			{
				result = string.Format("'{0}'", value);
			}
			else if (value is bool)
			{
				result = ((bool)value) ? "1" : "0";
			}
			else if (value is DateTime)
			{
				result = "CONVERT(datetime, '" + value.ToString() + "', 104)";
			}
			else throw new Exception("Не реализовано для " + value.GetType().ToString());
			return result;
		}
	}
}
