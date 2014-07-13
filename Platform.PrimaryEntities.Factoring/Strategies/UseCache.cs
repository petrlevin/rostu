using System;
using Microsoft.Practices.Unity;
using Platform.Caching.Common;
using Platform.PrimaryEntities.Interfaces;
using Platform.Utils.DecoratorPattern;

namespace Platform.PrimaryEntities.Factoring.Strategies
{
    public class UseCache<TData> : BaseDecorator<IFactoryStrategy<TData>>, IFactoryStrategy<TData>
    {

        [Dependency("Cache")]
        public ICache Cache { get; set; }





        public UseCache(IFactoryStrategy<TData> innerStrategy) :base(innerStrategy)
        {

        }



        public TResult GetResult<TResult>(ISelect<TData> @select, IBaseFactoryElement<TResult> factoryElement) where TResult : class
        {
            if (Cache == null)
                throw new InvalidOperationException("Не возмужно получить данные из кэша. Кэш не определен (null)");
            if (@select is SelectBase)
                return DoGetData((SelectBase)select, factoryElement);
            throw new InvalidOperationException(
                String.Format("Для работы кэша объект выборки данных должен быть производным от класса 'SelectBase' . Переданный объект имеет тип '{0}' ", select.GetType()));

        }


        public virtual ISelect<TData> CreateSelect(object parameter, string parameterName, string metadataName)
        {
            if (Inner == null)
                throw new InvalidOperationException("Не возможно создать объект выборки данных (Select). Внутренняя стратегиия получения данных не определена (null) ");
            return Inner.CreateSelect(parameter, parameterName, metadataName);
        }

        public IFactory GetFactory()
        {
            return Objects.GetFactory();
        }

        private TResult DoGetData<TResult>(SelectBase select, IBaseFactoryElement<TResult> factoryElement) where TResult : class
        {


            var result = GetFromCache<TResult>(select);
            if (result != null)
                return result;
            result = GetFromInner<TResult>(select, factoryElement);
            PutToCache(select, result);
            return result;



        }


        protected virtual void PutToCache<TResult>(SelectBase select, TResult result)
        {
            Cache.Put(result, select.MetadataName, select.ParameterName, select.Parameter);
        }



        protected TResult GetFromInner<TResult>(SelectBase select, IBaseFactoryElement<TResult> factoryElement) where TResult : class
        {
            if (Inner == null)
                throw new InvalidOperationException("Не возможно извлечь данные. Внутренняя стратегиия получения данных не определена (null) ");
            return Inner.GetResult(select as ISelect<TData>, factoryElement);
        }


        protected TResult GetFromCache<TResult>(SelectBase select) where TResult : class
        {
            return Cache.Get<TResult>(select.MetadataName, select.ParameterName, select.Parameter);
        }

    }
}
