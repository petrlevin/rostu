using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Platform.Utils.FactoryPattern.Interfaces;

namespace Platform.Utils.FactoryPattern
{
	public class FactoryElement<T> : IFactoryElement where T : new()
	{
		public object New()
		{
			return new T();
		}
	}
}
