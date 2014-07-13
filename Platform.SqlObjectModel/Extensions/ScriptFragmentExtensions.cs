using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Data.Schema.ScriptDom;
using Microsoft.Data.Schema.ScriptDom.Sql;

namespace Platform.SqlObjectModel.Extensions
{
	/// <summary>
	/// Расширения для объекта IScriptFragment
	/// </summary>
	public static class ScriptFragmentExtensions
	{
		/// <summary>
		/// Преобразует объектную модель фрагмента sql-выражение в текстовое представление
		/// </summary>
		/// <param name="fragment"></param>
		/// <returns>SQL-выражение</returns>
		public static string Render(this IScriptFragment fragment)
		{
			SqlScriptGeneratorOptions options = new SqlScriptGeneratorOptions
				{
					//AlignClauseBodies = false,
					//AlignColumnDefinitionFields = false,
					//AlignSetClauseItem = false,
					//AsKeywordOnOwnLine = false,
					//IndentSetClause = false,
					//IndentViewBody = false,
					//MultilineInsertSourcesList = false,
					//MultilineInsertTargetsList = false,
					//MultilineSelectElementsList = false,
					//MultilineSetClauseItems = false,
					//MultilineViewColumnsList = false,
					//MultilineWherePredicatesList = false,
					//NewLineBeforeCloseParenthesisInMultilineList = false,
					//NewLineBeforeFromClause = false,
					//NewLineBeforeGroupByClause = false,
					//NewLineBeforeHavingClause = false,
					//NewLineBeforeJoinClause = false,
					//NewLineBeforeOpenParenthesisInMultilineList = false,
					//NewLineBeforeOrderByClause = false,
					//NewLineBeforeOutputClause = false,
					//NewLineBeforeWhereClause = false,
					//SqlVersion = SqlVersion.Sql100,
					//IndentationSize = 1
				};
			return Render(fragment, options);
		}

		/// <summary>
		/// Преобразует объектную модель фрагмента sql-выражение в текстовое представление
		/// </summary>
		/// <param name="fragment"></param>
		/// <param name="options">Опции рендеринга</param>
		/// <returns>SQL-выражение</returns>
		public static string Render(this IScriptFragment fragment, SqlScriptGeneratorOptions options)
		{
			string result;
			ScriptGenerator generator = new Sql100ScriptGenerator(options);
			generator.GenerateScript(fragment, out result);
			return result;
		}
	}
}
