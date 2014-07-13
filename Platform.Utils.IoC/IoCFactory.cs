using System;
using Platform.Utils.FactoryPattern.Interfaces;

namespace Platform.Utils.FactoryPattern
{
    public class IoCFactory<K, T> : FactoryBase<K, T>, IManagedFactory<K, T> where K : IComparable
    {
        public  void Add<V>(K key) where V :T, new()
        {
            AddByFactoryElement<IoCFactoryElement<V>>(key);
        }
    }
}
