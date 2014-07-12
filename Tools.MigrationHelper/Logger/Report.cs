using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tools.MigrationHelper.Logger
{
	public class Report
	{
		public List<Message> Messages = new List<Message>();

		public void WriteTo(TextWriter output)
		{
			string delimeter = string.Format("{0}{1}{2}{3}{4}", Environment.NewLine, Environment.NewLine, "<===>", Environment.NewLine, Environment.NewLine);
			output.WriteLine(DateTime.Now.ToString());
			output.WriteLine(String.Join(delimeter, Messages.Select(m => m.ToString()).ToArray()));
		}
	}
}
