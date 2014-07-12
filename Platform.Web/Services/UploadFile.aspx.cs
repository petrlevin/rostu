using System;
using System.IO;
using Newtonsoft.Json;
using Platform.Web.Common;
using Platform.Web.Services.Extensions;

namespace Platform.Web.Services
{

    public class FileUploadServiceResponse : ServerResponse
    {
        /// <summary>
        /// Наименование загруженного файла
        /// </summary>
        public string FileCaption { get; set; }

        public string FileDescription { get; set; }

        /// <summary>
        /// Идентификатор в справочнике LinkFile
        /// </summary>
        public int FileLinkId { get; set; }

    }

    public partial class UploadFile : System.Web.UI.Page
	{
        private string getFileType()
        {
            return Path.GetExtension(this.Request.Files[0].FileName).Remove(0,1);
        }

		protected void Page_Load(object sender, EventArgs e)
		{
            var file = Request.GetFirstFile();
		    var fileType = getFileType();
		    var fileCaption = this.Request.Params["caption"];
            var fileDescription = this.Request.Params["description"];

            var uploader = new UploadFileService();

            var resultMsg = uploader.Upload(file, fileCaption, fileType, fileDescription);

            Response.Write(resultMsg);
			Response.End();
		}
	}
}