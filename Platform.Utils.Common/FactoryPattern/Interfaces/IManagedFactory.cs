using System;

namespace Platform.Utils.FactoryPattern.Interfaces
{
    public interface IManagedFactory<K,T>: IFactory<K,T> ,IFactoryManager<K,T> where K : IComparable
    {
    }
}
