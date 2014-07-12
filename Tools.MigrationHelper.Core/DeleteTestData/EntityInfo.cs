using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Platform.PrimaryEntities.Common.DbEnums;
using Platform.PrimaryEntities.DbEnums;

namespace Tools.MigrationHelper.Core.DeleteTestData
{
    public class EntityInfo :IMetadataInfo
    {
        public int Id { get; set; }
        public String Name { get; set; }
        public EntityType EntityType { get; set; }

        public string EntityName
        {
            get { return Name; }
        }

        public SolutionProject Project { get; set; }
    }
}
