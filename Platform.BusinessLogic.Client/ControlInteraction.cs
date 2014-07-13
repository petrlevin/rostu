using Microsoft.Practices.Unity;
using Platform.BusinessLogic.Common.Exceptions;
using Platform.BusinessLogic.Common.Interfaces;
using Platform.Client;
using Platform.ClientInteraction;
using Platform.ClientInteraction.Actions;
using Platform.Unity.Common.Interfaces;

namespace Platform.BusinessLogic.Client
{
    public class ControlInteraction : IControlInteraction
    {
        private readonly CommunicationContext _context;

        public ControlInteraction([Dependency] CommunicationContext context)
        {
            _context = context;
        }

        public bool MaySkip(ControlResponseException ex)
        {

            var action = new ClientActionList(new YesSendQuestion(ex.Caption, string.Format("{0}{1}", ex.Message, "<br/>Продолжить?")));

            _context.GetAnswer(ex.Target.GetType().Name+ex.Action.Name ,action);

            return true;
        }

        public static void RegisterIn(IUnityContainer unityContainer)
        {
            unityContainer.RegisterType(typeof (IControlInteraction), typeof (ControlInteraction));
        }

        public class Registrator :IDefaultRegistration
        {
            public void Register(IUnityContainer unityContainer)
            {
                RegisterIn(unityContainer);
            }
        }
    }

    
}
