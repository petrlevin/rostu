using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BaseApp.Common.Interfaces;
using Microsoft.Data.Schema.ScriptDom.Sql;
using Platform.Common.Exceptions;
using Platform.Dal.Common.Interfaces.QueryBuilders;
using Platform.Dal.Decorators.Abstract;
using Platform.PrimaryEntities.Common.Interfaces;
using Platform.SqlObjectModel;
using Platform.SqlObjectModel.Extensions;

namespace BaseApp.Rights.Organizational.Decorators
{
    public class ImplementationBase: ImplementationBase<SelectStatement, ISelectQueryBuilder>
    {

        protected IEntity Entity
        {
            get { return QueryBuilder.Entity; }
        }

        protected virtual Identifier GetSourceAlias(string tableName, QuerySpecification query = null)
        {
            query = query ?? CurrentQuery;
            var alias = query.GetFirstAlias(tableName, true);
            if (alias != null)
                return alias;

            var table = query.FromClauses.OfType<TableSourceWithAlias>().FirstOrDefault();
            if (table != null)
                return table.Alias;

            return null;
        }

        protected Expression ColumnForField(IEntityField entityField)
        {
            var alias = GetSourceAlias(entityField.EnityName);
            if (alias != null)
                return Helper.CreateColumn(alias, entityField.Name);
            throw new PlatformException(String.Format("Не удалось найти поле '{0}' в теле запроса '{1}'", entityField.Name, CurrentQuery.Render()));
        }

        protected virtual void AddRightGroup(IGrouping<IEntityField, IOrganizationRightInfo> rightGroup)
        {
            Expression where = rightGroup.Aggregate<IOrganizationRightInfo, Expression>(null, (current, right) => AddRight(right, current));

            AddRightGroup(@where);
        }

        protected virtual void AddRightGroup(Expression @where)
        {
            if (@where != null)
                CurrentQuery.AddWhere(BinaryExpressionType.And, @where.ToParenthesisExpression());
        }


        private Expression AddRight(IOrganizationRightInfo right, Expression @where)
        {
            if (right.ParentField == null)
                AddWithoutParent(ref @where, right);
            else
            {
                var cteAlias = AddCommonTable(right);
                AddWithParent(ref @where, right, cteAlias);
            }
            return @where;
        }

        private string AddCommonTable(IOrganizationRightInfo right)
        {
            if (Source.WithCommonTableExpressionsAndXmlNamespaces == null)
                Source.WithCommonTableExpressionsAndXmlNamespaces = new WithCommonTableExpressionsAndXmlNamespaces();

            var entLink = right.Field.EntityLink;

            var cteAlias = "orgRights_cte_" + Source.WithCommonTableExpressionsAndXmlNamespaces.CommonTableExpressions.Count;

            if (right.Field.Name != "id")
            {
                if (!right.ElementEntity.IsVersioning)
                    Source.WithCommonTableExpressionsAndXmlNamespaces.CommonTableExpressions.Add(
                        new RecursiveSelect("", entLink.Schema, entLink.Name, cteAlias, "a", "id", right.ParentField.Name)
                            .GetCommonTable(false, right.IdElement.ToLiteral()));
                else
                    Source.WithCommonTableExpressionsAndXmlNamespaces.CommonTableExpressions.Add(
                        new RecursiveSelect("", entLink.Schema, entLink.Name, cteAlias, "a", "id", right.ParentField.Name)
                            .GetCommonTable(false, GetSelectForParentOfVersioning(right, right.ElementEntity)));
            }
            else if (!right.ElementEntity.IsVersioning)
                Source.WithCommonTableExpressionsAndXmlNamespaces.CommonTableExpressions.Add(
                    new RecursiveSelect("", Entity.Schema, Entity.Name, cteAlias, "a", "id", right.ParentField.Name)
                        .GetCommonTable(false, right.IdElement.ToLiteral()));
            else
                Source.WithCommonTableExpressionsAndXmlNamespaces.CommonTableExpressions.Add(
                    new RecursiveSelect("", Entity.Schema, Entity.Name, cteAlias, "a", "id", right.ParentField.Name)
                        .GetCommonTable(false, GetSelectForParentOfVersioning(right, Entity)));

            return cteAlias;
        }

        private SelectStatement GetSelectForParentOfVersioning(IOrganizationRightInfo right, IEntity entity)
        {

            QuerySpecification first = Helper.CreateQuerySpecification(entity.Schema, entity.Name, entity.Name, "id");
            SelectStatement whereRoot = new Select("idRoot", entity.Schema, entity.Name).GetQuery();
            whereRoot.Where(BinaryExpressionType.And, Helper.CreateBinaryExpression("id".ToColumn(), right.IdElement.ToLiteral(), BinaryExpressionType.Equals));

            WhenClause when =
                Helper.CreateWhenClause(
                    Helper.CreateCheckIsNotNull(whereRoot.ToSubquery()), whereRoot.ToSubquery());

            SelectColumn selectColumn = new SelectColumn
            {
                Expression = Helper.CreateCaseExpression(null, new List<WhenClause> { when }, right.IdElement.ToLiteral()),
                ColumnName = "idRoot".ToIdentifier()
            };

            Subquery whereHasRoot = new Subquery()
            {
                QueryExpression = Helper.CreateQuerySpecification(null, new List<TSqlFragment>() { selectColumn })
            };

            first.AddWhere(Helper.CreateBinaryExpression(string.Format("{0}.idRoot", entity.Name).ToColumn(), whereHasRoot, BinaryExpressionType.Equals));

            QuerySpecification second = Helper.CreateQuerySpecification(entity.Schema, entity.Name, entity.Name, "id");

            second.AddWhere(Helper.CreateBinaryExpression(string.Format("{0}.id", entity.Name).ToColumn(), right.IdElement.ToLiteral(), BinaryExpressionType.Equals));

            var result = new SelectStatement()
            {
                QueryExpression = Helper.CreateBinaryQueryExpression(first, second, BinaryQueryExpressionType.Union)
            };

            return result;
        }

        protected void AddWithParent(ref Expression @where, IOrganizationRightInfo right, string cteAlias)
        {
            where = where.AddExpression(
                CreateInPredicate(
                    right,
                    new Subquery() { QueryExpression = Helper.CreateQuerySpecification("", cteAlias, "", "id") }
                ),
                ExpressionType);
        }

        protected void AddWithoutParent(ref Expression @where, IOrganizationRightInfo right)
        {
            if (right.IdElement != null)
                if (!right.ElementEntity.IsVersioning)
                    where = where.AddExpression(CreateEquals(ColumnForField(right.Field), right.IdElement.ToLiteral()), ExpressionType);
                else
                    where = where.AddExpression(
                        CreateInPredicate(right, GetSelectForParentOfVersioning(right, right.Field.Name.ToLower() != "id" ? right.ElementEntity : Entity).ToSubquery()),
                        ExpressionType);
            else
                where = where.AddExpression(Helper.CreateCheckIsNull(ColumnForField(right.Field), !UseNot), ExpressionType);
        }

        protected Expression CreateEquals(Expression rightExpr, Literal leftExpr)
        {
            var result = Helper.CreateBinaryExpression(
                rightExpr,
                leftExpr,
                (!UseNot)
                    ? BinaryExpressionType.Equals
                    : BinaryExpressionType.NotEqualToBrackets);

            if (UseNot)
                result = Helper.CreateBinaryExpression(result, Helper.CreateCheckIsNull(rightExpr), BinaryExpressionType.Or);

            return result.ToParenthesisExpression();

        }

        protected Expression CreateInPredicate(IOrganizationRightInfo right, Subquery subquery)
        {
            Expression result = Helper.CreateInPredicate(ColumnForField(right.Field),
                                                  subquery, UseNot);
            if (UseNot)
                result = Helper.CreateBinaryExpression(
                    result,
                    Helper.CreateCheckIsNull(ColumnForField(right.Field)),
                    BinaryExpressionType.Or);

            return result.ToParenthesisExpression();
        }


        protected virtual bool UseNot
        {
            get { return false; }
        }

        protected BinaryExpressionType ExpressionType
        {
            get { return UseNot ? BinaryExpressionType.And : BinaryExpressionType.Or; }
        }

        protected QuerySpecification Query
        {
            get
            {
                var query = Source.QueryExpression as QuerySpecification;
                if (query == null)
                    throw new PlatformException(String.Format("Не возможно определить имена колонок в  теле запроса.Декоратор не может быть применен."));
                return query;
            }
        }

        private QuerySpecification _currentQuery;

        internal ImplementationBase(QuerySpecification query)
        {
            _currentQuery = query;
        }

        public ImplementationBase()
        {
            
        }


        protected virtual QuerySpecification CurrentQuery
        {
            get { return _currentQuery; }
        }

        internal void DoDecorate(IEnumerable<IGrouping<IEntityField, IOrganizationRightInfo>> rights)
        {
            foreach (var right in rights)
            {
                AddRightGroup(right);
            }
        }


        public override TSqlStatement Decorate()
        {
            DoDecorate(RightsData.Rights);
            return Source;
        }


        public IOrganizationRightData RightsData { get; set; }
    }
}
