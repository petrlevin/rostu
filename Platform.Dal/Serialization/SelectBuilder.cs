using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Data.Schema.ScriptDom.Sql;
using Platform.PrimaryEntities;
using Platform.PrimaryEntities.Common.DbEnums;
using Platform.PrimaryEntities.Common.Interfaces;
using Platform.PrimaryEntities.Factoring;
using Platform.SqlObjectModel;
using Platform.SqlObjectModel.Extensions;

namespace Platform.Dal.Serialization
{
    /// <summary>
    /// Посnроитель запроса на получение сериализованного в XML элемента
    /// </summary>
    public class SelectBuilder :SimpleSelectBuilder
    {
        #region Private




        private void AddMultilinks(QuerySpecification queryExpression)
        {
            Fields.For(Entity,Options.Multilink).ForEach(
                f => queryExpression.SelectElements.Add(Helper.CreateColumn(CreateMultilinkSubquery(f), f.Name))
                );
            
        }


        private void AddTableParts(QuerySpecification queryExpression)
        {
            Fields.For(Entity, Options.TableParts).ForEach(
                f => queryExpression.SelectElements.Add(Helper.CreateColumn(CreateTablePartSubquery(f),f.Name))
                );
        }






        Subquery CreateTablePartSubquery(IEntityField field)
        {
            
            var subquery = new QuerySpecification();
            AddFields(subquery,field.EntityLink);
            subquery.FromClauses.Add(Helper.CreateSchemaObjectTableSource("tp", field.EntityLink.Name));
            subquery.AddWhere(Helper.CreateBinaryExpression(field.OwnerField().Name.ToColumn(), String.Format("{0}.Id",GetMainAlias()).ToColumn(),BinaryExpressionType.Equals));
            var select = new SelectStatement();
            
            select.QueryExpression = subquery;
            AddForClause(@select,"item");
            return select.ToSubquery();

        }




        private Subquery CreateMultilinkSubquery(IEntityField field)
        {
            var subquery = new QuerySpecification();
            var rightField = MultilinkHelper.GetRightMultilinkField(field.EntityLink, Entity.Id);
            var leftField = MultilinkHelper.GetLeftMultilinkField(field.EntityLink, Entity.Id);
            subquery.SelectElements.Add(rightField.Name.ToColumn());
            subquery.FromClauses.Add(Helper.CreateSchemaObjectTableSource("ml", field.EntityLink.Name));
            subquery.AddWhere(Helper.CreateBinaryExpression(leftField.Name.ToColumn(), String.Format("{0}.Id", GetMainAlias()).ToColumn(), BinaryExpressionType.Equals));
            var select = new SelectStatement();
            select.QueryExpression = subquery;
            AddForClause(select,"");
            return select.ToSubquery();

        }

        #endregion

        #region Protected & Internal

        protected override QuerySpecification BuildSpecification()
        {
            var queryExpression = base.BuildSpecification();
            AddTableParts(queryExpression);
            AddMultilinks(queryExpression);
            return queryExpression;

        }

        #endregion

        #region Public

        /// <summary>
        /// 
        /// </summary>
        /// <param name="entity"></param>
        public SelectBuilder(IEntity entity) :base(entity)
        {

        }



        #endregion

    }
}
