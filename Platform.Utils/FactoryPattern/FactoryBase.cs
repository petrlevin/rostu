using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Platform.Utils.FactoryPattern.Interfaces;

namespace Platform.Utils.FactoryPattern
{
	public class FactoryBase<K, T> : IFactory<K, T> where K : IComparable
	{
		/// Elements that can be created
		Dictionary<K, IFactoryElement> elements = new Dictionary<K, IFactoryElement>();

        protected void AddByFactoryElement<TFactoryElement>(K key) where TFactoryElement : IFactoryElement, new()

		{
            elements.Add(key, new TFactoryElement());
		}

		public T Create(K key)
		{
			if (elements.ContainsKey(key))
			{
				return (T)elements[key].New();
			}
			throw new ArgumentException();
		}
	}
}
