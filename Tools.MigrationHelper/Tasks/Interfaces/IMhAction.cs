using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tools.MigrationHelper.Logger;

namespace Tools.MigrationHelper.Tasks.Interfaces
{
	interface IMhAction
	{
		int Process(MhConfiguration config, TextWriter output);
		Report Report { get; set; }
	}
}
