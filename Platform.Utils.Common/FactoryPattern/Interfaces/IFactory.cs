using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace Platform.Utils.FactoryPattern.Interfaces
{
	public interface IFactory<K, T> where K : IComparable
	{
		T Create(K key);
	}
}
