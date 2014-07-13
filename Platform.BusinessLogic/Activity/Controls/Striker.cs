using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Practices.Unity;
using Platform.BusinessLogic.Common.Enums;
using Platform.BusinessLogic.Common.Interfaces;
using Platform.BusinessLogic.Interfaces;
using Platform.PrimaryEntities.Common.Interfaces;
using Platform.Unity.Common.Interfaces;

namespace Platform.BusinessLogic.Activity.Controls
{
    public class Striker:IControlLauncher
    {
        public void ProcessControls(ControlType controlType, Sequence sequence, IBaseEntity entityValue, IBaseEntity oldEntityValue)
        {
            
        }


        public void InvokeControl(Action<IBaseEntity> control, IBaseEntity entity, MemberInfo memberInfo , IControlInfo initialControlInfo)
        {
            
        }


        public class DefaultRegistration : IDefaultRegistration
        {
            public void Register(IUnityContainer unityContainer)
            {
                unityContainer.RegisterType(typeof (IControlLauncher), typeof (Striker) ,ControlLauncher.NameForCUDControlRegistration);
            }
        }
    }
}
