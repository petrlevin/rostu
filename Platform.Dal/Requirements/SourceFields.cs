using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Platform.Dal.Interfaces;

namespace Platform.Dal.Requirements
{
    public class SourceFields :IRequirement
    {
        public ICollection<String> Fields { get; set; }
    }
}
