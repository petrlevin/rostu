using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace Platform.Caching.Common
{
    public class InvalidCommandException: Exception
    {
        public SqlCommand Command { get; private set; }

        public InvalidCommandException(string message) : base(message)
        {
        }

        public InvalidCommandException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected InvalidCommandException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }


        public InvalidCommandException(string message, SqlCommand command):this(message)
        {
            Command = command;
        }
    }
}
