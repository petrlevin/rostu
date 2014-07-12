using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NAnt.Core.Attributes;
using Tools.MigrationHelper.Core.DbManager;
using Tools.MigrationHelper.Helpers;

namespace Tools.MigrationHelper.Core.Tasks
{
    [TaskName("enumserialize")]
    public class EnumSerialize : SourceTask
    {
        protected override void ExecuteTask()
        {
            var meta = new DbDataSet(1);//указан рандомный DevId т.к. он должен быть просто > 0
            meta.FromFs(SourcePath);
            meta.WhriteDataSetWithStructure(SourcePath);
        }
    }
}
