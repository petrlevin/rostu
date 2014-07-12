using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tools.MigrationHelper.Tasks
{
	class DeployDb : MhTaskBase
	{
		public override int Process(MhConfiguration config, System.IO.TextWriter output)
		{
			base.Process(config, output);
			CreateMigrationHelper();
			migrationHelper.DeployDb();
			Report = migrationHelper.Report;
			return Success;
		}
	}
}
