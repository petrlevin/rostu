using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Practices.Unity;

namespace Platform.Unity.Common.Interfaces
{
    /// <summary>
    /// Класс зарегистрированный через DI
    /// </summary>
    public interface IDefaultRegistration
    {
        /// <summary>
        /// Зарегистрировать класс в Unity
        /// </summary>
        void Register(IUnityContainer unityContainer);
    }
}
