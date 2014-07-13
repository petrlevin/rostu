using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using BaseApp.Environment;
using BaseApp.Environment.Interfaces;
using BaseApp.Environment.Storages;

using Platform.Environment;

namespace BaseApp.Environment
{
	/// <summary>
	/// Объект среды (BaseAppEnvironment).
	/// </summary>
    public  class BaseAppEnvironment : PlatformEnvironment<ApplicationStorage, SessionStorage, RequestStorage>
	{
        /// <summary>
        /// Конструктор для теститирования
        /// </summary>
        /// <param name="httpContextProvider"></param>
        internal BaseAppEnvironment(IHttpContextProvider httpContextProvider)
            : base(httpContextProvider)
	    {
	    }
        /// <summary>
        /// Рабочий конструктор
        /// </summary>
        public BaseAppEnvironment(HttpApplication httpApplication)
            : base(httpApplication)
        {
        }

	    /// <summary>
	    /// 
	    /// </summary>
	    /// <param name="applicationStorage"></param>
	    /// <returns></returns>
	    /// <exception cref="ArgumentNullException"></exception>
	    public override Platform.Environment.Interfaces.IEnvironment<ApplicationStorage, SessionStorage, RequestStorage> ApplicationStart(ApplicationStorage applicationStorage)
        {
            Instance = this;
            return base.ApplicationStart(applicationStorage);
        
        }

	    /// <summary>
	    /// 
	    /// </summary>
	    static public BaseAppEnvironment Instance
	    {
	        get; set;
        }
	}
}
