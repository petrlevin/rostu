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
		/// DbStructure
		/// </summary>
        public const string AllowGenericLinks = "AllowGenericLinks";

        
		
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
		/// dbo.DevDbRevision
		/// </summary>
		public const string DevDbRevision = "dbo.DevDbRevision";

		/// <summary>
		/// ref
		/// </summary>
		public const string Ref = "ref";

        /// <summary>
        /// reg
        /// </summary>
        public const string Reg = "reg";

		/// <summary>
		/// ml
		/// </summary>
		public const string Ml = "ml";

		/// <summary>
		/// Index
		/// </summary>
		public const string Index = "Index";

        /// <summary>
        /// ItemsDependencies
        /// </summary>
        public const string ItemsDependencies = "ItemsDependencies";

        /// <summary>
        /// UpdateRevision
        /// </summary>
        public const string UpdateRevision = "UpdateRevision";

        /// <summary>
        /// idObject
        /// </summary>
        public const string IDObject = "idObject";

        /// <summary>
        /// idObjectEntity
        /// </summary>
        public const string IDObjectEntity = "idObjectEntity";

        /// <summary>
        /// idDependsOn
        /// </summary>
        public const string IDDependsOn = "idDependsOn";

        /// <summary>
        /// idDependsOnEntity
        /// </summary>
        public const string IDDependsOnEntity = "idDependsOnEntity";

		/// <summary>
		/// Index_EntityField
		/// </summary>
		public const string IndexEntityField = "Index_EntityField_Indexable";

	    /// <summary>
	    /// Index_EntityField
	    /// </summary>
	    public const string IncludedEntityField = "Index_EntityField_Included";

        /// <summary>
        /// Programmability
        /// </summary>
        public const string Programmability = "Programmability";

		/// <summary>
		/// ToDo: Данный список следует сформировать в PlatformDb, убрав копипаст в 
		/// </summary>
		public static string[] PrimaryEnums = new[] { "EntityType", "EntityFieldType", "SolutionProject", "ForeignKeyType", "CalculatedFieldType", "ComparisionOperator", "LogicOperator", "ProgrammabilityType", "IndexType", "FieldDefaultValueType", "RefStatus" };

		/// <summary>
		/// Список файлов которые необходимо скопировать в папку с дистбутивом необходимых для выполнения тасков MigrationHelper
		/// </summary>
		public static string[] FilesCopyToDistr = new []{"nant.build", "mh.xml","DeployDb.bat", "ToFs.bat", "Update.bat", "CheckDatabase.bat"};

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

		/// <summary>
		/// ref.Index
		/// </summary>
		public static string RefIndex
		{
			get { return Names.Ref + "." + Names.Index; }
		}

		/// <summary>
		/// ref.Index
		/// </summary>
		public static string MlIndexEntityField
		{
			get { return Names.Ml + "." + Names.IndexEntityField; }
		}
		/// <summary>
		/// ref.Index
		/// </summary>
		public static string MlIncludedEntityField
		{
			get { return Names.Ml + "." + Names.IncludedEntityField; }
		}

        /// <summary>
        /// ref.Programmability
        /// </summary>
        public static string RefProgrammability
        {
            get { return Names.Ref + "." + Names.Programmability; }
        }

        /// <summary>
        /// reg.ItemsDependencies
        /// </summary>
        public static string RegItemsDependencies
        {
            get { return Names.Reg + "." + Names.ItemsDependencies; }
        }

        /// <summary>
        /// reg.UpdateRevision
        /// </summary>
        public static string RegUpdateRevision
        {
            get { return Names.Reg + "." + Names.UpdateRevision; }
        }

        /// <summary>
        /// idEntityField
        /// </summary>
        public const string IDEntityField = "idEntityField";

        /// <summary>
        /// idIndex
        /// </summary>
        public const string IdIndex = "idIndex";

        /// <summary>
        /// enm
        /// </summary>
        public const string Enm = "enm";
	}
}
