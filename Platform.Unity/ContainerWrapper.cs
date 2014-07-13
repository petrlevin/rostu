using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Practices.Unity;

namespace Platform.Unity
{
    internal class ContainerWrapper : IDisposable , IUnityContainer
    {
        private readonly IUnityContainer _inner;


        internal IUnityContainer Inner
        {
            get { return _inner; }
        }

        public ContainerWrapper(IUnityContainer inner)
        {
            _inner = inner;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <exception cref="InvalidOperationException"></exception>
        public void Dispose()
        {
            if (OnDisposed != null)
                OnDisposed(this);
            _inner.Dispose();
            Disposed = true;
        }

        internal event Action<ContainerWrapper> OnDisposed;

        internal bool Disposed { get; private set; }

        public IUnityContainer RegisterType(Type @from, Type to, string name, LifetimeManager lifetimeManager,
                                            params InjectionMember[] injectionMembers)
        {
            _inner.RegisterType(@from, to, name, lifetimeManager, injectionMembers);
            return this;
        }

        public IUnityContainer RegisterInstance(Type t, string name, object instance, LifetimeManager lifetime)
        {
            _inner.RegisterInstance(t, name, instance, lifetime);
            return this;
        }

        public object Resolve(Type t, string name, params ResolverOverride[] resolverOverrides)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<object> ResolveAll(Type t, params ResolverOverride[] resolverOverrides)
        {
            throw new NotImplementedException();
        }

        public object BuildUp(Type t, object existing, string name, params ResolverOverride[] resolverOverrides)
        {
            throw new NotImplementedException();
        }

        public void Teardown(object o)
        {
            throw new NotImplementedException();
        }

        public IUnityContainer AddExtension(Microsoft.Practices.Unity.UnityContainerExtension extension)
        {
            throw new NotImplementedException();
        }

        public object Configure(Type configurationInterface)
        {
            throw new NotImplementedException();
        }

        public IUnityContainer RemoveAllExtensions()
        {
            throw new NotImplementedException();
        }

        public IUnityContainer CreateChildContainer()
        {
            throw new NotImplementedException();
        }

        public IUnityContainer Parent
        {
            get { throw new NotImplementedException(); }
        }

        public IEnumerable<ContainerRegistration> Registrations
        {
            get { return _inner.Registrations; }
        }
    }
}
