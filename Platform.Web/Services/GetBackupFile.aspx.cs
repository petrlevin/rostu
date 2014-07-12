using System;
using System.IO;
using Platform.BusinessLogic.Common.Exceptions;
using Tools.IrkutskBackUp;

namespace Platform.Web.Services
{
    /// <summary>
    /// Получить последний бэкап текущей базы 
    /// </summary>
    public partial class GetBackupFile : System.Web.UI.Page
    {
        private string _localPath;
        private static object _lock = new object();

        const string TempDbDir = "_dbBackUp";

        protected void Page_Load(object sender, EventArgs e)
        {
            Action<bool, string> resultEvent = (status, msg) =>
            {
                if (!status)
                    throw new SystemUFException(msg);

                DoDownload(msg);
            };

            _localPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logs", TempDbDir);
            var fileDir = new DirectoryInfo(_localPath);

            if (!fileDir.Exists)
            {
                var di = new DirectoryInfo(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logs"));
                try
                {
                    di.CreateSubdirectory(TempDbDir);
                }
                catch (Exception ex)
                {
                    throw new SystemUFException("Не удалось создать папку. " + ex.Message);
                }
            }
            

            lock (_lock)
            {
                ClearDirectory();
                new SqlBackuper(_localPath, resultEvent).CopyBacup();    
            }
	    }

        private void ClearDirectory()
        {
            var dirInfo = new DirectoryInfo(_localPath);
            foreach (FileInfo file in dirInfo.GetFiles())
            {
                file.Delete();
            }
            foreach (DirectoryInfo dir in dirInfo.GetDirectories())
            {
                dir.Delete(true);
            } 
        }

        private void DoDownload(string pathToFile)
	    {
            var response = Context.Response;
            response.Clear();

	        using (var fs = new FileStream(pathToFile, FileMode.Open))
	        {
	            var fileSize = (int)fs.Length;
                byte[] content = new byte[fileSize];

	            fs.Read(content, 0, fileSize);

                response.Buffer = true;
	            response.BinaryWrite(content);
                
                response.AddHeader("Content-Length", fileSize.ToString());
	        }

	        var fileName = Path.GetFileName(pathToFile);
            response.AddHeader("Content-Disposition", "attachment;filename=\"" + fileName + "\"");

            var extension = (Path.GetExtension(pathToFile) ?? String.Empty).TrimStart('.') == "zip" ? "zip" : "x-7z-compressed";
	        response.ContentType = "application/" + extension;

            response.Flush();
            response.End();
        }
	}
}