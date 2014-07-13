using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.Practices.Unity;
using Platform.Client;
using Platform.Common.Exceptions;
using Platform.Unity.Common.Interfaces;

namespace Platform.BusinessLogic.ReportingServices.Reports
{
    public class ReportsInfo: InfoBase<Func<object>, ReportAttribute>
    {
        public class Registrator : IDefaultRegistration
        {
            public void Register(IUnityContainer unityContainer)
            {
                var info = new ReportsInfo();
                info.Init();
                unityContainer.RegisterInstance(typeof(ReportsInfo), info);
            }
        }

        public object CreateInstance(string entityName, params object[] ctorParams)
        {
            var ctor = GetConstructorBy(
                t => t.Name == entityName,
                string.Format("В системе не зарегистрирован класс отчета для сущности {0}", entityName)
                );
            return CreateInstance(ctor, ctorParams);
        }

        protected override object CreateInstance(Func<object> ctor, params object[] ctorParams)
        {
            if (ctorParams.Count() != 0)
                throw new PlatformException("Для создания экземпляра класса отчетной формы используется конструктор без параметров, а вы пытаетесь в него что-то передать");

            return ctor();
        }
    }
}
