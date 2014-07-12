using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Platforms.Tests.Common
{
	[ExcludeFromCodeCoverage]
	public class SqlTestBase
    {
        private SqlConnection _connection;
        protected SqlConnection connection
        {
            get
            {
                if (_connection == null)
                {
                    _connection = new SqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings["PlatformDBConnectionString"].ConnectionString);
                }
                if (_connection.State != ConnectionState.Open)
                {
                    _connection.Open();
                }
                return _connection;
            }
        }

        protected string connectionString
        {
            get
            {
                return
                    System.Configuration.ConfigurationManager.ConnectionStrings["PlatformDBConnectionString"]
                        .ConnectionString;
            }
        }


    }
}
