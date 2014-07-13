using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Platform.PrimaryEntities.Reference;

namespace BaseApp.XmlExchange.Export
{
    interface IHasEntities
    {
        IQueryable<Entity> Entities { get; }
    }
}
