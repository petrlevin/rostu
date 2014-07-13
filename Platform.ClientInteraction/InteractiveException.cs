using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using Platform.Common.Exceptions;
using Platform.Common.Interfaces;

namespace Platform.ClientInteraction
{
	/// <summary>
	/// Интерактивное исключение.
    /// http://conf.rostu-comp.ru/pages/viewpage.action?pageId=13599286
	/// </summary>
    [JsonObject(MemberSerialization.OptOut)]
    public class InteractiveException : PlatformException, IHandledException
	{
		public InteractiveException(string interactionId, ClientActionList clientActions)
		{
			ClientActions = clientActions;
			InteractionId = interactionId;
		}

		public string InteractionId { get; set; }

		public bool IsInteractive
		{
			get { return true; }
		}

		public ClientActionList ClientActions { get; set; } 

		public override string ToString()
		{
			var formatting = Formatting.Indented;
			return JsonConvert.SerializeObject(this, formatting, getSerializerSettings());
		}

		private JsonSerializerSettings getSerializerSettings()
		{
			JsonSerializerSettings settings;
			settings = new JsonSerializerSettings();
			settings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
			settings.NullValueHandling = NullValueHandling.Ignore;
			return settings;
		}
	}
}
