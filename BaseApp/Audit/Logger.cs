using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BaseApp.Common.Interfaces;
using Microsoft.Practices.Unity;
using NLog;
using Platform.BusinessLogic.Auditing;
using Platform.BusinessLogic.Auditing.Interfaces;
using Platform.BusinessLogic.Common.Interfaces;
using Platform.Unity.Common.Interfaces;

namespace BaseApp.Audit
{
    /// <summary>
    /// Основной логгер. Запись в Sql
    /// </summary>
    public class Logger : UserlessLogger
    {
        /// <summary>
        /// Текущий пользователь
        /// </summary>
        protected IUser User;

        /// <summary>
        /// Запоминаем текущего пользователя
        /// </summary>
        /// <param name="user"></param>
        /// <param name="auditConfiguration"></param>
        public Logger([Dependency("CurrentUser")] IUser user, [Dependency] AuditConfiguration auditConfiguration): base(auditConfiguration)
        {
            User = user;
        }

        /// <summary>
        /// Выполнить запись в лог
        /// </summary>
        /// <param name="sqlLogData"></param>
        protected override void DoLog(ISqlLogData sqlLogData)
        {
            try
            {
                using (var conn = new SqlConnection(AuditConfiguration.ConnectionString))
                {
                    conn.Open();
                    using (var comm = sqlLogData.CreateCommand(conn, User.Id))
                    {
                        comm.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                InnerLoger.ErrorException("Ошибка аудита", ex);
                InnerLoger.Error(sqlLogData.ToString);
            }
        }

        /// <summary>
        /// Регистрируем логер в Uninity при старте приложения
        /// </summary>
        public class LoggerRegistrator : IDefaultRegistration
        {
            /// <summary>
            /// Зарегистрировать в Unity под именем Logger
            /// </summary>
            /// <param name="unityContainer"></param>
            public void Register(IUnityContainer unityContainer)
            {
                unityContainer.RegisterType(typeof(IAuditLogger), typeof(Logger));
            }
        }

        static private readonly NLog.Logger InnerLoger = LogManager.GetCurrentClassLogger();
    }
}
