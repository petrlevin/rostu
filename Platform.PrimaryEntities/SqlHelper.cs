using System.Data;
using System.Data.SqlClient;

namespace Platform.PrimaryEntities
{
    public static class SqlHelper
    {
        static public DataTable GetTable(SqlCommand select)
        {
            var adapter = new SqlDataAdapter(select);

            var dataTable = new DataTable();
            adapter.Fill(dataTable);
            return dataTable;

        }

    }

}
