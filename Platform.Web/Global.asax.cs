using System;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Web;
using System.Diagnostics;
using System.Web.Configuration;
using System.Web.SessionState;
using BaseApp.Common.Interfaces;
using BaseApp.Environment;
using BaseApp.Environment.Storages;
using BaseApp.SystemDimensions;
using Microsoft.Practices.Unity;
using Platform.BusinessLogic.Auditing.Auditors;
using Platform.BusinessLogic.Auditing;
using Platform.BusinessLogic.EntityFramework;
using Platform.BusinessLogic.ReportProfiles;
using Platform.Caching;
using Platform.Common;
using Platform.Unity;
using Platform.Web.ExtDirectManagement;
using Platform.Web.Services;
using System.Data.SqlClient;
using Listener = Platform.Web.UserManagement.Listener;
using Platform.Unity;

namespace Platform.Web
{
    public class Global : HttpApplication
    {
        /// <summary>
        /// Строки, используемые в коде.
        /// </summary>
        private static class Names
        {
            public static string PlatformEnvironment = "PlatformEnvironment";
        }

        private BaseAppEnvironment Env
        {
            get { return (BaseAppEnvironment) Application[Names.PlatformEnvironment]; }
            set { Application[Names.PlatformEnvironment] = value; }
        }

        protected void Application_Start(object sender, EventArgs e)
        {
            Debug.WriteLine("Application_Start");

            Platform.Application.Application.OnBeforeStart();

            SqlDependency.Start(ConnectionString);
            DbContextInitializer.TraceEnabled = true;
            IUnityContainer unityContainer = new UnityContainer();

            using (var dataContext = new Tests.DataContext())
            {
                unityContainer.RegisterInstance<DbContext>("AppStartContext", dataContext); 

                // Инициализация Среды и Резолвера зависимостей
                Env = new BaseAppEnvironment(this);
                IoCServices.InitWith(new DependencyResolver(Env, unityContainer));

                ProfileService.OnLogout += () => GetSession().Abandon();
                // Инициализация хранилища уровня приложения
                var app = new ApplicationStorage()
                {
                    Cache = new ThreadSafeCache(),
                    ConnectionString = ConnectionString
                };

                // Регистрация хранилища уровня приложения
                Env.ApplicationStart(app);

                // Подготовка Ext.Direct
                ExtDirect.Initialize();
                ExtDirect.BeforeInvoke += (m, t) => BaseAppEnvironment.Instance.RequestStorage.ClearDecorators();

                Platform.Application.Application.OnAfterStart();
            }
        }

        private HttpSessionState GetSession()
        {
            if ((HttpContext.Current != null) && (HttpContext.Current.Session != null))
                return HttpContext.Current.Session;
            return Session;

        }

        protected void Session_Start(object sender, EventArgs e)
        {
            Debug.WriteLine("Session_Start");

            // Инициализация сессии
            var session = new SessionStorage
                {
                    CurentDimensions = new SysDimensionsState(),
                    Id = Guid.NewGuid()
                };

            BaseApp.Reference.User.AddListener(new Listener(session));
            // Регистрация сессии
            Env.SessionStart(session);

            Session["SessionId"] = Guid.NewGuid();
            Audit<SessionAuditor>.Do((startedAt, auditor) => auditor.SessionStart(Session["SessionId"].ToString(), startedAt));
        }


        protected void Application_BeginRequest(object sender, EventArgs e)
        {
            Debug.WriteLine("Application_BeginRequest");

        }


        protected void Application_AcquireRequestState(object sender, EventArgs e)
        {
            Debug.WriteLine("Application_AcquireRequestState: Session is {0}",
                            Context.Session == null ? "null" : "not null");

            if (Context.Session != null)
            {
                // Инициализация хранилища уровня запроса
                var dbContext = new Tests.DataContext(); //Sbor.DataContext()
                (dbContext as IObjectContextAdapter).ObjectContext.CommandTimeout = int.Parse(WebConfigurationManager.AppSettings["ExecuteSqlCommandTimeout"]);
                var request = new RequestStorage
                    {
                        DbConnection = new SqlConnection(ConnectionString),
                        DbContext = dbContext
                    };

                request.DbConnection.Open();

                Env.RequestStart(request); //зачем это было написано 2! раза?

                // Регистрация хранилища уровня запроса
                Env.RequestStart(request);
                Platform.Application.Application.OnBeginRequest();

                // Для тестирования изменений коллекций:
                //request.DbContext.Cast<Tests.DataContext>().Report1.Local.CollectionChanged += delegate(object o, NotifyCollectionChangedEventArgs args)
                //    {
                //        //if (args.Action == NotifyCollectionChangedAction.Add)
                //        //{
                //            Debug.WriteLine(string.Format("", args));
                //        //}
                //    };
            }
        }

        private static string ConnectionString
        {
            get
            {
                return BusinessLogic.DbConnectionString.Get();
            }
        }

        protected void Application_EndRequest(object sender, EventArgs e)
        {
            Platform.Application.Application.OnEndRequest();
            var requestStorage = Env.RequestStorage;
            if (requestStorage != null)
            {
                if (requestStorage.DbConnection != null)
                    requestStorage.DbConnection.Close();
                if (requestStorage.DbContext != null)
                    requestStorage.DbContext.Dispose();

            }

        }

        protected void Application_AuthenticateRequest(object sender, EventArgs e)
        {

        }

        protected void Application_Error(object sender, EventArgs e)
        {

        }

        protected void Session_End(object sender, EventArgs e)
        {
            IUser user = IoC.Resolve<IUser>("CurrentUser");
            TemporaryItemsRemover.RemoveAllTemporaryProfiles(user.Id);
            
            // закрываем соединение с БД
            BaseApp.Reference.User.RemoveListener(new Listener(BaseAppEnvironment.Instance.SessionStorage));
            Audit<SessionAuditor>.Do((finishedAt, auditor) => auditor.SessionEnd(Session["SessionId"].ToString(), finishedAt));
        }

        protected void Application_End(object sender, EventArgs e)
        {
            SqlDependency.Stop(ConnectionString);
            //if ((Env.ApplicationStorage.Cache!=null)&&(Env.ApplicationStorage.Cache is IDisposable))
            //	((IDisposable)Env.ApplicationStorage.Cache).Dispose();
        }
    }

}