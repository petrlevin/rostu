using System;
using Microsoft.Practices.Unity;
using Platform.Unity;

namespace Platform.ClientInteraction.Scopes
{
    /// <summary>
    /// Область видимости <see cref="CommunicationContext">контекста взаимодействия с пользователем</see>.
    /// </summary>
    public class CommunicationContextScope : ScopeBase
    {
        public CommunicationContextScope(CommunicationContext communicationContext) 
        {
            if (communicationContext == null) 
                throw new ArgumentNullException("communicationContext");
            UnityContainer.RegisterInstance(communicationContext);
        }
    }
}
