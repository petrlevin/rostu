using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Practices.Unity;
using Platform.Common;
using Platform.Unity.Common.Interfaces;

namespace Platform.Unity
{
    public static class IoCServices
    {
        private static IUnityContainer _rootContainer;
        [ThreadStatic]private static List<ContainerWrapper> _containersStacks;

        private static List<ContainerWrapper>GetStack()
        {
            return _containersStacks ?? (_containersStacks = new List<ContainerWrapper>());
        }

        internal static IUnityContainer CurrentContainer
        {
            get
            {
                
               return  GetStack().Any()
                    ? GetStack().Last().Inner
                    : _rootContainer;
            }
        }


        public static IUnityContainer CreateScope()
        {
           if (_rootContainer== null)
               throw new InvalidOperationException("Cервисы внедрения зависимостей не были проинициализированы.");
            var stack = GetStack();
            IUnityContainer newContainer = CurrentContainer.CreateChildContainer();
            var scope = new ContainerWrapper(newContainer);
            stack.Add(scope);
            scope.OnDisposed+= OnScopeDisposed;
            return scope;

        }

        private static void OnScopeDisposed(ContainerWrapper scope)
        {
            if (scope.Disposed)
                return;
            var stack = GetStack();
            if (stack.Last()!= scope)
                throw new InvalidOperationException("Область действия регистрации (Scope) следует отпускать (dispose) в порядке получения объектов. Используйте конструкцию using.");
            stack.RemoveAt(stack.Count-1);


        }

        public static void InitWith(DependencyResolverBase resolver, bool registerDefaultsRegistrations)
        {
            Init(resolver);
            if (registerDefaultsRegistrations)
                RegisterDefaults(_rootContainer, new List<Type>());
        }

        private static void Init(DependencyResolverBase resolver)
        {
            _rootContainer = resolver.UnityContainer;
            if (_rootContainer == null)
                throw new ArgumentException(
                    "У резолвера не установлен контейнер. Инициализация сервисов внедрения зависимостей (IoC) невозможна.");

            _rootContainer.AddExtension(new Remember());
            IoC.InitWith(resolver);
        }


        public static void InitWith(DependencyResolverBase resolver , IEnumerable<Type> exceptDefaultRegistrations = null )
        {
            Init(resolver);

            RegisterDefaults(_rootContainer, exceptDefaultRegistrations);

        }

        public static void InitWith(DependencyResolverBase resolver, params  Type[] exceptDefaultRegistrations )
        {
            InitWith(resolver,exceptDefaultRegistrations.ToList());
       } 


        private static void RegisterDefaults(IUnityContainer rootContainer, IEnumerable<Type> exceptDefaultRegistrations)
        {
            new DefaultRegistrator(rootContainer).RegisterAll(exceptDefaultRegistrations);
        }
    }
}
