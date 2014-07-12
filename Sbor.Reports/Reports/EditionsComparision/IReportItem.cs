using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sbor.Reports.EditionsComparision
{
    public interface IReportItem
    {
        string Id { get; }
        IEnumerable<DSComparision> GetData(string id, string parentId);
    }
}
