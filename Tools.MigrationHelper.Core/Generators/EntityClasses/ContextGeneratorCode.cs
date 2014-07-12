using System.Collections.Generic;
using Platform.PrimaryEntities.Reference;

namespace Tools.MigrationHelper.Core.Generators.EntityClasses
{
	partial class ContextGenerator
	{
		private readonly List<Entity> _entities;
		private readonly string _namespace;
		private readonly List<string> _entityNameSpaces;
		private readonly int? _previosSolution;

		/// <summary>
		/// Конструктор для T4 генератора
		/// </summary>
		/// <param name="data"></param>
		/// <param name="Namespace"></param>
		/// <param name="previosSolution"></param>
		public ContextGenerator(List<Entity> data, string Namespace, List<string> entityNameSpaces, int? previosSolution)
		{
			_entities = data;
			_namespace = Namespace;
			_entityNameSpaces = entityNameSpaces;
			_previosSolution = previosSolution;
		}
	}
}
