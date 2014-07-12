using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Tools.MigrationHelper.Core.DeleteTestData
{
    public class XmlProcessing
    {
        private readonly string _sourcePath;

        public XmlProcessing(string sourcePath)
        {
            _sourcePath = sourcePath;
        }

        public void ForEach(string regex, Action<String,Match> action)
        {
            ForEach(new Regex(regex,RegexOptions.IgnoreCase), action);
        }

        public void ForEach(string regex, Action<String> action)
        {
            ForEach(regex, (s, m) => action(s));
        }


        public void ForEach(Regex regex, Action<String> action)
        {
            ForEach(regex, (s, m) => action(s));
        }

        public void ForEach(Regex regex, Action<String,Match> action)
        {




            foreach (var file in Directory.EnumerateFiles(_sourcePath, "*.xml", SearchOption.AllDirectories))
            {
                var match = regex.Match(file);
                if (!match.Success)
                    continue;
                action(file,match);
            }

        }

    }
}
