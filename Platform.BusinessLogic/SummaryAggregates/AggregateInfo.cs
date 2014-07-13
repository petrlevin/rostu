using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Platform.BusinessLogic.DbEnums;
using Platform.Dal.Common.Interfaces;

namespace Platform.BusinessLogic.SummaryAggregates
{
    public class AggregateInfo: IAggregateInfo
    {
        public string Field { get; set; }

        public AggregateFunction Function { get; set; }
    }
}
