using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Platform.PrimaryEntities.Common.DbEnums;

namespace Tools.MigrationHelper.Core.DeleteTestData
{
    public class MetadataAttribute : Attribute, IMetadataInfo
    {
        private readonly EntityType _entityType;
        private readonly string _entityName;


        public MetadataAttribute(EntityType entityType, string entityName)
        {
            _entityType = entityType;
            _entityName = entityName;
        }

        public EntityType EntityType
        {
            get { return _entityType; }
        }

        public string EntityName
        {
            get { return _entityName; }
        }
    }
}
