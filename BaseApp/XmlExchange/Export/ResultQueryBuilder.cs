using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Data.Schema.ScriptDom.Sql;
using Platform.BusinessLogic;
using Platform.BusinessLogic.Reference;
using Platform.Common;
using Platform.Log;
using Platform.PrimaryEntities;
using Platform.PrimaryEntities.Common.DbEnums;
using Platform.PrimaryEntities.Common.Interfaces;
using Platform.PrimaryEntities.Factoring;
using Platform.PrimaryEntities.Reference;
using Platform.SqlObjectModel;
using Platform.SqlObjectModel.Extensions;

namespace BaseApp.XmlExchange.Export
{
    /// <summary>
    /// 
    /// </summary>
    public class ResultQueryBuilder : Platform.Dal.Serialization.SelectBuilderBase, IHasEntities
    {

        private readonly DataContext _dataContext;

        protected override IEntity Entity
        {
            get { return _currentEntity; }
        }

        private Entity _currentEntity;

        private SelectStatement _source;

        private TemplateImport _templateImport;

        public ResultQueryBuilder(SelectStatement source, 
                                  TemplateImport templateImport = null, DataContext dataContext = null)
        {

            _source = source;

            
            _templateImport = templateImport;
            _dataContext = (dataContext ?? IoC.Resolve<DbContext>()).Cast<DataContext>();
        }

        public TSqlStatement Build()
        {
            var _sourceEntities = GetSourceEntities(_source);
            var result = new SelectStatement();
            _source.CopyCtes(result);

            var sourceCteName = _source.WithCommonTableExpressionsAndXmlNamespaces == null
                          ? "finalsource_cte0"
                          : "finalsource_cte" +
                            _source.WithCommonTableExpressionsAndXmlNamespaces.CommonTableExpressions.Count(
                                ct =>
                                ct.ExpressionName.Value.StartsWith("finalsource_cte")).ToString();


            CommonTableExpression cte = _source.ToSubquery().ToCommonTableExpression(sourceCteName);
            _source = new Platform.SqlObjectModel.Select("idEntity,id,originalId",null,sourceCteName).GetQuery();
            if (result.WithCommonTableExpressionsAndXmlNamespaces == null)
                result.WithCommonTableExpressionsAndXmlNamespaces = new WithCommonTableExpressionsAndXmlNamespaces();
            result.WithCommonTableExpressionsAndXmlNamespaces.CommonTableExpressions.Add(cte);

            

            var query = new QuerySpecification();
            var entities = _dataContext.Entity.ToList();
            foreach (var entity in entities)
            {
                if (!_sourceEntities.Contains(entity.Id))
                    continue;

                if (entity.EntityType == EntityType.Enum)
                    continue;

                if (entity.EntityType == EntityType.Registry)
                    continue;

                _currentEntity = entity;
                var currenStatement = (SelectStatement)BuildStatement(String.Format("{0}.{1}", _currentEntity.Schema, _currentEntity.Name));

                if (entity.EntityType != EntityType.Multilink)
                    BuildUsual(currenStatement);
                else
                    BuildMultilink(currenStatement);

                query.AddField(currenStatement.ToSubquery());

            }
            var all = new SelectStatement()
                          {
                              QueryExpression = query
                          };
            AddForClause(all, "");
            result.QueryExpression = Helper.CreateQuerySpecification(all.ToSubquery());
            AddForClause(result, "root");
            return result;
        }

        private List<Int32> GetSourceEntities(SelectStatement source)
        {
            var select = new SelectStatement()
                             {

                             };
            var query = Helper.CreateQuerySpecification(new QueryDerivedTable()
            {
                Subquery = source.ToSubquery(),
                Alias = "source".ToIdentifier()
            }, "idEntity");

            query.UniqueRowFilter = UniqueRowFilter.Distinct;
            select.QueryExpression = query;
            source.CopyCtes(select);
            using (var command = _dataContext.Database.Connection.CreateCommand())
            {
                command.CommandText = select.Render();
                using (var reader = command.ExecuteReaderLog())
                {
                    var result = new List<Int32>();
                    while (reader.Read())
                    {
                        result.Add(reader.GetInt32(0));
                    }
                    return result;
                }
            }

        }

        private void JoinWithSource(SelectStatement currenStatement)
        {
            currenStatement.Join(QualifiedJoinType.Inner, _source.ToQueryDerivedTable("source"),
                                 Helper.CreateBinaryExpression(
                                     String.Format("[{0}].[id]", GetMainAlias()).ToColumn(),
                                     "[source].[id]".ToColumn(), BinaryExpressionType.Equals
                                     ).AddExpression(
                                         Helper.CreateBinaryExpression("[source].[idEntity]".ToColumn(),
                                                                       _currentEntity.Id.ToLiteral(),
                                                                       BinaryExpressionType.Equals
                                             ), BinaryExpressionType.And));
        }

        private SelectStatement BuildMultilink(SelectStatement currenStatement)
        {
            var firstField =

                _dataContext.EntityField.First(ef => (ef.IdEntity == _currentEntity.Id) && (ef.IdEntityLink.HasValue))
                          ;

            var secondField =

                    _dataContext.EntityField.Where(ef => (ef.IdEntity == _currentEntity.Id) && (ef.IdEntityLink.HasValue)).ToList().Last()
                              ;

            JoinWithSourceField(currenStatement, firstField, QualifiedJoinType.Inner);
            JoinWithSourceField(currenStatement, secondField, QualifiedJoinType.Inner);
            return currenStatement;

        }


        private SelectStatement BuildUsual(SelectStatement currenStatement)
        {

            JoinWithSource(currenStatement);

            foreach (var entityField in _dataContext.EntityField.Where(ef => (ef.IdEntity == _currentEntity.Id) && (
                                                                                                                     ef
                                                                                                                         .IdEntityFieldType ==
                                                                                                                     (
                                                                                                                     byte
                                                                                                                     )
                                                                                                                     EntityFieldType
                                                                                                                         .Link ||
                                                                                                                     ef
                                                                                                                         .IdEntityFieldType ==
                                                                                                                     (
                                                                                                                     byte
                                                                                                                     )
                                                                                                                     EntityFieldType
                                                                                                                         .DocumentEntity ||
                                                                                                                     ef
                                                                                                                         .IdEntityFieldType ==
                                                                                                                     (
                                                                                                                     byte
                                                                                                                     )
                                                                                                                     EntityFieldType
                                                                                                                         .ReferenceEntity ||
                                                                                                                     ef
                                                                                                                         .IdEntityFieldType ==
                                                                                                                     (
                                                                                                                     byte
                                                                                                                     )
                                                                                                                     EntityFieldType
                                                                                                                         .TablepartEntity ||
                                                                                                                     ef
                                                                                                                         .IdEntityFieldType ==
                                                                                                                     (
                                                                                                                     byte
                                                                                                                     )
                                                                                                                     EntityFieldType
                                                                                                                         .ToolEntity)
                ).Where(ef => !ef.IdCalculatedFieldType.HasValue).ToList())
            {
                JoinWithSourceField(currenStatement, entityField);
            }
            return currenStatement;
        }

        private void JoinWithSourceField(SelectStatement currenStatement, EntityField entityField, QualifiedJoinType qualifiedJoinType = QualifiedJoinType.LeftOuter)
        {


            Expression joinWithSourceCondition =             Helper.CreateBinaryExpression(
                String.Format("[{0}].[{1}]", GetMainAlias(), entityField.Name).ToColumn(),
                String.Format("[{0}].[originalId]", GetFieldTableAliasSource(entityField)).ToColumn(),
                BinaryExpressionType.Equals)
                         .AddExpression(
                             Helper.CreateBinaryExpression(
                                 String.Format("[{0}].[idEntity]", GetFieldTableAliasSource(entityField))
                                       .ToColumn(),
                                       (entityField.EntityFieldType == EntityFieldType.Link)?
                                            (Expression)entityField.IdEntityLink.ToLiteral():
                                            String.Format("[{0}].[{1}Entity]", GetMainAlias(), entityField.Name).ToColumn(),
                                       BinaryExpressionType.Equals),
                             BinaryExpressionType.And
                );



            currenStatement.Join(qualifiedJoinType,
                                 _source.ToQueryDerivedTable(GetFieldTableAliasSource(entityField))
                                 , joinWithSourceCondition);
        }


        private string GetFieldTableAliasSource(IEntityField entityField)
        {
            return String.Format("sourceAsRefs_{0}", entityField.Name);

        }


        protected override string GetFieldAlias(IEntityField entityField)
        {
            var t = GetTemplateImportXLS(entityField.IdEntity);
            if (t == null)
                return base.GetFieldAlias(entityField);
            var map = t.FieldsMap.SingleOrDefault(fm => fm.IdEntityField == entityField.Id);
            if (map == null)
                return base.GetFieldAlias(entityField);
            return String.IsNullOrWhiteSpace(map.NameColumn)
                       ? base.GetFieldAlias(entityField)
                       : map.NameColumn.Replace(" ", "_");
        }

        private TemplateImportXLS GetTemplateImportXLS(int idEntity)
        {
            if (_templateImport == null)
                return null;
            return _templateImport.TemplatesByEntity.SingleOrDefault(i => i.Entity.Id == idEntity);

        }

        protected override TSqlFragment BuildField(IEntityField entityField)
        {
            return BuildField(entityField, GetMainAlias())
            ;
        }

        private TSqlFragment BuildField(IEntityField entityField, string fromAlias, bool lookInSource = true)
        {
            if (
                entityField.EntityFieldType == EntityFieldType.Link ||
                entityField.EntityFieldType == EntityFieldType.DocumentEntity ||
                entityField.EntityFieldType == EntityFieldType.ReferenceEntity ||
                entityField.EntityFieldType == EntityFieldType.TablepartEntity ||
                entityField.EntityFieldType == EntityFieldType.ToolEntity)
            {
                if (entityField.Entity.EntityType != EntityType.Multilink)
                    return BuildFieldUsual(entityField, fromAlias, lookInSource);
                else
                    return BuildFieldMultilink(entityField);
            }
            else
                return base.BuildField(entityField, fromAlias);
        }

        private TSqlFragment BuildFieldUsual(IEntityField entityField, string fromAlias, bool lookInSource)
        {
            var selectIfNull = new SelectStatement()
                                   {
                                       QueryExpression =
                                           Helper.CreateQuerySpecification(
                                               String.Format("{0}.{1}.{2}", fromAlias,
                                                             entityField.Name, GetFieldAlias(entityField)).ToSelectColumn())
                                   };
            AddForClause(selectIfNull, "");

            var selectSource = lookInSource
                                   ? new SelectStatement()
                                         {
                                             QueryExpression =
                                                 Helper.CreateQuerySpecification(
                                                     String.Format("{0}.{1}.{2}", GetFieldTableAliasSource(entityField),
                                                                   "id", "source").ToSelectColumn())
                                         }
                                   : null;
            if (lookInSource)
                AddForClause(selectSource, GetFieldAlias(entityField));


            SelectStatement selectDestination = (entityField.EntityFieldType == EntityFieldType.Link)
                                                    ? BuildDestinationForLink(entityField, fromAlias)
                                                    : BuildDestinationForCommonLink(entityField, fromAlias);

            AddForClause(selectDestination, GetFieldAlias(entityField));
            var whenIsNull = Helper.CreateWhenClause(
                Helper.CreateCheckIsNull(
                    String.Format("{0}.[{1}]", fromAlias, entityField.Name).ToColumn()),
                selectIfNull.ToSubquery()
                );

            var when = lookInSource
                           ? Helper.CreateWhenClause(
                               Helper.CreateCheckIsNotNull(
                                   String.Format("{0}.[id]", GetFieldTableAliasSource(entityField)).ToColumn()),
                               selectSource.ToSubquery())
                           : null;
            var @case = Helper.CreateCaseExpression(null, selectDestination.ToSubquery(), whenIsNull, when);
            return @case;
        }


        private TSqlFragment BuildFieldMultilink(IEntityField entityField)
        {

            var selectSource =
                                    new SelectStatement()
                                    {
                                        QueryExpression =
                                            Helper.CreateQuerySpecification(
                                                String.Format("{0}.{1}.{2}", GetFieldTableAliasSource(entityField),
                                                              "id", "source").ToSelectColumn())
                                    }
                                   ;

            AddForClause(selectSource, GetFieldAlias(entityField));



            return selectSource.ToSubquery();
        }


        private SelectStatement BuildDestination(IEntityField entityField, int idEntityLink, string fromAlias)
        {
            return BuildDestination(entityField, Objects.ById<Entity>(idEntityLink), fromAlias);
        }

        private int _subsCount = -1;
        private string GetSubAlias()
        {
            return "sub" + _subsCount;
        }

        private SelectStatement BuildDestination(IEntityField entityField, Entity entityLink, string fromAlias)
        {

            var t = GetTemplateImportXLS(entityLink.Id);
            var fields = new List<TSqlFragment>();


            if (t == null)
                fields.Add(
                    Helper.CreateFunctionCall("GetCaption", "dbo",
                                              (Object)entityLink.Id,
                                              String.Format("[{0}].[{1}]", fromAlias, entityField.Name)
                        ).ToSelectColumn("Caption")
                    );
            else
                fields.AddRange(
                    t.KeyField.Select(
                        keyField =>
                        {
                            _subsCount++;
                            var result = (keyField.EntityField.Name == "id")
                                             ? (TSqlFragment)
                                               String.Format("[{0}].[{1}]", fromAlias, entityField.Name).ToColumn()
                                             : Helper.CreateQuerySpecification(
                                                 Helper.CreateSchemaObjectTableSource(entityLink.Schema,
                                                                                      entityLink.Name, GetSubAlias()),
                                                 BuildField(keyField.EntityField, GetSubAlias(), false))
                                                     .AddWhere(
                                                         String.Format("{0}.id", GetSubAlias()).ToColumn()
                                                                 .IsEquals(
                                                                     String.Format("[{0}].[{1}]", fromAlias,
                                                                                   entityField.Name).ToColumn())
                                                   ).ToSelectStatement().AddForClause("").ToSubquery();
                            _subsCount--;
                            return result;
                        }
                        )
                        );

            var select = new SelectStatement()
                             {
                                 QueryExpression = Helper.CreateQuerySpecification(null, fields)
                             };

            AddForClause(@select, "destination");
            return @select;
        }


        private SelectStatement BuildDestinationForLink(IEntityField entityField, string fromAlias)
        {

            var @select = BuildDestination(entityField, entityField.IdEntityLink.Value, fromAlias);
            return new SelectStatement()
            {
                QueryExpression = Helper.CreateQuerySpecification(select.ToSubquery())
            };


        }


        private SelectStatement BuildDestinationForCommonLink(IEntityField entityField, string fromAlias)
        {
            var entities = _dataContext.Entity.Where(e => e.AllowGenericLinks).ToList();
            var whens = new List<WhenClause>();
            foreach (var entity in entities)
            {
                var curSelect = BuildDestination(entityField, entity.Id, fromAlias);
                whens.Add(Helper.CreateWhenClause(entity.Id.ToLiteral(), curSelect.ToSubquery()));

            }
            var @case =
                Helper.CreateCaseExpression(
                    String.Format("{0}.{1}Entity", fromAlias, entityField.Name).ToColumn(), whens, null);
            return new SelectStatement()
                       {
                           QueryExpression = Helper.CreateQuerySpecification(@case)
                       };

        }

        IQueryable<Entity> IHasEntities.Entities
        {
            get { return _dataContext.Entity; }
        }

    }
}

