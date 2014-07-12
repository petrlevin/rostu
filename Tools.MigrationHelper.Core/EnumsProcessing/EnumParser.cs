using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Platform.PrimaryEntities.DbEnums;
using Tools.MigrationHelper.Helpers;

namespace Tools.MigrationHelper.Core.EnumsProcessing
{
    public class EnumsParser
    {
        private readonly string _path;
        private readonly DataTable _entityTable;
        private readonly string[] _namesEnums;
        private readonly List<DataTable> _enumTables;


        public EnumsParser(string path, string[] namesEnums)
        {
            _path = path;
            _namesEnums = namesEnums;
            _enumTables = new List<DataTable>();
            _entityTable = GetEntityTable();
        }

        public List<DataTable> GetEnums()
        {
            var list = new List<DataTable>();
            var files = GetFiles();
            foreach (var file in files)
            {
                Parse(file);
            }
            list.Add(_entityTable);
            list.AddRange(_enumTables);
            return list;
        }

        private IEnumerable<string> GetFiles()
        {
            var filesText = new List<string>();
            var directories = SolutionHelper.GetXMLDirectories(_path);
            foreach (var directory in directories)
            {
                var files = new DirectoryInfo(directory).GetFiles("*.cs");
                foreach (var fileInfo in files)
                {
                    using (var reader = new StreamReader(fileInfo.FullName))
                    {
                        filesText.Add(reader.ReadToEnd());
                    }
                }
            }
            return filesText;
        }

        public void Parse(string file)
        {
            //Имя класса(наименование сущности)
            var className = Regex.Match(file, @"public enum\s(?<value>[^\s]*)").Groups["value"].Value;

            //Если указаны конкретные перечисления для отбора
            if (_namesEnums != null && !_namesEnums.Contains(className))
                return;

            //получаем текст внутри фигурных скобок
            var text = Regex.Match(file, @"(?<=\{)(.*)(?=\})", RegexOptions.Singleline).Groups[0].Value;

            //Получаем namespace
            String nameSpace = Regex.Match(file, @"namespace\s(?<value>[^\s]*)").Groups["value"].Value;
            //Получаем имя проекта
            var projectName = nameSpace.Substring(0, nameSpace.LastIndexOf('.')).Replace('.', '_');
            
            var inObj =
                InnerObject.InnerText(
                    Regex.Match(text, @"(?<=<summary>).*?(?=</summary>)", RegexOptions.Singleline).Groups[0].Value
                                                                                                            .Replace(@"///","").Trim());
            var newEntityRow = _entityTable.NewRow();

            newEntityRow[Names.Caption] = inObj.Caption;
            newEntityRow[Names.Description] = inObj.Description;
            newEntityRow[Names.IdProject] = (int) Enum.Parse(typeof (SolutionProject), projectName);
            newEntityRow[Names.Name] = className;
            newEntityRow[Names.IdEntityType] = 1;
            newEntityRow[Names.AllowGenericLinks] = false;

            _entityTable.Rows.Add(newEntityRow);

            var enumTable = GetEnumTable(className);
            var classBody = Regex.Match(text, @"(?<=\{)(.*)(?=\})", RegexOptions.Singleline).Groups[0].Value;
            var strings = Regex.Split(classBody, @"(.+?)\r\n\s*\r\n", RegexOptions.Singleline);
            foreach (var s in strings)
            {
                if (string.IsNullOrEmpty(s))
                    continue;

                string condition;
                //разные шаблоны для перечислений у которых указаны значения и у которых нет
                if (!string.IsNullOrEmpty(Regex.Match(s, @"(\w+\s*=\s*\d+)").Groups[0].Value))
                {
                    condition = @"(<summary>(?<summ>.*?)</summary>).*?(?<key>[\w]+)\s*=\s*(?<value>[\d]+)";
                }
                else
                {
                    condition = @"(<summary>(?<summ>.*?)</summary>).*?(?<key>[\w]+)\s*(?<value>)";
                }

                 var regex = Regex.Matches(s,
                                        condition,
                                        RegexOptions.Singleline);

                foreach (Match match in regex)
                {
                    var newRow = enumTable.NewRow();
                    var fieldInObj = InnerObject.InnerText(match.Groups["summ"].Value.Replace(@"///", "").Trim());

                    newRow[Names.Id] = string.IsNullOrEmpty(match.Groups["value"].Value)
                                           ? enumTable.Rows.Count
                                           : int.Parse(match.Groups["value"].Value);
                    newRow[Names.Name] = match.Groups["key"].Value;
                    newRow[Names.Caption] = fieldInObj.Caption;
                    newRow[Names.Description] = fieldInObj.Description;

                    if (
                        !string.IsNullOrEmpty(newRow[Names.Name].ToString() + newRow[Names.Caption] +
                                              newRow[Names.Description]))
                        enumTable.Rows.Add(newRow);
                }

            }

            _enumTables.Add(enumTable);
        }


        private static DataTable GetEntityTable()
        {
            // Create table.
            var table = new DataTable { TableName = Names.RefEntity };

            // Create columns.
            table.Columns.Add(Names.Name, typeof(string));
            table.Columns.Add(Names.Caption, typeof(string));
            table.Columns.Add(Names.Description, typeof(string));
            table.Columns.Add(Names.IdEntityType, typeof(byte));
            table.Columns.Add(Names.IdProject, typeof(int));
            table.Columns.Add(Names.AllowGenericLinks, typeof(bool));

            table.AcceptChanges();
            return table;
        }

        private static DataTable GetEnumTable(string tableName)
        {
            // Create table.
            var table = new DataTable { TableName = "enm." + tableName };

            // Create columns.
            DataColumn idColumn = table.Columns.Add(Names.Id, typeof(int));
            table.Columns.Add(Names.Name, typeof(string));
            table.Columns.Add(Names.Caption, typeof(string));
            table.Columns.Add(Names.Description, typeof(string));

            // Set the ID column as the primary key column.
            table.PrimaryKey = new[] { idColumn };

            table.AcceptChanges();
            return table;
        }

        public class InnerObject
        {
            public string Caption;
            public string Description;

            /// <summary>
            /// Разбор текста
            /// </summary>
            /// <param name="innerText"></param>
            /// <returns></returns>
            public static InnerObject InnerText(string innerText)
            {
                var inObj = new InnerObject();
                // Разделяем на 2 строки по переносу каретки
                string[] strings = innerText.Split(new[] { '\r', '\n' }, 2, StringSplitOptions.RemoveEmptyEntries);

                if (strings.Any())
                {
                    var str = strings[0].Trim();
                    inObj.Caption = string.IsNullOrWhiteSpace(str) ? null : str;
                    if (strings.Count() > 1)
                    {
                        inObj.Description = strings[1].Trim();
                    }
                }
                else
                {
                    inObj.Caption = null;
                    inObj.Description = null;
                }

                return inObj;
            }
        }
    }
}
