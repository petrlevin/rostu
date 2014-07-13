using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Data.Schema.ScriptDom.Sql;
using Platform.Dal.Common.Interfaces.QueryBuilders;
using Platform.Dal.Decorators.Abstract;
using Platform.PrimaryEntities.Common.DbEnums;
using Platform.PrimaryEntities.Common.Interfaces;
using Platform.SqlObjectModel;
using Platform.SqlObjectModel.Extensions;

namespace Platform.Dal.Decorators
{
    public class MultilinkAsString : SelectDecoratorBase
    {
        private ISelectQueryBuilder builder { get; set; }

        protected override TSqlStatement Decorate(SelectStatement source, IQueryBuilder queryBuilder)
        {
            builder = (queryBuilder as ISelectQueryBuilder);
            IEnumerable<IEntityField> multilinks = getMultilinks();
            if (!multilinks.Any())
            {
                return source;
            }

            SelectStatement result = source;
            addMultilinks(multilinks, result);
            return result;
        }

        private IEnumerable<IEntityField> getMultilinks()
        {
            IEnumerable<string> selectedFields = builder.Fields != null && builder.Fields.Any()
                ? builder.Fields
                : builder.Entity.Fields.Select(ef => ef.Name);

            IEnumerable<IEntityField> multilinks = builder.Entity.Fields.Where(ef => ef.EntityFieldType == EntityFieldType.Multilink);

            return multilinks.Where(ml => selectedFields.Contains(ml.Name));
        }

        private void addMultilinks(IEnumerable<IEntityField> multilinks, SelectStatement source)
        {
            List<Field> mlFields = multilinks.Select(ml => new Field
                {
                    Alias = ml.Name,
                    Experssion = Helper.CreateFunctionCall("[dbo].[MultilinkAsString]", new List<Expression>()
                        {
                            ml.EntityLink.Id.ToLiteral(),
                            builder.Entity.Id.ToLiteral(),
                            string.Format("{0}.{1}", builder.AliasName, "id").ToColumn()
                        })
                }).ToList();

            source.Fields(mlFields);
        }
    }
}
