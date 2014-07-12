using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Practices.Unity;
using NAnt.Core;
using NAnt.Core.Attributes;
using Platform.Common;
using Platform.Unity;
using Tools.MigrationHelper.Core.Tasks;


namespace Tools.MigrationHelper.Core
{
    [TaskName("registerindi")]
    public class RegisterInDI:MhTask
    {

        [TaskAttribute("connectionstring", Required = true)]
        public string ConnectionString
        {
            get;
            set;
        }


        protected override void ExecuteTask()
        {
            IUnityContainer unityContainer = new UnityContainer();

            Platform.PrimaryEntities.Factoring.DependencyInjection.RegisterIn(unityContainer, true, false, ConnectionString);
            IoCServices.InitWith(new DependencyResolverBase(unityContainer));

        }
    }
}
