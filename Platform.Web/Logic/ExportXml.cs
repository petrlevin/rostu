using System;
using System.Collections.Specialized;
using BaseApp.Service;
using BaseApp.Service.Common;
using Platform.Web.Common;
using Platform.Web.Interfaces;

namespace Platform.Web.Logic
{
    /// <summary>
    /// Summary description for ExportXml
    /// </summary>
    public class ExportXml : IFileDownload
    {

        private string _exportType;
        private int _idEntity;

        public FileDataInfo GetFile()
        {
            string extension = "xml";

            string contentType = null;
            if (!string.IsNullOrEmpty(_exportType))
                contentType = "application/vnd.ms-" + _exportType;

            string result = new XmlExchange().SimpleExport(_idEntity);

            return new FileDataInfo
                {
                    ContentType = contentType,
                    FileName = String.Format("export({0}).{1}",_idEntity, extension),
                    FileData = result
                };
        }

        public void SetParams(NameValueCollection values)
        {
            _exportType = values["type"];
            _idEntity = values.GetInt("idEntity");

        }
    }
}