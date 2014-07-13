using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;

namespace Platform.PrimaryEntities.Factoring
{
    public class PerTableSqlDependencySelect:SqlDependencySelect
    {
        private SqlCommand _sqlDependencyCommand;
        public override System.Data.SqlClient.SqlCommand SqlDependencyCommand
        {
            get
            {
                if (_sqlDependencyCommand == null)
                {
                    _sqlDependencyCommand = DbConnection.CreateCommand();
                    _sqlDependencyCommand.CommandText = String.Format("SELECT {0} FROM [ref].[{1}] ",
                                                                      GetColumns(MetadataName), MetadataName);
                }
                return _sqlDependencyCommand;

            }
        }
    }
}
