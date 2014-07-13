using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using Platform.Dal.QueryBuilders.Multilink;
using Platform.Dal.QueryBuilders.QueryParts;
using Platform.PrimaryEntities.Common.DbEnums;
using Platform.PrimaryEntities.Factoring;
using Platform.PrimaryEntities.Reference;
using Platform.SqlObjectModel.Extensions;

namespace Platform.Dal.Tests.QueryBuilders.Multilink
{
	[TestFixture]
	[ExcludeFromCodeCoverage]
	public class MultilinkSelectQueryBuilderTests : DalTestsBase
	{
		/// <summary>
		/// Ожидаемое выражение
		/// </summary>
		const string _expected =
			@"WITH     [UserRole]
			AS       (SELECT [a].[id],
								[a].[Name],
								[a].[Caption],
								ROW_NUMBER() OVER ( ORDER BY [a].[Caption] ASC) AS [RowNumber]
						FROM   [ref].[Role] AS [a]
								INNER JOIN
								[ml].[UserRole] AS [ml]
								ON [a].[id] = [ml].[idRole]
								AND [ml].[idUser] = 1)
			SELECT   [a].[id],
						[a].[Name],
						[a].[Caption]
			FROM     [UserRole] AS [a]
			WHERE    [RowNumber] BETWEEN 0 AND 9
			ORDER BY [RowNumber]";

		/// <summary>
		/// Данная промежуточная переменная введена чтобы избавиться от тормозов, когда в отладчике пытаешься вычислить выражение.
		/// Уже вычисленное выражение (из данной переменной) показывается быстро.
		/// </summary>
		private string _formattedExpected;

		/// <summary>
		/// Отформатированое ожидаемое выражение
		/// </summary>
		private string expected
		{
			get
			{
				if (string.IsNullOrEmpty(_formattedExpected))
					_formattedExpected = _expected.ToTSqlStatement().Render();
				return _formattedExpected;
			}
		}

		[Test]
		public void MultilinkOwnerUsage()
		{
			Entity multilink = Objects.ByName<Entity>("UserRole");
			MultilinkSelectQueryBuilder selectBuilder = getPreconfiguredBuilder(multilink);
			selectBuilder.MultilinkOwnerId = Objects.ByName<Entity>("User").Id;

			var actual = renderBuilder(selectBuilder);
			Assert.AreEqual(expected, actual);
		}

		private MultilinkSelectQueryBuilder getPreconfiguredBuilder(Entity entity)
		{
			return new MultilinkSelectQueryBuilder() 
			{
                Entity = entity,
				Fields = new List<string> { "id", "Name", "Caption" },
				FilterValue = 1,
				Paging = new Paging() { Start = 0, Count = 10 },
				Order = new Order() { { "Caption", true } }
			};
		}

		private string renderBuilder(MultilinkSelectQueryBuilder selectBuilder)
		{
			string sql = selectBuilder.GetSqlCommand(this.connection).CommandText;
			return sql.ToTSqlStatement().Render();
		}
	}
}
