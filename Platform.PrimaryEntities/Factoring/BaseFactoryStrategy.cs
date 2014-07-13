using System;
using Platform.PrimaryEntities.Interfaces;

namespace Platform.PrimaryEntities.Factoring
{
    public abstract class BaseFactoryStrategy<TData> :IFactoryStrategy<TData>
    {
        public virtual TResult GetResult<TResult>(ISelect<TData> @select, IBaseFactoryElement<TResult> factoryElement ) where TResult :class
        {
            if (factoryElement == null) throw new ArgumentNullException("factoryElement");
            var fact = factoryElement as BaseFactoryElement<TResult, TData>;
            if (fact == null)
                throw new InvalidOperationException(
                    String.Format("Фабрика  должна быть производной от 'BaseFactory'. Тип фабрики - '{0}'",
                                  factoryElement.GetType()));
            ;

            return fact.GetResult(@select);
            
            
        }

        public abstract ISelect<TData> CreateSelect(object parameter, string parameterName, string metadataName);
        public abstract IFactory GetFactory();
    }
}
