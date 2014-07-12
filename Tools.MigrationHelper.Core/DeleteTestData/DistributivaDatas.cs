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
    [Metadata(EntityType.Reference, "DistributiveData")]
    public class DistributivaDatas :Metadata<DistributivaDataInfo>
    {

        public DistributivaDatas(string sourcePath) :base(sourcePath)
        {

        }

        protected override DistributivaDataInfo CreateItem(XElement descendant)
        {
            return new DistributivaDataInfo()
            {
                IdElement = (Int32)descendant.Descendants(XName.Get("idElement")).First(),
                IdElementEntity = (Int32)descendant.Descendants(XName.Get("idElementEntity")).First()
            };

        }
    }
}
