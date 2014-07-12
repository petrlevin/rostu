using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tools.MigrationHelper.DbManager;

namespace Tools.MigrationHelper.Tasks
{
	class CreateDistributive : MhTaskBase
	{
		public override int Process(MhConfiguration config, TextWriter output)
		{
			base.Process(config, output);
			CreateMigrationHelper();
			var outDir = new DirectoryInfo(_config.OutputDir);
			if (!outDir.Exists)
				Directory.CreateDirectory(_config.OutputDir);

			var metadata = new Metadata();
			metadata.FromFs(_config.SourcePath);
			metadata.WhriteDataSet(_config.OutputDir);
			
			return Success;
		}
	}
}
