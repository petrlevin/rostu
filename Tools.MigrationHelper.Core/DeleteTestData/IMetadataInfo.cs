using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Platform.PrimaryEntities.Common.DbEnums;

namespace Tools.MigrationHelper.Core.DeleteTestData
{
    public interface IMetadataInfo
    {
        EntityType EntityType { get; }

        string EntityName { get; }
        
    }
}
