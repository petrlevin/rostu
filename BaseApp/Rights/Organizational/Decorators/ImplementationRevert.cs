using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BaseApp.Common.Interfaces;
using Microsoft.Data.Schema.ScriptDom.Sql;
using Platform.PrimaryEntities.Common.Interfaces;
using Platform.SqlObjectModel;
using Platform.SqlObjectModel.Extensions;

namespace BaseApp.Rights.Organizational.Decorators
{
    /// <summary>
    /// 
    /// </summary>
    public class ImplementationRevert : Implementation
    {
        protected QueryExpression _union;
        private QuerySpecification _currentGroupSelect;

        protected override QuerySpecification CurrentQuery
        {
            get
            {
                return _currentGroupSelect;
            }
        }


        protected override void AddRightGroup(Expression @where)
        {
            if (@where != null)
                CurrentQuery.AddWhere(BinaryExpressionType.And, @where.ToParenthesisExpression());
        }




        protected override void AddExtensionWhere(Expression @where)
        {


            Query.AddWhere(
                ExpressionType, @where);
        }

        protected override void Complete()
        {
            
        }


        

        protected override void AddRightGroup(IGrouping<IEntityField, IOrganizationRightInfo> rightGroup)
        {
            //List<TSqlFragment> fields = FieldsName.Select(s => Helper.CreateColumn(AliasName, s)).Cast<TSqlFragment>().ToList();
            var alias = "rg_" + rightGroup.Key.Name;
            var fields = new List<TSqlFragment>
                             {
                                 Helper.CreateColumn(alias, "id"),
                                 Helper.CreateColumn(rightGroup.Key.Name.ToLiteral(), "field")
                             };

            var currentGroup = CrateCurrentGroup(alias);
            _currentGroupSelect =
                Helper.CreateQuerySpecification(currentGroup
                    , fields);
            if (_union != null)
                _union = new BinaryQueryExpression()
                             {
                                 FirstQueryExpression = _union,
                                 SecondQueryExpression = _currentGroupSelect
                             };

            base.AddRightGroup(rightGroup);
            if (_union == null)
                _union = _currentGroupSelect;



        }

        protected override bool UseNot
        {
            get
            {
                return true;
            }
        }

        protected virtual  SchemaObjectTableSource CrateCurrentGroup(string alias)
        {
            var currentGroup = Helper.CreateSchemaObjectTableSource(Entity.Schema, Entity.Name, alias);
            return currentGroup;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override TSqlStatement Decorate()
        {
            if (RightsData.Rights.Any())
            {
                base.Decorate();
            }

            AddJoin();
            

            return Source;

        }

        private void AddJoin()
        {
            if (_union != null)
            {
                var alias = "orgRights_notMatched".ToIdentifier();
                var aliasInner = "inner".ToIdentifier();
                var queryDerivedTableInner = new QueryDerivedTable()
                                            {
                                                Subquery = _union.ToSubquery(),
                                                Alias = aliasInner
                                            };
                var query = Helper.CreateQuerySpecification(queryDerivedTableInner, "id,field");
                var select = new SelectStatement()
                                 {
                                     QueryExpression = query
                                         
                                 };
                
                
                var queryDerivedTable = new QueryDerivedTable()
                {
                    Subquery = select.ToSubquery(),
                    Alias = alias
                };

                Query.AddJoin(QualifiedJoinType.Inner, queryDerivedTable,
                              Helper.CreateBinaryExpression(QueryBuilder.AliasName.ToIdentifier(), "id", alias, "id"));

                Query.SelectElements.Add(Helper.CreateColumn(alias, "field"));
            }
            else
            {

                    Query.AddWhere(Helper.CreateBinaryExpression(1.ToLiteral(), 2.ToLiteral(), BinaryExpressionType.Equals
                        ));

            }
        }
    }
}
