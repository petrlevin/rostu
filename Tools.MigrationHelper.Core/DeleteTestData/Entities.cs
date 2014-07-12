using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using Platform.PrimaryEntities.Common.DbEnums;
using Platform.PrimaryEntities.DbEnums;

namespace Tools.MigrationHelper.Core.DeleteTestData
{
    [Metadata(EntityType.Reference, "Entity")]
    public class Entities :Metadata<EntityInfo>
    {


        public Entities(string sourcePath)
            : base(sourcePath)
        {

        }


        protected override EntityInfo CreateItem(XElement descendant)
        {
            return new EntityInfo()
                       {
                           Id = (Int32) descendant.Descendants(XName.Get("id")).First(),
                           Name = (String) descendant.Descendants(XName.Get("Name")).First(),
                           EntityType = (EntityType) (Int32) descendant.Descendants(XName.Get("idEntityType")).First(),
                           Project = (SolutionProject) (Int32) descendant.Descendants(XName.Get("idProject")).First()
                       };
        }
    }
}
