using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Practices.Unity;
using Microsoft.Practices.Unity.InterceptionExtension;

namespace AutoClient
{
    public class Redirector
    {
        private RedirectInterceptionBehavior redirectInterceptor;
        
        public Redirector(string url)
        {
            ServerUrl = url;
        }

        private string _serverUrl;
        public string ServerUrl 
        {
            get { return _serverUrl; }
            set 
            { 
                _serverUrl = value;
                redirectInterceptor = new RedirectInterceptionBehavior(ServerUrl);
            }
        }

        public void DoRequest<T>(Action<T> action, bool waitResponse = true) where T: class, new()
        {
            T service = Intercept.NewInstance<T>(new VirtualMethodInterceptor(), new[] { redirectInterceptor });
            action(service);
        }
    }
}
