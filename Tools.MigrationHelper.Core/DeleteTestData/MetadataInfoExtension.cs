using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Linq;
using Platform.PrimaryEntities;

namespace Tools.MigrationHelper.Core.DeleteTestData
{
    public static class MetadataInfoExtension
    {
        static public Regex GetRegex(this IMetadataInfo metadataInfo)
        {
            var result =
                new Regex(@"DbStructure\\" + Schemas.ByEntityType(metadataInfo.EntityType) + @"\\" + metadataInfo.EntityName + @"\.xml$", RegexOptions.Compiled | RegexOptions.IgnoreCase);
            return result;
            
        }

        static public XName GetXName(this IMetadataInfo metadataInfo)
        {
            return
                XName.Get(String.Format("{0}.{1}", Schemas.ByEntityType(metadataInfo.EntityType),
                                        metadataInfo.EntityName));
        }
    }
}
