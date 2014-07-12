using System;
using System.Collections.Generic;
using System.Linq;
using Platform.PrimaryEntities.Common.DbEnums;
using Platform.PrimaryEntities.Common.Interfaces;
using Platform.PrimaryEntities.Reference;
using Tools.MigrationHelper.Helpers;

namespace Tools.MigrationHelper.Core.Generators.EntityClasses
{
    partial class MapGenerator
    {
        private readonly List<IEntityField> _fields;
        private readonly Entity _entity;
        private readonly string _namespace;
        private readonly List<Entity> _entities;
        private readonly List<IEntityField> _multilinkFields;

        public List<Entity> MappedMultiLinks{get; set;}

        /// <summary>
        /// Конструктор для T4 генератора
        /// </summary>
        /// <param name="entity">Сущность для которой генерируется маппинг</param>
        /// <param name="Namespace"></param>
        /// <param name="entities">Все сущности</param>
        /// <param name="multilinkFields">Поля сущности мультилинка которые ссылаются на данную сущность</param>
        /// GetMultiLinkRelationship
        public MapGenerator(Entity entity, string Namespace, List<Entity> entities, List<IEntityField> multilinkFields)
        {
            _fields = entity.Fields.ToList();
            _entity = entity;
            _namespace = Namespace;
            _entities = entities;
            _multilinkFields = multilinkFields;
        }

        public string GetLinkedFieldName(EntityField field)
        {
            //получаем сущность на которую ссылается поле
            var linkedEntity = _entities.FirstOrDefault(w => w.Id == field.IdEntityLink);
            if (linkedEntity == null)
                return "";

            var linkedFields = linkedEntity.Fields.Where(f =>
            f.IdEntityLink == _entity.Id
            && f.EntityFieldType != EntityFieldType.Link
            && f.IdOwnerField == field.Id).ToList();

            EntityField linkedField = null;

            if (linkedFields.Count == 1)
                linkedField = (EntityField) linkedFields.First();
            if (linkedFields.Count > 1)
            {
                linkedField = (EntityField)linkedFields.FirstOrDefault(f=> f.EntityFieldType != EntityFieldType.VirtualTablePart);
            }

            if (linkedField != null)
                return "t => t." + GeneratorHelper.TablePartPropertyName(linkedField);

            //для иерархии
            if (linkedEntity.Id == _entity.Id)
            {
                return "t => t." + GeneratorHelper.GetChildrenCollectionName(field);
            }

            //для зависимых ТЧ
            if (linkedEntity.EntityType == EntityType.Tablepart && _entity.EntityType == EntityType.Tablepart)
            {
                return "t => t." + GeneratorHelper.GetTpCollectionName(_entity.Name);
            }

            return "";
        }

        public string GetPrimaryKey()
        {
            if (_entity.EntityType == EntityType.Multilink)
            {
                return "t => new { t." + GeneratorHelper.FirstToUp(_fields[0].Name) + ", t." + GeneratorHelper.FirstToUp(_fields[1].Name) + "}";
            }
            if (_entity.EntityType == EntityType.Registry && _fields.All(a => a.Name.ToLower() != "id"))
            {
                return string.Format("{0}.PropertyAccessExpressions.Id", GeneratorHelper.GetEntityName(_entity));
            }
            return "t => t.Id";
        }

        public List<string> GetMultiLinkRelationship()
        {
            var list = new List<string>();

            foreach (EntityField field in _multilinkFields)
            {
                //сущность мультиссылки
                var multiLinkEntity = GeneratorHelper.GetEntity(field.IdEntity, _entities);

                if(MappedMultiLinks.Contains(multiLinkEntity))
                    continue;

                //сущность с которой связываемся мультиссылкой
                var leftEntity = GeneratorHelper.GetLinkedEntity(multiLinkEntity, _entities, _entity);

                if (leftEntity == null) 
                    continue;

                var flag = leftEntity.Fields.Select(s => s.IdEntityLink).Contains(multiLinkEntity.Id);

                //имя столбца мультилинка который ссылается на данную сущность
                var mapRightKey = multiLinkEntity.Fields.FirstOrDefault(f => f.IdEntityLink == _entity.Id);
                CheckField(mapRightKey, multiLinkEntity.Name, _entity.Name);
                //имя стобца мультилинка который ссылкается на другую сущность
                var mapLeftKey = multiLinkEntity.Fields.FirstOrDefault(f => f.IdEntityLink == leftEntity.Id);
                CheckField(mapLeftKey, multiLinkEntity.Name, leftEntity.Name);

                var mapLeftKeyName = mapLeftKey.Name;
                var mapRightKeyName = mapRightKey.Name;

                //если у сущности нет поля ссылающегося на данный мультилинк то называем свойство именем мультилинка
                var rightField = leftEntity.Fields.FirstOrDefault(f => f.IdEntityLink == multiLinkEntity.Id);
                var rightFieldName = rightField == null ? multiLinkEntity.Name : rightField.Name;

                var leftField = _fields.FirstOrDefault(f => f.IdEntityLink == multiLinkEntity.Id);
                var leftFieldName = leftField == null ? multiLinkEntity.Name : leftField.Name;

                list.Add(string.Format(
                    "this.HasMany(t => t.{0}).WithMany({1}).Map(m => m.MapLeftKey(\"{2}\").MapRightKey(\"{3}\").ToTable(\"{4}\", \"ml\"));"
                    , GeneratorHelper.MultilinkPropertyName(leftFieldName)
                    , leftEntity.Name != Names.Entity && leftEntity.Name != Names.EntityField && flag ? "r => r." + GeneratorHelper.MultilinkPropertyName(rightFieldName) : "" 
                    , mapRightKeyName
                    , mapLeftKeyName
                    , multiLinkEntity.Name));

                MappedMultiLinks.Add(multiLinkEntity);
            }

            return list;
        }

        private void CheckField(IEntityField field, string multilinkName, string entityName)
        {
            if (field == null)
                throw new Exception(
                    string.Format("Ошибка генерации кода! У мультилинка {0} отсутсвует поле со ссылкой на сущность {1}",
                                  multilinkName, entityName));
        }

        public List<string> GetRelationship()
        {
            var list = new List<string>();
            var fields =
                _fields.Where(
                    w =>
                    w.IdCalculatedFieldType == null && w.EntityFieldType == EntityFieldType.Link &&
                    _entities.Any(
                        a =>
                        a.Id == w.IdEntityLink && a.IdEntityType != (int)EntityType.Enum &&
                        (a.GenerateEntityClass || GeneratorHelper.GetPrimaryProjects().Contains(a.IdProject))));

            foreach (EntityField field in fields)
            {
                list.Add(string.Format("this.{0}(t => t.{1}).WithMany({2}).HasForeignKey(d => d.{3});"
                                       , (field.AllowNull ? "HasOptional" : "HasRequired")
                                       , GeneratorHelper.ModifyName(field.Name)
                                       , GetLinkedFieldName(field)
                                       , GeneratorHelper.FirstToUp(field.Name)));
            }
            return list;
        }

        public List<string> GetProperties()
        {
            var list = new List<string>();
            var fields =
				_fields.Where(w => (w.IdCalculatedFieldType == null || w.IdCalculatedFieldType == (byte)CalculatedFieldType.DbComputed || w.IdCalculatedFieldType == (byte)CalculatedFieldType.DbComputedPersisted)
                    && !new[]
						{
							EntityFieldType.Multilink, EntityFieldType.Tablepart, EntityFieldType.DataEndpoint, EntityFieldType.VirtualTablePart
						}.Contains(w.EntityFieldType));

            foreach (EntityField field in fields)
            {
                var additionalOption = string.Empty;

                if (field.IdEntityFieldType == (byte)EntityFieldType.Numeric || field.IdEntityFieldType == (byte)EntityFieldType.Money)
                    additionalOption = string.Format(".HasPrecision({0},{1})", field.Size, field.Precision);
				if (field.IsDbComputed)
                    additionalOption = ".HasDatabaseGeneratedOption(DatabaseGeneratedOption.Computed)";
                
                list.Add(
                    string.Format("this.Property(t => t.{0}){2}.HasColumnName(\"{1}\");"
                    , GeneratorHelper.FirstToUp(field.Name)
                    , field.Name
                    , additionalOption));
            }

            if (_fields.All(a => a.Name.ToLower() != "id"))
                list.Add(string.Format("this.Property({0}.PropertyAccessExpressions.Id).HasColumnName(\"id\");"
                    , GeneratorHelper.GetEntityName(_entity)));
            return list;
        }


    }
}
