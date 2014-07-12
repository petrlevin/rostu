using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Linq;
using Platform.PrimaryEntities.Common.DbEnums;
using Platform.PrimaryEntities.DbEnums;

namespace Tools.MigrationHelper.Core.DeleteTestData
{
    [Metadata(EntityType.Reference, "DistributiveDataMultiLink")]
    public class DistributivaDatasMultilink :Metadata<DistributivaDataMultilinkInfo>
    {

        public DistributivaDatasMultilink(string sourcePath)
            : base(sourcePath)
        {

        }

        protected override DistributivaDataMultilinkInfo CreateItem(XElement descendant)
        {
            return new DistributivaDataMultilinkInfo()
            {
                IdEntity = (Int32)descendant.Descendants(XName.Get("idEntity")).First(),
                IdRight = (Int32)descendant.Descendants(XName.Get("idRight")).First(),
                IdLeft = (Int32)descendant.Descendants(XName.Get("idLeft")).First()
            };

        }
    }
}
