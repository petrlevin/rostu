using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Platform.PrimaryEntities.Common.Interfaces;
using Platform.PrimaryEntities.Exceptions;
using Platform.PrimaryEntities.Interfaces;
//using Platform.Common;

namespace Platform.PrimaryEntities.Factoring
{

    public abstract class BaseFactoryElement<TResult, TData>
    {
        private IFactoryStrategy<TData> _factoryStrategy;

        protected IFactoryStrategy<TData> FactoryStrategy
        {
            get { return _factoryStrategy; }
        }

        public BaseFactoryElement(IFactoryStrategy<TData> factoryStrategy)
        {
            if (factoryStrategy == null) throw new ArgumentNullException("factoryStrategy");
            _factoryStrategy = factoryStrategy;
        }

        internal TResult GetResult(ISelect<TData> select)
        {
            IEnumerable<TData> datas = select.Execute();
            return Load(datas.ToList());
        }

        internal protected abstract TResult Load<TDataFromLoad>(IList<TDataFromLoad> dataTable);

        protected virtual IFiller<TDataFromFill> GetFiller<TDataFromFill>()
        {
            if (typeof (DataRow).IsAssignableFrom(typeof (TDataFromFill)))
                return (IFiller<TDataFromFill>) new Filler();
            if (typeof(IDictionary<string,object>).IsAssignableFrom(typeof(TDataFromFill)))
                return (IFiller<TDataFromFill>)new DictFiller();
            throw new InvalidOperationException(String.Format("Объект не может заполнен переданными данными. Тип данных '{0}'  не потдерживается " ,typeof(TDataFromFill).Name));

        }

    }

    public abstract class BaseFactoryElement<TResult, TObject, TData> : BaseFactoryElement<TResult, TData>, IBaseFactoryElement<TResult>
        where TObject : Metadata
        where TResult : class
    {



        



        public BaseFactoryElement(IFactoryStrategy<TData> factoryStrategy)
            : base(factoryStrategy)
        {


        }




        public TObject CreateObject()
        {
            try
            {
                return DoCreateObject();
            }
            catch (Exception ex)
            {
                throw new ObjectCreationException(
                    String.Format("Ошибка создания объекта типа {0}  фабрикой {1} ", typeof(TObject), GetType()), ex,
                    typeof(TObject), this);
            }

        }

        private TObject DoCreateObject()
        {
            IFactory factory = FactoryStrategy.GetFactory();
            TObject result;
            try
            {
                //result = (TObject)Activator.CreateInstance(typeof(TObject), factory);
                result = (TObject)Activator.CreateInstance(typeof(TObject));
            }
            catch (MissingMethodException)
            {

                result = (TObject)Activator.CreateInstance(typeof(TObject), true);
                //result.Objects = factory;

            }
            return result;
        }


        protected TResult CreateBy(string parameterName, object parameter)
        {
            try
            {
                return DoCreateBy(parameterName, parameter);
            }
            catch (Exception ex)
            {

                throw new DbFactoryException(
                    "Ошибка создания объекта типа {0} по параметру '{1}'.  Запрашиваемое значение параметра: {2} ", ex,
                    this, typeof(TResult), parameterName, parameter);
            }


        }

        private TResult DoCreateBy(string parameterName, object parameter)
        {
            ISelect<TData> select = (this is ISelectProvider<TData>)
                                        ? (this as ISelectProvider<TData>).GetSelect(parameterName, parameter)
                                        : FactoryStrategy.CreateSelect(parameter, parameterName, MetadataName);
            return CreateBy(@select);
        }


        private TResult CreateBy(ISelect<TData> select)
        {
            return FactoryStrategy.GetResult(select, this);
        }



        public TResult CreateById(int id)
        {
            TResult result = CreateBy(IdName, id);
            if (result == null)
                throw new ObjectNotFoundException(String.Format("Объект типа '{0}' не найден по идентификатору ({1}) {2} ", typeof(TResult), IdName, id), typeof(TResult), this, id);
            return result;

        }




        protected virtual string MetadataName
        {
            get { return typeof(TObject).Name; }

        }


        protected abstract string IdName { get; }




    }



}