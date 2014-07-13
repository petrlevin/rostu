using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BaseApp.DbEnums;
using BaseApp.Environment.Dependencies;
using BaseApp.References;
using BaseApp.SystemDimensions;
using Microsoft.Practices.Unity;

namespace BaseApp.Tests.Unity.Fixtures
{
	/// <summary>
	/// Коллекция зависимых классов
	/// </summary>
	[ExcludeFromCodeCoverage]
	public class Depends
	{
		public class OnDbConnection
		{
			[Dependency(Names.DbConnection)]
			public SqlConnection DbConnection { get; set; }
			
			public SqlConnection OtherConnection { get; set; }
		}

		public class OnCurrentUserAndDimensions
		{
			[Dependency(Names.CurrentUser)]
			public User CurrentUser { get; set; }

			[Dependency(Names.CurentDimensions)]
			public SysDimensionsState CurentDimensions { get; set; }
		}
	}
}
