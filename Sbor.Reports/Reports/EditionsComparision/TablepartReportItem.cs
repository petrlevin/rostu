using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Platform.BusinessLogic.EditionsComparision;

namespace Sbor.Reports.EditionsComparision
{
    public class TablepartReportItem: IReportItem
    {
        private TablepartInfo tpInfo;

        public TablepartReportItem(TablepartInfo tpInfo)
        {
            this.tpInfo = tpInfo;
        }

        public string Id
        {
            get { return tpInfo.Field.Name; }
        }

        public IEnumerable<DSComparision> GetData(string id, string parentId)
        {
            return new List<DSComparision>()
                {
                    new DSComparision()
                        {
                            Id = id,
                            Parent = parentId,
                            AttributeName = tpInfo.Field.Caption
                        }
                };
        }
    }
}
