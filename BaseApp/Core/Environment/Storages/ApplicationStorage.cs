using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Practices.Unity;
using Platform.Common;
using Platform.Common.Interfaces;

namespace BaseApp.Environment.Storages
{
	public class ApplicationStorage : ISharedStorage
	{
		#region Implementation of ISharedStorage

		public IUnityContainer Container { get; set; }
		public void InitUnityContainer(IUnityContainer container)
		{
			this.Container = container;
		}

		#endregion
	}
}
