using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using BaseApp.Environment;
using BaseApp.Environment.Interfaces;
using BaseApp.Environment.Storages;
using Microsoft.Practices.Unity;
using Platform.Common;
using Platform.Common.Interfaces;

namespace BaseApp.Environment
{
	/// <summary>
	/// Объект среды (Environment).
	/// </summary>
	public abstract class EnvironmentBase : IEnvironment
	{
		#region Implementation of IEnvironment

		public virtual ApplicationStorage ApplicationStorage { get; protected set; }
		public virtual SessionStorage SessionStorage { get; protected set; }
		public virtual RequestStorage RequestStorage { get; protected set; }

		public virtual IEnvironment ApplicationStart(ApplicationStorage appStor)
		{
			this.ApplicationStorage = appStor;
			this.ApplicationStorage.InitUnityContainer(new UnityContainer());
			return this;
		}

		public virtual IEnvironment SessionStart(SessionStorage sessionStor)
		{
			this.SessionStorage = sessionStor;
			this.SessionStorage.InitUnityContainer(this.ApplicationStorage.Container.CreateChildContainer());
			return this;
		}

		public virtual IEnvironment RequestStart(RequestStorage reqStor)
		{
			this.RequestStorage = reqStor;
			this.RequestStorage.InitUnityContainer(this.SessionStorage.Container.CreateChildContainer());
			return this;
		}

		#endregion

		#region Implementation of IDependencyResolver

		public void InitWith(IUnityContainerProvider env)
		{
			throw new NotImplementedException();
		}

		public T Resolve<T>()
		{
			return RequestStorage.Container.Resolve<T>();
		}

		public IEnumerable<T> ResolveAll<T>()
		{
			return RequestStorage.Container.ResolveAll<T>();
		}

		#endregion

		#region Implementation of IUnityContainerProvider

		public abstract IUnityContainer Container { get; }

		#endregion
	}
}
