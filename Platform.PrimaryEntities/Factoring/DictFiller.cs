using System.Collections.Generic;
using Platform.PrimaryEntities.Common.Interfaces;
using Platform.PrimaryEntities.Interfaces;

namespace Platform.PrimaryEntities.Factoring
{
    public class DictFiller : IFiller<IDictionary<string, object>>
    {
        public void Fill(IBaseEntity objectToFill, IDictionary<string, object> data)
        {
            objectToFill.FromDictionary(data);
        }

    }

    //public class DictFiller<TObject> : DictFiller, IFiller<TObject, IDictionary<string, object>>
    //    where TObject : IBaseEntity
    //{
    //    public void Fill(TObject objectToFill, IDictionary<string, object> data)
    //    {
    //        Fill((IBaseEntity) objectToFill, data);
    //    }
    //}
}

