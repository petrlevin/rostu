using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BaseApp.Common.Interfaces;
using Microsoft.Data.Schema.ScriptDom;
using Microsoft.Data.Schema.ScriptDom.Sql;
using Platform.Common.Exceptions;
using Platform.Dal.Common.Interfaces.QueryBuilders;
using Platform.Dal.Decorators;
using Platform.Dal.Decorators.Abstract;
using Platform.PrimaryEntities.Common.Interfaces;
using Platform.SqlObjectModel;
using Platform.SqlObjectModel.Extensions;
using Platform.Utils.Extensions;

namespace BaseApp.Rights.Organizational.Decorators
{
    /// <summary>
    /// 
    /// </summary>
    public class Implementation : ImplementationBase
    {
        #region Protected & Private


        private Expression _filter;

        protected override void AddRightGroup(Expression @where)
        {
            if (@where != null)
                _filter = _filter.AddExpression(@where.ToParenthesisExpression(), BinaryExpressionType.And);

        }

        protected virtual void AddExtensionWhere(Expression @where)
        {
            if (@where != null)
                _filter = _filter.AddExpression(@where, ExpressionType);

        }






        protected override QuerySpecification CurrentQuery
        {
            get { return Query; }

        }

        protected void AddExtensions()
        {
			if (RightsData.Extensions != null && RightsData.Rights.Any())
            {
                foreach (IOrganizationRightExtension organizationRightExtension in RightsData.Extensions)
                {
                    var sql = BuildFromTemplate(organizationRightExtension);
                    foreach (IEntity entity in organizationRightExtension.Entities)
                    {
                        SelectStatement sqlStatement = BuildForExtended(entity, sql, RightsData[entity.Id]);
                        AddExtensionWhere(
                            Helper.CreateInPredicate(
                                String.Format("{0}.{1}", QueryBuilder.AliasName, "id").ToColumn(),
                                sqlStatement.ToSubquery(),
                                UseNot)
                            );
                    }
                }
            }
        }


        private SelectStatement BuildFromTemplate(IOrganizationRightExtension organizationRightExtension)
        {
            var sqlTemplate = organizationRightExtension.SqlTemplate;
            var result = Parse(sqlTemplate);
            if (result == null)
                throw new PlatformException(String.Format("Для расширения организационных прав  не правильно указан шаблон sql : '{0}'", organizationRightExtension.SqlTemplate));
            return result;
        }

        private SelectStatement Parse( string sqlTemplate)
        {
            try
            {
                //return Helper.Parse<SelectStatement>(String.Format(sqlTemplate, "[" + Entity.Name + "]"));
                //неа, спасение утопающих, дело рук самих утопающих...
                return Helper.Parse<SelectStatement>(String.Format(sqlTemplate, Entity.Name));
            }
            catch (SqlParseException ex)
            {
                throw new PlatformException(
                    String.Format("Для расширения организационных прав  не правильно указан шаблон sql : '{0}' <br/> {1}",
                                  sqlTemplate, ex.Errors.ToString(",", pe => pe.Message)));
            }
            
        }

        private SelectStatement BuildForExtended(IEntity entity, SelectStatement @select, IEnumerable<IGrouping<IEntityField, IOrganizationRightInfo>> ofExtendedRight)
        {
            var result = new Select("", entity.Schema, entity.Name, "ro").GetQuery();

            //var joined = new Select("id", Entity.Schema, Entity.Name, Entity.Name).GetQuery();
            var query = result.QueryExpression as QuerySpecification;
            
            var impl = new ImplementationBase(query);
            impl.Source = Source;
            impl.QueryBuilder = QueryBuilder;
            
            impl.DoDecorate(ofExtendedRight);
            var fromSelect = Helper.CreateQuerySpecification(select.ToQueryDerivedTable("templ"), "id,idEntity");
            fromSelect.AddWhere(BinaryExpressionType.And,
                                Helper.CreateBinaryExpression("templ.idEntity".ToColumn(), entity.Id.ToLiteral(),
                                                              BinaryExpressionType.Equals));

            query.AddJoin(
                QualifiedJoinType.Inner,
                new UnqualifiedJoin()
                {
                    FirstTableSource = Helper.CreateSchemaObjectTableSource(Entity.Schema, Entity.Name, Entity.Name),
                    SecondTableSource = new SelectStatement() { QueryExpression = fromSelect }.ToQueryDerivedTable("t"),
                    UnqualifiedJoinType = UnqualifiedJoinType.CrossApply
                },
                Helper.CreateBinaryExpression("t.id".ToColumn(), "ro.id".ToColumn(), BinaryExpressionType.Equals)
            );

            query.SelectElements.Add(Helper.CreateColumn(Entity.Name, "id", "id"));

            return result;
        }

        #endregion

        #region Public

        public override TSqlStatement Decorate()
        {
            base.Decorate();
			AddExtensions();
            Complete();
            return Source;
        }

        protected virtual void Complete()
        {
            if (_filter != null)
                Query.AddWhere(BinaryExpressionType.And, _filter.ToParenthesisExpression());
        }

        

        #endregion

    }
}
