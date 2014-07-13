
namespace Platform.Utils.FactoryPattern.Interfaces
{
    public interface IFactoryManager<K,T>
    {
        void Add<V>(K key) where V : T, new();
    }
}
