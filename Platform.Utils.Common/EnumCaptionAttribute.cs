using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Platform.Common
{

    public class EnumCaptionAttribute: Attribute
    {
        private readonly string _value;

        public EnumCaptionAttribute(string value)
        {
            _value = value;
        }

        public string Value
        {
            get { return _value; }
        }
    }
}
