using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Web;
using BaseApp.Export;
using BaseApp.Service.Common;
using Platform.BusinessLogic.Auditing;
using Platform.BusinessLogic.Auditing.Auditors;
using Platform.Common.Exceptions;
using Platform.Web.Common;
using Platform.Web.Interfaces;

namespace Platform.Web.Logic
{
    public class TableReport : IFileDownload
    {
        private int _reportId;

        public FileDataInfo GetFile()
        {
            var report = new TableReportExport(_reportId);
            string reportResult = string.Empty;

            bool success = true;
            var auditor = Audit<ReportExecutionsAuditor>.Start(new ReportExecutionsAuditor()
            {
                ReportType = ReportExecutionsAuditor.ReportTypeEnum.TableReport,
                ReportEntityItem = _reportId,
                Date = DateTime.Now
            });
            try
            {
                reportResult = report.BuildReport();
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

            return new FileDataInfo
                {
                    ContentType = "application/vnd.ms-excel",
                    FileName = "tableReport.xls",
                    FileData = reportResult
                };
        }

        public void SetParams(NameValueCollection values)
        {
            if (!int.TryParse(values["reportId"], out _reportId))
            {
                throw new PlatformException("Не передан обязательный параметр reportId");
            }
        }
    }
}