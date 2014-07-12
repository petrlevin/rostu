using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using BaseApp.Common.Interfaces;
using Microsoft.Reporting.WebForms;
using Platform.BusinessLogic;
using Platform.BusinessLogic.Auditing;
using Platform.BusinessLogic.Auditing.Auditors;
using Platform.BusinessLogic.ReportingServices.PrintForms;
using Platform.Common;
using Platform.PrimaryEntities.Common.DbEnums;
using Platform.PrimaryEntities.Reference;
using Platform.Web.Services.Extensions;

namespace Platform.Web.Services
{
    public partial class PrintForm : System.Web.UI.Page
    {
        private ReportViewer reportViewer
        {
            get { return (ReportViewer)Master.FindControl("ReportViewer1"); }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                readParams();
                bool success = true;
                var auditor = Audit<ReportExecutionsAuditor>.Start(new ReportExecutionsAuditor()
                {
                    ReportType = ReportExecutionsAuditor.ReportTypeEnum.PrintForm,
                    ReportEntity = entity.Id,
                    ReportEntityItem = docId,
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
                    auditor.Auditor.Success = success;
                    auditor.Complete();
                }
                
            }
        }

        private Tests.DataContext db
        {
            get { return IoC.Resolve<DbContext>().Cast<Tests.DataContext>(); }
        }

        private string entityNameParam;

        /// <summary>
        /// Имя сущности документа
        /// </summary>
        private string entityName { get; set; }

        private Entity _entity;
        /// <summary>
        /// Сущность документа для запрошенной печатной формы
        /// </summary>
        private Entity entity
        {
            get
            {
                if (_entity == null)
                {
                    _entity = db.Entity.Single(e => e.Name == entityName);
                }
                return _entity;
            }
        }

        private PrintFormsInfo reportsInfo;

        public PrintForm()
        {
            entityNameParam = "entityName";
        }

        /// <summary>
        /// Имя класса печатной формы
        /// </summary>
        private string printFormClassName { get; set; }

        /// <summary>
        /// id документа
        /// </summary>
        private int docId { get; set; }

        private void process()
        {
            reportsInfo = IoC.Resolve<PrintFormsInfo>();
            reportViewer.LocalReport.ReportPath = getReportPath(); //ToDo: fix path
            fillReportData();
        }

        /// <summary>
        /// Получает полный путь до файла макета *.rdlc
        /// </summary>
        /// <returns></returns>
        private string getReportPath()
        {
            string filePathStand = Server.MapPath(string.Format("~/bin/PrintForms/{0}/{1}.rdlc", entityName, printFormClassName));
#if DEBUG
            //Если есть файл разработчика, берем его, чтобы не было необходимости в компиляции при изменении макета отчета
            string filePathDeveloper = Server.MapPath("~/") + string.Format(@"../Sbor.Reports/PrintForms/{0}/{1}.rdlc", entityName, printFormClassName);

            if (File.Exists(filePathDeveloper))
                return filePathDeveloper;
            return filePathStand;
#else
            return filePathStand;
#endif

        }

        private void readParams()
        {
            entityName = this.Request.Params[entityNameParam];
            printFormClassName = this.Request.Params["printFormClassName"];
            docId = this.GetInt("docId");
        }

        private void fillReportData()
        {
            Dictionary<string, object> dataSources = reportsInfo.GetDataSources(entityName, printFormClassName, docId);

            foreach (var dataSource in dataSources)
            {
                reportViewer.LocalReport.DataSources.Add(new ReportDataSource(dataSource.Key, dataSource.Value));
            }
        }
    }
}