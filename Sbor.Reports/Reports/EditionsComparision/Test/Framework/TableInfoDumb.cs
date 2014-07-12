using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Platform.BusinessLogic.EditionsComparision;
using Platform.PrimaryEntities.Common.Interfaces;
using Platform.PrimaryEntities.Reference;

namespace Sbor.Reports.Reports.EditionsComparision.Test.Framework
{
    public class TableInfoDumb: ITableInfo
    {
        public Entity TableEntity { get; set; }
        public IEnumerable<IEntityField> Fields { get; set; }
        public bool HasCaptionField { get; set; }
        public string CaptionFieldName { get; set; }
    }
}
