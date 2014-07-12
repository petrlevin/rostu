using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Platform.PrimaryEntities.Common.DbEnums;
using Platform.PrimaryEntities.DbEnums;
using Platform.PrimaryEntities.Reference;
using Tools.MigrationHelper.Helpers;

namespace Tools.MigrationHelper.Generator.Entities
{
	partial class EntitiesGenerator
	{
		private readonly Entity _entity;
		private readonly string _namespace;
		private readonly List<Entity> _entities;
		private readonly string _loadDir;

		/// <summary>
		/// Конструктор для T4 генератора
		/// </summary>
		/// <param name="entity">Сущность для которой создается класс</param>
		/// <param name="Namespace">Namespace создаваемого класса</param>
		/// <param name="entities">Коллекция сущностей для полей ссылок</param>
		/// <param name="uploadDir">Папка куда генерируется код</param>
		public EntitiesGenerator(Entity entity, string Namespace, List<Entity> entities, string uploadDir)
		{
			_entity = entity;
			_namespace = Namespace;
			_entities = entities;
			_loadDir = uploadDir;
		}

		private List<string> GetUsings()
		{
			List<string> result = new List<string>();
			var entityLinkes = _entity.Fields.Where(w => w.IdEntityLink != null).Select(s => s.IdEntityLink ?? -1);
			var entityTypes = _entities.Where(w => entityLinkes.Contains(w.Id)).Select(s => new { s.EntityType, s.IdProject });
			foreach (var entityType in entityTypes)
			{
				if (entityType.EntityType == EntityType.Enum && entityType.IdProject == 100)
				{
					var addStr = "using " + SolutionHelper.GetProjectName(entityType.IdProject == 100 ? 200 : entityType.IdProject) + "." + "DbEnums" + ";";
					
					if (!result.Contains(addStr))
					{
						result.Add(addStr);
					}
					var addStr2 = "using " + SolutionHelper.GetProjectName(entityType.IdProject == 100 ? 210 : entityType.IdProject) + "." + "DbEnums" + ";";
					if (!result.Contains(addStr2))
					{
						result.Add(addStr2);
					}
					continue;
				}
				if (entityType.IdProject == 100)
				{
					var addStr = "using " + SolutionHelper.GetProjectName(200) + "." + entityType.EntityType + ";";
					if (!result.Contains(addStr))
					{
						result.Add(addStr);
					}
					continue;
				}
//				var addSt = "using " + SolutionHelper.GetProjectName(entityType.IdProject) + "." + entityType.EntityType + ";";
//				if (!result.Contains(addSt))
//				{
//					result.Add(addSt);
//				}
			}

			return result;
		}
	}
}
