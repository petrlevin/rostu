using System;

namespace Tools.MigrationHelper.Core.Tasks.CheckTask
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
