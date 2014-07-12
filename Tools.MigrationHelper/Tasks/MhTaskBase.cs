using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Practices.Unity;
using Platform.Common;
using Platform.Unity;
using Tools.MigrationHelper.Tasks.Interfaces;
using Tools.MigrationHelper.Logger;

namespace Tools.MigrationHelper.Tasks
{
	class MhTaskBase : IMhAction
	{
		protected MhConfiguration _config;
		protected TextWriter _output;
		protected DbManager.MigrationHelper migrationHelper;
		protected const int Success = 0;

		#region Implementation of IMhAction

		public virtual int Process(MhConfiguration config, TextWriter output)
		{
			this._config = config;
			this._output = output;
			Report = new Report();
			return Success;
		}

		public Report Report { get; set; }

		#endregion

		public void CreateMigrationHelper()
		{
			//DirectoryInfo directoryInfo = new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory);
			if (string.IsNullOrWhiteSpace(_config.ConnectionString))
				throw new Exception("Не указана строка соединения");
			if (string.IsNullOrWhiteSpace(_config.SourcePath))
				throw new Exception("Не указан пусть к проекту");


			this.migrationHelper = new DbManager.MigrationHelper(_config.DevId)
			{
				ConnectionString = _config.ConnectionString,
				ProjectPathSource = _config.SourcePath,
				ProjectPathTarget = _config.TargetPath,
				IsDeveloper = _config.DevId >= 0,
				Verbose = _config.Verbose
			};
		}
	}
}
