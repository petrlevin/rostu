using System;
using System.Data;
using System.Data.Entity;
using BaseApp.Service.Common;
using Newtonsoft.Json;
using Platform.BusinessLogic;
using Platform.BusinessLogic.Reference;
using Platform.Common;
using DataContext = Sbor.Reports.DataContext;

namespace Platform.Web.Services
{
    public class UploadFileService : DataAccessService
	{
        /// <summary>
        /// Возвращает идентификатор файла сохраненного в ref.FileStore
        /// </summary>
        /// <param name="file"></param>
        /// <param name="fileCaption"></param>
        /// <param name="fileType"></param>
        /// <param name="fileDescription"></param>
        /// <returns></returns>
        public string Upload(byte[] file, string fileCaption, string fileType, string fileDescription)
		{
            var dc = IoC.Resolve<DbContext>().Cast<DataContext>();

            var fileStoreElement = dc.FileStore.Create<FileStore>();
            fileStoreElement.File = file;

            try
		    {
                dc.Entry(fileStoreElement).State = EntityState.Added;
		        dc.SaveChanges();

		        var fileLinkElement = new FileLink
                    {
		                Caption = fileCaption + "." + fileType,
                        Date = DateTime.Now,
                        Description = fileDescription,
                        FileSize = file.Length,
                        IdFileStore = fileStoreElement.Id,
                        IsDbStore = true,
                        Extension = fileType
		            };
                dc.Entry(fileLinkElement).State = EntityState.Added;
		        
		        dc.SaveChanges();

		        const string resultMsg = "Файл успешно загружен";
                var result = new FileUploadServiceResponse
		                            {
		                                success = true,
		                                msg = resultMsg,
                                        FileCaption = fileLinkElement.Caption,
                                        FileLinkId = fileLinkElement.Id,
                                        FileDescription = fileLinkElement.Description
		                            };

		        return JsonConvert.SerializeObject(result);
		    }
		    catch (Exception ex)
		    {
		        var resultMsg = "Произошла ошибка загрузки файла :" + ex.Message;
		        var result = new ServerResponse
		                            {
		                                success = false,
		                                msg = resultMsg
		                            };
		        return JsonConvert.SerializeObject(result);
		    }
		}

	}
}