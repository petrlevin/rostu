using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Platform.Common.Exceptions;

namespace Platform.Dal.Exceptions
{
    public class XmlSerializationException: PlatformException
    {
        public XmlSerializationException(string message, Exception innerException) : base(message, innerException)
        {
        }

        public XmlSerializationException(string message) : base(message)
        {
        }

        public XmlSerializationException()
        {
        }
    }
}
