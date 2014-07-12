using System;
using System.Diagnostics;
using System.Reflection;
using Platform.Web.UserManagement;

namespace Platform.Web.Services
{
    /// <summary>
    /// Сервис для получения системных значений
    /// </summary>
    public class SystemService
    {
        /// <summary>
        /// Получить наименование
        /// </summary>
        /// <returns>Возвращается информация о текущем сеансе - версия приложения и значения системных измерений</returns>
        public String GetTitle()
        {
            Assembly assembly = Assembly.GetExecutingAssembly();

            FileVersionInfo fvi = FileVersionInfo.GetVersionInfo(assembly.Location);
            string version = fvi.ProductVersion;

            var sysDimensions = Users.Current == null
                                    ? String.Empty
                                    : ", " + BaseApp.Environment.BaseAppEnvironment.Instance.SessionStorage.CurentDimensions
                                             .GetCaption();

            //ToDo: В версии файла можно хранить только 4 чилса, захотели 5, вынес первое число сюда т.к. оно неизвестно когда поменяется
            return string.Format("3.{0}{1}", version , sysDimensions);
        }
    }
}