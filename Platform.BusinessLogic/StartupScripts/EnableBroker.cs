using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Platform.Application.Common;
using Platform.Common;
using Platform.DbCmd;

namespace Platform.BusinessLogic.StartupScripts
{
    public class EnableBroker : IBeforeAplicationStart
    {
        private static string sqlEnableBroker = @"
            alter database {0} set NEW_BROKER;            
            alter database {0} set enable_broker with rollback immediate;
        ";

        public void Execute()
        {
            var sqlConnBuilder = new SqlConnectionStringBuilder(DbConnectionString.Get());
            using (var conn = new SqlConnection(DbConnectionString.Get()))
            {
                //conn.Open();
                //using (var sqlCmd = conn.CreateCommand())
                //{
                //    sqlCmd.CommandText = string.Format(sqlEnableBroker, sqlConnBuilder.InitialCatalog);
                //    sqlCmd.ExecuteNonQuery();
                //}
            }
        

        }
    }
}
