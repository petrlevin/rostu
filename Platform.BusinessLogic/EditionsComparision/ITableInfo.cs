using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Platform.PrimaryEntities.Common.Interfaces;
using Platform.PrimaryEntities.Reference;

namespace Platform.BusinessLogic.EditionsComparision
{
    public interface ITableInfo
    {
        Entity TableEntity { get; set; }
        IEnumerable<IEntityField> Fields { get; }
        bool HasCaptionField { get; }
        string CaptionFieldName { get; set; }
    }
}
