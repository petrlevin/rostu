using System;
using Platform.PrimaryEntities.Common.DbEnums;

namespace Platform.PrimaryEntities
{
	public class Schemas
	{
        /// <summary>
        /// Получить имя схемы по типу сущности
        /// </summary>
        public static string ByEntityType(Byte entityType)
        {
            return ByEntityType((EntityType) entityType);
        }

		/// <summary>
		/// Получить имя схемы по типу сущности
		/// </summary>
		public static string ByEntityType(EntityType entityType)
		{
			switch (entityType)
			{
				case EntityType.Enum:
					return "enm";
				case EntityType.Reference:
					return "ref";
				case EntityType.Tablepart:
					return "tp";
                case EntityType.Multilink:
					return "ml";
                case EntityType.Document:
					return "doc";
                case EntityType.Tool:
					return "tool";
                case EntityType.Registry:
					return "reg";
                case EntityType.Report:
					return "rep";
			}
			return null;
		}

	    /// <summary>
	    /// Получить тип сущности по имени схемы
	    /// </summary>
	    /// <param name="schema"></param>
	    /// <returns></returns>
	    public static EntityType? EntityTypeBySchema(string schema)
	    {
            switch (schema)
            {
                case "enm":
                    return EntityType.Enum;
                case "ref":
                    return EntityType.Reference;
                case "tp":
                    return EntityType.Tablepart;
                case "ml":
                    return EntityType.Multilink;
                case "doc":
                    return EntityType.Document;
                case "tool":
                    return EntityType.Tool;
                case "reg":
                    return EntityType.Registry;
                case "rep":
                    return EntityType.Report;
            }
            return null;
	    }
	}
}
