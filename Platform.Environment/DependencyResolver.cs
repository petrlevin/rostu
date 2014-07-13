using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Practices.Unity;
using Platform.Caching.Common;
using Platform.Environment.Dependencies;
using Platform.Environment.Interfaces;

namespace Platform.Environment
{
    public abstract class DependencyResolver<TApplicationStorage, TSessionStorage, TRequestStorage> : DependencyResolverBase<TApplicationStorage, TSessionStorage, TRequestStorage>
        where TRequestStorage:IRequestStorageBase
        where TApplicationStorage : IApplicationStorageBase
        
    {
        protected DependencyResolver(IStorageContainer<TApplicationStorage, TSessionStorage, TRequestStorage> environment) : base(environment)
        {
        }

        protected DependencyResolver(IStorageContainer<TApplicationStorage, TSessionStorage, TRequestStorage> environment ,IUnityContainer unityContainer)
            : base(environment,unityContainer)
        {
        }

        /// <summary>
        /// Регистрирует динамический экземпляр уровня запроса соединение с БД
        /// </summary>
        protected override void DefineResolvance()
        {
 	        base.DefineResolvance();
            RegisterApplicationInstance<ICache>("Cache",a=>a.Cache);
            RegisterApplicationInstance<IManagedCache>("Cache", a => a.Cache);
            RegisterRequestInstance("DbConnection", r => r.DbConnection);
            
	        RegisterRequestInstance("Decorators", r => r.Decorators);
        }
    }
}
