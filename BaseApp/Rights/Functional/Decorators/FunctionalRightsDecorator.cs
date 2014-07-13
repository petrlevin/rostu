using System.Collections.Generic;
using BaseApp.Common.Interfaces;
using BaseApp.Reference;
using BaseApp.Tablepart;
using Microsoft.Data.Schema.ScriptDom.Sql;
using Microsoft.Practices.Unity;
using Platform.Dal.Common.Interfaces;
using Platform.Dal.Common.Interfaces.QueryBuilders;
using Platform.Dal.Decorators.Abstract;
using Platform.Log;
using Platform.PrimaryEntities.Factoring;
using Platform.PrimaryEntities.Reference;
using Platform.SqlObjectModel;
using Platform.SqlObjectModel.Extensions;
using Platform.SqlObjectModel.Helpers;
using Select = Platform.SqlObjectModel.Select;

namespace BaseApp.Rights.Functional.Decorators
{
	/// <summary>
	/// Класс описывающий декоратор серверного фильтра
	/// </summary>
	public class FunctionalRightsDecorator : SelectDecoratorBase
	{

		public FunctionalRightsDecorator([Dependency("CurrentUser")]IUser user)
		{
			_user = user;
		}

		/// <summary>
		/// Конструктор
		/// </summary>
        public FunctionalRightsDecorator()
		{
			
		}

		/// <summary>
		/// Построитель запроса
		/// </summary>


	    private readonly IUser _user;

	    #region Implementation of IQueryDecorator

		protected override TSqlStatement Decorate(SelectStatement source, IQueryBuilder queryBuilder)
		{

		    var result = source;
            
            var selectIds = new Select("id", "ref", typeof(Entity).Name, typeof(Entity).Name).GetQuerySpecification();
            var selectHidden = new Select("id", "ref", typeof(Entity).Name, typeof(Entity).Name).GetQuerySpecification();
            selectIds.AddFields(new List<SelectColumn>() { new SelectColumn() { Expression = 1.ToLiteral(), ColumnName = "hidden".ToIdentifier() } });
            selectHidden.AddFields(new List<SelectColumn>() { new SelectColumn() { Expression = 0.ToLiteral(), ColumnName = "hidden".ToIdentifier() } });

		    
            //JOIN [tp].[Role_FunctionalRight] AS [Role_FunctionalRight] ON [Entity].[id]= [Role_FunctionalRight].[idEntity]
            Join<Entity, Role_FunctionalRight>.Add(selectHidden, QualifiedJoinType.Inner, "tp", e => e.Id,
		                                                                      rf => rf.IdEntity);
            //JOIN [ref].[Role] AS [Role] ON [Entity].[idOwner] = [Role_FunctionalRight].Id
            Join<Role_FunctionalRight, Role>.Add(selectHidden, QualifiedJoinType.Inner, "ref",
		    
                                                                rfr => rfr.IdOwner, r => r.Id);

            // JOIN [ml].[UserRole] as [UserRole] ON [Role].[id] = [UserRole].idRole
            var mlUserRole = Objects.ById<Entity>(Constants.UserRoleId);
            selectHidden.AddJoin(QualifiedJoinType.Inner, "ml", mlUserRole.Name, mlUserRole.Name, typeof(Role).Name, "id", "idRole");

            //WHERE [UserRole].idUser = <userId>
            selectHidden.AddWhere(
		                 Helper.CreateBinaryExpression("idUser".ToColumn(), _user.Id.ToLiteral(), BinaryExpressionType.Equals));


		    
            var union = new BinaryQueryExpression
                            {
                                FirstQueryExpression = selectIds,
                                SecondQueryExpression = selectHidden
                            };

		    var qdt = new QueryDerivedTable {Subquery = union.ToSubquery(), Alias = "union".ToIdentifier()};


		    var fields = new List<TSqlFragment>
		                     {
		                         "id".ToIdentifier(),
		                         Helper.CreateColumn(
		                             Helper.CreateFunctionCall("MIN",
		                                                       new List<Expression> {"hidden".ToColumn()}),
		                             "hidden")
		                     };

		    var hiddenInfo = Helper.CreateQuerySpecification(qdt, fields);
            hiddenInfo.GroupByClause = new GroupByClause();
            hiddenInfo.GroupByClause.GroupingSpecifications.Add(new ExpressionGroupingSpecification() {Expression = "id".ToColumn()});

            
            
		    
            result.QueryExpression.AddJoin(QualifiedJoinType.Inner, new QueryDerivedTable() {Subquery = hiddenInfo.ToSubquery() ,Alias = "hiddenInfo".ToIdentifier()},
                Helper.CreateBinaryExpression(result.GetAliasOnTable(typeof(Entity).Name), "id",
                                                                                              "hiddenInfo", "id",
                                                                                              BinaryExpressionType.Equals));
            
            result.QueryExpression.AddFields("hiddenInfo","hidden");
			//this.Log(result, queryBuilder);
			return result;
		}


		#endregion
	}
}
