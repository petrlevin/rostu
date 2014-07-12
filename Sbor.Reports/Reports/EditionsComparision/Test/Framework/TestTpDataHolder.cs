using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Platform.BusinessLogic.EditionsComparision;
using Platform.PrimaryEntities.Common.Interfaces;
using Platform.PrimaryEntities.Reference;

namespace Sbor.Reports.Reports.EditionsComparision.Test
{
    public class TestTpDataHolder: TpDataHolder 
    {
        public TestTpDataHolder(TablepartInfo tpInfo): base(tpInfo)
        {
        }

        public void SetTables(DataTable tA, DataTable tB)
        {
            tableA = tA;
            tableB = tB;
        }
    }
}
