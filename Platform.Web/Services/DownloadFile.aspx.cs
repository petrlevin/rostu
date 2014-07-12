using System;
using System.Linq;
using System.Reflection;
using BaseApp.Service.Common;
using Platform.Common.Exceptions;
using Platform.Web.Common;
using Platform.Web.Interfaces;

namespace Platform.Web.Services
{
    /// <summary>
    /// Входная точка для того, что в ответ должно отдавать файл
    /// </summary>
	public partial class DownloadFile : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!Request.Params.AllKeys.Contains("handlerType"))
                throw new PlatformException("Не указан тип обработчика");

            var handlerTypeName = Request.Params["handlertype"];
            var handlerType = Assembly.GetExecutingAssembly().GetTypes().FirstOrDefault(t => t.Name.ToLowerInvariant() == handlerTypeName.ToLowerInvariant());
            if (handlerType == null)
                throw new PlatformException(String.Format("Не удается найти класс обработчика по имени: {0}", handlerTypeName ));

            var handlerClass = Activator.CreateInstance(handlerType);

            if (!(handlerClass is IFileDownload))
                throw new PlatformException(String.Format("Не удается найти класс обработчика по имени: {0}", handlerTypeName));
	        
            ((IFileDownload)handlerClass).SetParams(Request.Params);
            DoDownload(((IFileDownload)handlerClass).GetFile());
	    }

	    private void DoDownload(FileDataInfo file)
	    {
            var response = Context.Response;
            response.Clear();

            if (file.FileData is byte[])
                response.BinaryWrite(file.FileData as byte[]);
            else
                response.Write(file.FileData.ToString());

            //todo: добавлять mime-тип по расширению файла
	        if (!String.IsNullOrEmpty(file.ContentType))
	            response.ContentType = file.ContentType;
            
            response.AddHeader("Content-Disposition", "attachment;filename=\"" + file.FileName + "\"");

            response.Flush();
            response.End();
        }
	}
}