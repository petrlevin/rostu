using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using Platform.PrimaryEntities.Common.DbEnums;
using Platform.PrimaryEntities.DbEnums;
using Platform.PrimaryEntities.Reference;
using Tools.MigrationHelper.Helpers;

namespace Tools.MigrationHelper.Generator.Entities
{
	class CodeGenerator
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

		public CodeGenerator(string solutionPath, DataSet dataSet, string unloadDir)
		{
			_solutionPath = solutionPath;
			_unloadDir = unloadDir;
			_dataSet = dataSet;
		}

		public CodeGenerator(string solutionPath, DataSet dataSet)
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
			foreach (var project in Enum.GetValues(typeof(SolutionProject)))
			{
//				if ((int)project == 100 || (int)project == 200)// Пока не реализован механизм признака сущности по генерации кода, 
//					continue;                                  //не генерируем для проектов MigrationHelper и PrimaryEntities

				// список имен файлов для вставки в файл проекта. Пример одного элемента: DbStructure\Abc.xml
				var names = new List<string>();
				int idProject = (int) project;
				string projectName = SolutionHelper.GetProjectName(idProject);
				string projectPath = GetProjectPath(idProject);
				List<string> entityNameSpaces = new List<string>();

				List<Entity> projectEntities = entities.Where(w => w.IdProject == idProject && w.IdEntityType != (byte)EntityType.DataEndpoint && w.IdEntityType != (byte)EntityType.Enum && w.GenerateEntityClass).ToList();

				if(!projectEntities.Any())
					continue;

				//Создание файлов на основе экземляров класса
				foreach (Entity entity in projectEntities)
				{
					string entityNameSpace = string.Format("{0}.{1}", projectName, entity.EntityType);
					
					string filePath = Path.Combine(_unloadDir, entity.GetClassName().Replace('.','\\') + ".cs");

					EntitiesGenerator entitiesGenerator = new EntitiesGenerator(entity, entityNameSpace, entities, _unloadDir);
					WriteToFile(Path.Combine(projectPath, filePath), entitiesGenerator.TransformText());
					names.Add(filePath);

					if (!entityNameSpaces.Contains(entityNameSpace))
						entityNameSpaces.Add(entityNameSpace);

					var format = string.Format("{0}\\{1}\\{2}", entity.EntityType, "Mappings", entity.Name);
					var mappingFilePath = Path.Combine(_unloadDir, format + "Map.cs");

					entityNameSpace += ".Mappings";

					MapGenerator mapGenerator = new MapGenerator(entity, entityNameSpace, entities);
					WriteToFile(Path.Combine(projectPath, mappingFilePath), mapGenerator.TransformText());
					names.Add(mappingFilePath);

					if (!entityNameSpaces.Contains(entityNameSpace))
						entityNameSpaces.Add(entityNameSpace);
				}

				// создаем контекст
				if (names.Any())
				{
					var contextFilePath = Path.Combine(_unloadDir, @"DataContext.cs");

					ContextGenerator contextGenerator = new ContextGenerator(projectEntities, projectName, entityNameSpaces, previosSolution);
					WriteToFile(Path.Combine(projectPath, contextFilePath), contextGenerator.TransformText());
					names.Add(contextFilePath);
				}
				//Вставка информации о файлах в файл проекта и добавление к свн
				SolutionHelper.InsertToProject(names, projectPath, SolutionHelper.GetProjectName(idProject), "Compile");

				previosSolution = (int)project;
			}
		}

		#region private methods

		private List<Entity> GetEntities()
		{
			List<Entity> entities = (List<Entity>)CollectionHelper.ConvertTo<Entity>(_dataSet.Tables[Names.RefEntity]);
			List<EntityField> entitiesFields = (List<EntityField>)CollectionHelper.ConvertTo<EntityField>(_dataSet.Tables[Names.RefEntityField]);

			foreach (var entity in entities)
			{
				entity.Fields = entitiesFields.Where(w => w.IdEntity == entity.Id);
			}
			return entities;
		}

		/// <summary>
		/// Путь до папки проекта
		/// </summary>
		/// <param name="idSolutionProject">Идентификатор проекта</param>
		/// <returns></returns>
		private string GetProjectPath(int idSolutionProject)
		{
			return Path.Combine(_solutionPath, SolutionHelper.GetProjectName(idSolutionProject));
		}

		/// <summary>
		/// Запись файла с созданием пути
		/// </summary>
		/// <param name="path">Пример: C:\example.xml</param>
		/// <param name="text"></param>
		private void WriteToFile(string path, string text)
		{
			var file = new FileInfo(path);
			if (!Directory.Exists(file.Directory.FullName))
				Directory.CreateDirectory(file.Directory.FullName);
			File.WriteAllText(path, text);
		}

		#endregion
	}
}
