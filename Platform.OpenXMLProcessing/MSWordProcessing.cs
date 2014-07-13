using System.Collections.Generic;
using System.IO;
using DocumentFormat.OpenXml.Packaging;
using Platform.OpenXMLProcessing.MSWordHelpers;

namespace Platform.OpenXMLProcessing
{
    public class MSWordProcessing
    {
        private readonly WordprocessingDocument _document;
        private readonly MemoryStream _processingStream;

        public MSWordProcessing(byte[] data) : this(data, false)
        {
             
        }

        public MSWordProcessing(byte[] data, bool formatSimpleCodes)
        {
            _processingStream = new MemoryStream(data);
            _document = WordprocessingDocument.Open(_processingStream, true);
            
            if (formatSimpleCodes)
                FormatSimpleCodes();
        }

        public void FormatSimpleCodes()
        {
            MSWordSimpleFieldHelper.ConvertFieldCodes(_document.MainDocumentPart.Document);

            foreach (HeaderPart hpart in _document.MainDocumentPart.HeaderParts)
            {
                MSWordSimpleFieldHelper.ConvertFieldCodes(hpart.Header);
                hpart.Header.Save();
            }

            // process footer(s)
            foreach (FooterPart fpart in _document.MainDocumentPart.FooterParts)
            {
                MSWordSimpleFieldHelper.ConvertFieldCodes(fpart.Footer);
                fpart.Footer.Save();
            }
        }

        public IEnumerable<string> GetTags()
        {
            return _document.GetMergedFieldTags();
        }

        public void FillMergedFields(Dictionary<string, string> values)
        {
            _document.ReplaceFieldValues(values);
        }

        public byte[] GetDocument()
        {
            return _processingStream.ToArray();
        }
    }
}
