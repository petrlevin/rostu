using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using BaseApp.Common.Interfaces;
using Microsoft.Reporting.WebForms;
using Platform.BusinessLogic;
using Platform.BusinessLogic.Auditing;
using Platform.BusinessLogic.Auditing.Auditors;
using Platform.BusinessLogic.EntityTypes;
using Platform.BusinessLogic.ReportingServices.Reports;
using Platform.Common;
using System.IO;
using Platform.Common.Exceptions;
using Platform.PrimaryEntities.Common.DbEnums;
using Platform.PrimaryEntities.Reference;

namespace Platform.Web.Services
{
    // ToDo: Следует вынести логику получения отчета в отдельный класс для печатных форм и отчетов, т.к. нахождение этой логике в классах Page препятствует использованию общего базового класса.
    public partial class Report : System.Web.UI.Page
    {
        #region Private Properties

        private string entityNameParam = "_entityName";

        private object reportInstance;

        private ReportViewer reportViewer
        {
            get { return (ReportViewer) Master.FindControl("ReportViewer1"); }
        }

        private Tests.DataContext db
        {
            get { return IoC.Resolve<DbContext>().Cast<Tests.DataContext>(); }
        }

        private int itemId
        {
            get { return int.Parse(this.Request.Form["id"]); }
        }

        /// <summary>
        /// Принудительно строим отчет только по данным из запроса
        /// </summary>
        private bool NoSavedItem
        {
            get
            {
                bool temp;
                return bool.TryParse(this.Request.Form["noSavedItem"], out temp) && temp;
            }
        }

        private Entity _entity;
        /// <summary>
        /// Сущность запрошенного отчета
        /// </summary>
        private Entity entity
        {
            get
            {
                if (_entity == null)
                {
                    _entity = db.Entity.Single(e => e.Name == entityName && e.IdEntityType == (byte)EntityType.Report);
                }
                return _entity;
            }
        }

        /// <summary>
        /// Имя сущности документа
        /// </summary>
        private string entityName
        {
            get
            {
                return this.Request.Params[entityNameParam];
            }
        }
        
        private ReportsInfo reportsInfo
        {
            get { return IoC.Resolve<ReportsInfo>(); }
        }

        /// <summary>
        /// Параметры, установленные для макета отчета
        /// </summary>
        IEnumerable<ReportParameter> rdlcParams { get; set; }

        #endregion

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                //ReportEntity reportInstance = 
                reportViewer.ReportError += OnError;

                bool success = true;
                var auditor = Audit<ReportExecutionsAuditor>.Start(new ReportExecutionsAuditor()
                {
                    ReportType = ReportExecutionsAuditor.ReportTypeEnum.EntityReport,
                    ReportEntity = entity.Id,
                    ReportEntityItem = itemId,
                    Date = DateTime.Now
                });
                try
                {
                    process();
                }
                catch (Exception)
                {
                    success = false;
                    throw;
                }
                finally
                {
                    auditor.Auditor.Data = serializeReportParams(rdlcParams);
                    auditor.Auditor.Success = success;
                    auditor.Complete();
                }
            }
        }

        #region Private Methods

        /// <summary>
        /// Формирование отчета. 
        /// </summary>
        private void process()
        {
            reportViewer.LocalReport.ReportPath = getReportPath();
            reportInstance = getInstance();
            rdlcParams = setParametersForRdlc();
            fillReportData();
        }

        /// <summary>
        /// Получаем объект отчета с заполненными свойствами 
        /// </summary>
        /// <returns></returns>
        private object getInstance()
        {
            if (NoSavedItem || !entity.HasEntityClass())
            {
                // => в системе не зарегистрирован сущностной класс, просто создаем объект
                object reportInstance = reportsInfo.CreateInstance(entityName);
                setParametersForObject(reportInstance);
                return reportInstance;
            }
            
            // => в системе должен быть зарегистрирован  получаем объект из БД
            var dbSet = db.Set(entity);
            return dbSet.Find(itemId);
        }

        /// <summary>
        /// Получаем объект отчета с заполненными свойствами.
        /// </summary>
        /// <remarks>
        /// Метод не работает. Однако данный способ - без обращения к БД - предпочтительнее. Оставлен с целью допиливания.
        /// </remarks>
        /// <returns></returns>
        private object getInstance2()
        {
            object reportInstance;
            if (!entity.HasEntityClass()) // => в системе не зарегистрирован сущностной класс, просто создаем объект
            {
                reportInstance = reportsInfo.CreateInstance(entityName); 
            }
            else // => в системе должен быть зарегистрирован  получаем объект из БД
            {
                reportInstance = db.Set(entity).Create();
                db.Entry(reportInstance).State = EntityState.Unchanged;
            }
            
            setParametersForObject(reportInstance);
            return reportInstance;
        }

        /// <summary>
        /// Получает полный путь до файла макета *.rdlc
        /// </summary>
        /// <returns></returns>
        private string getReportPath()
        {
            string filePathStand = Server.MapPath(string.Format("~/bin/Reports/{0}/{0}.rdlc", entityName));
#if DEBUG
            //Если есть файл разработчика, берем его, чтобы не было необходимости в компиляции при изменении макета отчета
            string filePathDeveloper = Server.MapPath("~/") + string.Format(@"../Sbor.Reports/Reports/{0}/{0}.rdlc", entityName);
            
            if (File.Exists(filePathDeveloper))
                return filePathDeveloper;
            return filePathStand;
#else
            return filePathStand;
#endif

        }

        /// <summary>
        /// Установка значений для параметров объекта отчета.
        /// Вызывается только при отсутствии сущностного класса. При его наличии происходит чтение объекта (со всеми его параметрами) из БД.
        /// </summary>
        /// <param name="instance"></param>
        private void setParametersForObject(object instance)
        {
            foreach (string name in this.Request.Form.Keys)
            {
                if (name != entityNameParam )  
                {
                    var propertyType = instance.GetType().GetProperty(name, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);

                    if (propertyType != null)
                    {
                        try
                        {
                            // ToDo: сделать проверку допустимости установки значения вместо try
                            instance.SetValue(name, this.Request.Form[name], propertyType.PropertyType);
                        }
                        catch
                        {
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Установка значений для параметров, заданных в макете отчета
        /// </summary>
        /// <returns>
        /// Список установленных для отчета параметров. имя = значение
        /// </returns>
        private IEnumerable<ReportParameter> setParametersForRdlc()
        {
            ReportParameterInfoCollection parametersInfo = reportViewer.LocalReport.GetParameters();
            var parameters = new List<ReportParameter>();
            
            foreach (ReportParameterInfo parameterInfo in parametersInfo)
            {
                if (!string.IsNullOrEmpty(this.Request.Form[parameterInfo.Name]))
                {
                    string value = this.Request.Form[parameterInfo.Name];

                    if (parameterInfo.DataType == ParameterDataType.DateTime)
                    {
                        
                        DateTime temp;

                        if (DateTime.TryParseExact(value,
                                                   "ddd, d MMM yyyy HH:mm:ss UTC",
                                                   CultureInfo.InvariantCulture, new DateTimeStyles(),  out temp))
                        {
                            value = temp.ToUniversalTime().ToString();
                        }
                        
                    }

                    parameters.Add(new ReportParameter(parameterInfo.Name, value));
                }
            }
            reportViewer.LocalReport.SetParameters(parameters);
            return parameters;
        }

        /// <summary>
        /// Получение данных из источников, заполнение наборов данных отчета.
        /// </summary>
        private void fillReportData()
        {
            Dictionary<string, object> dataSources = reportsInfo.GetDataSources(reportInstance);

            foreach (var dataSource in dataSources)
            {
                reportViewer.LocalReport.DataSources.Add(new ReportDataSource(dataSource.Key, dataSource.Value));
            }
        }

        /// <summary>
        /// Получение отчета в формате PDF.
        /// Пока не используется, оставлено как пример реализации.
        /// </summary>
        private void RenderInPdf()
        {
            Microsoft.Reporting.WebForms.Warning[] warnings = null;
            string[] streamids = null;
            string mimeType = null;
            string encoding = null;
            string extension = null;
            string deviceInfo = "<DeviceInfo><SimplePageHeaders>True</SimplePageHeaders></DeviceInfo>";

            byte[] bytes = reportViewer.LocalReport.Render("PDF", deviceInfo, out mimeType, out encoding, out extension, out streamids, out warnings);

            Response.ClearContent();
            Response.ClearHeaders();
            Response.ContentType = "application/pdf";
            Response.BinaryWrite(bytes);
            Response.Flush();
            Response.Close();
        }

        private void OnError(object sender, ReportErrorEventArgs eventArgs)
        {
            throw new PlatformException("При обработке отчета произошла ошибка", eventArgs.Exception);
        }

        /// <summary>
        /// Возвращает параметры отчета в сериализованном (текстовом) виде для записи в БД аудита
        /// </summary>
        /// <param name="parameters"></param>
        /// <returns></returns>
        private string serializeReportParams(IEnumerable<ReportParameter> parameters)
        {
            return parameters.Select(p => string.Format("{0}={1}",
                p.Name,
                p.Values.Cast<string>().DefaultIfEmpty().Aggregate((a, b) => string.Format("{0}, {1}", a, b))
                ))
                .DefaultIfEmpty()
                .Aggregate((a, b) => string.Format("{0}&{1}", a, b));
        }

        #endregion

    }
}