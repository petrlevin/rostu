using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Data.Schema.ScriptDom.Sql;

namespace Platform.SqlObjectModel.Tests
{
	/// <summary>
	/// Базовый класс для модулей тестирования
	/// </summary>
	[ExcludeFromCodeCoverage]
	public class BaseFixture
	{
		/// <summary>
		/// Опции генерации текста sql запроса без форматирования
		/// </summary>
		protected SqlScriptGeneratorOptions options = new SqlScriptGeneratorOptions
		{
			AlignClauseBodies = false,
			AlignColumnDefinitionFields = false,
			AlignSetClauseItem = false,
			AsKeywordOnOwnLine = false,
			IndentSetClause = false,
			IndentViewBody = false,
			MultilineInsertSourcesList = false,
			MultilineInsertTargetsList = false,
			MultilineSelectElementsList = false,
			MultilineSetClauseItems = false,
			MultilineViewColumnsList = false,
			MultilineWherePredicatesList = false,
			NewLineBeforeCloseParenthesisInMultilineList = false,
			NewLineBeforeFromClause = false,
			NewLineBeforeGroupByClause = false,
			NewLineBeforeHavingClause = false,
			NewLineBeforeJoinClause = false,
			NewLineBeforeOpenParenthesisInMultilineList = false,
			NewLineBeforeOrderByClause = false,
			NewLineBeforeOutputClause = false,
			NewLineBeforeWhereClause = false,
			SqlVersion = SqlVersion.Sql100,
			IndentationSize = 1
		};
	}
}
