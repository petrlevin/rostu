using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Platform.Dal.Common.Interfaces;

namespace Platform.Environment.Interfaces
{
    /// <summary>
    /// Хранилище уровня запроса
    /// </summary>
    public interface IRequestStorageBase
    {
        SqlConnection DbConnection { get; set; }

		List<TSqlStatementDecorator> Decorators { get; set; }

		List<ITSqlStatementValidator> Validators { get; set; }
    }
}
