using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Platform.Utils.Collections
{
	public class IgnoreCaseDictionary<TValue> : Dictionary<string, TValue>
	{
		public IgnoreCaseDictionary() : base(StringComparer.OrdinalIgnoreCase)
		{

		}

		public IgnoreCaseDictionary(Dictionary<string, TValue> dictionary): this(dictionary.AsEnumerable())
		{
		}

		public IgnoreCaseDictionary(IEnumerable<KeyValuePair<string, TValue>> keyValuePairs): base(StringComparer.OrdinalIgnoreCase)
		{
			foreach (KeyValuePair<string, TValue> keyValuePair in keyValuePairs)
			{
				Add(keyValuePair.Key, keyValuePair.Value);
			}
		}
	}
}
