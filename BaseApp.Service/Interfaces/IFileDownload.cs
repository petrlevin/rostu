using System.Collections.Specialized;
using BaseApp.Service.Common;

namespace Platform.Web.Interfaces
{
    public interface IFileDownload
    {
        /// <summary>
        /// �������� ����
        /// </summary>
        /// <returns></returns>
        FileDataInfo GetFile();

        /// <summary>
        /// � ����������� ���������� ���������, ��������� �� ������� 
        /// </summary>
        /// <returns></returns>
        void SetParams(NameValueCollection values);
    }
}
