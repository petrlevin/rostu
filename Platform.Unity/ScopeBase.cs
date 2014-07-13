using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Practices.Unity;

namespace Platform.Unity
{
    public abstract class ScopeBase :IDisposable
    {
        protected IUnityContainer UnityContainer { get; private set; }
    

        public void Dispose()
        {
            if (UnityContainer!=null)
                UnityContainer.Dispose();
        }

        protected ScopeBase()
        {
            UnityContainer = IoCServices.CreateScope();

        }

        protected ScopeBase(Func<bool> condition)
        {
            if (condition())
                UnityContainer = IoCServices.CreateScope();
        }

    }
}
