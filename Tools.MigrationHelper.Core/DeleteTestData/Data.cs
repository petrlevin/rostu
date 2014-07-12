using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Tools.MigrationHelper.Helpers;

namespace Tools.MigrationHelper.Core.DeleteTestData
{
    public class Data :XmlDataProcessing
    {
        private readonly EntityInfo _entityInfo;

        public Data(string sourcePath,int entityId) :base(sourcePath)
        {
            _entityInfo = new Entities(sourcePath).Get().Single(ei => ei.Id == entityId);
        }

        public void Delete(Func<Int32, bool> condition)
        {
            Delete(n => condition((Int32) n.Descendants(XName.Get("id")).First()));
        }


        protected override IMetadataInfo GetMetadataInfo()
        {
            return _entityInfo;
        }
    }
}
