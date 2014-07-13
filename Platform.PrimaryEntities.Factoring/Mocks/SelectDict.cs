using System.Collections.Generic;
using Platform.PrimaryEntities.Interfaces;

namespace Platform.PrimaryEntities.Factoring.Mocks
{
    public class SelectDict : SelectBase, ISelect<Dictionary<string, object>>
    {
        public IEnumerable<Dictionary<string, object>> Execute()
        {

            if (!Storage.Dictionary.ContainsKey(this.MetadataName))
                return new List<Dictionary<string, object>>();
            IEnumerable<Dictionary<string, object>> result = new List<Dictionary<string, object>>();
            return result;
            //var objects = Storage.Dictionary[this.MetadataName];
            //foreach (Dictionary<string, object> dictionary in objects)
            //{
            //    if ()
            //}

        }
    }
}
