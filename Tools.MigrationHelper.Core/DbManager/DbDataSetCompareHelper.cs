using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Platform.PrimaryEntities.Common.DbEnums;
using Platform.PrimaryEntities.Reference;
using Tools.MigrationHelper.Helpers;

namespace Tools.MigrationHelper.Core.DbManager
{
    public static class DbDataSetCompareHelper
    {
        public static int? ToNullableInt32(string s)
        {
            int i;
            if (Int32.TryParse(s, out i)) return i;
            return null;
        }

        /// <summary>
        /// Проверка на равенство идентификаторов
        /// </summary>
        /// <param name="id1">object типа Int32</param>
        /// <param name="id2">object типа Int32</param>
        /// <returns>истина или ложь</returns>
        public static bool IsEqualId(object id1, object id2)
        {
            return Convert.ToInt32(id1) == Convert.ToInt32(id2);
        }

        /// <summary>
        /// Сортировка сущностей в порядке зависимости (начиная от сущностей, на которые никто не ссылается)
        /// </summary>
        /// <param name="dataSet">Источник, откуда берется таблица Entity</param>
        /// <returns>Отсортированный список идентификаторов сущностей</returns>
        public static List<int> OrderEntityForDelete(DataSet dataSet)
        {
            List<int> result = (from DataRow entityRow in dataSet.Tables[Names.RefEntity].Rows let existLinkedEntity = dataSet.Tables[Names.RefEntityField].AsEnumerable().Any(a => a.Field<int>(Names.IdEntity) != entityRow.Field<int>(Names.Id) && a.Field<byte>(Names.IdEntityFieldType) == (byte)EntityFieldType.Link && a.Field<int>(Names.IdEntityLink) == entityRow.Field<int>(Names.Id)) where !existLinkedEntity select entityRow.Field<int>(Names.Id)).ToList();
            result = GetReferencedEntity(dataSet, result);
            return result;
        }

        /// <summary>
        /// Возвращает идентифкаторы сущностей, на которые ссылаются поля сущностей из списка listEntityId
        /// </summary>
        /// <param name="dataSet">Источник, откуда берется таблица Entity</param>
        /// <param name="listEntityId">Список идентифкаторов сущностей</param>
        /// <returns></returns>
        private static List<int> GetReferencedEntity(DataSet dataSet, List<int> listEntityId)
        {
            var added = new List<int>();
            var result = new List<int>();

            foreach (DataRow entityRow in dataSet.Tables[Names.RefEntity].Rows)
            {
                if (listEntityId.Contains(entityRow.Field<int>(Names.Id)))
                    continue;
                //Смотрим, есть ли сущности которые ссылаются на данную сущность и при этом которых нет в списке
                var sdf =
                    dataSet.Tables[Names.RefEntityField]
                        .AsEnumerable().Any(
                            w =>
                            w.Field<byte>(Names.IdEntityFieldType) == (byte)EntityFieldType.Link
                            && w.Field<int>(Names.IdEntity) != entityRow.Field<int>(Names.Id)
                            && !listEntityId.Contains(w.Field<int>(Names.IdEntity))
                            && w.Field<int>(Names.IdEntityLink) == entityRow.Field<int>(Names.Id)
                            );
                if (!sdf)
                    added.Add(entityRow.Field<int>(Names.Id));
            }
            result.AddRange(listEntityId);
            result.AddRange(added);
            if (added.Count > 0)
            {
                result = GetReferencedEntity(dataSet, result);
            }
            return result;
        }

        /// <summary>
        /// Сортировка сущностей в порядке зависимости (начиная от сущностей, которые ни на кого не ссылаются)
        /// </summary>
        /// <param name="dataSet">Источник, откуда берется таблица Entity</param>
        /// <returns>Отсортированный список идентификаторов сущностей</returns>
        public static List<int> OrderEntityForInsert(DataSet dataSet)
        {
            List<int> result = dataSet.Tables[Names.RefEntity].AsEnumerable().Where(
                    a =>
                    dataSet.Tables[Names.RefEntityField].AsEnumerable().Where(
                                    b => b.Field<int>(Names.IdEntity) == a.Field<int>(Names.Id)
                                            && (b.Field<byte>(Names.IdEntityFieldType) == (byte)EntityFieldType.Link || b.Field<byte>(Names.IdEntityFieldType) == (byte)EntityFieldType.FileLink))
                                            .All(c => !c.Field<int?>(Names.IdEntityLink).HasValue))
                    .Select(a => a.Field<int>(Names.Id)).Distinct().ToList();
            result = GetLinkedEntity(dataSet, result).ToList();
            return result;
        }

        /// <summary>
        /// Возвращает идентифкаторы сущностей, в которых поля ссылаются только на сущности из списка listEntityId
        /// </summary>
        /// <param name="dataSet">Источник, откуда берется таблица Entity</param>
        /// <param name="listEntityId">Список идентифкаторов сущностей</param>
        /// <returns></returns>
        private static IEnumerable<int> GetLinkedEntity(DataSet dataSet, List<int> listEntityId)
        {
            List<DataRow> entitis =
                dataSet.Tables[Names.RefEntity].AsEnumerable().Where(a => !listEntityId.Contains(a.Field<int>(Names.Id))).
                    ToList();
            var result = new List<int>();
            List<int> added = (from entity in entitis
                               let exist = dataSet.Tables[Names.RefEntityField].AsEnumerable().Count(b => b.Field<int>(Names.IdEntity) == entity.Field<int>(Names.Id) && b.Field<byte>(Names.IdEntityFieldType) == (byte)EntityFieldType.Link && b.Field<int?>(Names.IdEntityLink).HasValue && (listEntityId.Contains(b.Field<int>(Names.IdEntityLink)) || b.Field<int>(Names.IdEntityLink) == entity.Field<int>(Names.Id)))
                               let notExist = dataSet.Tables[Names.RefEntityField].AsEnumerable().Count(b => b.Field<int>(Names.IdEntity) == entity.Field<int>(Names.Id) && b.Field<byte>(Names.IdEntityFieldType) == (byte)EntityFieldType.Link && b.Field<int?>(Names.IdEntityLink).HasValue && b.Field<int>(Names.IdEntityLink) != entity.Field<int>(Names.Id) && !listEntityId.Contains(b.Field<int>(Names.IdEntityLink)))
                               where exist > 0 && notExist == 0
                               select entity.Field<int>(Names.Id)).ToList();
            result.AddRange(listEntityId);
            result.AddRange(added);
            if (added.Count > 0)
            {
                result = GetLinkedEntity(dataSet, result).ToList();
            }
            return result;
        }

        /// <summary>
        /// Получение имени таблицы по идентификатору
        /// </summary>
        /// <remarks>
        /// Получаю не из ДатаСорса потому что так намного быстрее
        /// </remarks>
        /// <param name="id"></param>
        /// <returns></returns>
        public static string TableNameById(int id)
        {
            if (Entity.EntityIdStatic == id)
                return Names.RefEntity;
            if (EntityField.EntityIdStatic == id)
                return Names.RefEntityField;
            if (Programmability.EntityIdStatic == id)
                return Names.RefProgrammability;

            throw new Exception(
                string.Format(
                    "Здесь получаем имя таблицы по идентификатору для зависимостей между объектами, для идентификатора '{0}' таблицы не нашлось",
                    id));
        }
    }
}
