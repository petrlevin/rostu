using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Practices.Unity;
using NLog;
using Platform.Common.Exceptions;
using Platform.Unity.Common;
using Platform.Unity.Common.Interfaces;
using Platform.Utils;
using Platform.Utils.Extensions;

namespace Platform.Unity
{
    internal class DefaultRegistrator
    {
        private readonly IUnityContainer _unityContainer;

        public DefaultRegistrator(IUnityContainer unityContainer)
        {
            _unityContainer = unityContainer;
        }

        public void  RegisterAll(IEnumerable<Type> except= null)
        {
            

            except = except ?? new List<Type>();
            try
            {
                ProcessRegistrations(except);
                ProcessAuto();
            }
            catch (Exception ex)
            {
                
                throw new PlatformException("Ошибка при регистрации дефолтных значений в юнити контейнере", ex);
            }
        }

        private void ProcessAuto()
        {
            foreach (var baseType in Assemblies.AllTypes().WhitchHasAttribute<AutoRegistrationAttribute>())
            {
                foreach (var type in Assemblies.AllTypes().WhitchInherit(baseType, TypeOptions.Public | TypeOptions.NotAbstract | TypeOptions.IsClass))
                {
                    _unityContainer.RegisterType(baseType, type);
                }
            }

        }

        private void ProcessRegistrations(IEnumerable<Type> except)
        {
            Logger.Debug("Регистрация Dependency Injection");
            foreach (var assembly in Assemblies.All())
            {
                Logger.Debug(
                    "--------------------------------------------------------------------------------------");
                Logger.Debug("Регистрация Dependency Injection - {0}", assembly.FullName);
                Type[] types;
                if (!TryGetTypes(assembly, out types))
                    continue;

                Logger.Debug("Регистрация Dependency Injection - {0} .Типы успешно извлечены.", assembly.FullName);

                foreach (var source in types
                    .Where(t => typeof (IDefaultRegistration).IsAssignableFrom(t))
                    .Where(t => !except.Contains(t))
                    .Where(t => t.IsClass)
                    .Where(t => !t.IsNotPublic)
                    .Where(t => !t.IsAbstract))
                {
                    ((IDefaultRegistration) Activator.CreateInstance(source)).Register(_unityContainer);
                }
            }
        }


        private static bool TryGetTypes(Assembly assembly,out Type[] types)
        {
            try
            {
                types = assembly.GetTypes();
                return true;

            }
            catch (Exception)
            {
                types = null;
                return false;
            }
        }

        protected static Logger Logger = LogManager.GetCurrentClassLogger();
    }
}
