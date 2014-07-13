using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Platform.Utils.FactoryPattern.Interfaces;

namespace Platform.Utils.FactoryPattern
{
    public class Factory<K, T> : FactoryBase<K, T>, IManagedFactory<K,T> where K : IComparable
	{
        /// <summary>
        ///  Add a new creatable kind of object to the factory. Here is the key with the beauty of the constrains in generics. Look that we are saying that V should be derived of T and it must be creatable
        /// </summary>

		public void Add<V>(K key) where V : T, new()
		{
            AddByFactoryElement<FactoryElement<V>>(key);
		}



	}
}
