using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Platform.PrimaryEntities.Common.DbEnums;

namespace Tools.MigrationHelper.Core.DeleteTestData
{
    public class MultilinkData:Data
    {
        private readonly List<EntityFieldInfo> _entityFields;
        public MultilinkData(string sourcePath, int entityId) : base(sourcePath, entityId)
        {
            _entityFields = new EntityFields(sourcePath).Get(entityId);
        }

        public void Delete(Func<Int32,Int32, bool> condition)
        {
            var left = _entityFields.First(ef => ef.EntityFieldType == EntityFieldType.Link).Name;
            var right = _entityFields.Last(ef => ef.EntityFieldType == EntityFieldType.Link).Name;
            Delete(n => condition((Int32)n.Descendants(XName.Get(left)).First(), (Int32)n.Descendants(XName.Get(right)).First()));
        }

    }
}
