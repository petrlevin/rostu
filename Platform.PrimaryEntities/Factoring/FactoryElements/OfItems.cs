using System.Collections.Generic;
using Platform.PrimaryEntities.Interfaces;

namespace Platform.PrimaryEntities.Factoring.FactoryElements
{
    public class OfItems<T,TData> :BaseFactoryElement<IList<T>, T ,TData>
        where T:Metadata 
    {
        private readonly string _idName;

        public OfItems(IFactoryStrategy<TData> factoryStrategy, string idName)
            : base(factoryStrategy)
        {
            _idName = idName;
        }



        protected override string IdName
        {
            get { return _idName; }
        }

        internal protected override IList<T> Load<TDataFromLoad>(IList<TDataFromLoad> rows)
        {
            var result = new List<T>();
            foreach (TDataFromLoad row in rows)
            {
                var field = (T)CreateObject();
                GetFiller<TDataFromLoad>().Fill(field, row);
                result.Add(field);
            }
            return result;

        }
    }
}
