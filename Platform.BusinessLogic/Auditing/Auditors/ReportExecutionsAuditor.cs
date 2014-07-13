using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Platform.BusinessLogic.Auditing.Auditors.Abstract;
using Platform.BusinessLogic.Auditing.Interfaces;
using Platform.Dal;

namespace Platform.BusinessLogic.Auditing.Auditors
{
    public class ReportExecutionsAuditor : AuditorBase, IStartEndAuditor, ISqlLogData
    {
        public enum ReportTypeEnum
        {
            /// <summary>
            /// Оперативный отчет
            /// </summary>
            TableReport = 1,
            
            /// <summary>
            /// Обычный отчет (которому соответствует сущность типа Отчет)
            /// </summary>
            EntityReport = 2,
            
            /// <summary>
            /// Печатная форма
            /// </summary>
            PrintForm = 3
        }

        public ReportTypeEnum ReportType { get; set; }

        /// <summary>
        /// Сущность отчета. 
        /// При записи информации об оперативном отчете не указывается.
        /// При печатной форме - сущность документа.
        /// </summary>
        public int ReportEntity { get; set; }

        /// <summary>
        /// Идентификатор элемента. Возможны варианты:
        /// id профиля отчета
        /// id документа
        /// id элемента справочника "Оперативные отчеты"
        /// </summary>
        public int ReportEntityItem { get; set; }

        /// <summary>
        /// Не указывается в случае оперативного отчета и ПФ.
        /// </summary>
        public string Data { get; set; }

        /// <summary>
        /// Признак успешного построения отчета
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// Время начала выполнения отчета
        /// </summary>
        public DateTime Date { get; set; }

        private int elapsedMilliseconds;

        public void Start(DateTime startedAt)
        {
        }

        public void End(int elapsedMilliseconds)
        {
            this.elapsedMilliseconds = elapsedMilliseconds;
            logger.Log(this);
        }

        public SqlCommand CreateCommand(SqlConnection connection, int userId)
        {
            string sql = @"
                INSERT INTO dbo.report (
                     [ReportType]
                    ,[ReportEntity]
                    ,[ReportEntityItem]
                    ,[Data]
                    ,[UserId]
                    ,[Date]
                    ,[ElapsedTime]
                    ,[Success]
                ) VALUES (
                     @ReportType
                    ,@ReportEntity
                    ,@ReportEntityItem
                    ,@Data
                    ,@UserId
                    ,@Date
                    ,@ElapsedTime
                    ,@Success
                )
            ";

            var parameters = new Dictionary<string, object>
                {
                    {"ReportType", (int) ReportType},
                    {"ReportEntity", ReportEntity},
                    {"ReportEntityItem", ReportEntityItem},
                    {"Data", string.IsNullOrEmpty(Data) ? "" : Data },
                    {"UserId", userId},
                    {"Date", Date},
                    {"ElapsedTime", elapsedMilliseconds},
                    {"Success", Success}
                };

            return new SqlCommandFactory(sql, connection).CreateCommand(parameters);
        }
    }
}
