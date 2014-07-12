using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tools.MigrationHelper.Core.Helpers
{
	/// <summary>
	/// Перечисление тасков
	/// Используется в MetaData для определения какой таск сейчас выполняется
	/// </summary>
	public enum TasksEnum
	{
		DeployAppDb,
		DeployPlatformDb,
		UpdateDb,
		CreateDistibutive,
        CreateUpdateSource,
        Update
	}
}
