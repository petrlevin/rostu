using System.Collections.Generic;
using Microsoft.Data.Schema.ScriptDom.Sql;
using Platform.BusinessLogic.Registry;
using Platform.Dal;
using Platform.Dal.Serialization;
using Platform.PrimaryEntities.Common.Interfaces;
using Platform.SqlObjectModel;
using Platform.SqlObjectModel.Extensions;
using Platform.Utils;

namespace Platform.BusinessLogic.Activity.Operations.Serialization
{
    /// <summary>
    /// Построитель запроса на вставку в регистр (Сериализованный элемент сущности)
    ///  </summary>
    public class InsertBuilder:SelectBuilder
    {

        #region Public

        /// <summary>
        /// 
        /// </summary>
        /// <param name="entity"></param>
        public InsertBuilder(IEntity entity) : base(entity)
        {
        }


        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public new SerializationCommandFactory Build()
        {
            return new SerializationCommandFactory(BuildStatement().Render());
        }

        #endregion


        #region Protected && Internal
        protected override TSqlStatement BuildStatement(string tag = "root")
        {

            
            var select = (SelectStatement)base.BuildStatement();
            var insert = new InsertStatement();
            var target = new SchemaObjectDataModificationTarget();
            target.SchemaObject = Helper.CreateSchemaObjectName("reg", typeof(SerializedEntityItem).Name);
            insert.Target = target;
            insert.Columns.Add(Columns.Data);
            insert.Columns.Add(Columns.IdTool);
            insert.Columns.Add(Columns.IdToolEntity);
            insert.InsertOption = InsertOption.Into;
            var insertSource  = new ValuesInsertSource();
            var rowValue = new RowValue();
            rowValue.ColumnValues.Add(select.ToSubquery());
            rowValue.ColumnValues.Add(SerializationCommandFactory.GetThisParameter());
            rowValue.ColumnValues.Add(Entity.Id.ToLiteral());

            insertSource.RowValues.Add(rowValue);
            insert.InsertSource = insertSource;
            return insert;
        }


        #endregion
    }
}
