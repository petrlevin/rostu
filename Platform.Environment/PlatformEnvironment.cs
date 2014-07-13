using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.SessionState;
using Microsoft.Practices.Unity;
using Platform.Common;
using Platform.Environment.Interfaces;

namespace Platform.Environment
{
    public class PlatformEnvironment<TApplicationStorage, TSessionStorage, TRequestStorage> : EnvironmentBase<TApplicationStorage, TSessionStorage, TRequestStorage>
        where TRequestStorage : class, IRequestStorageBase
        where TApplicationStorage : class
        where TSessionStorage : class
    {

        #region Private

        private readonly IHttpContextProvider _httpContextProvider;


        private HttpContextBase GetContext()
        {
            return _httpContextProvider.GetContext();
        }

        private HttpApplicationStateBase GetApplication()
        {
            return _httpContextProvider.Application;
        }

        private HttpSessionStateBase GetSession()
        {
            return _httpContextProvider.Session;
        }

        /// <summary>
        /// Реализует дуступ к текущему Http контексту веб-приложения
        /// 
        /// </summary>
        class DefaulHttpContextProvider : IHttpContextProvider
        {
            private readonly HttpApplication _httpApplication;
            public DefaulHttpContextProvider(HttpApplication httpApplication)
            {
                _httpApplication = httpApplication;
            }

            public HttpContextBase GetContext()
            {
                return HttpContext.Current != null?new HttpContextWrapper(HttpContext.Current):null;
            }

            public HttpApplicationStateBase Application
            {
                get
                {
                    return ((GetContext()!=null) && (GetContext().Application!=null))?GetContext().Application:new HttpApplicationStateWrapper(_httpApplication.Application);
                }
            }

            public HttpSessionStateBase Session
            {
                get
                {
                    return ((GetContext() != null) && (GetContext().Session != null)) ? GetContext().Session : new HttpSessionStateWrapper(_httpApplication.Session);
                }
            }
        }

        #endregion

        #region Protected

        /// <summary>
        /// Конструктор для тестирования
        /// </summary>
        /// <param name="httpContextProvider"></param>
        /// <param name="application"></param>
        protected PlatformEnvironment(IHttpContextProvider httpContextProvider)
            : base()
        {
            if (httpContextProvider == null) throw new ArgumentNullException("httpContextProvider");
            _httpContextProvider = httpContextProvider;

        }

        #endregion

        #region Public

        public PlatformEnvironment(HttpApplication application)
            : this(new DefaulHttpContextProvider(application))
        {

        }




        #region Overrides
        public override TApplicationStorage ApplicationStorage
        {
            get
            {

                return
                    (TApplicationStorage)GetApplication()["ApplicationStorage"];

            }
            protected set
            {
                GetApplication()["ApplicationStorage"] = value;

            }
        }

        /// <exception cref="InvalidOperationException">Сессия не определена</exception>>
        public override TSessionStorage SessionStorage
        {
            get
            {
                var httpSessionStateBase = GetSession();
                if (httpSessionStateBase != null)
                    return (TSessionStorage)httpSessionStateBase["SessionStorage"];
                throw new InvalidOperationException("Сессия не определена");
            }
            protected set
            {
                var httpSessionStateBase = GetSession();
                if (httpSessionStateBase != null)
                    httpSessionStateBase["SessionStorage"] = value;
                else
                    throw new InvalidOperationException("Сессия не определена");

            }
        }

        public override TRequestStorage RequestStorage
        {
            get { return (TRequestStorage)GetContext().Items["RequestStorage"]; }
            protected set { GetContext().Items["RequestStorage"] = value; }
        }
        #endregion

        #endregion




    }

    /// <summary>
    /// 
    /// </summary>
    public interface IHttpContextProvider
    {
        HttpContextBase GetContext();
        HttpApplicationStateBase Application { get; }
        HttpSessionStateBase Session { get; }

    }

}
