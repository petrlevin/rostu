using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Practices.Unity;
using NDesk.Options;
using Platform.Common;
using Platform.Unity;
using Tools.MigrationHelper.Tasks;
using Tools.MigrationHelper.Tasks.Interfaces;
using Tools.MigrationHelper.DbManager;
using Tools.MigrationHelper.Logger;

namespace Tools.MigrationHelper
{
	/// <summary>
	/// 
	/// </summary>
	public class ConsoleApp
	{
		private List<string> extra = new List<string>();

		public MhConfiguration Config { get; set; }
		
		public ConsoleApp()
		{
			Config = new MhConfiguration();
		}

		public ConsoleApp(string[] args)
		{
			Config = new MhConfiguration();
			var p = new OptionSet() {
				{ "devId=",   v => Config.DevId = int.Parse(v) },
				{ "action=",  v => Config.Action = v },
				{ "h|?|help", v => Config.Action = "help" },
				{ "conn|connectionString=", v => Config.ConnectionString = v },
				{ "src|sourcePath=",		v => Config.SourcePath = v },
				{ "target|targetPath=",		v => Config.TargetPath = v },
				{ "v|verbose",				v => Config.Verbose = true },
				{ "withClasses", v=> Config.GenerateCode = true},
				{ "out|outputDir=",		v => Config.OutputDir = v }
			};
			extra = p.Parse(args);
		}

		public int Run()
		{
			RegisterInDI();

			if (extra.Any())
				Console.WriteLine("Были переданы неизвестные параметры");

			Type actionType = Type.GetType(typeof(MhTaskBase).Namespace + "." + Config.Action, false, true);
			if (actionType == null)
			{
				throw new Exception(string.Format("Указано недопустимое действие: {0}", Config.Action));
			}
			IMhAction action = (IMhAction) Activator.CreateInstance(actionType);
			
			int result = action.Process(Config, Console.Out);
			printReport(action);
			return result;
		}

		private void printReport(IMhAction action)
		{
			if (action.Report.Messages.Any(m => m.Type == MessageType.Error))
			{
				Console.Error.WriteLine("Работа завершилась с ошибками!");
			}
			action.Report.WriteTo(Console.Out);
		}

        private void RegisterInDI()
        {
            IUnityContainer unityContainer = new UnityContainer();

            Platform.PrimaryEntities.Factoring.DependencyInjection.RegisterIn(unityContainer, true, false, Config.ConnectionString);
            IoC.InitWith(new DependencyResolverBase(unityContainer));
        }
	}
}
