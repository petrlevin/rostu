using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Platform.PrimaryEntities.Common.Interfaces;
using Platform.PrimaryEntities.Reference;

namespace Tools.MigrationHelper.Generator.Entities
{
	partial class MapGenerator
	{
		private readonly List<IEntityField> _fields;
		private readonly Entity _entity;
		private readonly string _namespace;
		private readonly List<Entity> _entities;

		/// <summary>
		/// Конструктор для T4 генератора
		/// </summary>
		/// <param name="entity"></param>
		/// <param name="Namespace"></param>
		/// <param name="entities"></param>
		public MapGenerator(Entity entity, string Namespace, List<Entity> entities) 
		{ 
			_fields = entity.Fields.ToList();
			_entity = entity;
			_namespace = Namespace;
			_entities = entities;
		}

	}
}
