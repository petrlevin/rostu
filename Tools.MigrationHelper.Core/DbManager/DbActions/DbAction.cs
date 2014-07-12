using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using Platform.PrimaryEntities;
using Tools.MigrationHelper.Core.DbManager.DbActions.Interfaces;

namespace Tools.MigrationHelper.Core.DbManager.DbActions
{
    public class DbAction 
    {
        /// <summary>
        /// Строка
        /// </summary>
        public DataRow Row { get; set; }

        /// <summary>
        /// Действия, которые должны предшествовать данному
        /// </summary>
        public List<DbAction> DependsOn { get; set; }

        /// <summary>
        /// ActionBatch к которому принадлежит данный DbAction
        /// </summary>
        public IDbActionBatch ActionBatch { get; set; }

        /// <summary>
        /// Имя сущности которой принадлежит строка
        /// </summary>
        public string TableName { get; set; }

        /// <summary>
        /// Имя сущности которой принадлежит строка
        /// </summary>
        public string EntityName
        {
            get { return TableName.Split('.')[1]; }
        }

        /// <summary>
        /// Тип сущности которой принадлежит строка
        /// </summary>
        public int IDEntityType
        {
            get { return (int) Schemas.EntityTypeBySchema(TableName.Split('.')[0]); }
        }

        public List<SqlCommand> GetCommand()
        {
            return ActionBatch.GetCommand(Row);
        }
    }
}
