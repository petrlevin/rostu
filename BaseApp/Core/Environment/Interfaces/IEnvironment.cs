using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BaseApp.Environment.Storages;
using Microsoft.Practices.Unity;
using Platform.Common;
using Platform.Common.Interfaces;

namespace BaseApp.Environment.Interfaces
{
	public interface IEnvironment : IUnityContainerProvider, IDependencyResolver
	{
		ApplicationStorage ApplicationStorage { get; }
		SessionStorage SessionStorage { get; }
		RequestStorage RequestStorage { get; }

		/// <summary>
		/// Действия, выполняемые при старте веб-приложения
		/// </summary>
		/// <param name="appStor">Объект хранилища уровня приложения</param>
		/// <returns>Объект среды (для возможности fluent-синтаксиса)</returns>
		IEnvironment ApplicationStart(ApplicationStorage appStor);
		IEnvironment SessionStart(SessionStorage sessionStor);
		IEnvironment RequestStart(RequestStorage reqStor);
	}
}
