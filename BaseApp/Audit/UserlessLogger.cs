using System;
using System.Data;
using System.Data.SqlClient;
using System.Threading;
using BaseApp.Common.Interfaces;
using Microsoft.Practices.Unity;
using NLog;
using Platform.BusinessLogic.Auditing;
using Platform.BusinessLogic.Auditing.Interfaces;
using Platform.BusinessLogic.Common.Enums;
using Platform.BusinessLogic.Common.Interfaces;
using Platform.Common.Exceptions;
using Platform.Dal.Serialization;
using Platform.PrimaryEntities.Common.Interfaces;
using Platform.PrimaryEntities.Factoring;
using Platform.PrimaryEntities.Reference;
using Platform.Unity.Common.Interfaces;

namespace BaseApp.Audit
{
    /// <summary>
    /// Логгирование изменений элементов
    /// </summary>
    public class UserlessLogger : IAuditLogger
    {
        /// <summary>
        /// Конфигурация логгера
        /// </summary>
        protected readonly AuditConfiguration AuditConfiguration;

        /// <summary>
        /// Коструктор по-умолчанию
        /// </summary>
        /// <param name="auditConfiguration"></param>
        public UserlessLogger([Dependency] AuditConfiguration auditConfiguration)
        {
            AuditConfiguration = auditConfiguration;
        }
        
	    /// <summary>
	    /// Записать информацию о изменении мультилинка
	    /// </summary>
	    /// <param name="multilinkEntityId"></param>
	    /// <param name="firstElementId"></param>
	    /// <param name="secondElementId"></param>
	    /// <param name="operation"></param>
	    public void Log(int multilinkEntityId, int firstElementId, int secondElementId, MultilinkOperations operation)
        {
            Log(new MultilinkSqlLogData()
            {
                EntityId = multilinkEntityId,
                Operation = operation,
                FirstId = firstElementId,
                SecondId = secondElementId
            });

        }
        
	    /// <summary>
	    /// Записать информацию при изменении элемента
	    /// </summary>
	    /// <param name="entityId"></param>
	    /// <param name="elementId"></param>
	    /// <param name="operation"></param>
	    /// <param name="xmlBefore"></param>
	    /// <param name="xmlAfter"></param>
	    public void Log(int entityId, int elementId, Operations operation, string xmlBefore, string xmlAfter)
        {
            Log(new SqlLogData
            {
                ElementId = elementId,
                EntityId = entityId,
                Operation = operation,
                XmlAfter = xmlAfter,
                XmlBefore = xmlBefore
            });
        }
        
        /// <summary>
        /// Записать информацию при выполнении действия над элементом
        /// </summary>
        /// <param name="entityId"></param>
        /// <param name="elementId"></param>
        /// <param name="transactionScope"></param>
        /// <param name="operationType"></param>
        /// <param name="operationId"></param>
        /// <param name="operationName"></param>
        /// <param name="operationTime"></param>
        public void Log(int entityId, int elementId, int transactionScope, ProcessOperationTypes operationType, int? operationId,
                        string operationName, int? operationTime)
        {
            Log(new OperationSqlLogData
            {
                ElementId = elementId,
                EntityId = entityId,
                TransactionScope = transactionScope,
                OperationId = operationId,
                OperationName = operationName,
                OperationTime = operationTime,
                OperationType = operationType
            });
        }

        /// <summary>
        /// Записать информацию при выполнении метода веб-сервиса
        /// </summary>
        /// <param name="methodName"></param>
        /// <param name="methodTime"></param>
        /// <param name="jsonData"></param>
        public void Log(string methodName, int? methodTime, string jsonData)
        {
            Log(new RequestSqlLogData
            {
                JsonData   =  jsonData,
                MethodName = methodName,
                MethodTime = methodTime
            });
        }

        /// <summary>
        /// Записать информацию о начале сессии
        /// </summary>
        /// <remarks>id пользователя будет передано в ISqlLogData.CreateCommand(conn, _user.Id)</remarks>
        /// <param name="sessionId"></param>
        /// <param name="time"></param>
        public void SessionStart(string sessionId, DateTime time)
        {
            Log(new SessionSqlLog
            {
                SessionId = sessionId,
                EventId = 0,
                Time = time
            });
        }

        /// <summary>
        /// Записать информацию о завершении сессии
        /// </summary>
        /// <remarks>id пользователя будет передано в ISqlLogData.CreateCommand(conn, _user.Id)</remarks>
        /// <param name="sessionId"></param>
        /// <param name="time"></param>
        public void SessionEnd(string sessionId, DateTime time)
        {
            Log(new SessionSqlLog
            {
                SessionId = sessionId,
                EventId = 1,
                Time = time
            });
        }

        /// <summary>
        /// Записать информацию о входе пользователя в систему
        /// </summary>
        /// <param name="sessionId"></param>
        /// <param name="time"></param>
        public void Login(string sessionId, DateTime time)
        {
            Log(new LoginSqlLog
            {
                SessionId = sessionId,
                Time = time
            });
        }

        public void Log(ISqlLogData sqlLogData)
        {
            if (!IsEnabled)
                return;
            DoLogAsync(sqlLogData);
        }

        /// <summary>
        /// Разрешить логирование
        /// </summary>
        public bool IsEnabled
        {
            get { return AuditConfiguration.Enabled; }
        }

        /// <summary>
        /// Логировать время выполненеия действий над элементами (операции/контроли/etc)
        /// </summary>
        public bool IsOperationsEnabled
        {
            get { return AuditConfiguration.OperationsEnabled; }
        }

        /// <summary>
        /// Логировать клиентские запросы
        /// </summary>
        public bool IsRequestsEnabled
        {
            get { return AuditConfiguration.RequestsEnabled; }
        }

        private void DoLogAsync(ISqlLogData sqlLogData)
        {
            ThreadPool.QueueUserWorkItem(o => DoLog((ISqlLogData)o), sqlLogData);
        }

        /// <summary>
        /// Выполнить запись в базу
        /// </summary>
        /// <param name="sqlLogData"></param>
        protected virtual void DoLog(ISqlLogData sqlLogData)
        {
            try
            {
                using (var conn = new SqlConnection(AuditConfiguration.ConnectionString))
                {
                    conn.Open();
                    using (var comm = sqlLogData.CreateCommand(conn, 0))
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
        /// Регистратор в DI
        /// </summary>
        public class Registrator: IDefaultRegistration
        {
            /// <summary>
            /// Зарегистрировать класс в Unity
            /// </summary>
            public void Register(IUnityContainer unityContainer)
            {
                unityContainer.RegisterType(typeof(IAuditLogger), typeof(UserlessLogger), "UserlessLogger");
            }
        }

        static private readonly NLog.Logger InnerLoger = LogManager.GetCurrentClassLogger();
    }
}
