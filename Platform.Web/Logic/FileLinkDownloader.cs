using System;
using System.Collections.Specialized;
using System.Data.Entity;
using System.Linq;
using BaseApp.Service.Common;
using Platform.BusinessLogic;
using Platform.Common;
using Platform.Common.Exceptions;
using Platform.Web.Common;
using Platform.Web.Interfaces;
using DataContext = Sbor.Reports.DataContext;

namespace Platform.Web.Logic
{
    public class FileLinkDownloader : IFileDownload
    {
        private int _idFileLink;

        public FileDataInfo GetFile()
        {
            var dc = IoC.Resolve<DbContext>().Cast<DataContext>();

            var fileLink = dc.FileLink.FirstOrDefault(f => f.Id == _idFileLink);
            if (fileLink == null)
                throw new PlatformException(String.Format("Элемент справочника FileLink #{0} не найден", _idFileLink ));

            return new FileDataInfo
                {
                    FileData = fileLink.FileStore.File,
                    FileName = fileLink.Caption
                };
        }

        public void SetParams(NameValueCollection values)
        {
            var linkFileString = values["fileLinkId"];
            if (String.IsNullOrEmpty(linkFileString))
                throw new PlatformException("Не указан обязательный параметр fileLinkId");

            if (!Int32.TryParse(linkFileString, out _idFileLink))
                throw new PlatformException("Параметр fileLinkId не является числом");
        }
    }
}