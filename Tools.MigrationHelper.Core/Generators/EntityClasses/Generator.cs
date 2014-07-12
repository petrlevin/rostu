using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using Platform.PrimaryEntities.Common.DbEnums;
using Platform.PrimaryEntities.Common.Extensions;
using Platform.PrimaryEntities.DbEnums;
using Platform.PrimaryEntities.Reference;
using Tools.MigrationHelper.Helpers;

namespace Tools.MigrationHelper.Core.Generators.EntityClasses
{
    internal class Generator
    {
        /// <summary>
        /// Путь до Решения(Solution)
        /// </summary>
        private readonly string _solutionPath;

        /// <summary>
        /// Имя папки для сгенерированных файлов сущностных классов
        /// </summary>
        private readonly string _unloadDir;

        /// <summary>
        /// DataSet с данными
        /// </summary>
        private readonly DataSet _dataSet;

        public Generator(string solutionPath, DataSet dataSet, string unloadDir)
        {
            _solutionPath = solutionPath;
            _unloadDir = unloadDir;
            _dataSet = dataSet;
        }

        public Generator(string solutionPath, DataSet dataSet)
        {
            _dataSet = dataSet;
            _unloadDir = @"GeneratedCode";
            _solutionPath = solutionPath;
        }

        /// <summary>
        /// Генерация сущностных классов на основе экземпляров объектов
        /// </summary>
        public void Generate()
        {
            //Получаем таблицу Entity с заполненой коллекцией EntityFields
            var entities = GetEntities();
            //Для наследования контекста сохраняем идентификатор предыдущего проекта
            int? previosSolution = null;
            foreach (var project in Enum.GetValues(typeof (SolutionProject)))
            {
                // список имен файлов для вставки в файл проекта. Пример одного элемента: DbStructure\Abc.xml
                var names = new List<string>();
                var idProject = (int) project;
                string projectName = SolutionHelper.GetProjectName(idProject);
                string projectPath = SolutionHelper.GetProjectPath(_solutionPath,idProject);
                var entityNameSpaces = new List<string>();

                List<Entity> projectEntities =
                    entities.Where(
                        w =>
                        w.IdProject == idProject && w.IdEntityType != (byte) EntityType.Multilink &&
                        w.IdEntityType != (byte) EntityType.DataEndpoint && w.IdEntityType != (byte) EntityType.Enum &&
                        w.GenerateEntityClass).ToList();

                if (!projectEntities.Any())
                    continue;

                //список всех ссылочных полей мультилинков
                var list =
                    entities.Where(w => w.IdEntityType == (byte) EntityType.Multilink)
                            .SelectMany(e => e.Fields)
                            .Where(w => w.IdEntityLink != null)
                            .ToList();

                //Это коллекция с уже созданными мультилинками, для одного мультилинка один маппинг
                var mappedMultiLinks = new List<Entity>();

                //Создание файлов на основе экземляров класса
                foreach (Entity entity in projectEntities)
                {
                    //поля мультилинков которые ссылаются на данную сущность
                    var multilinkFields =
                        list.Where(
                            w =>
                            w.IdEntityLink == entity.Id &&
                            entity.Fields.Select(s => s.IdEntityLink).Contains(w.IdEntity)).ToList();


                    string entityNameSpace = string.Format("{0}.{1}", projectName, entity.EntityType);

                    string filePath = Path.Combine(_unloadDir, entity.GetClassName().Replace('.', '\\') + ".cs");

                    var entitiesGenerator = new EntitiesGenerator(entity, entityNameSpace, entities, multilinkFields);
                    GeneratorHelper.WriteToFile(Path.Combine(projectPath, filePath), entitiesGenerator.TransformText());
                    names.Add(filePath);

                    if (!entityNameSpaces.Contains(entityNameSpace))
                        entityNameSpaces.Add(entityNameSpace);

                    var format = string.Format("{0}\\{1}\\{2}", entity.EntityType, "Mappings", entity.Name);
                    var mappingFilePath = Path.Combine(_unloadDir, format + "Map.cs");

                    entityNameSpace += ".Mappings";

                    var mapGenerator = new MapGenerator(entity, entityNameSpace, entities, multilinkFields)
                        {
                            MappedMultiLinks = mappedMultiLinks
                        };
                    GeneratorHelper.WriteToFile(Path.Combine(projectPath, mappingFilePath), mapGenerator.TransformText());
                    names.Add(mappingFilePath);

                    if (!entityNameSpaces.Contains(entityNameSpace))
                        entityNameSpaces.Add(entityNameSpace);
                }

                // создаем контекст
                if (names.Any())
                {
                    var contextFilePath = Path.Combine(_unloadDir, @"DataContext.cs");

                    var contextGenerator = new ContextGenerator(projectEntities, projectName, entityNameSpaces,
                                                                previosSolution);
                    GeneratorHelper.WriteToFile(Path.Combine(projectPath, contextFilePath), contextGenerator.TransformText());
                    names.Add(contextFilePath);
                }
                //Вставка информации о файлах в файл проекта и добавление к свн
                SolutionHelper.InsertToProject(names, projectPath, projectName, "Compile");

                previosSolution = (int) project;
            }
        }

        #region private methods

        private List<Entity> GetEntities()
        {
            var entities = (List<Entity>) CollectionHelper.ConvertTo<Entity>(_dataSet.Tables[Names.RefEntity]);
            var entitiesFields =
                (List<EntityField>) CollectionHelper.ConvertTo<EntityField>(_dataSet.Tables[Names.RefEntityField]);

            foreach (var entity in entities)
            {
                entity.Fields = entitiesFields.Where(w => w.IdEntity == entity.Id);
            }
            return entities;
        }

        #endregion
    }
}
