using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Practices.Unity;
using Platform.Unity.Common.Interfaces;

namespace Platform.Unity
{
    public abstract class InListRegistration<TType> :IDefaultRegistration
    {
        public void Register(IUnityContainer unityContainer)
        {
            //if (unityContainer.Registrations.All(cr => cr.RegisteredType != typeof(List<TType>)))
            //    unityContainer.RegisterType<List<TType>>()


        }
    }
}
