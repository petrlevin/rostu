using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Design;
using System.Data.Entity.Infrastructure;
using System.Data.Mapping;
using System.Data.Metadata.Edm;
using System.IO;
using System.Reflection;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using Platform.PrimaryEntities.DbEnums;
using Tools.MigrationHelper.Helpers;

namespace Tools.MigrationHelper.Core.Generators.PreGenerateEfViews
{
    public class Generator
    {
        public Generator(string sourcePath, string connectionString)
        {
            _sourcePath = sourcePath;
            _connectionString = connectionString;
            _unloadDir = @"GeneratedCode";
        }

        public void Generate()
        {
            var names = new List<string>();

            //Берем последний проект и для него делаем вьюху
            var proj = Enum.GetValues(typeof (SolutionProject)).Cast<int>().Max();

            var projectName = SolutionHelper.GetProjectName(proj);
            string projectPath = SolutionHelper.GetProjectPath(_sourcePath,proj);

            var file = Directory.EnumerateFiles(Path.Combine(projectPath, "bin/Debug"), projectName + ".dll").FirstOrDefault();

            if(file == null)
                throw new Exception("Не найдена сборка: " + projectName + ".dll " + "Ожидалась в папке: " + Path.Combine(projectPath, "bin/Debug"));

            var asm = Assembly.LoadFrom(file);

            if (asm == null)
                throw new Exception("Ошибка");

            var contextType = asm.GetTypes().SingleOrDefault(w => w.Name == "DataContext");

            if (contextType != null)
            {
                var ms = new MemoryStream();

                using (XmlWriter writer = XmlWriter.Create(ms))
                {
                    EdmxWriter.WriteEdmx(
                        (DbContext) Activator.CreateInstance(contextType, _connectionString),
                        writer);
                }

                //сначало в строку, потом в xml, напрямую почему то падает
                ms.Position = 0;
                var sr = new StreamReader(ms);
                var edmxString = sr.ReadToEnd();

                var generatedViewText = GetString(XDocument.Parse(edmxString));

                GeneratorHelper.WriteToFile(
                    Path.Combine(projectPath, Path.Combine(_unloadDir, @"DataContext.Views.cs")), 
                    generatedViewText);

                names.Add(Path.Combine(_unloadDir, @"DataContext.Views.cs"));
            }

            SolutionHelper.InsertToProject(names, projectPath, projectName, "Compile", false);
        }

        private string GetString(XDocument edmx)
        {
            // extract csdl, ssdl and msl artifacts from the Edmx
            XmlReader csdlReader, ssdlReader, mslReader;
            SplitEdmx(edmx, out csdlReader, out ssdlReader, out mslReader);

            // Initialize item collections
            var edmItemCollection = new EdmItemCollection(new[] { csdlReader });
            var storeItemCollection = new StoreItemCollection(new[] { ssdlReader });
            var mappingItemCollection = new StorageMappingItemCollection(edmItemCollection, storeItemCollection, new[] { mslReader });
            var viewGenerator = new EntityViewGenerator { LanguageOption = LanguageOption.GenerateCSharpCode };

            // generate views
            using (var memoryStream = new MemoryStream())
            {
                var writer = new StreamWriter(memoryStream);

                var errors = viewGenerator.GenerateViews(mappingItemCollection, writer, GetEdmxSchemaVersion(edmx)).ToList();

                if (errors.Any())
                {
                    var mess = errors.Aggregate(string.Empty, (current, error) => current + error.Message + ";   ");

                    throw new Exception(mess);
                }

                memoryStream.Position = 0;
                var reader = new StreamReader(memoryStream);
                return reader.ReadToEnd();
            }
        }

        private void SplitEdmx(XDocument edmx, out XmlReader csdlReader, out XmlReader ssdlReader, out XmlReader mslReader)
        {
            // xml namespace agnostic to make it work with any version of Entity Framework
            var edmxNs = edmx.Root.Name.Namespace;

            var storageModels = edmx.Descendants(edmxNs + "StorageModels").Single();
            var conceptualModels = edmx.Descendants(edmxNs + "ConceptualModels").Single();
            var mappings = edmx.Descendants(edmxNs + "Mappings").Single();

            ssdlReader = storageModels.Elements().Single(e => e.Name.LocalName == "Schema").CreateReader();
            csdlReader = conceptualModels.Elements().Single(e => e.Name.LocalName == "Schema").CreateReader();
            mslReader = mappings.Elements().Single(e => e.Name.LocalName == "Mapping").CreateReader();
        }

        private Version GetEdmxSchemaVersion(XDocument edmx)
        {
            var edmxNs = edmx.Root.GetDefaultNamespace();

            if (edmxNs == "http://schemas.microsoft.com/ado/2009/11/edmx")
            {
                return EntityFrameworkVersions.Version3;
            }
            if (edmxNs == "http://schemas.microsoft.com/ado/2008/10/edmx")
            {
                return EntityFrameworkVersions.Version2;
            }

            // V1, greater than V3 or non-edmx edmxNs
            throw new ArgumentException("Unsupported edmx version");
        }

        private readonly string _sourcePath;
        private readonly string _connectionString;
        /// <summary>
        /// Имя папки для сгенерированных файлов сущностных классов
        /// </summary>
        private readonly string _unloadDir;
    }
}
