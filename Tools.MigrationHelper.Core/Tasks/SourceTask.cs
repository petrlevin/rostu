using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NAnt.Core.Attributes;

namespace Tools.MigrationHelper.Core.Tasks
{
    public abstract class SourceTask: MhTask
    {

        [TaskAttribute("sourcepath", Required = true)]
        public string SourcePath
        {
            get;
            set;
        }

    }
}
