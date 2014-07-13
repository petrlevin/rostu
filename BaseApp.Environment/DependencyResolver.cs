using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BaseApp.Common.Interfaces;
using BaseApp.Environment.Dependencies;
using BaseApp.Environment.Storages;
using BaseApp.SystemDimensions;
using Microsoft.Practices.Unity;
using Platform.Environment;
using Platform.Environment.Interfaces;

namespace BaseApp.Environment
{
    /// <summary>
    /// 
    /// </summary>
    public class DependencyResolver : DependencyResolver<ApplicationStorage, SessionStorage, RequestStorage>
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="environment"></param>
        public DependencyResolver(IStorageContainer<ApplicationStorage, SessionStorage, RequestStorage> environment) : base(environment)
        {

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="environment"></param>
        /// <param name="unityContainer"></param>
        public DependencyResolver(IStorageContainer<ApplicationStorage, SessionStorage, RequestStorage> environment, IUnityContainer unityContainer)
            : base(environment, unityContainer)
        {

        }

        /// <summary>
        /// Регистрирует динамический экземпляр уровня сессии пользователь , системные измерения 
        /// вызывает базовый
        /// </summary>
        protected override void DefineResolvance()
        {
 	        base.DefineResolvance();
            RegisterSessionInstance("CurrentUser", ss => ss.CurrentUser);
            RegisterSessionInstance("CurentDimensions", ss => ss.CurentDimensions);
            RegisterApplicationInstance("ConnectionString",aps=>aps.ConnectionString);
            RegisterRequestInstance(rs=>rs.DbContext);

            RegisterRequestInstance(rs => rs.ControlDispatcher);
            RegisterRequestType(rs=>rs.Locks);



        }

        
    }
}
