using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NAnt.Core;
using NAnt.Core.Attributes;

namespace Tools.MigrationHelper.Core.Tasks
{
    [TaskName("enabledistributivedata")]
    public class EnableDistributiveData : CommandTask
    {



        public EnableDistributiveData()
        {
            FailOnError = false;
        }


        protected override void OnException(Exception ex)
        {
            Error("Ошибка при установке режима ввода эталонных данных.", ex);
        }

        protected override void InitializeCommand(SqlCommand command)
        {
            command.CommandType = CommandType.StoredProcedure;
            command.CommandText = "[dbo].[SetDistributiveData]";

        }
    }
}
