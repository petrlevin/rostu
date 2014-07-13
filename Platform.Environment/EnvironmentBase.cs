using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Platform.Environment.Interfaces;

namespace Platform.Environment
{
    /// <summary>
    /// Базовый класс среды
    /// </summary>
    /// <typeparam name="TApplicationStorage"> тип хранилища уровня приложения</typeparam>
    /// <typeparam name="TSessionStorage">тип хранилища уровня сессии</typeparam>
    /// <typeparam name="TRequestStorage">тип хранилища уровня запроса</typeparam>
    public class EnvironmentBase<TApplicationStorage, TSessionStorage, TRequestStorage> : IEnvironment<TApplicationStorage, TSessionStorage, TRequestStorage>
        where TRequestStorage:class,IRequestStorageBase 
        where TApplicationStorage:class 
        where TSessionStorage:class
        
    {
        public virtual TApplicationStorage ApplicationStorage { get; protected set; }
        public virtual TSessionStorage SessionStorage { get; protected set; }
        public virtual TRequestStorage RequestStorage { get; protected set; }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="applicationStorage"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public virtual IEnvironment<TApplicationStorage, TSessionStorage, TRequestStorage> ApplicationStart(TApplicationStorage applicationStorage)
        {
            if (applicationStorage==null)
                throw new ArgumentNullException("applicationStorage");
            ApplicationStorage = applicationStorage;
            return this;
        }

        /// <exception cref="ArgumentNullException"></exception>
        public IEnvironment<TApplicationStorage, TSessionStorage, TRequestStorage> SessionStart(TSessionStorage  sessionStorage)
        {
            if (sessionStorage == null)
                throw new ArgumentNullException("sessionStorage");

            SessionStorage = sessionStorage;
            return this;
        }

        /// <exception cref="ArgumentNullException"></exception>
        public IEnvironment<TApplicationStorage, TSessionStorage, TRequestStorage> RequestStart(TRequestStorage requestStorage)
        {
            if (requestStorage == null)
                throw new ArgumentNullException("requestStorage");
            RequestStorage = requestStorage;
            return this;
        }
    }
}
