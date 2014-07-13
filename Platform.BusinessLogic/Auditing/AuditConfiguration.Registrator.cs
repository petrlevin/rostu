using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Practices.Unity;
using Platform.Common.Exceptions;
using Platform.Unity.Common.Interfaces;

namespace Platform.BusinessLogic.Auditing
{
    public partial class AuditConfiguration
    {
        /// <summary>
        /// Начальная настройка аудита и регистрация объекта в контейнере Unity
        /// </summary>
        public class Registrator : IDefaultRegistration
        {
            /// <summary>
            /// Зарегистрировать аудит
            /// </summary>
            /// <param name="unityContainer"></param>
            /// <exception cref="PlatformException"></exception>
            public void Register(IUnityContainer unityContainer)
            {
                var section = ConfigurationManager.GetSection("audit") as Section;
                if ((section == null) || (!section.Enabled))
                    unityContainer.RegisterInstance(new AuditConfiguration { Enabled = false, OperationsEnabled = false, RequestsEnabled = false });

                else
                {
                    var operationsEnabled = section.OperationsEnabled;
                    var requestsEnabled = section.RequestsEnabled;
                    string connectionString = String.IsNullOrEmpty(section.ConnectionstringName)
                        ? AuditConnectionString.Get()
                        : ConfigurationManager.ConnectionStrings[section.ConnectionstringName].ConnectionString;

                    if (string.IsNullOrEmpty(connectionString))
                        throw new PlatformException(String.Format("Строка соединения '{0}' указанная в секции 'audit' не найдена", section.ConnectionstringName));

                    unityContainer.RegisterInstance(new AuditConfiguration
                    {
                        Enabled = true,
                        OperationsEnabled = operationsEnabled,
                        RequestsEnabled = requestsEnabled,
                        ConnectionString = connectionString,
                    });
                }
            }
        }
    }
}
