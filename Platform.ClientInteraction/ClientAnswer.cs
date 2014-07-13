using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Platform.ClientInteraction
{
	public class ClientAnswer
	{
		public string ButtonId { get; set; }
		public Dictionary<string, object> FieldValues { get; set; }
	}
}
