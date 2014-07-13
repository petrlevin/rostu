using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using BaseApp.Environment.Storages;
using Microsoft.Practices.Unity;
using Platform.Common;

namespace BaseApp.Environment
{
	public class PlatformEnvironment : EnvironmentBase
	{
		#region Overrides
		public override ApplicationStorage ApplicationStorage
		{
			get { return (ApplicationStorage)HttpContext.Current.Application["ApplicationStorage"]; }
			protected set { HttpContext.Current.Application["ApplicationStorage"] = value; }
		}

		public override SessionStorage SessionStorage
		{
			get { return (SessionStorage)HttpContext.Current.Session["SessionStorage"]; }
			protected set { HttpContext.Current.Session["SessionStorage"] = value; }
		}

		public override RequestStorage RequestStorage
		{
			get { return (RequestStorage)HttpContext.Current.Items["RequestStorage"]; }
			protected set { HttpContext.Current.Items["RequestStorage"] = value; }
		}
		#endregion

		#region Implementation of IUnityContainerProvider

		public override IUnityContainer Container
		{
			get { return this.RequestStorage.Container; }
		}

		#endregion
	}
}
