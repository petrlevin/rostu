using System;
using System.Configuration;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.SqlClient;
using System.Linq;
using BaseApp.Common.Interfaces;
using BaseApp.Environment;
using BaseApp.Environment.Storages;
using BaseApp.Service.Common;
using BaseApp.Service.Interfaces;
using BaseApp.SystemDimensions;
using Microsoft.Practices.Unity;
using Platform.BusinessLogic;
using Platform.BusinessLogic.EntityFramework;
using Platform.Caching;
using Platform.Unity;

namespace BgTasks.Core
{
    public class Proxy: IDisposable
    {
        public Tests.DataContext Context { get; set; }

        private SimpleEnvironment Env { get; set; }

        private OperationsManager operationsManager { get; set; }

        private static string ConnectionString
        {
            get
            {
                return DbConnectionString.Get();
            }
        }

        private bool isApplicationStarted
        {
            get { return Env != null; }
        }

        public void Init()
        {
            StartApplication();
            IUser user = Context.User.Single(u => u.Name == "admin");
            StartSession(user);
        }

        public void ProcessOperation(Action<IOperationsManager> action)
        {
            StartRequest();
            action(operationsManager);
            EndRequest();
        }

        public void Dispose()
        {
            EndRequest();
        }

        private void StartApplication()
        {
            Platform.Application.Application.OnBeforeStart();

            SqlDependency.Start(ConnectionString);
            DbContextInitializer.TraceEnabled = true;
            IUnityContainer unityContainer = new UnityContainer();
            Context = new Tests.DataContext();
            unityContainer.RegisterInstance<DbContext>("AppStartContext", Context);

            // Инициализация Среды и Резолвера зависимостей
            Env = new SimpleEnvironment();
            IoCServices.InitWith(new DependencyResolver(Env, unityContainer));

            // Инициализация хранилища уровня приложения
            var app = new ApplicationStorage()
            {
                Cache = new ThreadSafeCache(),
                ConnectionString = ConnectionString
            };

            // Регистрация хранилища уровня приложения
            Env.ApplicationStart(app);

            Platform.Application.Application.OnAfterStart();
        }

        private void StartSession(IUser user)
        {
            if (!isApplicationStarted)
                StartApplication();

            // Инициализация сессии
            var session = new SessionStorage
            {
                CurentDimensions = new SysDimensionsState(),
                Id = Guid.NewGuid(),
                CurrentUser = user
            };

            // Регистрация сессии
            Env.SessionStart(session);
            operationsManager = new OperationsManager();
        }

        private void StartRequest()
        {
            var dbContext = new Tests.DataContext();
            (dbContext as IObjectContextAdapter).ObjectContext.CommandTimeout = int.Parse(ConfigurationManager.AppSettings["ExecuteSqlCommandTimeout"]);

            var request = new RequestStorage
            {
                DbConnection = new SqlConnection(ConnectionString),
                DbContext = dbContext
            };

            request.DbConnection.Open();

            Env.RequestStart(request);

            // Регистрация хранилища уровня запроса
            Env.RequestStart(request);
            Platform.Application.Application.OnBeginRequest();
        }

        private void EndRequest()
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
    }
}
