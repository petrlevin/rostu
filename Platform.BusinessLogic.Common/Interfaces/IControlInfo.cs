using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Platform.PrimaryEntities.Common.Interfaces;

namespace Platform.BusinessLogic.Common.Interfaces
{
    public interface IControlInfo
    {
        bool Enabled { get; }
        bool Skippable { get; }
        string Caption { get; }
        string UNK { get; }
        Int32? IdEntity { get; }
        bool HasDbEntry { get; }

    }
}
