using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using BaseApp.Environment;
using BaseApp.Environment.Interfaces;
using BaseApp.Environment.Storages;
using Microsoft.Practices.Unity;
using Platform.Common;

namespace BaseApp.Tests.Unity.Fixtures
{
	/// <summary>
	/// Mock-объект для тестирования <see cref="PlatformEnvironment">Среды</see>
	/// </summary>
	[ExcludeFromCodeCoverage]
	public class PlatformEnvironmentMock : EnvironmentBase
	{
		#region Private Fields
		private int currentSessionNum;
		private int currentRequestNum;
		#endregion

		public Dictionary<int, SessionStorage> Sessions { get; set; }
		public Dictionary<int, Dictionary<int, RequestStorage>> Requests { get; set; }
		
		public void RequestStart(int sessionNum, RequestStorage reqStor)
		{
			this.currentSessionNum = sessionNum;
			this.RequestStart(reqStor);
		}

		/// <summary>
		/// Переключить текущее состояние (currentSessionNum и currentRequestNum) на последний запрос последней сессии.
		/// Внимание: Это не обязательно будет последний созданный запрос во времени.
		/// </summary>
		/// <returns></returns>
		public PlatformEnvironmentMock SwitchToLastRequest()
		{
			currentSessionNum = Sessions.Keys.Max();
			currentRequestNum = Requests[currentSessionNum].Keys.Max();
			return this;
		}

		/// <summary>
		/// Устанавливает состояние объекта среды на конкретную сессию и запрос.
		/// </summary>
		/// <param name="sessionNum">Номер сессии</param>
		/// <param name="requestNum">Номер запроса внутри сессии</param>
		/// <returns>Объект среды</returns>
		public PlatformEnvironmentMock SetState(int sessionNum, int requestNum)
		{
			currentSessionNum = sessionNum;
			currentRequestNum = requestNum;
			return this;			
		}

		#region Overrides

		public override SessionStorage SessionStorage
		{
			get { return Sessions[currentSessionNum]; }
			protected set
			{
				currentSessionNum = (Sessions.Keys.Max(a => (int?)a) ?? 0) + 1;
				this.Sessions[currentSessionNum] = value;
				this.Requests.Add(currentSessionNum, new Dictionary<int, RequestStorage>());
			}
		}

		public override RequestStorage RequestStorage
		{
			get { return Requests[currentSessionNum][currentRequestNum]; }
			protected set
			{
				currentRequestNum = (Requests[currentSessionNum].Keys.Max(a => (int?)a) ?? 0) + 1;
				Requests[currentSessionNum][currentRequestNum] = value;
			}
		}

		public override IEnvironment ApplicationStart(ApplicationStorage appStor)
		{
			Sessions = new Dictionary<int, SessionStorage>();
			Requests = new Dictionary<int, Dictionary<int, RequestStorage>>();
			currentRequestNum = 0;
			currentSessionNum = 0;
			return base.ApplicationStart(appStor);
		}

		#endregion

		#region Implementation of IUnityContainerProvider

		public override IUnityContainer Container
		{
			get { return this.RequestStorage.Container; }
		}

		#endregion
	}
}
