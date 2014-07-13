using System;
using Microsoft.Data.Schema.ScriptDom.Sql;
using Platform.BusinessLogic.Activity;
using Platform.BusinessLogic.Activity.Controls;
using Platform.BusinessLogic.Common.Attributes;
using Platform.BusinessLogic.Common.Interfaces;
using Platform.Dal.Common.Interfaces;
using Platform.Dal.Common.Interfaces.QueryBuilders;
using Platform.PrimaryEntities.Common.DbEnums;
using Platform.PrimaryEntities.Factoring;
using Platform.PrimaryEntities.Reference;
using Platform.SqlObjectModel;
using Platform.SqlObjectModel.Extensions;

namespace Platform.BusinessLogic.Decorators
{
	/// <summary>
	/// Класс описывающий реализацию декоратора для формы выбора Сущности у общей ссылки
	/// </summary>
	public class AddFilterNoParentForHierarchyReference : TSqlStatementDecorator
	{
		#region Private fields

		private readonly int _entityFromFieldId;
        
		/// <summary>
		/// Построитель запроса
		/// </summary>
		private ISelectQueryBuilder _builder;
        #endregion

		#region Constructors
		/// <summary>
		/// Дефолтный конструктор закрыт, без параметров все равно не отработает
		/// </summary>
		private AddFilterNoParentForHierarchyReference()
		{
		}

	    /// <summary>
	    /// 
	    /// </summary>
	    /// <param name="entityFieldId"></param>
	    public AddFilterNoParentForHierarchyReference(int entityFieldId)
		{
			_entityFromFieldId = entityFieldId;
		}
		
        #endregion
		#region Implementation of ITSqlStatementDecorator
		/// <summary>
		/// Применить декоратор
		/// </summary>
		/// <param name="source">Расширяемое выражение</param>
		/// <param name="queryBuilder">Построитель</param>
		/// <returns>TSqlStatement</returns>
		protected override TSqlStatement DoDecorate(TSqlStatement source, IQueryBuilder queryBuilder)
		{

            var entityFromField = Objects.ById<EntityField>(_entityFromFieldId);
            var entityFrom = Objects.ById<Entity>(entityFromField.IdEntity);
		    var typeLocator = new TypeLocator();

		    Type entityFromType;
		    if (entityFrom.EntityType == EntityType.Enum || !typeLocator.TryGetType(entityFrom, out entityFromType))
		        return source;
            if (!Attribute.IsDefined(entityFromType, typeof(SelectionWithNoChildsAttribute)))
		        return source;
		    
		    _builder = (queryBuilder as ISelectQueryBuilder);
			if (_builder == null)
				return source;
			
            var entity = (_builder.Entity as Entity);
            if (entity == null)
                return source;
            Type entityType;
		    if (entity.EntityType == EntityType.Enum || !typeLocator.TryGetType(entity, out entityType))
		        return source;
            if (entityType.GetInterface(typeof (IHierarhy).FullName) == null || !Attribute.IsDefined(entityType, typeof (SelectedWithNoChildsAttribute)))
		        return source;
            
            var result = (source as SelectStatement);
			if (result == null)
				return source;

            var nonChildsSelect = new SelectStatement();
		        const string subAlias = "childs";
                var nonChildsQuery = new QuerySpecification();
                nonChildsQuery.FromClauses.Add(Helper.CreateSchemaObjectTableSource(_builder.Entity.Schema, _builder.Entity.Name, subAlias));
                nonChildsQuery.SelectElements.Add(Helper.CreateColumn(subAlias, "id"));
                nonChildsQuery.WhereClause = new WhereClause{ SearchCondition = Helper.CreateBinaryExpression((subAlias+".IdParent").ToColumn(), (_builder.AliasName + ".id").ToColumn(), BinaryExpressionType.Equals) };
                
            nonChildsSelect.QueryExpression = nonChildsQuery;
            
            result = result.Where(BinaryExpressionType.And, new UnaryExpression()
                {
                    Expression = Helper.CreateExistsPredicate( nonChildsSelect ),
                    UnaryExpressionType = UnaryExpressionType.Not
                }
            );
			
            return result;
		}
		#endregion

	}
}
