using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Platform.PrimaryEntities.Common.DbEnums;

namespace Tools.MigrationHelper.Core.DeleteTestData
{
    [Metadata(EntityType.Reference, "EntityField")]
    public class EntityFields:Metadata<EntityFieldInfo>
    {
        public EntityFields(string sourcePath) : base(sourcePath)
        {
        }

        protected override EntityFieldInfo CreateItem(XElement descendant)
        {
            return new EntityFieldInfo()
            {
                Id = (Int32)descendant.Descendants(XName.Get("id")).First(),
                Name = (String)descendant.Descendants(XName.Get("Name")).First(),
                IdEntity = (Int32)descendant.Descendants(XName.Get("idEntity")).First(),
                EntityFieldType = (EntityFieldType)(Int32)descendant.Descendants(XName.Get("idEntityFieldType")).First(),
            };

        }

        public List<EntityFieldInfo> Get(int entityId)
        {
            return Get().Where(efi => efi.IdEntity == entityId).ToList();
        }

        public void DeleteByEntity(Func<Int32, bool> condition)
        {
            Delete(n => condition((Int32)n.Descendants(XName.Get("idEntity")).First()));
        }
        
    }
}
