using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Practices.Unity;
using Platform.BusinessLogic.Denormalizer.Analyzers;
using Platform.Unity.Common.Interfaces;
using Platform.Utils;

namespace Platform.BusinessLogic.AppServices
{
    /// <summary>
    /// Информация о зарегистрированных пунктах меню "Действия" для форм списка.
    /// </summary>
    public class ListFormActionsInfo
    {
        public class Registrator : IDefaultRegistration
        {
            public void Register(IUnityContainer unityContainer)
            {
                var temp = new ListFormActionsInfo();
                temp.Init();

                unityContainer.RegisterInstance(typeof(ListFormActionsInfo), temp);
            }
        }

        /// <summary>
        /// Пункт меню "Действия"
        /// </summary>
        public class MenuItem
        {
            /// <summary>
            /// Наименование веб-сервиса
            /// </summary>
            public string Service { get; set; }

            /// <summary>
            /// Имя метода веб-сервиса
            /// </summary>
            public string Method { get; set; }

            /// <summary>
            /// Наименование пункта меню
            /// </summary>
            public string Caption { get; set; }
        }

        public Dictionary<string, List<MenuItem>> Actions { get; set; }

        public List<MenuItem> GetAction(string entityName)
        {
            if (!Actions.ContainsKey(entityName))
                return null;

            return Actions[entityName];
        }

        public void Init()
        {
            Actions = Assemblies.AllTypes()
                .Where(t => t.GetCustomAttributes(typeof(AppServiceAttribute), true).Any())
                .SelectMany(t => t.GetMethods().Where(mi => mi.GetCustomAttributes(typeof(ListFormActionAttribute), true).Any()))
                .GroupBy(mi => getAttribute(mi).EntityName)
                .ToDictionary(
                    a => a.Key,
                    a => a.OrderBy(mi => getAttribute(mi).Order).Select(getMenuConfig).ToList()
                    );
        }

        private ListFormActionAttribute getAttribute(MemberInfo memberInfo)
        {
            return memberInfo.GetCustomAttribute<ListFormActionAttribute>();
        }

        private MenuItem getMenuConfig(MethodInfo mi)
        {
            ListFormActionAttribute attr = getAttribute(mi);
            return new MenuItem()
                {
                    Service = mi.DeclaringType.Name,
                    Method = mi.Name,
                    Caption = attr.Caption,
                };
        }
    }
}
