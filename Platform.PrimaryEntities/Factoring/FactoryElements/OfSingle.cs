using System;
using System.Collections.Generic;
using System.Data;
using Platform.PrimaryEntities.Exceptions;
using Platform.PrimaryEntities.Interfaces;

namespace Platform.PrimaryEntities.Factoring.FactoryElements
{
    public class OfSingle<T, TData> : BaseFactoryElement<T, T, TData>, IFactoryElement<T> 
        where T : Metadata 

    {
        public OfSingle(IFactoryStrategy<TData> factoryStrategy ) : base(factoryStrategy)
        {
        }




        protected override string IdName
        {
            get
            {
                return "id";
            }
        }

        public T CreateByName(string name)
        {
            T result = CreateBy(NameName, name);
            if (result == null)
                throw new ObjectNotFoundException(String.Format("Объект типа '{0}' не найден по имени({1}) {2} ", typeof(T), NameName, name), typeof(T), this, name);
            return result;
        }


        protected virtual string NameName { get { return "Name"; } }

        internal protected override T Load<TDataToLoad>(IList<TDataToLoad> rows)
        {

            if (rows.Count == 0)
                return null;
            T result = CreateObject();
            GetFiller<TDataToLoad>().Fill(result, rows[0]);

            return (T)result;

        }
    }



}
