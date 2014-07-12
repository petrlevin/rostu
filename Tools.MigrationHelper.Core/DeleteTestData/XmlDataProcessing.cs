using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Linq;
using Tools.MigrationHelper.Helpers;

namespace Tools.MigrationHelper.Core.DeleteTestData
{
    public abstract class XmlDataProcessing :XmlProcessing
    {


        public XmlDataProcessing(string sourcePath) :base(sourcePath)
        {

        }


        protected abstract IMetadataInfo GetMetadataInfo();


        protected void Delete(Func<XElement, bool> condition)
        {
            var regex = GetMetadataInfo().GetRegex();
            ForEach(regex, f => DeleteFromFile(f, condition));

        }



        public void DeleteWholeFile()
        {
            var regex = GetMetadataInfo().GetRegex();
            ForEach(regex, DeleteFile);
        }

        private void DeleteFile(string s)
        {
            if (File.Exists(s))
            {
                File.Delete(s);
                try
                {
                    SolutionHelper.DeleteFromProject(new FileInfo(s));
                }
                catch (FileNotFoundException)
                {
                }
                
            }
        }


        protected void DeleteFromFile(string file, Func<XElement, bool> condition)
        {
            XDocument doc;
            using (var stream = new StreamReader(file))
            {
                doc = XDocument.Load(stream);

            }
            var nodes = doc.Descendants(GetMetadataInfo().GetXName())
               .Where(n => condition(n));
            if (nodes.Any())
            {
                nodes.Remove();
                doc.Save(file);
            }
        }

    }
}
