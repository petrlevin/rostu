using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NAnt.Core;
using NAnt.Core.Attributes;
using Tools.MigrationHelper.Helpers;

namespace Tools.MigrationHelper.Core.Tasks
{
    public abstract class DeployBase : SourceTask
    {
        [TaskAttribute("devid", Required = false)]
        public int DevId
        {
            get;
            set;
        }



		/// <summary>
		/// Признак того, что MH запущен с машины разработчика. 
		/// false - на тестовом или рабочем сервере.
		/// </summary>
		/// <returns></returns>
        protected bool IsDeveloper()
        {
            return DevId > 0;
        }

    }
}
