using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using BaseApp.Common.Interfaces;
using BaseApp.Rights.Organizational.Decorators;
using Microsoft.Data.Schema.ScriptDom.Sql;
using Platform.Dal.Common.Interfaces;
using Platform.Dal.Decorators;
using Platform.Dal.QueryBuilders.QueryParts;
using Platform.PrimaryEntities.Common.DbEnums;
using Platform.PrimaryEntities.Common.Interfaces;
using Platform.SqlObjectModel;
using Platform.SqlObjectModel.Extensions;
using AddCaptions = Platform.Dal.Decorators.AddCaptions;


namespace BaseApp.Rights.Organizational
{
    internal class SelectQueryBuilder : Platform.Dal.QueryBuilders.SelectQueryBuilder
    {
        static public  string CaptionAlias
        {
            get { return "caption"; }
        }

        public const string DescriptionAlias = "description";
        
        internal static string WithPartName
        {
            get { return "cteSource"; }
        }


        public string CaptionAliasFor(string fieldName)
        {
            if (fieldName.ToLower() == "id")
                return CaptionAlias;
            return _addCaption.CaptionFieldNameForLink(fieldName);
        }

        public string DescriptionAliasFor(string fieldName)
        {
            if (fieldName.ToLower() == "id")
                return DescriptionAlias;
            return _addDescription.DescriptionFieldNameForLink(fieldName);
        }

        protected AddCaptions _addCaption;
        protected AddDescriptions _addDescription;

        protected SelectQueryBuilder(IEntity entity, List<string> fields)
            : base(entity, fields)
        {

        }
    }

    internal class SelectQueryBuilder<TImplementation> : SelectQueryBuilder where TImplementation : ImplementationRevert, new()
    {


        protected override void InitPrivateDecorators()
        {

        }

        static private List<String> BuildFields(IEnumerable<IGrouping<IEntityField, IOrganizationRightInfo>> rights, Dictionary<string, object> fromValues = null, bool withCaption = false)
        {

            var result = rights.Select(r => r.Key.Name).ToList();
            if (fromValues != null)
                result = result.Where(r => fromValues.Any(kvp => kvp.Key.ToLower() == r.ToLower())).ToList();
            if (!result.Contains("id"))
                result.Add("id");
            if (withCaption)
                result.Add(CaptionAlias);

            return result;
        }


        private static Dictionary<string, object> BuildSource(
            IEnumerable<IGrouping<IEntityField, IOrganizationRightInfo>> rights, Dictionary<string, object> fromValues, int itemId, string caption = null)
        {
            if (fromValues == null)
                return null;
            var result = new Dictionary<string, object>(fromValues, fromValues.Comparer);
            result =
                result.Where(kvp => rights.Any(r => r.Key.Name.ToLower() == kvp.Key.ToLower()))
                          .ToDictionary(kvp => kvp.Key, kvp => kvp.Value, fromValues.Comparer);
            result["id"] = itemId;
            if (caption != null)
                result[CaptionAlias] = caption;
            return result;
        }

        public SelectQueryBuilder(IEntity entity, int? itemId,
                                  IOrganizationRightData rights, Dictionary<string, object> fromValues)
            : this(entity, itemId.HasValue ? itemId.Value : 1, rights)
        {
            _source = BuildSource(rights.Rights, fromValues, itemId.HasValue ? itemId.Value : 1);
        }


        public SelectQueryBuilder(IEntity entity, string captionValue,
                                  IOrganizationRightData rights, Dictionary<string, object> fromValues)
            : this(entity, new[] { 1 }, rights, true)
        {
            _source = BuildSource(rights.Rights, fromValues, 1, captionValue);
        }



        public SelectQueryBuilder(IEntity entity, int itemId,
                                  IOrganizationRightData rights)
            : this(entity, new[] { itemId }, rights)
        {

        }

        public SelectQueryBuilder(IEntity entity, int[] itemIds,
                                  IOrganizationRightData rights)
            : this(entity, itemIds, rights, false)
        {

        }


        private SelectQueryBuilder(IEntity entity, int[] itemIds,
                                  IOrganizationRightData rights, bool withCaption)
            : this(entity, itemIds, BuildFields(rights.Rights, null, withCaption), rights)
        {

        }


        private SelectQueryBuilder(IEntity entity,
                                            int[] itemIds, List<String> fields, IOrganizationRightData rights)
            : base(entity, fields)
        {

            var filterConditions = new FilterConditions { Type = LogicOperator.Or };
            filterConditions.Operands = new List<IFilterConditions>();
            foreach (int itemId in itemIds)
            {
                var fcChild = new FilterConditions
                                  {
                                      Operator = ComparisionOperator.Equal,
                                      Field = "id",
                                      Value = itemId,
                                      Type = LogicOperator.Simple
                                  };
                filterConditions.Operands.Add(fcChild);
            }
            QueryDecorators.Add(new OrganizationRightsDecorator<TImplementation>(rights));
            QueryDecorators.Add(new AddWhere(filterConditions, LogicOperator.And));
            QueryDecorators.Add(new AddThisCaption(captionAlias: "caption"));
            _addCaption = new AddCaptions(rights.Rights.Where(g => g.Key.Name.ToLower() != "id").Select(g => g.Key));
            _addDescription = new AddDescriptions(rights.Rights.Where(g => g.Key.Name.ToLower() != "id").Select(g => g.Key));
            QueryDecorators.Add(_addCaption);
            QueryDecorators.Add(_addDescription);

        }



        private Dictionary<string, object> _source;


        public override TSqlStatement GetTSqlStatement()
        {
            if (_source == null)
                return base.GetTSqlStatement();
            else
            {

                var fields = _source.Where(kvp => Fields.Any(f => kvp.Key.ToLower() == f.ToLower())).Select(kvp => Helper.CreateColumn(kvp.Value.ToLiteral(), kvp.Key)).Cast<TSqlFragment>().ToList();
                var queryExpression = new QuerySpecification();
                fields.ForEach(
                    f => queryExpression.SelectElements.Add(f)
                    );
                var result = new SelectStatement();
                result.WithCommonTableExpressionsAndXmlNamespaces = new WithCommonTableExpressionsAndXmlNamespaces();
                result.WithCommonTableExpressionsAndXmlNamespaces.CommonTableExpressions.Add(
                    queryExpression.ToSubquery().ToCommonTableExpression(WithPartName)
                    );

                result.QueryExpression = Helper.CreateQuerySpecification(Helper.CreateSchemaObjectTableSource("", WithPartName, AliasName), _source.Select(kvp => Helper.CreateColumn(Helper.CreateColumn(AliasName, kvp.Key), kvp.Key)).Cast<TSqlFragment>().ToList());

                return result;

            }
       }




    }
}
