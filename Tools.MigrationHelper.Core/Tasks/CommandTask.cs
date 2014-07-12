using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NAnt.Core.Attributes;

namespace Tools.MigrationHelper.Core.Tasks
{
    public abstract class CommandTask :MhTask
    {
        [TaskAttribute("connectionstring", Required = true)]
        public string ConnectionString { get; set; }

        protected override void ExecuteTask()
        {
            try
            {
                using (var connection = new SqlConnection(ConnectionString))
                {
                    connection.Open();
                    using (var command = connection.CreateCommand())
                    {
                        InitializeCommand(command);
                        command.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                OnException(ex);
            }
        }

        protected abstract void OnException(Exception ex);

        protected abstract void InitializeCommand(SqlCommand command);
    }
}
