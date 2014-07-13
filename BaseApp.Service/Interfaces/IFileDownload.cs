using System.Collections.Specialized;
using BaseApp.Service.Common;

namespace Platform.Web.Interfaces
{
    public interface IFileDownload
    {
        /// <summary>
        /// Получить файл
        /// </summary>
        /// <returns></returns>
        FileDataInfo GetFile();

        /// <summary>
        /// В обработчике установить параметры, пришедшие из запроса 
        /// </summary>
        /// <returns></returns>
        void SetParams(NameValueCollection values);
    }
}
