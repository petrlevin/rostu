using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Practices.Unity;
using Platform.BusinessLogic.ReportingServices.PrintForms;
using Platform.Common.Exceptions;
using Platform.Unity.Common.Interfaces;

namespace Platform.BusinessLogic.ReportingServices
{
    public abstract class InfoBase<TCtor, TAttr> where TAttr: Attribute
    {
        public void Init()
        {
            Constructors = getConstructors();
        }

        public Dictionary<string, object> GetDataSources(object instance)
        {
            var list = new Dictionary<string, object>();
            IEnumerable<MethodInfo> methods = getDataSetMethods(instance);

            foreach (var methodInfo in methods)
            {
                object result;
                try
                {
                    result = methodInfo.Invoke(instance, new object[] { });
                }
                catch (TargetException ex)
                {
                    throw new PlatformException(string.Format("При выполнении метода получения данных для датасета произошла ошибка. " +
                                                              "\n Имя метода: {0}.{1}. " +
                                                              "\n Исключение: {2}", instance.GetType().FullName, methodInfo.Name, ex.Message), ex);
                }
                
                list.Add(methodInfo.Name, result);
            }
            return list;
        }

        #region Protected Members

        /// <summary>
        /// Словарь вида:
        /// Ключ - тип класса печатной формы/отчета/... Класс помечен аттрибутом TAttr.
        /// Значение - лямбда-выражение для создания объекта класса TType.
        /// </summary>
        protected Dictionary<Type, TCtor> Constructors { get; set; }

        protected TCtor GetConstructorBy(Predicate<Type> condition, string errorMsg = defaultClassNotExistsMessage)
        {
            KeyValuePair<Type, TCtor> result = Constructors.SingleOrDefault(a => condition(a.Key));
            if (result.Equals(default(KeyValuePair<Type, TCtor>)))
            {
                throw new PlatformException(errorMsg);
            }
                
            return result.Value;
        }

        protected abstract object CreateInstance(TCtor ctor, params object[] ctorParams);

        #endregion

        #region Private Members

        private const string defaultClassNotExistsMessage = "В системе не зарегистрирован класс отчета или печатной формы";

        private IEnumerable<MethodInfo> getDataSetMethods(object instance)
        {
            return instance.GetType().GetMethods(
                BindingFlags.Public 
                | BindingFlags.Instance
                //| BindingFlags.DeclaredOnly убран, т.к. db.Set(entity).Find(itemId) возвращает объект *сгенерированного наследника*
                )
                .Where(a => 
                    !a.IsSpecialName
                    && a.ReturnType.IsGenericType
                    && (a.ReturnType.GetGenericTypeDefinition() == typeof(List<>) || a.ReturnType.GetGenericTypeDefinition() == typeof(IEnumerable<>))
                    );
        }

        private Dictionary<Type, TCtor> getConstructors()
        {
            IEnumerable<Assembly> assemblies = getAppAssemblies();
            if (!assemblies.Any())
                throw new PlatformException("Не найдено ни одной сборки с отчетами. Ссылки на них должны быть в Platform.Web");

            return assemblies.SelectMany(a => a.GetTypes())
                             .Where(t => t.GetCustomAttribute<TAttr>() != null)
                             .ToDictionary(t => t, createFactory);
        }

        private TCtor createFactory(Type type)
        {
            ConstructorInfo constuctor = type.GetConstructors().OrderBy(x => x.GetParameters().Count()).First();
            var p = new List<ParameterExpression>();
            foreach (ParameterInfo parameterInfo in constuctor.GetParameters())
            {
                p.Add(Expression.Parameter(parameterInfo.ParameterType));
            }
            Expression exp = Expression.New(constuctor, p);
            return Expression.Lambda<TCtor>(exp, p).Compile();
        }

        private IEnumerable<Assembly> getAppAssemblies()
        {
            var appAssemblies = new string[]
                {
                    "Sbor.Reports",
                };

            return AppDomain.CurrentDomain.GetAssemblies().Where(a => appAssemblies.Contains(a.GetName().Name));
        }

        #endregion
    }
}
