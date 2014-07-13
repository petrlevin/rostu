using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Platform.Unity.Common
{
    [AttributeUsage(AttributeTargets.Class|AttributeTargets.Interface)]
    public class AutoRegistrationAttribute: Attribute
    {
    }
}
