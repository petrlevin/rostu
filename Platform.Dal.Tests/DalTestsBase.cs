using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Data.Schema.ScriptDom.Sql;
using Microsoft.Practices.Unity;
using NUnit.Framework;
using Platform.Common;
using Platform.PrimaryEntities.Factoring;
using Platform.Unity;
using Platforms.Tests.Common;

namespace Platform.Dal.Tests
{
	[ExcludeFromCodeCoverage]
	public class DalTestsBase : SqlTests
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
