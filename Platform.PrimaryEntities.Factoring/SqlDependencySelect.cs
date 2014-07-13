using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;

using Platform.Caching.Common;
using Platform.Utils;

namespace Platform.PrimaryEntities.Factoring
{

    public class SqlDependencySelect : Select
    {

        internal ICache Cache { get; set; }
    

        public virtual SqlCommand SqlDependencyCommand
        {
            get { return SqlCommand;  }
            
        }

        protected override string GetColumns(string metadataName)
        {
            string result=null;
            if (Cache == null)
                throw new InvalidOperationException("Кэш не определен (null)");
             result = Cache.Get<String>("Columns", metadataName);
            if (result!= null)
                return result;

            var command = BuildCommand(metadataName);
            result = BuildColumns(SqlHelper.GetTable(command));

            Cache.Put(result,"Columns", metadataName);
            return result;

        }

        private SqlCommand BuildCommand(string metadataName)
        {
            return new SqlCommand(String.Format(SelectcolumnsTmpl, metadataName)) {Connection = DbConnection};

        }


        private string BuildColumns(DataTable dataTable)
        {
            var sb = new StringBuilder();
            foreach (DataRow row in dataTable.Rows)
            {
                sb.Append("[");
                sb.Append(row["name"]);
                sb.Append("]");
                sb.Append(",");
            }
            return sb.ToString().Substring(0, sb.ToString().Length - 1);

        }


        private const string SelectcolumnsTmpl =
            "select  [sys].[columns] .name  from [sys].[columns] INNER JOIN [sys].[tables]  ON [sys].[columns].object_id= [sys].[tables].object_id where [sys].[tables].[name] ='{0}' ";

    }
}
