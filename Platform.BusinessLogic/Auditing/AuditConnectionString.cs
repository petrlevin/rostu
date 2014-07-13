using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Platform.BusinessLogic.Auditing
{
    /// <summary>
    /// Строка соединения с БД аудита
    /// </summary>
    public class AuditConnectionString : ConnectionString<AuditConnectionString>
    {
        public static string Get()
        {
            return Instance.Builder.ToString();
        }

        protected override SqlConnectionStringBuilder get()
        {
            var conStrBuilder = new SqlConnectionStringBuilder(DbConnectionString.Get());
            conStrBuilder.InitialCatalog += "_Audit";
            return conStrBuilder;
        }
    }
}
