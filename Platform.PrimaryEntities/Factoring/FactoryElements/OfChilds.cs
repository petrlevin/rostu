using System;
using System.Data;
using Platform.PrimaryEntities.Interfaces;

namespace Platform.PrimaryEntities.Factoring.FactoryElements
{
    public class  OfChilds<TObject,TParent,TData> :OfItems<TObject,TData> 
        where TObject : Metadata 
    {
        public OfChilds(IFactoryStrategy<TData> factoryStrategy)
            : base(factoryStrategy, GetIdName())
        {
        }


        private static  string GetIdName()
        {

            return String.Format("id{0}", typeof (TParent).Name);
        }

    }

    public class OfChilds<TObject, TParent> : OfChilds<TObject, TParent, DataRow> where TObject : Metadata
    {
        public OfChilds(IFactoryStrategy<DataRow> factoryStrategy) : base(factoryStrategy)
        {
        }
    }
}
