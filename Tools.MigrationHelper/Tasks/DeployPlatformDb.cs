using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tools.MigrationHelper.Tasks
{
	class DeployPlatformDb : MhTaskBase
	{
		public override int Process(MhConfiguration config, System.IO.TextWriter output)
		{
			base.Process(config, output);
			this.CreateMigrationHelper();
			migrationHelper.DeployPlatformDb();
			//ToFs(false);
			Report = migrationHelper.Report;
			return Success;
		}
	}
}
