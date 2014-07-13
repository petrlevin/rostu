using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.Practices.Unity;
using Platform.Client;
using Platform.Common.Exceptions;
using Platform.Unity.Common.Interfaces;

namespace Platform.BusinessLogic.ReportingServices.PrintForms
{
    public class PrintFormsInfo : InfoBase<Func<int, PrintFormBase>, PrintFormAttribute>
    {
        public class Registrator : IDefaultRegistration
        {
            public void Register(IUnityContainer unityContainer)
            {
                var info = new PrintFormsInfo();
                info.Init();
                unityContainer.RegisterInstance(typeof (PrintFormsInfo), info);
            }
        }

        public Dictionary<string, object> GetDataSources(string entityName, string className, params object[] ctorParams)
        {
            var ctor = GetConstructorBy(
                t => t.Name == className && getEntityNameByType(t) == entityName,
                string.Format("В системе не зарегистрирован класс печатной формы с именем {0} для сущности {1}", className, entityName)
                );
            var instance = CreateInstance(ctor, ctorParams);

            return GetDataSources(instance);
        }

        /// <summary>
        /// Получает список пунктов меню splitbutton для сущности <paramref name="entityName"></paramref>, каждый из которых соответствует печатной форме.
        /// </summary>
        /// <param name="entityName"></param>
        /// <returns></returns>
        public IEnumerable<ExtMenuItem> GetPrintFormsFor(string entityName)
        {
            return Constructors
                .Where(a => getEntityNameByType(a.Key) == entityName)
                .Select(a => new ExtMenuItem()
                    {
                        name = a.Key.Name,
                        text = a.Key.GetCustomAttribute<PrintFormAttribute>().Caption
                    });
        }

        /// <summary>
        /// Определяет имя сущности по имени типа ПФ или Отчета
        /// </summary>
        /// <param name="type">Класс ПФ или отчета</param>
        /// <returns>Имя пространства имен, непосредственно в котором находится определение класса ПФ</returns>
        protected string getEntityNameByType(Type type)
        {
            var tmp = type.FullName.Substring(0, type.FullName.Length - type.Name.Length - 1);
            return tmp.Substring(tmp.LastIndexOf('.') + 1);
        }


        protected override object CreateInstance(Func<int, PrintFormBase> ctor, params object[] ctorParams)
        {
            if (ctorParams.Count() != 1 || ctorParams.First().GetType() != typeof(int))
                throw new PlatformException("Для создания экземпляра класса печатной формы должен быть передан один параметр типа int.");

            return ctor((int) ctorParams[0]);
        }
    }
}
