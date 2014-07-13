using Platform.Common;
using Platform.Utils.FactoryPattern.Interfaces;

namespace Platform.Utils.FactoryPattern
{



    public class IoCFactoryElement<T> : IFactoryElement
    {
        public object New()
        {
            return IoC.Resolve<T>();
        }

        static public T Create()
        {
            return (T)new IoCFactoryElement<T>().New();

        }

    }


    
}
