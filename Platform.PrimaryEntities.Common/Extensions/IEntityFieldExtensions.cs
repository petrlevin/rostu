using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Platform.PrimaryEntities.Common.DbEnums;
using Platform.PrimaryEntities.Common.Interfaces;

namespace Platform.PrimaryEntities.Common.Extensions
{
	/// <summary>
	/// Доп. методы для полей сущности
	/// </summary>
	public static class IEntityFieldExtensions
	{
		/// <summary>
		/// Является ли поле ссылочным: ссылка или общая ссылка
		/// </summary>
		/// <param name="field"></param>
		/// <returns></returns>
		public static bool IsLinkField(this IEntityField field)
		{
			return
				field.EntityFieldType == EntityFieldType.Link
				|| field.EntityFieldType == EntityFieldType.FileLink
				|| field.EntityFieldType == EntityFieldType.ReferenceEntity
				|| field.EntityFieldType == EntityFieldType.DocumentEntity
				|| field.EntityFieldType == EntityFieldType.TablepartEntity
				|| field.EntityFieldType == EntityFieldType.ToolEntity;
		}

        /// <summary>
        /// Может ли быть в БД у этого поля внешний ключ
        /// </summary>
        /// <param name="field"></param>
        /// <returns></returns>
        public static bool IsFieldWithForeignKey(this IEntityField field)
	    {
	        return
	            field.EntityFieldType == EntityFieldType.Link
	            || field.EntityFieldType == EntityFieldType.FileLink;
	    }

	    /// <summary>
		/// Определяет является ли данное поле  общей ссылкой
		/// </summary>
		public static bool IsCommonReference(this IEntityField entityField)
		{
			return (entityField.EntityFieldType == EntityFieldType.TablepartEntity)
				   || (entityField.EntityFieldType == EntityFieldType.ToolEntity)
				   || (entityField.EntityFieldType == EntityFieldType.DocumentEntity)
				   || (entityField.EntityFieldType == EntityFieldType.ReferenceEntity);

		}
	}
}
