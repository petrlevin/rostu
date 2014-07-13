using BaseApp.Activity.Controls;
using BaseApp.Activity.Operations;
using BaseApp.Numerators;
using Microsoft.Practices.Unity;
using Platform.BusinessLogic.DataAccess;
using Platform.BusinessLogic.Interfaces;
using Platform.Unity.Common.Interfaces;

namespace BaseApp
{
    /// <summary>
    /// 
    /// </summary>
    public  class DependencyInjection : IDefaultRegistration
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="unityContainer"></param>
        public static void RegisterIn(IUnityContainer unityContainer)
        {
            unityContainer.RegisterType(typeof (IOperationDispatcher), typeof (OperationDispatcher));
			unityContainer.RegisterType(typeof(object), typeof(BaseAppNumerators), "DeafaultValues");
	        unityContainer.RegisterType(typeof (IDataManagerFactory), typeof (BaseApp.DataAccess.DataManagerFactory));
            unityContainer.RegisterType(typeof (IControlDispatcher), typeof (ControlDispatcher));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="unityContainer"></param>
        public void Register(IUnityContainer unityContainer)
        {
            RegisterIn(unityContainer);
        }
    }
}
