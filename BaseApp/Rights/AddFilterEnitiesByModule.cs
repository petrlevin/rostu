using System;
using System.Collections.Generic;
using BaseApp.Numerators;
using BaseApp.Rights.Functional.Decorators;
using Microsoft.Data.Schema.ScriptDom.Sql;
using Platform.Dal.Common.Interfaces;
using Platform.Dal.Common.Interfaces.QueryBuilders;
using Platform.Dal.Decorators;
using Platform.Log;
using Platform.SqlObjectModel;
using Platform.Utils.Common;
using Platform.Utils.Common.Interfaces;
using Platform.SqlObjectModel.Extensions;

namespace BaseApp.Rights
{
	public class AddFilterEnitiesByModule : TSqlStatementDecorator, IOrdered
	{
		private ISelectQueryBuilder _builder;

		private readonly int _idPublicLegalFormation;

		public AddFilterEnitiesByModule()
		{
			BaseAppNumerators baseAppNumerators = new BaseAppNumerators();
			_idPublicLegalFormation = baseAppNumerators.IdPublicLegalFormation();
		}

		#region Implementation of ITSqlStatementDecorator

		/// <summary>
		/// Применить декоратор
		/// </summary>
		/// <param name="source">Расширяемое выражение</param>
		/// <param name="queryBuilder">Построитель</param>
		/// <returns>TSqlStatement</returns>
		protected override TSqlStatement DoDecorate(TSqlStatement source, IQueryBuilder queryBuilder)
		{
			_builder = (queryBuilder as ISelectQueryBuilder);
			if (_builder == null)
				return source;
			
			if (!_builder.Entity.Name.Equals("entity", StringComparison.OrdinalIgnoreCase))
				return source;
			
			SelectStatement result = (source as SelectStatement);
			if (result == null)
				return source;

			SelectStatement subQuery = new Select("id", "ref", "PublicLegalFormationModule", "a").GetQuery();
			subQuery.Join(QualifiedJoinType.Inner, "ml", "PublicLegalFormationModule_Module", "b", "a", "id",
			              "idPublicLegalFormationModule");
			subQuery.Join(QualifiedJoinType.Inner, "ml", "Module_Entity", "c", "b", "idModule", "idModule");
			subQuery.Join(QualifiedJoinType.Inner, "ref", "Module", "d", "c", "idModule", "id");
			subQuery.Fields("c", new List<string> {"idEntity"});
			subQuery.RemoveField("id");
			subQuery.Where(BinaryExpressionType.And,
			               Helper.CreateBinaryExpression("d.On".ToColumn(), 1.ToLiteral(), BinaryExpressionType.Equals));
			subQuery.Where(BinaryExpressionType.And,
			               Helper.CreateBinaryExpression("a.idPublicLegalFormation".ToColumn(),
			                                             _idPublicLegalFormation.ToLiteral(), BinaryExpressionType.Equals));
			
			result.Join(QualifiedJoinType.LeftOuter, subQuery.ToQueryDerivedTable("b"),
			            Helper.CreateBinaryExpression("b.idEntity".ToColumn(), "a.id".ToColumn(), BinaryExpressionType.Equals));

			var existColumnHidden = result.GetSelectColumn("hidden");
			WhenClause whenClause;
			
			if (existColumnHidden != null)
			{
				result.RemoveField("hidden");
				BinaryExpression binaryExpression = Helper.CreateBinaryExpression(Helper.CreateCheckFieldIsNull("b", "idEntity"),
																				  Helper.CreateBinaryExpression(
																					  existColumnHidden, 1.ToLiteral(),
																					  BinaryExpressionType.Equals),
																				  BinaryExpressionType.Or);
				whenClause = Helper.CreateWhenClause(binaryExpression, 1.ToLiteral());
			}
			else
			{
				whenClause = Helper.CreateWhenClause(Helper.CreateCheckFieldIsNull("b", "idEntity"), 1.ToLiteral());
			}
			result.AddFields(new List<SelectColumn>
				{
					Helper.CreateColumn(Helper.CreateCaseExpression(null, new List<WhenClause> {whenClause},0.ToLiteral()), "hidden")
				});

			//this.Log(result, queryBuilder);
			return result;
		}
		
		#endregion

		#region Implementation of IOrdered

		/// <summary>
		/// Список декораторов, до которых должен быть применен данный
		/// </summary>
		public IEnumerable<Type> Before { get ; private set; }

		/// <summary>
		/// Список декораторов, после которых должен быть применен данный
		/// </summary>
		public IEnumerable<Type> After
		{
			get
			{
				return new List<Type>()
					{
						typeof (FunctionalRightsDecorator)
					};
			}
		}

		/// <summary>
		/// Если декоратор хочет быть первым после указанных в списке <see cref="After"/> он должен вернуть 'true'
		/// </summary>
		public Order WantBe { get; private set; }

		#endregion
	}
}
