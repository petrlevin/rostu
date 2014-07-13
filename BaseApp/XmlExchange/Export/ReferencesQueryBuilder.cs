using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Data.Schema.ScriptDom.Sql;
using Platform.BusinessLogic;
using Platform.BusinessLogic.Common.Interfaces;
using Platform.BusinessLogic.Reference;
using Platform.Common;
using Platform.PrimaryEntities;
using Platform.PrimaryEntities.Common.DbEnums;
using Platform.PrimaryEntities.Factoring;
using Platform.PrimaryEntities.Reference;
using Platform.SqlObjectModel;
using Platform.SqlObjectModel.Extensions;
using Select = Platform.SqlObjectModel.Select;

namespace BaseApp.XmlExchange.Export
{
    public class ReferencesQueryBuilder : BuilderBase
    {
        private readonly SelectStatement _source;
        private string _expRefsSelectCteName;

        #region Public

        /// <summary>
        /// 
        /// </summary>
        /// <param name="source"></param>
        /// <param name="dbContext"></param>
        public ReferencesQueryBuilder(SelectStatement source, DbContext dbContext = null)
            : base(dbContext, null)
        {
            _source = source;
        }

        public InsertStatement BuildInsert(SchemaObjectName destination, SelectStatement exept, SelectStatement expRefSelect)
        {
            var select = BuildSelect(exept, expRefSelect);
            var result = new InsertStatement()
                             {
                                 Target = destination.ToSchemaObjectDataModificationTarget(),
                                 InsertOption = InsertOption.Into,


                             };
            result.Columns.Add("idEntity".ToColumn());
            result.Columns.Add("idElement".ToColumn());
            result.Columns.Add("originalIdElement".ToColumn());

            select.CopyCtes(result);
            //select.DeleteCtes();
            result.InsertSource = select.QueryExpression;
            return result;

        }

        private static QuerySpecification BuildRefsSelectQuery(SchemaObjectName refs)
        {
            var columns = new List<TSqlFragment>();
            columns.Add(Helper.CreateColumn("idEntity".ToColumn(), "idEntity"));
            columns.Add(Helper.CreateColumn("idElement".ToColumn(), "id"));
            columns.Add(Helper.CreateColumn("originalIdElement".ToColumn(), "originalId"));
            return Helper.CreateQuerySpecification(new SchemaObjectTableSource()
                                                {
                                                    SchemaObject = refs
                                                }, columns);

        }


        public static SelectStatement BuildRefsSelect(SchemaObjectName refs)
        {
            return new SelectStatement()
                             {
                                 QueryExpression = BuildRefsSelectQuery(refs)
                             };


        }

        public SelectStatement BuildSourceAndRefs(SchemaObjectName refs)
        {
            var result = new SelectStatement();
            _source.CopyCtes(result);


            result.QueryExpression = _source.QueryExpression.Union(BuildRefsSelectQuery(refs)
                , false, true);
            return result;

        }

        public SelectStatement BuildSelect(SelectStatement except, SelectStatement expRefsSelect)
        {
            QueryExpression unionQuery = null;
            var sourceCteName = _source != null ? _source.WithCommonTableExpressionsAndXmlNamespaces == null
                                      ? "source_cte0"
                                      : "source_cte" +
                                        _source.WithCommonTableExpressionsAndXmlNamespaces.CommonTableExpressions.Count(
                                            ct =>
                                            ct.ExpressionName.Value.StartsWith("source_cte")).ToString() : null;
            _expRefsSelectCteName = expRefsSelect != null ? "expRefs_cte" : null;
            var exceptCteName = "expept_cte";

            CommonTableExpression cte = sourceCteName != null ? _source.ToSubquery().ToCommonTableExpression(sourceCteName) : null;
            CommonTableExpression cte_expRefSelect = expRefsSelect != null ? expRefsSelect.ToSubquery().ToCommonTableExpression(_expRefsSelectCteName) : null;
            CommonTableExpression cte_except = except != null ? except.ToSubquery().ToCommonTableExpression(exceptCteName) : null;




            var entities = _dataContext.Entity.ToList();
            foreach (
                var entityField in
                    _dataContext.EntityField.Where(ef =>
                        ef.IdEntityFieldType == (byte)EntityFieldType.Link ||
                    ef.IdEntityFieldType == (byte)EntityFieldType.Multilink ||
                    ef.IdEntityFieldType == (byte)EntityFieldType.Tablepart ||
                    ef.IdEntityFieldType == (byte)EntityFieldType.VirtualTablePart ||
                    ef.IdEntityFieldType == (byte)EntityFieldType.DocumentEntity ||
                    ef.IdEntityFieldType == (byte)EntityFieldType.ReferenceEntity ||
                    ef.IdEntityFieldType == (byte)EntityFieldType.TablepartEntity ||
                    ef.IdEntityFieldType == (byte)EntityFieldType.ToolEntity

                        ).Where(ef => !ef.IdCalculatedFieldType.HasValue).ToList())
            {
                var entity = entities.Single(e => e.Id == entityField.IdEntity);
                if (entity.EntityType == EntityType.Multilink)
                    continue;
                var query = new QuerySpecification();
                query.FromClauses.Add(Helper.CreateSchemaObjectTableSource(null, sourceCteName,
                                                                           sourceCteName));
                switch (entityField.EntityFieldType)
                {
                    case EntityFieldType.Link:
                        {
                            AddForLink(query, entityField, sourceCteName, entity);
                            break;
                        }
                    case EntityFieldType.Multilink:
                        {
                            AddForMultilink(entities, entityField, query, sourceCteName);
                            break;
                        }
                    case EntityFieldType.VirtualTablePart:
                    case EntityFieldType.Tablepart:
                        {
                            AddForTablePart(entityField, query, sourceCteName, entity);
                            break;
                        }
                    case EntityFieldType.DocumentEntity:
                    case EntityFieldType.ReferenceEntity:
                    case EntityFieldType.TablepartEntity:
                    case EntityFieldType.ToolEntity:
                        {
                            AddForCommonLink(query, sourceCteName, entityField, entity);


                            break;
                        }
                }
                unionQuery = unionQuery.UnionWith(query);

            }



            var result = new SelectStatement();


            var resultQuery = Helper.CreateQuerySpecification(new QueryDerivedTable()
                                                                         {
                                                                             Subquery = unionQuery.ToSubquery(),
                                                                             Alias = "union".ToIdentifier()
                                                                         }, "idEntity,id,originalId");

            resultQuery.UniqueRowFilter = UniqueRowFilter.Distinct;
            result.QueryExpression = resultQuery;




            QueryExpression queryExpression = Helper.CreateQuerySpecification(result.ToQueryDerivedTable("u"), "idEntity,id,originalId");
            queryExpression = new BinaryQueryExpression
            {
                FirstQueryExpression = queryExpression,
                SecondQueryExpression = Helper.CreateQuerySpecification(Helper.CreateSchemaObjectTableSource(null, except == null ? sourceCteName : exceptCteName,
                                                                       except == null ? sourceCteName : exceptCteName), "idEntity,id,originalId"),
                BinaryQueryExpressionType = BinaryQueryExpressionType.Except


            };
            result = new SelectStatement();
            result.QueryExpression = queryExpression;

            if (_source != null)
                _source.CopyCtes(result);
            if (result.WithCommonTableExpressionsAndXmlNamespaces == null)
                result.WithCommonTableExpressionsAndXmlNamespaces = new WithCommonTableExpressionsAndXmlNamespaces();
            if (_source != null)
                result.WithCommonTableExpressionsAndXmlNamespaces.CommonTableExpressions.Add(cte);
            if (cte_expRefSelect != null)
                result.WithCommonTableExpressionsAndXmlNamespaces.CommonTableExpressions.Add(cte_expRefSelect);
            if (cte_except != null)
                result.WithCommonTableExpressionsAndXmlNamespaces.CommonTableExpressions.Add(cte_except);



            return result;



        }

        #endregion


        #region Private

        private void AddForCommonLink(QuerySpecification query, string source_cte_Name, EntityField entityField,
                                             Entity entity)
        {
            if (_source != null)
            {

                query.AddWhere(BinaryExpressionType.And,
                               Helper.CreateBinaryExpression(
                                   String.Format("[{0}].[idEntity]", source_cte_Name).ToColumn(),
                                   entityField.IdEntity.ToLiteral(), BinaryExpressionType.Equals));
                query.AddJoin(QualifiedJoinType.Inner, Schemas.ByEntityType(entity.IdEntityType),
                              entity.Name, entity.Name, source_cte_Name, "id",
                              "id");
            }

            var idEntity = Helper.CreateColumn(entity.Name, entityField.Name + "Entity");
            query.SelectElements.Add(idEntity.ToSelectColumn("idEntity"));
            query.SelectElements.Add(
                this.CreateSelectColumnForHierarhyDoc(entity.Name, entityField.Name, entityField.Name + "Entity", "id"));
            var originalId = Helper.CreateColumn(entity.Name, entityField.Name);
            query.SelectElements.Add(originalId.ToSelectColumn("originalId"));

            query.AddWhere(BinaryExpressionType.And,
                           Helper.CreateCheckIsNotNull(
                           String.Format("[{0}].[{1}]", entity.Name, entityField.Name).ToColumn()));
            query.AddWhere(BinaryExpressionType.And,
                           Helper.CreateCheckIsNotNull(
                               String.Format("[{0}].[{1}]", entity.Name, entityField.Name + "Entity").ToColumn()));

            AddIfInEXpRefsWhere(query, originalId, idEntity);

        }

        private void AddForTablePart(EntityField entityField, QuerySpecification query, string source_cte_Name,
                                            Entity entity)
        {
            var entityLink = Objects.ById<Entity>(entityField.IdEntityLink.Value);
            query.SelectElements.Add(Helper.CreateColumn(entityField.IdEntityLink.ToLiteral(),
                                            "idEntity"));

            if (_source != null)
            {
                query.AddWhere(BinaryExpressionType.And,
                               Helper.CreateBinaryExpression(
                                   String.Format("[{0}].[idEntity]", source_cte_Name).ToColumn(),
                                   entityField.IdEntity.ToLiteral(), BinaryExpressionType.Equals));
                query.AddJoin(QualifiedJoinType.Inner, Schemas.ByEntityType(entity.IdEntityType),
                              entity.Name, entity.Name, source_cte_Name, "id",
                              "id");
            }

            query.AddJoin(QualifiedJoinType.Inner, Schemas.ByEntityType(entityLink.IdEntityType),
                          entityLink.Name, entityLink.Name, entity.Name, "id",
                          Objects.ById<EntityField>(entityField.IdOwnerField.Value).Name);

            query.SelectElements.Add(Helper.CreateColumn(entityLink.Name, "id", "id"));
            var originalId = Helper.CreateColumn(entityLink.Name, "id");
            query.SelectElements.Add(originalId.ToSelectColumn("originalId"));
            if (entityField.EntityFieldType == EntityFieldType.VirtualTablePart)
                AddIfInEXpRefsWhere(query, originalId, entityField.IdEntityLink.ToLiteral());
        }

        private void AddForMultilink(List<Entity> entities, EntityField entityField, QuerySpecification query,
                                            string source_cte_Name)
        {
            var entityMultilink = entities.Single(e => e.Id == entityField.IdEntityLink);
            var rightEntity =
                MultilinkHelper.GetRightMultilinkEntity(entityMultilink, entityField.IdEntity);
            var idRightEntity = rightEntity.Id;
            ;
            query.SelectElements.Add(Helper.CreateColumn(idRightEntity.ToLiteral(),
                                                         "idEntity"));

            if (_source != null)
            {
                query.AddWhere(BinaryExpressionType.And,
                               Helper.CreateBinaryExpression(
                                   String.Format("[{0}].[idEntity]", source_cte_Name).ToColumn(),
                                   entityField.IdEntity.ToLiteral(), BinaryExpressionType.Equals));
                query.AddJoin(QualifiedJoinType.Inner, "ml",
                              entityMultilink.Name, entityMultilink.Name, source_cte_Name, "id",
                              MultilinkHelper.GetLeftMultilinkField(entityMultilink, entityField.IdEntity).Name);
            }
            var rightField = MultilinkHelper.GetRightMultilinkField(entityMultilink,
                                                                    entityField.IdEntity);

            if (this.IdIsOfHierarhyDoc(idRightEntity))
                query.SelectElements.Add(this.CreateColumnForHierarhyDoc(entityMultilink.Name, rightField.Name, idRightEntity).ToSelectColumn("id"));
            else
                query.SelectElements.Add(Helper.CreateColumn(entityMultilink.Name, rightField.Name, "id"));
            var originalId = Helper.CreateColumn(entityMultilink.Name, rightField.Name);
            query.SelectElements.Add(originalId.ToSelectColumn("originalId"));
            AddIfInEXpRefsWhere(query, originalId, idRightEntity.ToLiteral());
        }

        private void AddForLink(QuerySpecification query, EntityField entityField, string source_cte_Name, Entity entity)
        {

            query.SelectElements.Add(Helper.CreateColumn(entityField.IdEntityLink.ToLiteral(),
                                                         "idEntity"));
            if (_source != null)
            {
                query.AddWhere(BinaryExpressionType.And,
                               Helper.CreateBinaryExpression(
                                   String.Format("[{0}].[idEntity]", source_cte_Name).ToColumn(),
                                   entityField.IdEntity.ToLiteral(), BinaryExpressionType.Equals));
                query.AddJoin(QualifiedJoinType.Inner, Schemas.ByEntityType(entity.IdEntityType),
                              entity.Name, entity.Name, source_cte_Name, "id", "id");
            }
            query.AddWhere(BinaryExpressionType.And,
                           Helper.CreateCheckIsNotNull(
                               String.Format("[{0}].[{1}]", entity.Name, entityField.Name).ToColumn()));
            if (this.IdIsOfHierarhyDoc(entityField.IdEntityLink.Value))
                query.SelectElements.Add(this.CreateColumnForHierarhyDoc(entity.Name, entityField.Name, entityField.IdEntityLink.Value).ToSelectColumn("id"));
            else
                query.SelectElements.Add(Helper.CreateColumn(entity.Name, entityField.Name, "id"));
            var originalId = Helper.CreateColumn(entity.Name, entityField.Name);
            query.SelectElements.Add(originalId.ToSelectColumn("originalId"));
            AddIfInEXpRefsWhere(query, originalId, entityField.IdEntityLink.ToLiteral());

        }

        private void AddIfInEXpRefsWhere(QuerySpecification query, Column column, Expression entityExpression)
        {
            if (_expRefsSelectCteName == null)
                return;
            Expression joinCondition =
                column.IsEquals("expRefs.originalId".ToColumn())
                      .AddExpression(entityExpression.IsEquals("expRefs.idEntity".ToColumn()), BinaryExpressionType.And);
            query.AddJoin(QualifiedJoinType.Inner, Helper.CreateSchemaObjectTableSource(null, _expRefsSelectCteName, "expRefs"), joinCondition)
            ;
        }


        #endregion


    }
}
