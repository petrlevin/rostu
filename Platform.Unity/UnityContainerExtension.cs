using System;
using System.Linq.Expressions;
using Microsoft.Practices.Unity;

namespace Platform.Unity
{
    static public class UnityContainerExtension
    {

        /// <summary>
        /// Регистрация инстанса для резолвинга . Инстанс не хранится в контейнере но получается динамически вызовом <cref>getValueHandler</cref>
        /// </summary>
        /// <typeparam name="TInterface"> Тип инстанса</typeparam>
        /// <param name="container"></param>
        /// <param name="name">наименование инстанса</param>
        /// <param name="getValueHandler">динамический хэндлер </param>
        /// <returns></returns>
        public static IUnityContainer RegisterInstance<TInterface>(this IUnityContainer container, string name,
                                                                    Func<Object> getValueHandler)
            where TInterface:class
        {
            //return container.RegisterInstance(typeof(TInterface), name, MockRepository.GenerateMock<TInterface>(), new ExternalStorageLifetimeManager(getValueHandler));
            return container.RegisterType(typeof(TInterface), name,  new ExternalStorageLifetimeManager(getValueHandler));
        }


        /// <summary>
        /// Регистрация инстанса для резолвинга . Инстанс не хранится в контейнере но получается динамически вызовом <cref>getValueHandler</cref>
        /// </summary>
        /// <typeparam name="TInterface"> Тип инстанса</typeparam>
        /// <param name="container"></param>

        /// <param name="getValueHandler">динамический хэндлер </param>
        /// <returns></returns>
        public static IUnityContainer RegisterInstance<TInterface>(this IUnityContainer container, 
                                                                    Func<Object> getValueHandler)
            where TInterface : class
        {
            //return container.RegisterInstance(typeof(TInterface), name, MockRepository.GenerateMock<TInterface>(), new ExternalStorageLifetimeManager(getValueHandler));
            return container.RegisterType(typeof(TInterface), new ExternalStorageLifetimeManager(getValueHandler));
        }


        /// <summary>
        /// Регистрация типа для резолвинга . Инстанс не хранится в контейнере но получается динамически вызовом <cref>getValueHandler</cref>
        /// </summary>
        /// <typeparam name="TInterface"> Тип инстанса</typeparam>
        /// <typeparam name="TStorage"></typeparam>
        /// <param name="container"></param>
        /// <param name="valueLamda"></param>
        /// <param name="typeTo"></param>
        /// <returns></returns>
        public static IUnityContainer RegisterType<TInterface, TStorage>(this IUnityContainer container, Type typeTo, Func<TStorage> getStorage,
                                                                    Expression<Func<TStorage, TInterface>> valueLamda)
            where TInterface : class
        {

            return container.RegisterType(typeof(TInterface), typeTo, new LifiTimeManagerFactory<TStorage>().CreateManager(getStorage,valueLamda));
            
        }



    }
}

