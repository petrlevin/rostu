using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Platform.PrimaryEntities.Common.DbEnums;
using Platform.PrimaryEntities.Common.Interfaces;
using Platform.PrimaryEntities.DbEnums;
using Platform.PrimaryEntities.Reference;
using Platform.Utils.Extensions;
using Tools.MigrationHelper.Helpers;

namespace Tools.MigrationHelper.Core.Generators
{
    /// <summary>
    /// Общие методы для генерации сущностных классов
    /// </summary>
    public class GeneratorHelper
    {
        public static string GetEntityName(Entity entity)
        {
            return entity.Name;
        }

        /// <summary>
        /// Возвращает полное имя сущностного класса (вместе с пространством имен)
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public static string GetFullClassName(Entity entity)
        {
            return new[]
                {
                    SolutionHelper.GetProjectName(entity.IdProject),
                    entity.EntityType.ToString(),
                    entity.Name
                }.Aggregate((a, b) => string.Format("{0}.{1}", a, b));
        }

        public static string ModifyName(string str)
        {
            return char.ToUpper(str[2]) + str.Substring(3);
        }

        public static string CommonRefferenceId(string str)
        {
            return String.Format("{0}Id", ModifyName(str));
        }

        public static string FirstToUp(string str)
        {
            return str.FirstUpper();
        }

        public static string MultilinkPropertyName(string mlField)
        {
            return FirstToUp(Regex.Replace(mlField, "^ml", "", RegexOptions.IgnoreCase));
        }

        public static string TablePartPropertyName(IEntityField mlField)
        {
            return FirstToUp(Regex.Replace(mlField.Name, "^tp", "", RegexOptions.IgnoreCase));
        }

        public static Entity GetEntity(int? idEntity, List<Entity> entities)
        {
            if (idEntity == null)
                return null;
            var entity = entities.FirstOrDefault(w => w.Id == idEntity);
            return entity;
        }

        /// <summary>
        /// Получение сущности с которой связаны по средством мультилинка
        /// </summary>
        /// <remarks>
        /// Если у второй сущности не стоит признак генерации класса, то возвращаем null
        /// </remarks>
        /// <param name="multilinkEntity">Сущность мультилинка</param>
        /// <param name="entities">Все сущности</param>
        /// <param name="entity">Текущая сущность</param>
        /// <returns>Возвращает экземпляр Entity</returns>
        public static Entity GetLinkedEntity(Entity multilinkEntity, List<Entity> entities, Entity entity)
        {
            //Находим поле у мультиссылки которое ссылается на другую сущность
            if (multilinkEntity != null)
            {
                var field =
                    multilinkEntity.Fields.FirstOrDefault(f => f.IdEntityLink != null && f.IdEntityLink != entity.Id);
                if (field == null)
                    throw new Exception(
                        string.Format(
                            "Ошибка генерации сущностного класса! У мультиссылки {0} есть ссылка на сущность {1}, при этом отсутствует ссылка на другую сущность",
                            multilinkEntity.Name, entity.Name));
                var idLinkedEntityInMl = field.IdEntityLink;
                var result = entities.FirstOrDefault(w => w.Id == idLinkedEntityInMl && w.EntityType != EntityType.Enum && (w.GenerateEntityClass || GetPrimaryProjects().Contains(w.IdProject)));
                return result;
            }
            return null;
        }

        /// <summary>
        /// Проекты в которых сущностные классы описаны вручную
        /// </summary>
        /// <returns></returns>
        public static List<int> GetPrimaryProjects()
        {
            var list = new List<int>
                {
                    (int) SolutionProject.Tools_MigrationHelper_Core,
                    (int) SolutionProject.Platform_PrimaryEntities_Common,
                    (int) SolutionProject.Platform_PrimaryEntities
                };
            return list;
        }

        public static string GetChildrenCollectionName(EntityField field)
        {
            return "ChildrenBy" + field.Name;
        }

        public static string GetTpCollectionName(string name)
        {
            return name;
        }

        /// <summary>
        /// Запись файла с созданием пути
        /// </summary>
        /// <param name="path">Пример: C:\example.xml</param>
        /// <param name="text"></param>
        public static void WriteToFile(string path, string text)
        {
            if(string.IsNullOrEmpty(path))
                throw new Exception("Ошибка записи в файл! Не был передан путь файла.");
            var file = new FileInfo(path);
            if (!Directory.Exists(file.Directory.FullName))
                Directory.CreateDirectory(file.Directory.FullName);
            File.WriteAllText(path, text);
        }
    }
}
