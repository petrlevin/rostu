using System;

using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Linq;
using Platform.PrimaryEntities;
using Platform.PrimaryEntities.Common.DbEnums;
using Platform.PrimaryEntities.DbEnums;

namespace Tools.MigrationHelper.Core.DeleteTestData
{
    public abstract class Metadata<TResult> :XmlDataProcessing
    {

        [ThreadStatic] private static string _currentSource; 

        public Metadata(string sourcePath):base(sourcePath)
        {
            if (_currentSource != sourcePath)
                _list = null;
            _currentSource = sourcePath;

        }

        [ThreadStatic] static private List<TResult> _list;

        public List<TResult> Get()
        {
            if (_list != null)
                return _list;
            var result = new List<TResult>();
            var regexIsThisMetada = GetRegexIsThisMetadata();

            
            ForEach(regexIsThisMetada,f=>AddFromFile(f,result));

            _list = result;
            return result;
       }

        private void AddFromFile(string enumerateFile, List<TResult> result)
        {
            using (var stream = new StreamReader(enumerateFile))
            {
                XDocument doc = XDocument.Load(stream);
                var attr = GetMetadataInfo();
                foreach (
                    XElement descendant in
                        doc.Descendants(attr.GetXName())
                    )
                {
                    result.Add(CreateItem(descendant));
                }
            }


        }


        protected abstract TResult CreateItem(XElement descendant);

        protected virtual Regex GetRegexIsThisMetadata()
        {
            return GetMetadataInfo().GetRegex();
        }

        protected override IMetadataInfo GetMetadataInfo()
        {
            var attr = GetType().GetCustomAttributes(typeof (MetadataAttribute), false).FirstOrDefault() as MetadataAttribute;
            if (attr == null)
                throw new InvalidOperationException("Не опеределаны тип метаданных и имя . Используйте атрибут 'Metadata'");
            return attr;
        }
    }
}
