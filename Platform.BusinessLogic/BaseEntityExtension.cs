using System;
using System.Data.Objects;
using Platform.BusinessLogic.EntityTypes.Interfaces;
using Platform.BusinessLogic.Interfaces;
using Platform.PrimaryEntities;
using Platform.PrimaryEntities.Common.DbEnums;
using Platform.PrimaryEntities.Common.Interfaces;

namespace Platform.BusinessLogic
{
    /// <summary>
	/// Расширения для классов реализующий интерфейс IBaseEntity
    /// </summary>
	public static class BaseEntityExtension
    {
        /// <summary>
        /// Получить тип сущности для сущности
        /// </summary>
        /// <param name="baseEntity"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        static public EntityType GetEntityType(this IBaseEntity baseEntity)
        {
            if (baseEntity is IDocumentEntity)
                return EntityType.Document;
            if (baseEntity is IToolEntity)
                return EntityType.Tool;
            if (baseEntity is IReportEntity)
                return EntityType.Report;
            if (baseEntity is IReferenceEntity)
                return EntityType.Reference;
            if (baseEntity is IMultilinkEntity)
                return EntityType.Multilink;
            if (baseEntity is ITablePartEntity)
                return EntityType.Tablepart;
            throw  new InvalidOperationException(String.Format("Не возможно определить тип сущности для {0}",baseEntity));
        }

        /// <summary>
        /// Получить полное имя таблицы со схемой (например [ref].[SomeEntity])
        /// </summary>
        /// <param name="baseEntity"></param>
        /// <returns></returns>
        static public string GetFullTableName(this IBaseEntity baseEntity)
        {
            return String.Format("[{0}].[{1}]", Schemas.ByEntityType(GetEntityType(baseEntity)), ObjectContext.GetObjectType(baseEntity.GetType()).Name);
        }

    }
}
