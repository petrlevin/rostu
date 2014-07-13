using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using BaseApp.Environment.Dependencies;
using BaseApp.DbEnums;
using Microsoft.Practices.Unity;
using Platform.Common;
using Platform.Common.Interfaces;

namespace BaseApp.Environment.Storages
{
	public class RequestStorage : ISharedStorage
	{
		public SqlConnection DbConnection { get; set; }

		#region Implementation of ISharedStorage

		public IUnityContainer Container { get; set; }
		public void InitUnityContainer(IUnityContainer container)
		{
			this.Container = container;
			container.RegisterInstance(Names.DbConnection, this.DbConnection);
		}

		#endregion
	}
}
