using System;
using Platform.Application.Common;
using Platform.Utils;
using Platform.Utils.Extensions;

namespace Platform.Application
{
    /// <summary>
    /// 
    /// </summary>
    public static class Application
    {
        public static event Action EndRequest;
        public static event Action BeginRequest;

        public static void OnBeginRequest()
        {
            var handler = BeginRequest;
            if (handler != null) handler();
        }

        static Application()
        {
            
        }

        public static void OnEndRequest()
        {
            var handler = EndRequest;
            if (handler != null) handler();
        }

        public static void OnAfterStart()
        {
            foreach (Type type in Assemblies.AllTypes<IAfterAplicationStart>(TypeOptions.AutoInvokable))
            {
                ((IAfterAplicationStart)Activator.CreateInstance(type,false)).Execute();
            }
            
        }

        public static void OnBeforeStart()
        {
            foreach (Type type in Assemblies.AllTypes<IBeforeAplicationStart>(TypeOptions.AutoInvokable))
            {
                ((IBeforeAplicationStart)Activator.CreateInstance(type ,false)).Execute();
            }
        }
    }
}
