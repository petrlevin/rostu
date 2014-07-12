using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Platform.BusinessLogic;
using Platform.BusinessLogic.Reference;
using Platform.Common;
using Platform.Common.Exceptions;
using Platform.OpenXMLProcessing;

namespace Sbor.Reports.Report
{
    partial class WordCommonReport
    {

        private DataContext _dc;
        public DataContext Context
        {
            get{ return _dc ?? (_dc = IoC.Resolve<DbContext>().Cast<DataContext>()); }
        }

        private FileLink _templateFileLink;
        public FileLink TemplateFileLink
        {
            get
            {
                if (_templateFileLink == null)
                {
                    var fileLink = Context.FileLink.FirstOrDefault(f => f.Id == IdTemplateFile);
                    if (fileLink == null)
                        throw new PlatformException("Отсутствует файл");

                    _templateFileLink = fileLink;
                }

                return _templateFileLink;
            }
        }

        private byte[] _templateFile;
        public byte[] TemplateFile {
            get { return _templateFile ?? (_templateFile = TemplateFileLink.FileStore.File); }
        }

        public byte[] GetReportResult()
        {
            var processor = new MSWordProcessing(TemplateFile, true);

            var tagValues = GetTagValues(processor.GetTags());
            processor.FillMergedFields(tagValues);

            return processor.GetDocument();
        }

        private Dictionary<string, string> GetTagValues(IEnumerable<string> tags)
        {
            var result = new Dictionary<string, string>();

            foreach (var tag in tags)
                result[tag] = GetTagValue(tag);
            
            return result;
        }

        private string GetTagValue(string tag)
        {
            var regExpHasParams = new Regex(@"^(\b\w[\w,\s]*)");
            var regExpAdditionalParams = new Regex(@"\[(\w+)=(.*?)\]");

            var paramNameMatch = regExpHasParams.Match(tag);
            if (paramNameMatch.Success == false)
                return String.Format("Ошибка вычисления значения маркера: неверный формат маркера {0}. Не удается определить имя параметра", tag);

            var paramName = regExpHasParams.Match(tag).Groups[0].Value;

            var reportParam = Context.WordCommonReportParams.FirstOrDefault(p => p.Caption == paramName);
            if (reportParam == null)
                return String.Format("Ошибка вычисления значения маркера: в справочнике «Переменные отчетов» отсутствует параметр с именем {0}", paramName);

            var matches = regExpHasParams.Matches(tag);
            if (matches.Count > 2)
                return String.Format("Ошибка вычисления значения маркера: неверный формат маркера «{0}»", tag);

            var additionalParams = new Dictionary<string, string>();

            var m = regExpAdditionalParams.Match(tag);
            while (m.Success)
            {
                Console.WriteLine("   " + m.Groups[0].Value);
                additionalParams[m.Groups[1].Value] = m.Groups[2].Value.Replace("\\\"", "\"");

                m = m.NextMatch();
            }

            return reportParam.GetValue(additionalParams);
        }

    }
}
