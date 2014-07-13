using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json;
using Platform.Client.Converters;

namespace Platform.Client
{
	/// <summary>
	/// Класс обертка для реализации функций, используемых в компонентах.
	/// </summary>
	[JsonConverter(typeof(ExtFunctionConverter))]
	public class ExtFunction
	{
		public ExtFunction()
		{
			Params = new List<string>();
		}

		/// <summary>
		/// Текст функции
		/// </summary>
		public string FunctionText { get; set; }

		/// <summary>
		/// Список аргументов функции (имена переменных)
		/// </summary>
		public List<string> Params { get; set; }
	}
}
