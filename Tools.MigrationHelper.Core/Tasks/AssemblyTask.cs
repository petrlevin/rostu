using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using NAnt.Core;
using NAnt.Core.Attributes;
using Tools.MigrationHelper.Core.Helpers;

namespace Tools.MigrationHelper.Core.Tasks
{
    [TaskName("deployassembly")]
    public class AssemblyTask : DbDeployTask
    {
        private readonly string _pathPlatformDbScripts = AppDomain.CurrentDomain.BaseDirectory + @"\PlatformDb\Scripts";

        [TaskAttribute("task", Required = false)]
        public int? Task
        {
            get;
            set;
        }

        [TaskAttribute("action", Required = true)]
        public int Action
        {
            get;
            set;
        }

        protected override void ExecuteTask()
        {
            Log(Level.Verbose, "Обновление сборок (*.dll) в БД ...");
            try
            {
                var directoryInfo2 = new DirectoryInfo(_pathPlatformDbScripts);
                FileInfo fileInfo2 = directoryInfo2.GetFiles("Assemblies.list")[0];
                List<string> listAssemblies = fileInfo2.OpenText()
                                                                .ReadToEnd()
                                                                .Split(new[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries).ToList();

                var ad = new AssemblyDeploy(ConnectionString, listAssemblies, SourcePath, Task);

                switch (Action)
                {
                    case (int)ActionEnum.Create: ad.CreateAssemblies(); break;
                    case (int)ActionEnum.Update: ad.UpdateAssemblies(); break;
                    case (int)ActionEnum.Delete: ad.DropAssemblies(); break;
                    default: Fatal("Ошибка! Такое действие не предусмотрено");break;
                }

                if(ad.Message != null)
                    Log(Level.Verbose, ad.Message);
            }
            catch (Exception ex)
            {
                Fatal("Фатальная ошибка при публикации сборок (*.dll) в БД ", ex);
            }
			
        }


    }
}
