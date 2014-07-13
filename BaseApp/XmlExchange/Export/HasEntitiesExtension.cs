using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Data.Schema.ScriptDom.Sql;
using Platform.BusinessLogic;
using Platform.BusinessLogic.Common.Interfaces;
using Platform.PrimaryEntities.Common.DbEnums;
using Platform.SqlObjectModel;
using Platform.SqlObjectModel.Extensions;

namespace BaseApp.XmlExchange.Export
{
    public static class HasEntitiesExtension
    {
        internal static SelectColumn CreateSelectColumnForHierarhyDoc(this IHasEntities builder,string tableAlias, string fieldAliasSource, string fieldEntityAliasSource , string fieldAlias)
        {


            return CreateColumnForHierarhyDoc(builder, tableAlias, fieldAliasSource, fieldEntityAliasSource).ToSelectColumn(fieldAlias);
            ;
        }

        internal static Expression CreateColumnForHierarhyDoc(this IHasEntities builder, string tableAlias, string fieldAliasSource, string fieldEntityAlias)
        {
            var hierarhyDocs = builder.GetHierarhyDocIds();

            return Helper.CreateCaseExpression(null, String.Format("{0}.{1}", tableAlias, fieldAliasSource).ToColumn(),
                                               Helper.CreateWhenClause(
                                                   Helper.CreateInPredicate(
                                                       String.Format("{0}.{1}", tableAlias, fieldEntityAlias).ToColumn(),
                                                       hierarhyDocs),
                                                   Helper.CreateFunctionCall("GetLastVersionId", "dbo",
                                                                             String.Format("{0}.{1}", tableAlias,
                                                                                           fieldEntityAlias),
                                                                             String.Format("{0}.{1}", tableAlias,
                                                                                           fieldAliasSource))));
            ;
        }


        internal static Expression CreateColumnForHierarhyDoc(this IHasEntities builder, string tableAlias,
                                                                string fieldAliasSource , int idEntity)
        {
            return Helper.CreateFunctionCall("GetLastVersionId", "dbo", idEntity,
                                             String.Format("{0}.{1}", tableAlias, fieldAliasSource));
        }


        internal static IEnumerable<Int32> GetHierarhyDocIds(this IHasEntities builder)
        {
            return  builder.Entities.Where(e => e.IdEntityType == (byte)EntityType.Document)
                                           .ToList()
                                           .Where(e => e.Is<IHierarhy>())
                                           .Select(e => e.Id);
            
        }

        internal static bool IdIsOfHierarhyDoc(this IHasEntities builder, int entityId)
        {
            return GetHierarhyDocIds(builder).Contains(entityId);
        }



    }
}
