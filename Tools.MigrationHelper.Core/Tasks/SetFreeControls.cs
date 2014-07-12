using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using NAnt.Core;
using NAnt.Core.Attributes;
using Platform.BusinessLogic.Activity.Controls;
using Platform.BusinessLogic.Common.Interfaces;
using Platform.PrimaryEntities.Factoring;
using Platform.PrimaryEntities.Reference;
using Platform.Utils.Extensions;

namespace Tools.MigrationHelper.Core.Tasks
{
    [TaskName("setfreecontrols")]
    public class SetFreeControls : SetControlsTask
    {
        protected override void ExecuteTask()
        {
            if (!IsDeveloper()) return;
            try
            {
                Log(Level.Verbose, "Получение данных о контролях...");
                Log(Level.Verbose, "Загрузка сборок ...");
                var asmbls = GetAssemblies();
                var controls = ReadControlsFromSource(asmbls);
                Log(Level.Verbose, "Записываем контролы в базу...");
                WriteControls(controls);

                Log(Level.Verbose, "Получение данных об общих контролях ...");
                var commonControlTargetsWithInital = GetCommonControlTargets(asmbls);
                var commonControls = GetCommonControlTypes(asmbls);

                WriteCommonControls(commonControls, commonControlTargetsWithInital);
            }
            catch (Exception ex)
            {
                Fatal("Фатальная ошибка при установке контролей", ex);
            }
        }

        private void WriteCommonControls(IEnumerable<Type> commonControls, IEnumerable<Type> commonControlTargetsWithInital)
        {
            WriteCommonControls(commonControls, commonControlTargetsWithInital, typeof(IFreeCommonControl<,>));
        }

        private IEnumerable<Type> GetCommonControlTypes(IEnumerable<Assembly> asmbls)
        {
            return GetCommonControlTypes(asmbls, typeof(IFreeCommonControl<,>));
        }

        private IEnumerable<ControlInfo> ReadControlsFromSource(IEnumerable<Assembly> asmbls)
        {
            var pattern = new Regex(GetPattern());
            var controls = new List<ControlInfo>();
            foreach (string fileName in GetFileNames())
            {
                using (var reader = new StreamReader(fileName))
                {
                    string fileContent = reader.ReadToEnd();
                    fileContent = RemoveComments(fileContent);

                    var match = pattern.Match(fileContent);

                    if (!match.Success)
                    {
                        continue;
                    }
                    var entityName = GetEntityName(fileContent);
                    var idEntity = GetIdEntity(entityName);
                    var entityType = GetEntityType(asmbls, entityName);

                    while (match.Success)
                    {
                        var cname = match.Groups["ControlName"].Value;
                        if (!controls.Any(cc => (cc.Name == cname) && (cc.IdEntity == idEntity)))
                        {
                            var mi = GetControlMethodInfo(entityType, cname);
                            var controlInfo = CreateControlInfo(mi,
                                mi.GetAttributeExactlyMatch<ControlInitialAttribute>(),
                                null, idEntity);
                            
                            controls.Add(controlInfo);
                        }
                        match = match.NextMatch();
                    }
                }
            }
            return controls;
        }

        //http://stackoverflow.com/questions/3524317/regex-to-strip-line-comments-from-c-sharp/3524689#3524689
        private string RemoveComments(string fileContent)
        {
            const string blockComments = @"/\*(.*?)\*/";
            const string lineComments = @"//(.*?)\r?\n";
            const string strings = @"""((\\[^\n]|[^""\n])*)""";
            const string verbatimStrings = @"@(""[^""]*"")+";

            return Regex.Replace(fileContent,
                blockComments + "|" + lineComments + "|" + strings + "|" + verbatimStrings,
                me =>
                {
                    if (me.Value.StartsWith("/*") || me.Value.StartsWith("//"))
                        return me.Value.StartsWith("//") ? Environment.NewLine : "";
                    // Keep the literal strings
                    return me.Value;
                },
                RegexOptions.Singleline);
        }

        private string GetEntityName(string fileContent)
        {
            return Regex.Match(fileContent, @" class\s(?<value>[^\s]*)").Groups["value"].Value;
        }

        private int? GetIdEntity(string entityName)
        {
            try
            {

                return Objects.ByName<Entity>(entityName).Id;
            }
            catch (Exception)
            {

                return null;
            }

        }

        private Type GetEntityType(IEnumerable<Assembly> asmbls, string entityName)
        {
            foreach (var assembly in asmbls)
            {
                try
                {
                    var type = assembly.GetTypes().FirstOrDefault(t => t.Name == entityName);
                    if (type != null)
                        return type;
                }
                catch (ReflectionTypeLoadException)
                {
                }
            }
            return null;
        }

        private MethodInfo GetControlMethodInfo(Type entityType, string methodName)
        {
            return entityType.GetMethod(methodName);
        }

        private IEnumerable<string> GetFileNames()
        {
            return Directory.EnumerateFiles(string.Format("{0}\\", SourcePath), "*.cs", SearchOption.AllDirectories);
        }

        private string GetPattern()
        {
            return @"ExecuteControl\([\w]+[\s]*=>[\s]*[\w]+\.(?'ControlName'[\w]+)\(";
        }
    }
}
