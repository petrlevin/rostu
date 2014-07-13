using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Data.Schema.ScriptDom;
using Platform.Common.Exceptions;

namespace Platform.SqlObjectModel
{
    public class SqlParseException: PlatformException
    {
        public SqlParseException(string message, Exception innerException) : base(message, innerException)
        {
        }

        public SqlParseException(string message) : base(message)
        {
        }

        public SqlParseException()
        {
        }

        public IList<ParseError> Errors { get; set; }
    }
}
