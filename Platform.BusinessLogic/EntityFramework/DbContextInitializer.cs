using System;
using System.Configuration;
using System.Data.Common;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using EFProviderWrapperToolkit;
using EFTracingProvider;
using NLog;

namespace Platform.BusinessLogic.EntityFramework
{
    public static class DbContextInitializer
    {
        static public bool TraceEnabled { get; set; }
        private static Logger logger = LogManager.GetCurrentClassLogger();

        public static void EnableTracing(DbContext dbContext)
        {
            if (TraceEnabled)
            {

                var oc = ((IObjectContextAdapter) dbContext).ObjectContext;
                 oc.EnableTracing();
                var conn =((IObjectContextAdapter) dbContext).ObjectContext.Connection.UnwrapConnection<EFTracingConnection>();
                conn.CommandExecuting += (s, ea) =>
                {
                    logger.Info(Environment.NewLine + ea.ToTraceString().TrimEnd() + Environment.NewLine);
                    
                };
                conn.CommandFailed += (s, ea) => logger.Error(
                    ea.Result.ToString() + Environment.NewLine + ea.ToTraceString().TrimEnd() + Environment.NewLine);
                conn.CommandFinished += (s, ea) =>
                                            {
                                                logger.Debug(Environment.NewLine + ea.ToTraceString().TrimEnd() + Environment.NewLine);
                                            };
            }
        }

        public static DbConnection CreateConnection(string nameOrConnectionString)
        {
            // does not support entity connection strings
            if (TraceEnabled)
            {

                EFTracingProviderFactory.Register();
            }

            ConnectionStringSettings connectionStringSetting =
                ConfigurationManager.ConnectionStrings[nameOrConnectionString];
            string connectionString;
            string providerName;

            if (connectionStringSetting != null)
            {
                connectionString = connectionStringSetting.ConnectionString;
                providerName = connectionStringSetting.ProviderName;
            }
            else
            {
                providerName = "System.Data.SqlClient";
                connectionString = nameOrConnectionString;
            }

            return CreateConnection(connectionString, providerName);
        }

        private static DbConnection CreateConnection(string connectionString, string providerInvariantName)
        {
            DbConnection connection = null;
            if (TraceEnabled)
            {
                connection = CreateTracingConnection(connectionString, providerInvariantName);
            }
            else
            {
                DbProviderFactory factory = DbProviderFactories.GetFactory(providerInvariantName);
                connection = factory.CreateConnection();
                connection.ConnectionString = connectionString;
            }
            return connection;
        }


        private static EFTracingConnection CreateTracingConnection(string connectionString, string providerInvariantName)
        {

            string wrapperConnectionString =
                String.Format(@"wrappedProvider={0};{1}", providerInvariantName, connectionString);

            EFTracingConnection connection =
                new DbTracingConnection
                {
                    ConnectionString = wrapperConnectionString
                };

            return connection;
        }

        public class DbTracingConnection : EFTracingConnection
        {
            protected override DbCommand CreateDbCommand()
            {
                return this.WrappedConnection.CreateCommand();
            }
        }

    }
}
