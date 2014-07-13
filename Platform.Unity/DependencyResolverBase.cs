using System.Collections.Generic;
using System.Linq;
using Microsoft.Practices.Unity;
using Platform.Unity.Common.Interfaces;

namespace Platform.Unity
{
    public  class DependencyResolverBase : IDependencyResolver 
    {
        protected internal readonly IUnityContainer UnityContainer;

        public DependencyResolverBase(IUnityContainer unityContainer)
        {
            UnityContainer = unityContainer;
        }

        public DependencyResolverBase()
        {
            UnityContainer = new UnityContainer();
        }


        public T Resolve<T>()
        {
            try
            {
                return GetCurrentContainer().Resolve<T>();
            }
            catch (ResolutionFailedException)
            {
                //Если тип не зарегистрирован пробуем разрешить используя не параметризованный констуктор
                if (GetCurrentContainer().Registrations.Any(cr => cr.RegisteredType == typeof(T)))
                    throw;
                else
                {
                    GetCurrentContainer().RegisterType<T>(new InjectionConstructor());
                    return Resolve<T>();
                }
            }
            
        }

        public T Resolve<T>(string name)
        {
            return GetCurrentContainer().Resolve<T>(name);
        }

        public IEnumerable<T> ResolveAll<T>()
        {
            return GetCurrentContainer().ResolveAll<T>();
        }

        public void Dispose()
        {
            UnityContainer.Dispose();
        }

        private IUnityContainer GetCurrentContainer()
        {
            return IoCServices.CurrentContainer ?? UnityContainer;
        }

    }
}