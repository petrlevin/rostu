using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using BaseApp.DbEnums;
using BaseApp.References;
using BaseApp.SystemDimensions;
using Microsoft.Practices.Unity;
using Platform.Common;
using BaseApp.Environment.Dependencies;
using Platform.Common.Interfaces;

namespace BaseApp.Environment.Storages
{
	public class SessionStorage : ISharedStorage
	{
		public User CurrentUser { get; set; }
		public SysDimensionsState CurentDimensions { get; set; }

		#region Implementation of ISharedStorage

		public IUnityContainer Container { get; set; }

		/// <summary>
		/// Регистрирует в контейнере Unity значения, хранимые в сессии.
		/// </summary>
		/// <param name="container"></param>
		public void InitUnityContainer(IUnityContainer container)
		{
			this.Container = container;
			container.RegisterInstance(Names.CurrentUser, this.CurrentUser);
			container.RegisterInstance(Names.CurentDimensions, this.CurentDimensions);
		}

		#endregion
	}
}
