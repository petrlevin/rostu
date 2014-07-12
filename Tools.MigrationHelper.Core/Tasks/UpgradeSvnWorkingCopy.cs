using System;
using NAnt.Core;
using NAnt.Core.Attributes;
using SharpSvn;

namespace Tools.MigrationHelper.Core.Tasks
{
    [TaskName("UpgradeSvnWorkingCopy")]
    public class UpgradeSvnWorkingCopy : SourceTask
    {
        protected override void ExecuteTask()
        {
            try
            {
                using (var svnClient = new SvnClient())
                {
                    svnClient.Upgrade(SourcePath);
                }
            }
            catch(Exception e)
            {
                Log(Level.Info, "Upgrade не прошел." + e.Message);
            }
        }
    }
}
