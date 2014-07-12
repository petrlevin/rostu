using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sbor.Reports.EditionsComparision
{
    public class MainReportItem: IReportItem
    {
        public string Id
        {
            get { return "root"; }
        }

        public IEnumerable<DSComparision> GetData(string id, string parentId)
        {
            return new List<DSComparision>()
                {
                    new DSComparision()
                        {
                            Id = id,
                            Parent = parentId,
                            AttributeName = this.Id
                        }
                };
        }
    }
}
