using System.Data;
using System.Data.SqlClient;
using Platform.BusinessLogic;

namespace Sbor
{
    public partial class DataContext
    {
        static partial void OnStaticConstruct()
        {
            DbContextExtension.RegisterContextName(typeof(DataContext), " прикладного решения ");
        }

	    public object ExecuteScalarCommand(string sql)
	    {
		    object result = null;
			using (SqlCommand command = (SqlCommand)Database.Connection.CreateCommand())
			{
				if (Database.Connection.State != ConnectionState.Open)
					Database.Connection.Open();
				command.CommandText = sql;
				command.CommandTimeout = command.Connection.ConnectionTimeout;
				result = command.ExecuteScalar();
			}
		    return result;
	    }
    }
}
