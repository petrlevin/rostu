using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Platform.BusinessLogic.Auditing
{
    public partial class AuditDbFactory
    {
        /// <summary>
        /// Инструкции по созданию таблиц в БД аудита
        /// </summary>
        public class SqlTpl
        {
            #region Шаблоны sql комманд

            private string tbl_data = @"
			    [Before] [xml] NULL,
			    [After] [xml] NULL,
			    [EntityId] [int] NOT NULL,
			    [Operation] [tinyint] NOT NULL,
			    [ElementId] [int] NOT NULL,
			    [IdUser] [int] NOT NULL,
			    [Date] [datetime] NOT NULL,";

            private string tbl_multilink_data = @"
			    [EntityId] [int] NOT NULL,
			    [Operation] [tinyint] NOT NULL,
			    [FirstId] [int] NOT NULL,
			    [SecondId] [int] NOT NULL,
			    [IdUser] [int] NOT NULL,
			    [Date] [datetime] NOT NULL,";

            private string tbl_operation_data = @"
			    [EntityId] [int] NOT NULL,
			    [ElementId] [int] NOT NULL,
			    [TransactionScope] [int] NOT NULL, 
			    [OperationType] [tinyint] NOT NULL,
			    [OperationName] [varchar](max) NULL,
			    [OperationTime] [int] NULL,
			    [OperationId] [int] NULL,
			    [UserId] [int] NULL,
			    [Date] [datetime] NULL,";

            private string tbl_request_data = @"
			    [MethodName] [varchar](max) NULL,
			    [MethodTime] [int] NULL,
			    [JsonData] [varchar](max) NULL,
			    [UserId] [int] NULL,
			    [Date] [datetime] NULL,";

            private string tbl_logins = @"
			    [SessionId] [nvarchar](50) NOT NULL,
			    [UserId] [int],
			    [Time] [datetime],";

            private string tbl_sessions = @"
			    [SessionId] [nvarchar](50) NOT NULL,
			    [EventId] [tinyint],
			    [Time] [datetime],";

            private string tbl_report = @"
			    [ReportType] [tinyint] NOT NULL, /* 1 - оперативный, 2 - обычный, 3 - печатная форма */
			    [ReportEntity] [int] NULL,
                [ReportEntityItem] [int] NOT NULL,
			    [Data] [varchar](max) NULL,
			    [UserId] [int] NOT NULL,
			    [Date] [datetime] NOT NULL,
                [ElapsedTime] [int] NOT NULL,
                [Success] [bit] DEFAULT 0,
                ";

            private string sql_createTableWithId = @"
		        CREATE TABLE [dbo].[{0}] (
			        [id] [int] IDENTITY(1,1) NOT NULL,
                    /* список полей таблицы */                            
                    {1} 
			        CONSTRAINT [PK_{0}_Id] PRIMARY KEY CLUSTERED ([id] ASC)
                    WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
                ) ON [PRIMARY]";

            private string sql_createTableWithoutId = @"
                CREATE TABLE [dbo].[{0}] (
                    /* список полей таблицы */                            
                    {1} 
                ) ON [PRIMARY]";

            #endregion

            #region Методы, возвращающие sql комманду для создания конкретного объекта

            private string getTbl_data(string tblName)
            {
                return string.Format(sql_createTableWithId, tblName, tbl_data);
            }

            private string getTbl_multilink_data(string tblName)
            {
                return string.Format(sql_createTableWithId, tblName, tbl_multilink_data);
            }

            private string getTbl_operation_data(string tblName)
            {
                return string.Format(sql_createTableWithId, tblName, tbl_operation_data) + " TEXTIMAGE_ON [PRIMARY]";
            }

            private string getTbl_request_data(string tblName)
            {
                return string.Format(sql_createTableWithId, tblName, tbl_request_data) + " TEXTIMAGE_ON [PRIMARY]";
            }

            private string getTbl_logins(string tblName)
            {
                return string.Format(sql_createTableWithoutId, tblName, tbl_logins);
            }

            private string getTbl_sessions(string tblName)
            {
                return string.Format(sql_createTableWithoutId, tblName, tbl_sessions);
            }

            private string getView_FullReportView(string viewName)
            {
                return @"
                CREATE VIEW " + @"[dbo].[" + viewName + @"] AS
                SELECT 
                    D.id, 
                    D.[Before], 
                    D.[After], 
                    E.Caption as EntityCaption, 
                    D.EntityId, 
                    D.Operation, 
                    D.ElementId, 
                    D.IdUser,
                    U.Caption as UserCaption, 
                    ([" + DbConnBuilder.InitialCatalog + @"].[dbo].[GetCaption]( D.EntityId, D.ElementId ) ) as ElementCaption, 
                    D.[Date] 
                FROM 
                    dbo.data D 
                    LEFT JOIN [" + DbConnBuilder.InitialCatalog + @"].[ref].[Entity] E on E.id = D.EntityId
                    LEFT JOIN [" + DbConnBuilder.InitialCatalog + @"].[ref].[User] U on U.id = D.IdUser";
            }
            
            private string getTbl_Reports(string tblName)
            {
                return string.Format(sql_createTableWithId, tblName, tbl_report);
            }

            #endregion

            /// <summary>
            /// Возвращает словарь вида ключ = имя объекта, значение = комманда для создания данного объекта в БД Аудита
            /// </summary>
            /// <returns></returns>
            public Dictionary<string, string> GetDdl()
            {
                var result = new Dictionary<string, string>();
                var ddl = new Dictionary<string, Func<string, string>>()
                {
                    { "data", getTbl_data },
                    { "multilink_data", getTbl_multilink_data },
                    { "operation_data", getTbl_operation_data },
                    { "request_data", getTbl_request_data },
                    { "logins", getTbl_logins },
                    { "sessions", getTbl_sessions },
                    { "report", getTbl_Reports },
                    { "FullReportView", getView_FullReportView }
                };

                foreach (var func in ddl)
                {
                    result.Add(func.Key, func.Value(func.Key));
                }
                return result;
            }
        }
    }
}
