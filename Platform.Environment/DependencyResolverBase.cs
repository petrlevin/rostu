using System;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Practices.Unity;

using Platform.Environment.Interfaces;
using Platform.Unity;
using Platform.Unity.Common.Interfaces;

namespace Platform.Environment
{
    public abstract class DependencyResolverBase<TApplicationStorage, TSessionStorage, TRequestStorage> : DependencyResolverBase, IDependencyResolver
        where TRequestStorage : IRequestStorageBase
    {
        #region Private

        private readonly IStorageContainer<TApplicationStorage, TSessionStorage, TRequestStorage> _environment;

        private void RegisterInstance<T>(string name, Func<T> getValue)
            where T : class
        {
            UnityContainer.RegisterInstance<T>(name, () => (Object)getValue());

        }

        private void RegisterInstance<T>( Func<T> getValue)
            where T : class
        {
            UnityContainer.RegisterInstance<T>(() => (Object)getValue());

        }



        #endregion

        #region Protected

        /// <summary>
        /// 
        /// </summary>
        /// <param name="environment">среда предоставляющая динамический доступ к экземплярам для далнейшего разрешения объектов</param>
        /// <param name="unityContainer"></param>
        protected DependencyResolverBase(IStorageContainer<TApplicationStorage, TSessionStorage, TRequestStorage> environment, IUnityContainer unityContainer):base(unityContainer)
        {
            if (environment == null) throw new ArgumentNullException("environment");
            if (unityContainer == null) throw new ArgumentNullException("unityContainer");

            _environment = environment;


            DefineResolvance();

        }

        protected DependencyResolverBase(
            IStorageContainer<TApplicationStorage, TSessionStorage, TRequestStorage> environment)
            : this(environment, new UnityContainer())
        {
            
        }

        /// <summary>
        /// Определение разрешения объектов
        /// Наследники должны переопределить этот метод
        /// используя в нем RegisterSessionInstance , RegisterRequestInstance, RegisterApplicationInstance
        /// </summary>
        protected virtual void DefineResolvance()
        {
        }



        /// <summary>
        /// Регистрация динамичского экземпляра
        /// уровня сесиии
        /// </summary>
        /// <typeparam name="T">Тип экземпляра</typeparam>
        /// <param name="name">Наименование экземпляра</param>
        /// <param name="getValue">Функция динамического получения экземпляра (s=>s.someProperty)</param>
        protected void RegisterSessionInstance<T>(string name, Func<TSessionStorage, T> getValue)
            where T : class
        {
            RegisterInstance(name, () => getValue(_environment.SessionStorage));
        }


        /// <summary>
        /// Регистрация динамичского экземпляра
        /// уровня сесиии без имени
        /// </summary>
        /// <typeparam name="T">Тип экземпляра</typeparam>

        /// <param name="getValue">Функция динамического получения экземпляра (s=>s.someProperty)</param>
        protected void RegisterSessionInstance<T>( Func<TSessionStorage, T> getValue)
            where T : class
        {
            RegisterInstance( () => getValue(_environment.SessionStorage));
        }

        /// <summary>
        /// Регистрация динамичского экземпляра
        /// уровня запрос
        /// </summary>
        /// <typeparam name="T">Тип экземпляра</typeparam>
        /// <param name="name">Наименование экземпляра</param>
        /// <param name="getValue">Функция динамического получения экземпляра (r=>r.someProperty)</param>
        protected void RegisterRequestInstance<T>(string name, Func<TRequestStorage, T> getValue)
            where T : class
        {
            RegisterInstance(name, () => getValue(_environment.RequestStorage));
        }

        /// <summary>
        /// Регистрация динамичского экземпляра
        /// уровня запрос без имени
        /// </summary>
        /// <typeparam name="T">Тип экземпляра</typeparam>

        /// <param name="getValue">Функция динамического получения экземпляра (r=>r.someProperty)</param>
        protected void RegisterRequestInstance<T>(Func<TRequestStorage, T> getValue)
            where T : class
        {
            RegisterInstance( () => getValue(_environment.RequestStorage));
        }


        /// <summary>
        /// Регистрация типа
        /// с хранением уровня запрос без имени
        /// </summary>
        /// <typeparam name="TIn">входной тип (тип регистрации - базовый)) </typeparam>
        /// <param name="typeTo"> выходной тип (тип реализации)</param>
        /// <param name="getValue">Выражение динамического получения и установки экземпляра (r=>r.someProperty)</param>
        protected void RegisterRequestType<TIn>(Type typeTo, Expression<Func<TRequestStorage, TIn>> getValue)
            where TIn : class
        {
            UnityContainer.RegisterType(typeTo,()=>_environment.RequestStorage,getValue);
        }

        /// <summary>
        /// Регистрация типа
        /// с хранением уровня запрос без имени
        /// Этот метод нужно использовать если вы не предоставляете экземпляр в хранилище
        /// но хотите чтоб разрешенный экземпляр хранился в нем
        /// </summary>
        /// <typeparam name="T">тип регистрации и реализации</typeparam>
        /// <param name="getValue">Выражение динамического получения и установки экземпляра (r=>r.someProperty)</param>
        public void RegisterRequestType<T>(Expression<Func<TRequestStorage, T>> getValue)
            where T : class
        {
            RegisterRequestType(typeof(T),getValue);
        }
        

        /// <summary>
        /// Регистрация динамичского экземпляра
        /// уровня приложения
        /// </summary>
        /// <typeparam name="T">Тип экземпляра</typeparam>
        /// <param name="name">Наименование экземпляра</param>
        /// <param name="getValue">Функция динамического получения экземпляра (a=>a.someProperty)</param>

        protected void RegisterApplicationInstance<T>(string name, Func<TApplicationStorage, T> getValue)
            where T : class
        {
            RegisterInstance(name, () => getValue(_environment.ApplicationStorage));
        }

        #endregion protected

        #region Public

        #endregion

    }
}
