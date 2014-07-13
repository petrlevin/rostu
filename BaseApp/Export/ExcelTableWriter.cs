using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Platform.PrimaryEntities.Common.DbEnums;
using Platform.PrimaryEntities.Common.Interfaces;
using Platform.Utils.Extensions;

namespace BaseApp.Export
{
	/// <summary>
	/// Экспорт в Excel
	/// </summary>
	public class ExcelTableWriter
	{
		private const string ExportType = "excel";

        public IEnumerable<IEntityField> Fields { get; set; }
        public string Title { get; set; }

        public string BuildReport(List<Dictionary<string, object>> result)
        {
            using (var sw = new StringWriter())
            {
                Begin(sw);
                WriteSystemInfo(sw);
                WriteTitle(sw);
                WriteColumnHeaders(sw);
                WriteReportBody(result, sw);
                End(sw);

                sw.Flush();
                return sw.ToString();
            }
        }

        public string BuildMessage(string text)
        {
            using (var sw = new StringWriter())
            {
                Begin(sw);
                WriteCell(sw, text);
                End(sw);

                sw.Flush();
                return sw.ToString();
            }
        }

        #region Private Members

        private void WriteColumnHeaders(StringWriter sw)
	    {
            sw.WriteLine("<tr>");
            foreach (var field in Fields)
	        {
                sw.Write("<td class=header>{0}</td>", field.Caption);
	        }
            sw.WriteLine("</tr>");
	    }

        /// <summary>
        /// Получение тела отчета Excel
        /// </summary>
        /// <param name="result"></param>
        /// <param name="sw"></param>
        private void WriteReportBody(List<Dictionary<string, object>> result, StringWriter sw)
        {
            //вставляем полученные значения
            foreach (var dic in result)
            {
                sw.WriteLine("<tr>");
                foreach (var field in Fields)
                {
                    var cssClass = "text";
                    var cssStyle = "";
                    var fieldName = field.Name;
                    if (!dic.ContainsKey(fieldName)) continue;
                    object value;
                    switch (field.EntityFieldType)
                    {
                        case EntityFieldType.Bool:
                            value = dic[fieldName].NullableToString().Trim().ToLower() == "true" ? "Истина" : "Ложь";
                            break;
                        case EntityFieldType.File:
                            value = "Вложение";
                            break;
                        case EntityFieldType.Int:
                        case EntityFieldType.BigInt:
                        case EntityFieldType.Numeric:
                        case EntityFieldType.Money:
                            cssClass = "number";
                            if (field.Precision.HasValue)
                                cssStyle = @"mso-number-format: '\#\,\#\#0\." + string.Join("", new int[field.Precision.Value]) + "';";
                            else
                                cssStyle = "mso-number-format: General;";

                            value = dic[fieldName].NullableToString().Trim().Replace('.', ',');
                            break;
                        case EntityFieldType.DateTime:
                            cssClass = "date";
                            value = dic[fieldName].NullableToString().Trim();
                            break;
                        case EntityFieldType.Link:
                        case EntityFieldType.FileLink:
                            cssClass = "text";
                            value = dic[fieldName + "_caption"].NullableToString().Trim();
                            break;
                        default:
                            cssClass = "text";
                            value = dic[fieldName];
                            break;
                    }
                    sw.Write("<td class=\"{0}\" style=\"{2}\">{1}</td>", cssClass, value, cssStyle);
                }
                sw.WriteLine("</tr>");
            }
        }

	    /// <summary>
        /// Формирование шапки документа Excel
        /// </summary>
        /// <param name="sw"></param>
        private static void Begin(StringWriter sw)
	    {
	        sw.WriteLine("<html xmlns:o=\"urn:schemas-microsoft-com:office:office\"");
	        sw.WriteLine("xmlns:x=\"urn:schemas-microsoft-com:office:" + ExportType + "\"");
	        sw.WriteLine("xmlns=\"http://www.w3.org/TR/REC-html40\">");
	        sw.WriteLine("<head>");
	        sw.WriteLine("<meta http-equiv=Content-Type content=\"text/html; charset=utf-8\">");
	        sw.WriteLine("<meta name=ProgId content=Excel.Sheet>");
	        sw.WriteLine("<link rel=File-List href=\"export.files/filelist.xml\">");
	        sw.WriteLine("<meta name=Generator content=\"БИС СБОР\">");
	        sw.WriteLine("<style id=\"export_Styles\">");
	        //sw.WriteLine("<!--");
	        sw.WriteLine("table	{mso-displayed-decimal-separator:\",\"; mso-displayed-thousand-separator:\" \";}");
            
            sw.WriteLine(".title {padding-top:1px;padding-right:1px;padding-left:1px;padding-bottom:1px;");
	        sw.WriteLine("color:black;font-size:16.0pt;font-weight:700;font-style:normal;");
	        sw.WriteLine("font-family:Calibri, sans-serif;mso-font-charset:204;");
	        sw.WriteLine("mso-number-format:General;text-align:left;vertical-align:middle;");
            sw.WriteLine("white-space:normal;height:42pt;}");
	        
            sw.WriteLine(@".footer {
                                    padding:1px;
                                    color:black;
                                    font-size:10.0pt;
                                    font-weight:700;
                                    font-style:normal;
                                    font-family:Calibri, sans-serif;
                                    mso-font-charset:204;
                                    mso-number-format:General;
                                    text-align:left;
                                    vertical-align:bottom;
                                    white-space:nowrap;
                                    border:0;
                                    height:32pt;}");
	        
            sw.WriteLine(".header {padding-top:1px;padding-right:1px;padding-left:1px;padding-bottom:1px;");
	        sw.WriteLine("color:black;font-size:11.0pt;font-weight:700;font-style:normal;");
	        sw.WriteLine("font-family:Calibri, sans-serif;mso-font-charset:204;");
	        sw.WriteLine("mso-number-format:General;text-align:center;vertical-align:middle;");
	        sw.WriteLine("background:#D8D8D8;border:.5pt solid windowtext;white-space:normal;}");
	        
            sw.WriteLine("td {padding-top:1px;padding-right:1px;padding-left:1px;padding-bottom:1px;");
	        sw.WriteLine("color:black;font-size:10.0pt;font-weight:400;font-style:normal;");
	        sw.WriteLine("font-family:Calibri, sans-serif;mso-font-charset:204;");
	        sw.WriteLine("mso-number-format:General;text-align:left;vertical-align:top;");
	        sw.WriteLine("border:.5pt solid windowtext;white-space:nowrap;}");

	        sw.WriteLine(@".number {padding:1px;
	                                color:#000;
                                    font-size:10.0pt;
                                    font-weight:400;
                                    font-style:normal;
                                    font-family:Calibri, sans-serif;
                                    mso-font-charset:204;   
                                    /*mso-number-format:Standard;*/
                                    text-align:right;
                                    vertical-align:top;
                                    border:.5pt solid windowtext;
                                    white-space:wrap;}");
            
            sw.WriteLine(".no-border {padding-top:1px;padding-right:1px;padding-left:1px;padding-bottom:1px;");
	        sw.WriteLine("color:black;font-size:10.0pt;font-weight:400;font-style:normal;");
	        sw.WriteLine("font-family:Calibri, sans-serif;mso-font-charset:204;");
	        sw.WriteLine("mso-number-format:General;text-align:left;vertical-align:top;");
	        sw.WriteLine("white-space:wrap;}");
	        
            // http://sa-action.blogspot.com/2009/07/export-to-excel-from-web-page-and-data.html
            sw.WriteLine(".text { mso-number-format: \"\\@\"; white-space:normal; }");
	        
            sw.WriteLine(".date { mso-number-format: 'Short Date'; }");
	        
            sw.WriteLine(@".datetime { mso-number-format: m\/d\/yy\ h\:mm\ AM\/PM; }");
	        
            //sw.WriteLine("-->");
	        sw.WriteLine("</style>");
	        sw.WriteLine("</head><body><div id=\"export\" align=center x:publishsource=\"Excel\"><table cellpadding=0 cellspacing=0>");
	    }

        private static void End(StringWriter sw)
        {
            sw.WriteLine("</div></table></body></html>");
        }

        private void WriteTitle(StringWriter sw)
        {
            sw.WriteLine("<tr><td colspan={0} class=\"title\">{1}</td></tr>", Fields.Count(), Title);
        }

        private void WriteFooter(StringWriter sw)
        {
            sw.WriteLine("<tr><td colspan={0} class=\"footer\">Экспортировано из системы: {1}</td></tr>", Fields.Count(), DateTime.Now);
        }

        private void WriteSystemInfo(StringWriter sw)
        {
            sw.WriteLine("<tr><td class=\"footer\">Выгружено из системы: {0}</td></tr>", DateTime.Now);
        }

        private void WriteRow(StringWriter sw, string text)
        {
            sw.WriteLine("<tr><td colspan={0}>{1}</td></tr>", Fields.Count(), text);
        }

        private void WriteCell(StringWriter sw, string text)
        {
            sw.WriteLine("<tr><td>{0}</td></tr>", text);
        }

        #endregion
    }
}
