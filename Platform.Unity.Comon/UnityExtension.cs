using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Microsoft.Practices.Unity;

namespace Platform.Unity.Common
{
    public static class UnityExtension
    {


        /// <summary>
        /// 
        /// </summary>
        /// <param name="unityContainer"></param>
        /// <param name="typeFrom">базовый класс или интерфейс который декорируется для него должна быть зарегистрировна недекорированная реализация. Регистрация недокорированной реализация не должна использовать InjectionMembers </param>
        /// <param name="typeTo">класс декоратора</param>
        /// <param name="registrationName">имя регистрации </param>
        /// <param name="lifetimeManager">менежджер который будет использован для декоратора , если не указан то будет использован менеджер декорируемого объекта (если до этого был зарегистирован другой декоратор с таким же именем и типом(typeFrom) то его менеджер ) </param>
        /// <exception cref="InvalidOperationException">Нет такой регистрации</exception>
        public static IUnityContainer RegisterDecorator(this IUnityContainer unityContainer, Type typeFrom, Type typeTo, string registrationName, LifetimeManager lifetimeManager = null)
        {
            if (!unityContainer.IsRegistered(typeFrom, registrationName))
                throw new InvalidOperationException(String.Format("Нет регистрации для типа '{0}' (typeFrom) c именем  '{1}' . Декоратор не может быть зарегистрирован ",typeFrom,registrationName));
            var registration = unityContainer.Registrations.First(r => r.Name == registrationName && r.RegisteredType == typeFrom);
            string replaceRegistrationName = string.Format("{0}{1}", registrationName,
                                                           registration.MappedToType.GetHashCode());

            var originalManager = registration.LifetimeManager;

            InjectionMember injectionMember = GetInjectionMember(unityContainer, typeFrom, registrationName);
            if (injectionMember != null)
                unityContainer.RegisterType(typeFrom, registration.MappedToType,
                                        replaceRegistrationName, CreateManager(originalManager, true), injectionMember);
            else
                unityContainer.RegisterType(typeFrom, registration.MappedToType,
                                        replaceRegistrationName, CreateManager(originalManager, true));

            if (null!=typeTo.GetConstructor( new Type[]{typeFrom}))
                injectionMember =
                    new InjectionConstructor(new ResolvedParameter(typeFrom, replaceRegistrationName));
            else if (typeTo.GetProperties(BindingFlags.Instance | BindingFlags.Public ).Any(pi=>(pi.PropertyType == typeFrom)&&(pi.CanWrite)))
                injectionMember = new InjectionProperty(typeTo.GetProperties(BindingFlags.Instance | BindingFlags.Public).First(pi => (pi.PropertyType == typeFrom) && (pi.CanWrite)).Name, new ResolvedParameter(typeFrom, replaceRegistrationName));
            else
                throw new InvalidOperationException(String.Format("Тип '{0}' не может быть декоратором для типа '{1}'. У него нет ни публичного конструктора параметром типа {1} ни публичного свойства с типом {1} с доступом записи  ",typeTo,typeFrom));

            unityContainer.RegisterType(typeFrom, typeTo, registrationName, lifetimeManager ?? CreateManager(originalManager, false),
                                        injectionMember);

            PutInjectionMember(unityContainer, typeFrom, registrationName, injectionMember);
            return unityContainer;



        }


        public static IUnityContainer RegisterDecorator<TFrom,TTo>(this IUnityContainer unityContainer,
                                             string registrationName, LifetimeManager lifetimeManager = null)
        {
            return RegisterDecorator(unityContainer,typeof(TFrom), typeof(TTo), registrationName, lifetimeManager);
        }

        static private Dictionary<IUnityContainer,Dictionary<Type,Dictionary<String,InjectionMember>>> _injectionMembers= new Dictionary<IUnityContainer, Dictionary<Type, Dictionary<string, InjectionMember>>>(); 
        static private InjectionMember GetInjectionMember(IUnityContainer unityContainer, Type typeFrom,
                                                           string registrationName)
        {
            if (!_injectionMembers.ContainsKey(unityContainer))
                return null;
            if (!_injectionMembers[unityContainer].ContainsKey(typeFrom))
                return null;
            if (!_injectionMembers[unityContainer][typeFrom].ContainsKey(registrationName))
                return null;
            return _injectionMembers[unityContainer][typeFrom][registrationName];

        }

        static private void PutInjectionMember(IUnityContainer unityContainer, Type typeFrom,
                                                           string registrationName, InjectionMember member)
        {
            if (!_injectionMembers.ContainsKey(unityContainer))
                _injectionMembers.Add(unityContainer, new Dictionary<Type, Dictionary<string, InjectionMember>>());
            if (!_injectionMembers[unityContainer].ContainsKey(typeFrom))
                _injectionMembers[unityContainer].Add(typeFrom, new Dictionary<string, InjectionMember>());

            _injectionMembers[unityContainer][typeFrom][registrationName] = member;


        }


            
        static private LifetimeManager CreateManager(LifetimeManager original ,bool setValue)
        {
            if (original == null)
                return null;
           var result =  (LifetimeManager)Activator.CreateInstance(original.GetType());
           if (setValue)
                result.SetValue(original.GetValue());
           return result;
        }





    }
}
