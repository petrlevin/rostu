using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tools.MigrationHelper.Tasks
{
	class DeployAppDb : MhTaskBase
	{
		public override int Process(MhConfiguration config, System.IO.TextWriter output)
		{
			base.Process(config, output);
			CreateMigrationHelper();
			migrationHelper.DeployAppDb();
			Report = migrationHelper.Report;
			return Success;
		}
	}
}
