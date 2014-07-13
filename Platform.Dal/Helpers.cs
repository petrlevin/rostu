using Microsoft.Data.Schema.ScriptDom.Sql;
using Platform.PrimaryEntities.Common.DbEnums;
using Platform.PrimaryEntities.Common.Interfaces;

namespace Platform.Dal
{
	public static class Helpers
	{
		public static QualifiedJoinType GetQualifiedJoinType(this IEntityField entityField)
		{
			return (entityField.AllowNull || entityField.Entity.EntityType == EntityType.Report)
				       ? QualifiedJoinType.LeftOuter
				       : QualifiedJoinType.Inner;
		}
	}
}