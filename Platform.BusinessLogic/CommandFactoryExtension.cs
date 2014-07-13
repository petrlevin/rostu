using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Platform.Dal;

namespace Platform.BusinessLogic
{
    public static class CommandFactoryExtension
    {
        static public SqlCommand CreateCommand(this SerializationCommandFactory commandFactory, int elementId, SqlConnection connection)
        {
            var result = commandFactory.CreateCommand(elementId);
            result.Connection = connection;
            return result;
        }
    }
}
