using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tools.MigrationHelper.Tasks.CheckTask
{
	[AttributeUsage(AttributeTargets.Method)]
	public class TestAttribute : Attribute
	{
		/// <summary>
		/// Игнориорвать тест
		/// </summary>
		public bool Ignore { get; set; }
	}
}
