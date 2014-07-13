using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Data.Schema.ScriptDom.Sql;
using Platform.Dal.Common.Interfaces;
using Platform.Dal.Common.Interfaces.QueryBuilders;

namespace BaseApp.Environment.Tests
{
	[ExcludeFromCodeCoverage]
	public class FakeDecorator : TSqlStatementDecorator
	{
		public bool isFake { get { return true; } }

		#region Implementation of ITSqlStatementDecorator

		/// <summary>
		/// Применить декоратор
		/// </summary>
		/// <param name="source">Расширяемое выражение</param>
		/// <param name="queryBuilder">Построитель</param>
		/// <returns>TSqlStatement</returns>
		protected override TSqlStatement DoDecorate(TSqlStatement source, IQueryBuilder queryBuilder)
		{
			return source;
		}

		#endregion
	}
}
