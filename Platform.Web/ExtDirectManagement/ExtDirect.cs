using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Configuration;
using BaseApp.Environment;
using ExtDirectHandler;
using NLog;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using Platform.BusinessLogic.AppServices;
using Platform.BusinessLogic.Auditing;
using Platform.BusinessLogic.Auditing.Auditors;
using Platform.Common.Exceptions;
using Platform.Common.Interfaces;
using Platform.Log;
using Platform.Utils;
using Platform.Web.Services;
using Platform.Web.UserManagement;

namespace Platform.Web.ExtDirectManagement
{
    public static class ExtDirect
    {
        /// <summary>
        /// Сервисы, обращение к которым разрешено до процедуры аутентификации
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        private static readonly List<Type> BeforeLoginServices = new List<Type>
            {
                typeof (ProfileService),
                typeof (EnumsService)
            };

        private static IEnumerable<Assembly> GetAppAssemblies()
        {
// ReSharper disable UseObjectOrCollectionInitializer
            var appAssemblies = new List<string>
// ReSharper restore UseObjectOrCollectionInitializer
                {
                    "Platform.Web",
                    "Platform.BusinessLogic",
                    "BaseApp",
                    "BaseApp.Service",
                    "Sbor",
                    "Sbor.Reports"
                };
            
#if DEBUG
            appAssemblies.Add("Tests");
#endif

            return Assemblies.All().Where(a => appAssemblies.Contains(a.GetName().Name));
        }

        static private IEnumerable<Type> GetAppServices()
        {
            var assemblies = GetAppAssemblies();

            foreach (var assembly in assemblies)
            {
                foreach (Type type in assembly.GetTypes())
                {
                    if (type.GetCustomAttributes(typeof(AppServiceAttribute), true).Length > 0)
                    {
                        yield return type;
                    }
                }
            }
        }

        static public void Initialize()
        {
            var metadata = new ExtDirectHandler.Configuration.ReflectionConfigurator()
                .RegisterType<ProfileService>()
                .RegisterType<ModelService>()
                .RegisterType<SystemService>()
                .RegisterType<FormService>()
                .RegisterType<EnumsService>()
                .RegisterType<FormulaService>();

#if DEBUG
            metadata.RegisterType<TestService>();
#endif

            var seriviceTypes = GetAppServices();
            metadata.RegisterTypes(seriviceTypes);

            DirectHttpHandler.SetMetadata(metadata);
            DirectHttpHandler.SetDirectHandlerInterceptor(interceptor);
        }

        private static void interceptor(Type type, MethodInfo method, DirectHandlerInvoker invoker)
        {
            if (!BeforeLoginServices.Contains(type))
            {
                if (Users.Current == null)
                    throw new NotLogedException();

                int userBandWidthInterval;
                int.TryParse(WebConfigurationManager.AppSettings["bandwidthInterval"], out userBandWidthInterval);
                if (userBandWidthInterval > 0)
                {
                    //проверяем скорость
                    var userBandWidth = Users.GetUserBandWidth(Users.Current.Id);
                    if (userBandWidth == null || (userBandWidth.Date - DateTime.Now).TotalDays > userBandWidthInterval)
                    {
                        throw new NotBandWidthException();
                    }
                }
                

                if (typeof(SysDimensionsService) != type)
                {
                    if (BaseAppEnvironment.Instance.SessionStorage.CurentDimensions.IsEmpty && !Users.SetCurrentDimensions())
                        throw new NotSysDimensionsException();
                    
                    if (!Users.Current.IsLicensed(BaseAppEnvironment.Instance.SessionStorage.CurentDimensions.PublicLegalFormation.Id))
                        throw new NotLicensedException();
                }
                
            }

            try
            {
                var methodName = type.Name + "." + method.Name;
                var requestData = GetRequestData();

                var audit = Audit<RequestAuditor>.Start(new RequestAuditor()
                    {
                        MethodName = methodName,
                        JsonData = requestData,
                        Disabled = BeforeLoginServices.Contains(type)
                    });
                
                OnBeforeInvoke(type, method);
                invoker.Invoke(jsonSerializer: GetSerializer());

                audit.Complete();
            }
            catch (TargetInvocationException ex)
            {
                var innerException = ex.InnerException;
                string path = null;
                if (!(innerException is IHandledException))
                    path = Log(innerException);

                if (innerException is PlatformException)
                {
                    if (innerException is IComplete)
                        ((IComplete)(innerException)).Complete();
                    if (!(innerException is IHandledException))
                        ((PlatformException)innerException).Url = GetUrl(path);
                    throw;

                }
                else
                {
                    var platformException = new PlatformException(innerException.Message, innerException);
                    if (!(innerException is IHandledException))
                        platformException.Url = GetUrl(path);
                    throw new TargetInvocationException(ex.Message, platformException);

                }
            }
        }

        private static string GetRequestData()
        {
            var request = HttpContext.Current.Request;
            request.InputStream.Position = 0;
            return (new StreamReader(request.InputStream)).ReadToEnd();
        }

        public static JsonSerializer GetSerializer()
        {
            var serializer = new JsonSerializer();
            serializer.Converters.Add(new IsoDateTimeConverter() { DateTimeFormat = "yyyy'-'MM'-'dd'T'HH':'mm':'ss" });
            serializer.ContractResolver = new CamelCasePropertyNamesContractResolver();
            return serializer;
        }

        private static void OnBeforeInvoke(Type type, MethodInfo method)
        {
            if (BeforeInvoke != null)
                BeforeInvoke(type, method);

        }

        private static string GetUrl(string physicalPath)
        {
            var server = HttpContext.Current.Server;
            string rootpath = server.MapPath("~/");
            physicalPath = physicalPath.Replace(rootpath, "");
            physicalPath = physicalPath.Replace("\\", "/");
            return physicalPath;
        }

        private static string Log(Exception exception)
        {
            _logger.ErrorException("",exception);
            return FilePerMessageTarget.LastPath();
        }

        public static event Action<Type, MethodInfo> BeforeInvoke; 

        private static Logger _logger = LogManager.GetCurrentClassLogger();

    }
}