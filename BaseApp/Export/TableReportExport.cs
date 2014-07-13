using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Configuration;
using BaseApp.Reference;
using BaseApp.Tablepart;
using Platform.Common;
using Platform.BusinessLogic;
using Platform.Common.Exceptions;
using Platform.Dal;
using Platform.PrimaryEntities.Common.DbEnums;
using Platform.PrimaryEntities.Common.Interfaces;
using Platform.PrimaryEntities.Reference;

namespace BaseApp.Export
{
    /// <summary>
    /// Оперативные отчеты
    /// </summary>
    public class TableReportExport : ExcelTableWriter
    {
        #region Private Properties

        private string _connectionStrReadonly;
        /// <summary>
        /// Строка соединения с БД от пользователя с правами только на чтение.
        /// Ограниченные права позволяют избежать проблем в случае ввода в поле запроса sql-инструкции, отличной от SELECT.
        /// </summary>
        private string connectionStrReadonly
        {
            get
            {
                if (_connectionStrReadonly == null)
                {
                    var cnStr = DbConnectionString.Get();
                    var cnBldr = new SqlConnectionStringBuilder(cnStr);
                    cnBldr.UserID = readonlyUser;
                    _connectionStrReadonly = cnBldr.ToString();
                }
                return _connectionStrReadonly;
            }
        }

        private SqlConnection connection
        {
            get { return IoC.Resolve<SqlConnection>("DbConnection"); }
        }

        private string _currentDbName;
        private string currentDbName
        {
            get
            {
                if (_currentDbName == null)
                {
                    var cnStr = DbConnectionString.Get();
                    var cnBldr = new SqlConnectionStringBuilder(cnStr);
                    _currentDbName = cnBldr.InitialCatalog;
                }
                return _currentDbName;
            }
        }

        /// <summary>
        /// Контекст
        /// </summary>
        private DataContext db
        {
            get { return IoC.Resolve<DbContext>().Cast<DataContext>(); }
        }

        /// <summary>
        /// Пользователь, из под которого будет выполнен запрос для отчета
        /// </summary>
        private string readonlyUser
        {
            get { return WebConfigurationManager.AppSettings["readonlyUser"]; }
        }

        /// <summary>
        /// Текст запроса на выборку, полученных из элемента справочника Оперативные отчеты.
        /// </summary>
        private string Sql { get; set; }

        private int? commandTimeout
        {
            get
            {
                int result;
                if (int.TryParse(WebConfigurationManager.AppSettings["tableReportTimeout"], out result))
                {
                    return result;
                }
                return null;
            }
        }

        private List<TableReport_ColumnType> ColumnTypes { get; set; }
    
        #endregion

        /// <summary>
        /// Класс для выполнения оперативных отчетов и выгрузки результата в Excel
        /// </summary>
        /// <param name="reportId"></param>
        public TableReportExport(int reportId)
        {
            ReportId = reportId;
            prepare();
        }

        #region Public Members

        /// <summary>
        /// id записи справчника Оперативные отчеты
        /// </summary>
        public int ReportId { get; set; }

        /// <summary>
        /// Построить отчет
        /// </summary>
        /// <returns>html-текст, интерпретируемый в дальнейшем как таблица Excel</returns>
        public string BuildReport()
        {
            string result;

            try
            {
                CheckReadonlyUser();
                List<Dictionary<string, object>> data;
                data = getData();

                if (data.Any())
                {
                    //Fields = data.First().Keys.Select(getField);
                    Fields = data.First().Select(getField).ToList();
                    result = BuildReport(data);
                }
                else
                {
                    result = BuildMessage("Запрос не вернул данных");
                }
            }
            catch (Exception ex)
            {
                result = BuildMessage(string.Format("При выполнении запроса произошла ошибка: {0}", ex.Message));
            }

            return result;
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Определяем заголовок, текст запроса
        /// </summary>
        private void prepare()
        {
            TableReport report = db.TableReport.SingleOrDefault(r => r.Id == ReportId);
            if (report == null)
                throw new PlatformException(string.Format("Не найден оперативный отчет (ref.TableReport) с идентификатором: {0}", ReportId));

            Title = report.Caption;
            Sql = report.Sql;
            ColumnTypes = report.ColumnTypes.ToList();
        }

        /// <summary>
        /// Получение данных для отчета
        /// </summary>
        /// <returns></returns>
        private List<Dictionary<string, object>> getData()
        {
            var result = new List<Dictionary<string, object>>();

            using (var connection = new SqlConnection(connectionStrReadonly))
            {
                connection.Open();
                SqlCommand cmd = new SqlCommandFactory(Sql, connection).CreateCommand();
                    
                if (commandTimeout.HasValue)
                {
                    cmd.CommandTimeout = commandTimeout.Value;
                }
                
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var row = new Dictionary<string, object>(StringComparer.InvariantCultureIgnoreCase);
                        for (int col = 0; col <= reader.FieldCount - 1; col++)
                        {
                            string colName = reader.GetName(col);
                            if (row.ContainsKey(colName))
                            {
                                throw new PlatformException(string.Format("Колонка с именем {0} указана более одного раза в тексте запроса.", colName));
                            }
                            row.Add(colName, reader[col]);
                        }

                        result.Add(row);
                    }
                    reader.Close();
                }                
                connection.Close();
            }

            return result;
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        private EntityField getField(string name)
        {
            return new EntityField
                {
                    Name = name,
                    Caption = name,
                    IdEntityFieldType = (byte)EntityFieldType.String
                };
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="field"></param>
        /// <returns></returns>
        private EntityField getField(KeyValuePair<string, object> field)
        {
            var entityField =  new EntityField
            {
                Name = field.Key,
                Caption = field.Key,
                IdEntityFieldType = (byte)EntityFieldType.String
            };

            var columnType = ColumnTypes.FirstOrDefault(c => c.FieldName.Trim().ToLower() == field.Key.Trim().ToLower() );

            if (columnType != null)
            {
                entityField.IdEntityFieldType = columnType.IdEntityFieldType;
                entityField.Precision = columnType.Precision;
            }
            else if (!(field.Value is DBNull))
            {
            
                //int
                int tempInt;
                if (Int32.TryParse(field.Value.ToString(), out tempInt))
                    entityField.IdEntityFieldType = (byte) EntityFieldType.Int;

                //double
                double tempDouble;
                if (Double.TryParse(field.Value.ToString(), NumberStyles.AllowDecimalPoint, new CultureInfo("ru-RU"), out tempDouble))
                    entityField.IdEntityFieldType = (byte)EntityFieldType.Numeric;
                
                //datetime
                DateTime tempDate;
                if (DateTime.TryParse(field.Value.ToString(), out tempDate))
                    entityField.IdEntityFieldType = (byte)EntityFieldType.DateTime;
            }
            
            return entityField;
        }


        private void CheckReadonlyUser()
        {
            var checkUserSql = string.Format("use {0} SELECT COUNT(*) FROM sys.database_principals WHERE name = '{1}'", currentDbName, readonlyUser);
            int cnt;
            using (var cmd = new SqlCommandFactory(checkUserSql, connection).CreateCommand())
            {
                cnt = (int)cmd.ExecuteScalar();
            }

            if (cnt != 1)
                throw new PlatformException(string.Format("В БД не найден пользователь {0} из под которого следует выполнять запросы на выборку. " +
                                                          "Обратитесь к администратору для создания пользователя с ролью db_datareader. " +
                                                          "http://msdn.microsoft.com/en-us/library/ms188629.aspx", readonlyUser));
        }

        #endregion
    }
}
