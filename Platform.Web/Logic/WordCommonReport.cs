using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data.Entity;
using System.Linq;
using System.Web;
using BaseApp.Service.Common;
using Platform.BusinessLogic;
using Platform.Common;
using Platform.Common.Exceptions;
using Platform.Web.Common;
using Platform.Web.Interfaces;
using Sbor.Reports;
using DataContext = Sbor.Reports.DataContext;

namespace Platform.Web.Logic
{
    public class WordCommonReport : IFileDownload
    {
        private int _id;

        public FileDataInfo GetFile()
        {
            var dc = IoC.Resolve<DbContext>().Cast<DataContext>();

            var report = dc.WordCommonReport.FirstOrDefault(f => f.Id == _id);
            if (report == null)
                throw new PlatformException(String.Format("Отчет #{0} не найден", _id));

            var reportFile = report.GetReportResult();

            return new FileDataInfo
                {
                    FileData = reportFile,
                    FileName = report.TemplateFileLink.Caption
                };
        }

        public void SetParams(NameValueCollection values)
        {
            var idReportString = values["id"];
            if (string.IsNullOrEmpty(idReportString) || !int.TryParse(idReportString, out _id))
                throw new PlatformException("В запросе не указан идентификатор сущности");
        }
    }
}