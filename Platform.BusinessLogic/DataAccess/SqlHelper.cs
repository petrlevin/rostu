using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Platform.Log;

namespace Platform.BusinessLogic.DataAccess
{
    public class SqlHelper
    {
        public static GridResult MakeResult(SqlCommand cmd)
        {
            var result = new GridResult();
            result.Rows = new List<IDictionary<string, object>>();
            result.Count = -1;
            using (SqlDataReader reader = cmd.ExecuteReaderLog())
            {
                Dictionary<string, object> row;
                while (reader.Read())
                {
                    row = new Dictionary<string, object>();
                    for (int col = 0; col <= reader.FieldCount - 1; col++)
                    {
                        string columnName = reader.GetName(col).ToLowerInvariant();
                        if (!columnName.Equals("_totalrow"))
                        {
                            row.Add(columnName, reader[col]);
                        }
                        if (columnName.Equals("_totalrow") && result.Count == -1)
                        {
                            result.Count = reader.GetInt32(col);
                        }
                    }

                    result.Rows.Add(row);
                }
				reader.Close();
            }
            return result;
        }

    }
}
