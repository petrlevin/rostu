using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tools.MigrationHelper.Tasks
{
	class Help : MhTaskBase
	{
		public override int Process(MhConfiguration config, TextWriter output)
		{
			base.Process(config, output);
			output.WriteLine("this is help");
			return Success;
		}
	}
}
