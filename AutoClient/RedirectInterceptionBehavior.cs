using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Practices.Unity.InterceptionExtension;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using Platform.Web.ExtDirectManagement;

namespace AutoClient
{
    public class RedirectInterceptionBehavior: IInterceptionBehavior
    {
        private string ServerUrl;

        private CookieContainer cookieContainer;

        public RedirectInterceptionBehavior(string serverUrl)
        {
            ServerUrl = serverUrl;
            cookieContainer = new CookieContainer();
        }

        public IMethodReturn Invoke(IMethodInvocation input, GetNextInterceptionBehaviorDelegate getNext)
        {
            doRequest(input);
            MethodInfo mi = (MethodInfo) input.MethodBase;
            return input.CreateMethodReturn(GetDefault(mi.ReturnType));
        }

        public IEnumerable<Type> GetRequiredInterfaces()
        {
            return Type.EmptyTypes;
        }

        public bool WillExecute
        {
            get { return true; }
        }

        private void doRequest(IMethodInvocation input)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(ServerUrl + "/rpc");
            request.CookieContainer = cookieContainer;
            request.ContentType = "application/json";
            request.Method = "POST";
            request.Timeout = 1000*60*20;

            string json = toJson(getRequestPayload(input));
            byte[] bytes = toBytes(json);
            request.ContentLength = bytes.Length;
            
            Stream newStream = request.GetRequestStream();
            newStream.Write(bytes, 0, bytes.Length);

            HttpWebResponse response;
            try
            {
                response = (HttpWebResponse)request.GetResponse();
            }
            catch (WebException ex)
            {
                if (ex.Status != WebExceptionStatus.Timeout)
                    throw;
                Debug.WriteLine("Timeout exception: " + json);
            }
            
        }


        private object getRequestPayload(IMethodInvocation input)
        {
            return new
                {
                    action = input.MethodBase.DeclaringType.Name,
                    method = toLowerFirst(input.MethodBase.Name),
                    data = input.Inputs,
                    tid = 1,
                    type = "rpc"
                };
        }

        private JsonSerializer getSerializer()
        {
            var serializer = new JsonSerializer();
            serializer.Converters.Add(new IsoDateTimeConverter() { DateTimeFormat = "yyyy'-'MM'-'dd'T'HH':'mm':'ss" });
            // serializer.ContractResolver = new CamelCasePropertyNamesContractResolver(); с данной настройкой первая буква в ключах словаря
            // приводится к нижнему регистру, чего однако не происходит при работе с системой через интерфейс.
            // Проверено на методе ProfileService.SetSysDimensions.
            return serializer;
        }

        #region Private Tools

        private string toJson(object data)
        {
            var sb = new StringBuilder();
            JsonSerializer serializer = getSerializer(); // правильнее было бы использовать ExtDirect.GetSerializer();, но ... см. комментарий в getSerializer
            serializer.Serialize(new JsonTextWriter(new StringWriter(sb)), data);
            return sb.ToString();
        }

        private byte[] toBytes(string str)
        {
            ASCIIEncoding encoding = new ASCIIEncoding();
            return encoding.GetBytes(str);
        }

        private string toLowerFirst(string str)
        {
            return Char.ToLowerInvariant(str[0]) + str.Substring(1);
        }

        public static object GetDefault(Type type)
        {
            if (type.IsValueType && type != typeof(void))
            {
                return Activator.CreateInstance(type);
            }
            return null;
        }

        #endregion

    }
}
