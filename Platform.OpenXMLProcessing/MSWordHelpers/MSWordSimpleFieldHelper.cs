using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;

namespace Platform.OpenXMLProcessing.MSWordHelpers
{
    public static class MSWordSimpleFieldHelper
    {
        private static readonly char[] SplitChar = new char[] { ' ' };

        private static bool IsMergedField(string[] instruction)
        {
            return instruction != null &&
                    instruction.Length > 0 &&
                    instruction[0].ToLower().Equals("mergefield");
        }

        private static string GetFieldName(string[] instruction)
        {
            if (instruction != null &&
                    instruction.Length > 1)
                return instruction[1];

            return null;
        }

        private static string[] GetFieldInstruction(SimpleField field)
        {
            var instruction = new string[2];

            var param = field.Instruction.Value;
            instruction[0] = param.Split(SplitChar, StringSplitOptions.RemoveEmptyEntries)[0];

            if (param.IndexOf('"') < 0)
                instruction[1] = param.Split(SplitChar, StringSplitOptions.RemoveEmptyEntries)[1];
            else
            {
                var reg = new Regex(@"[^\\\\]""(.*)""");
                instruction[1] = reg.Match(param).Groups[1].Value;
            }
            
            return instruction;
        }

        private static IEnumerable<string> GetTagsFromElement(OpenXmlElement element)
        {
            var tags = new List<string>();

            foreach (var field in element.Descendants<SimpleField>())
            {
                string[] instruction = GetFieldInstruction(field);

                if (IsMergedField(instruction))
                {
                    string fieldname = GetFieldName(instruction);
                    if (!string.IsNullOrEmpty(fieldname))
                        tags.Add(fieldname);
                }
            }

            return tags.Distinct();
        }

        /// <summary>
        /// Получить все уникальные значения кодов
        /// </summary>
        /// <param name="document"></param>
        /// <returns></returns>
        public static IEnumerable<string> GetMergedFieldTags(this WordprocessingDocument document)
        {
            var tags = new List<string>();

            tags.AddRange(GetTagsFromElement(document.MainDocumentPart.Document.Body));

            foreach (HeaderPart hpart in document.MainDocumentPart.HeaderParts)
                tags.AddRange(GetTagsFromElement(hpart.Header));

            foreach (FooterPart fpart in document.MainDocumentPart.FooterParts)
                tags.AddRange(GetTagsFromElement(fpart.Footer));

            return tags.Distinct();
        }

     
        #region http://www.codeproject.com/Articles/38575/Fill-Mergefields-in-docx-Documents-without-Microso
        /// <summary>
        /// Returns a <see cref="Run"/>-openxml element for the given text.
        /// Specific about this run-element is that it can describe multiple-line and tabbed-text.
        /// The <see cref="SimpleField"/> placeholder can be provided too, to allow duplicating the formatting.
        /// </summary>
        /// <param name="text">The text to be inserted.</param>
        /// <param name="placeHolder">The placeholder where the text will be inserted.</param>
        /// <returns>A new <see cref="Run"/>-openxml element containing the specified text.</returns>
        internal static Run GetRunElementForText(string text, SimpleField placeHolder)
        {
            string rpr = null;
            if (placeHolder != null)
            {
                foreach (RunProperties placeholderrpr in placeHolder.Descendants<RunProperties>())
                {
                    rpr = placeholderrpr.OuterXml;
                    break;  // break at first
                }
            }

            Run r = new Run();
            if (!string.IsNullOrEmpty(rpr))
            {
                r.Append(new RunProperties(rpr));
            }

            if (!string.IsNullOrEmpty(text))
            {
                // first process line breaks
                string[] split = text.Split(new string[] { "\n" }, StringSplitOptions.None);
                bool first = true;
                foreach (string s in split)
                {
                    if (!first)
                    {
                        r.Append(new Break());
                    }

                    first = false;

                    // then process tabs
                    bool firsttab = true;
                    string[] tabsplit = s.Split(new string[] { "\t" }, StringSplitOptions.None);
                    foreach (string tabtext in tabsplit)
                    {
                        if (!firsttab)
                        {
                            r.Append(new TabChar());
                        }

                        r.Append(new Text(tabtext));
                        firsttab = false;
                    }
                }
            }

            return r;
        }
        
        /// <summary>
        /// Since MS Word 2010 the SimpleField element is not longer used. It has been replaced by a combination of
        /// Run elements and a FieldCode element. This method will convert the new format to the old SimpleField-compliant 
        /// format.
        /// </summary>
        /// <param name="mainElement"></param>
        public static void ConvertFieldCodes(OpenXmlElement mainElement)
        {
            //  search for all the Run elements 
            Run[] runs = mainElement.Descendants<Run>().ToArray();
            if (runs.Length == 0) return;

            Dictionary<Run, Run[]> newfields = new Dictionary<Run, Run[]>();

            int cursor = 0;
            do
            {
                Run run = runs[cursor];

                if (run.HasChildren && run.Descendants<FieldChar>().Count() > 0
                    && (run.Descendants<FieldChar>().First().FieldCharType & FieldCharValues.Begin) == FieldCharValues.Begin)
                {
                    List<Run> innerRuns = new List<Run>();
                    innerRuns.Add(run);

                    //  loop until we find the 'end' FieldChar
                    bool found = false;
                    string instruction = null;
                    RunProperties runprop = null;
                    do
                    {
                        cursor++;
                        run = runs[cursor];

                        innerRuns.Add(run);
                        if (run.HasChildren && run.Descendants<FieldCode>().Count() > 0)
                            instruction += run.GetFirstChild<FieldCode>().Text;
                        if (run.HasChildren && run.Descendants<FieldChar>().Count() > 0
                            && (run.Descendants<FieldChar>().First().FieldCharType & FieldCharValues.End) == FieldCharValues.End)
                        {
                            found = true;
                        }
                        if (run.HasChildren && run.Descendants<RunProperties>().Count() > 0)
                            runprop = run.GetFirstChild<RunProperties>();
                    } while (found == false && cursor < runs.Length);

                    //  something went wrong : found Begin but no End. Throw exception
                    if (!found)
                        throw new Exception("Found a Begin FieldChar but no End !");

                    if (!string.IsNullOrEmpty(instruction))
                    {
                        //  build new Run containing a SimpleField
                        Run newrun = new Run();
                        if (runprop != null)
                            newrun.AppendChild(runprop.CloneNode(true));
                        SimpleField simplefield = new SimpleField();
                        simplefield.Instruction = instruction;
                        newrun.AppendChild(simplefield);

                        newfields.Add(newrun, innerRuns.ToArray());
                    }
                }

                cursor++;
            } while (cursor < runs.Length);

            //  replace all FieldCodes by old-style SimpleFields
            foreach (KeyValuePair<Run, Run[]> kvp in newfields)
            {
                kvp.Value[0].Parent.ReplaceChild(kvp.Key, kvp.Value[0]);
                for (int i = 1; i < kvp.Value.Length; i++)
                    kvp.Value[i].Remove();
            }
        }
        #endregion

        /// <summary>
        /// Заменить в документы значения в полях и сохранить изменения
        /// </summary>
        /// <param name="document"></param>
        /// <param name="replaceOperations"></param>
        /// <param name="withSave"></param>
        /// <returns></returns>
        public static void ReplaceFieldValues(this WordprocessingDocument document, Dictionary<string, string> replaceOperations, bool withSave = true)
        {
            if (!replaceOperations.Any())
                return;

            foreach (var field in document.MainDocumentPart.Document.Body.Descendants<SimpleField>())
            {
                string[] instruction = GetFieldInstruction(field);

                if (IsMergedField(instruction))
                {
                    string fieldname = GetFieldName(instruction);

                    string value = replaceOperations.ContainsKey(fieldname) ? replaceOperations[fieldname] : null;
                    if (value != null)
                        field.Parent.ReplaceChild<SimpleField>(GetRunElementForText(value, field), field);
                }
            }

            //Сохраняем изменения
            if (withSave)
                document.MainDocumentPart.Document.Save();
        }
    }
}
