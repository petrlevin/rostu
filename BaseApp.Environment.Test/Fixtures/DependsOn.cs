using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics.CodeAnalysis;
using BaseApp.Common.Interfaces;
using BaseApp.Environment.Dependencies;
using BaseApp.SystemDimensions;
using Microsoft.Practices.Unity;
using Platform.Dal.Common.Interfaces;

namespace BaseApp.Environment.Tests.Fixtures
{
	/// <summary>
	/// Коллекция зависимых классов
	/// </summary>
	[ExcludeFromCodeCoverage]
	public class Depends
	{
		public class OnDbConnection
		{
			[Dependency(Platform.Environment.Dependencies.Names.DbConnection)]
			public SqlConnection DbConnection { get; set; }
			
			public SqlConnection OtherConnection { get; set; }
		}

		public class OnCurrentUserAndDimensions
		{
			[Dependency(Names.CurrentUser)]
			public IUser CurrentUser { get; set; }

			[Dependency(Names.CurentDimensions)]
			public SysDimensionsState CurentDimensions { get; set; }
		}

		public class OnDecorators
		{
			[Dependency(Names.Decorators)]
			public List<TSqlStatementDecorator> Decorators { get; set; }
		}
	}
}
