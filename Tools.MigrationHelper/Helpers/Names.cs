namespace Tools.MigrationHelper.Helpers
{
	/// <summary>
	/// Строковые константы
	/// </summary>
	public static class Names
	{
		/// <summary>
		/// id
		/// </summary>
		public const string Id = "id";
		
		/// <summary>
		/// idEntity
		/// </summary>
		public const string IdEntity = "idEntity";
		
		/// <summary>
		/// Entity
		/// </summary>
		public const string Entity = "Entity";
		
		/// <summary>
		/// ref
		/// </summary>
		public const string EntitySchema = "ref";
		
		/// <summary>
		/// EntityField
		/// </summary>
		public const string EntityField = "EntityField";
		
		/// <summary>
		/// ref
		/// </summary>
		public const string EntityFieldSchema = "ref";
		
		/// <summary>
		/// idEntityFieldType
		/// </summary>
		public const string IdEntityFieldType = "idEntityFieldType";
		
		/// <summary>
		/// Name
		/// </summary>
		public const string Name = "Name";
		
		/// <summary>
		/// idEntityType
		/// </summary>
		public const string IdEntityType = "idEntityType";
		
		/// <summary>
		/// tstamp
		/// </summary>
		public const string Tstamp = "tstamp";
		
		/// <summary>
		/// idEntityLink
		/// </summary>
		public const string IdEntityLink = "idEntityLink";
		
		/// <summary>
		/// Caption
		/// </summary>
		public const string Caption = "Caption";
		
		/// <summary>
		/// Description
		/// </summary>
		public const string Description = "Description";
		
		/// <summary>
		/// isVersioning
		/// </summary>
		public const string IsVersioning = "isVersioning";

		/// <summary>
		/// idCalculatedFieldType
		/// </summary>
		public const string IdCalculatedFieldType = "idCalculatedFieldType";

		/// <summary>
		/// GenerateEntityClass
		/// </summary>
		public const string GenerateEntityClass = "GenerateEntityClass";

		/// <summary>
		/// DbStructure
		/// </summary>
		public const string DbStructure = "DbStructure";
		
		/// <summary>
		/// idProject
		/// </summary>
		public const string IdProject = "idProject";
		
		/// <summary>
		/// DbEnums
		/// </summary>
		public const string DbEnums = "DbEnums";
		
		/// <summary>
		/// ReadOnly
		/// </summary>
		public const string ReadOnly = "ReadOnly";
		
		/// <summary>
		/// Filter
		/// </summary>
		public const string Filter = "Filter";
		
		/// <summary>
		/// ref
		/// </summary>
		public const string FilterSchema = "ref";
		
		/// <summary>
		/// ToDo: Данный список следует сформировать в PlatformDb, убрав копипаст в 
		/// </summary>
		public static string[] PrimaryEnums = new[] { "EntityType", "EntityFieldType", "SolutionProject", "ForeignKeyType", "CalculatedFieldType", "ComparisionOperator", "LogicOperator", "ProgrammabilityType" };

		/// <summary>
		/// ref.Entity
		/// </summary>
		public static string RefEntity
		{
			get { return Names.EntitySchema + "." + Names.Entity; }
		}

		/// <summary>
		/// ref.EntityField
		/// </summary>
		public static string RefEntityField
		{
			get { return Names.EntityFieldSchema + "." + Names.EntityField; }
		}

		/// <summary>
		/// ref.Filter
		/// </summary>
		public static string RefFilter
		{
			get { return Names.FilterSchema + "." + Names.Filter; }
		}
	}
}
