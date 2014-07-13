using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Platform.BusinessLogic.DbEnums;

namespace Platform.BusinessLogic.Interfaces
{
    public interface IExportSettings
    {
        SelectionType SelectionType { get; }
        IEnumerable<ISelectItem> Entities { get; }
        string EntitiesSql { get; }
        TargetType TargetType { get; }

    }

    public enum TargetType
    {
        Source =1,
        Links =2
    }
}
