using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using Microsoft.Practices.Unity;
using Platform.Caching.Common;
using Platform.PrimaryEntities.Factoring.Strategies;
using Platform.PrimaryEntities.Interfaces;
using Platform.Unity.Common;
using Platform.Unity.Common.Interfaces;

namespace Platform.PrimaryEntities.Factoring
{
    public  class DependencyInjection :IDefaultRegistration
    {
        public static void RegisterIn(IUnityContainer unityContainer ,bool closeConnection = false,bool useCache = true , string connectionString =null)
        {
            if (!String.IsNullOrEmpty(connectionString))
                unityContainer.RegisterInstance<SqlConnection>( "DbConnection", new SqlConnection(connectionString));

            unityContainer.RegisterType<IFactory,DefaultFactory>("MetadataObjectsFactory");
            unityContainer.RegisterType<IFactoryStrategy<DataRow>, Base>("FactoryStrategy");
            if (useCache)
                unityContainer.RegisterDecorator<IFactoryStrategy<DataRow>, UseCacheWithSqlDependency<PerTableSqlDependencySelect>>(
                            "FactoryStrategy");

            if (closeConnection)
                unityContainer.RegisterDecorator<IFactoryStrategy<DataRow>, CloseConnection>(
                            "FactoryStrategy");

            Metadata.GetObjects = ()=> unityContainer.Resolve<IFactory>("MetadataObjectsFactory");

        }


        public void Register(IUnityContainer unityContainer)
        {
            RegisterIn(unityContainer);
        }
    }
}
