using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Platform.PrimaryEntities.Common.DbEnums;

namespace Tools.MigrationHelper.Core.DeleteTestData
{
    public class EntityFieldInfo
    {
        public int Id { get; set; }
        public String Name { get; set; }
        public int IdEntity { get; set; }
        public EntityFieldType EntityFieldType { get; set; }
    }
}
