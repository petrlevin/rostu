using System.Collections.Generic;
using System.IO;
using System.Web;

namespace Platform.Web.Common
{
    /// <summary>
    /// Общие методы для работой с соединением запроса
    /// </summary>
    public static class HttpRequestHelper
    {
        /// <summary>
        /// Первый файл из запроса
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public static byte[] GetFirstFile(this HttpRequest request)
        {
            return GetFile(request, 0);
        }

        /// <summary>
        /// Получить n-й файл из запроса
        /// </summary>
        /// <param name="request"></param>
        /// <param name="position"></param>
        /// <returns></returns>
        public static byte[] GetFile(this HttpRequest request, int position)
        {
            var stream = request.Files[position].InputStream;
			BinaryReader binReader = new BinaryReader(stream);
			
            var file = binReader.ReadBytes((int) stream.Length);
			
            binReader.Close();
			stream.Close();
			
            return file;
		}

        /// <summary>
        /// Получить все файлы, пришедшие в запросе
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public static IEnumerable<byte[]> GetFiles(this HttpRequest request)
        {
            var fileLength = request.Files.Count;

            var files = new List<byte[]>();

            for (int i = 0; i < fileLength; i++)
                files.Add(request.GetFile(i));
            
            return files;
        }

    }
}