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
    public class SimpleSelectBuilder :SelectBuilderBase
    {
        private IEntity _entity;

        #region Public

        /// <summary>
        /// 
        /// </summary>
        /// <param name="entity"></param>
        public SimpleSelectBuilder(IEntity entity)
        {
            _entity = entity;
        }


        protected override TSqlFragment BuildField(IEntityField f, string fromAlias)
        {
            return f.Name.ToColumn();
        }



        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public SerializationCommandFactory Build()
        {
            return new SerializationCommandFactory(BuildStatement().Render());
        }

        #endregion



        protected override IEntity Entity
        {
            get { return _entity; }
        }



        protected override void AddThis(QuerySpecification queryExpression)
        {
            base.AddThis(queryExpression);
            queryExpression.AddWhere(Helper.CreateBinaryExpression(String.Format("{0}.Id", GetMainAlias()).ToColumn(), SerializationCommandFactory.GetThisParameter(), BinaryExpressionType.Equals));


        }






    }
}
