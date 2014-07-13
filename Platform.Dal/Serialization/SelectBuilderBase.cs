using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Data.Schema.ScriptDom.Sql;
using Platform.PrimaryEntities.Common.Interfaces;
using Platform.SqlObjectModel;
using Platform.SqlObjectModel.Extensions;

namespace Platform.Dal.Serialization
{
    public abstract class SelectBuilderBase
    {


        #region Public

        /// <summary>
        /// 
        /// </summary>
        /// <param name="entity"></param>





        #endregion

        protected virtual TSqlStatement BuildStatement(string tag = "root")
        {


            var queryExpression = BuildSpecification();
            var result = new SelectStatement { QueryExpression = queryExpression };
            AddForClause(result, tag);
            return result;

        }



        protected virtual QuerySpecification BuildSpecification()
        {
            var queryExpression = new QuerySpecification();
            AddThis(queryExpression);
            return queryExpression;
        }

        protected abstract IEntity Entity { get; }

        protected static void AddForClause(SelectStatement @select, string pathValue)
        {
            @select.AddForClause(pathValue);
        }


        protected virtual void AddThis(QuerySpecification queryExpression)
        {
            AddFields(queryExpression, Entity);
            queryExpression.FromClauses.Add(Helper.CreateSchemaObjectTableSource(Entity.Schema, Entity.Name, GetMainAlias()));
        }


        protected void AddFields(QuerySpecification queryExpression, IEntity entity)
        {
            var fields = Fields.For(entity);

            fields.ForEach(
                f => AddField(queryExpression,f))
                ;
        }


        private void AddField(QuerySpecification queryExpression, IEntityField f)
        {
            queryExpression.SelectElements.Add(

                BuildField(f)
                );
        }

        protected virtual TSqlFragment BuildField(IEntityField f)
        {
            return BuildField(f, GetMainAlias());
        }

        protected virtual TSqlFragment BuildField(IEntityField f , string fromAlias)
        {
            return String.Format("{0}.{1}.{2}", fromAlias, f.Name, GetFieldAlias(f)).ToSelectColumn(false);
        }



        protected virtual string GetFieldAlias(IEntityField entityField)
        {
            return entityField.Name;
        }

        protected string GetMainAlias()
        {
            return "doc";
        }





    }
}
