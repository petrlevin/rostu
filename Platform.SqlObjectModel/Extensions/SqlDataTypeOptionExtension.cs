using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Data.Schema.ScriptDom.Sql;

namespace Platform.SqlObjectModel.Extensions
{
    public static class SqlDataTypeOptionExtension
    {
        public static SqlDataType ToSqlDataType(this SqlDataTypeOption sqlDataTypeOption)
        {
            return new SqlDataType()
                       {
                           SqlDataTypeOption = sqlDataTypeOption
                       };
        }
    }
}
