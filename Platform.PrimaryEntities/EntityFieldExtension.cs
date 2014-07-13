using System;
using System.Collections.Generic;
using Platform.PrimaryEntities.Common.DbEnums;
using Platform.PrimaryEntities.Common.Interfaces;

namespace Platform.PrimaryEntities.Reference
{
    /// <summary>
    /// 
    /// </summary>
    public static class EntityFieldExtension
    {
        private static readonly List<EntityFieldType> TableTypes = new List<EntityFieldType>()
		    {
			    EntityFieldType.DataEndpoint,
			    EntityFieldType.Multilink,
			    EntityFieldType.Tablepart,
			    EntityFieldType.VirtualTablePart
		    };

        /// <summary>
        /// Определяет является ли данное поле статусом для справочников
        /// </summary>
        /// <param name="entityField"></param>
        /// <returns></returns>
        public static bool IsRefStatus(this IEntityField entityField)
        {
            return (entityField.Entity.EntityType == EntityType.Reference) && (entityField.EntityLink.Id == Constants.RefStatusId) && (entityField.Name.ToLower() == "idrefstatus");
        }

        /// <summary>
        /// Хранимое в БД поле
        /// </summary>
        /// <param name="f"></param>
        /// <returns></returns>
        public static bool IsRealField(this IEntityField f)
        {
            return !TableTypes.Contains(f.EntityFieldType) &&
                   !(f.IdCalculatedFieldType.HasValue &&
                     (f.CalculatedFieldType == CalculatedFieldType.AppComputed ||
                      f.CalculatedFieldType == CalculatedFieldType.ClientComputed ||
                      f.CalculatedFieldType == CalculatedFieldType.Joined ||
                      f.CalculatedFieldType == CalculatedFieldType.NumeratorExpression ||
					  f.CalculatedFieldType == CalculatedFieldType.DbComputedFunction
                     )
                    );
        }

        /// <summary>
        /// Поле-идентификатор
        /// </summary>
        /// <param name="entityField"></param>
        /// <returns></returns>
        public static bool IsId(this IEntityField entityField)
        {
            return entityField.Name == "id";
        }
    }
}
