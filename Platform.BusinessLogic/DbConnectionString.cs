using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Platform.BusinessLogic
{
    /// <summary>
    /// Строка соединения с базой данных
    /// </summary>
    public class DbConnectionString : ConnectionString<DbConnectionString>
    {
        public static string Get()
        {
            return Instance.Builder.ToString();
        }

        protected override SqlConnectionStringBuilder get()
        {
            return new SqlConnectionStringBuilder(ConfigurationManager.ConnectionStrings["PlatformDBConnectionString"].ConnectionString);
        }
    }
}
