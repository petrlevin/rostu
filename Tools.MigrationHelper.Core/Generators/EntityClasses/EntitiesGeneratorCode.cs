using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Platform.PrimaryEntities.Common.DbEnums;
using Platform.PrimaryEntities.Common.Extensions;
using Platform.PrimaryEntities.Common.Interfaces;
using Platform.PrimaryEntities.DbEnums;
using Platform.PrimaryEntities.Factoring;
using Platform.PrimaryEntities.Reference;
using Tools.MigrationHelper.Helpers;

namespace Tools.MigrationHelper.Core.Generators.EntityClasses
{
    partial class EntitiesGenerator
    {
        private readonly Entity _entity;
        private readonly string _namespace;
        private readonly List<Entity> _entities;
        private readonly List<IEntityField> _multilinkFields;

        /// <summary>
        /// Конструктор для T4 генератора
        /// </summary>
        /// <param name="entity">Сущность для которой создается класс</param>
        /// <param name="Namespace">Namespace создаваемого класса</param>
        /// <param name="entities">Коллекция сущностей для полей ссылок</param>
        /// <param name="multilinkFields"></param>
        public EntitiesGenerator(Entity entity, string Namespace, List<Entity> entities, List<IEntityField> multilinkFields)
        {
            _entity = entity;
            _namespace = Namespace;
            _entities = entities;
            _multilinkFields = multilinkFields;
        }

        private string GetIStatus()
        {
            return _entity.IsRefernceWithStatus() ? ", IHasRefStatus" : string.Empty;
        }

        private string GetIVersioning()
        {
            return _entity.IsVersioning ? ", IVersioning" : string.Empty;
        }

        private string GetUseBaseAppInterfaces()
        {
            if (_entity.IdProject < (Int32)SolutionProject.BaseApp)
                return string.Empty;
            return "using BaseApp.Interfaces;";
        }

        private string GetIRegistryWithOperation()
        {
            if (_entity.EntityType != EntityType.Registry)
                return string.Empty;
            if  (_entity.IdProject <(Int32)SolutionProject.BaseApp)
                return string.Empty;
            var entityExecutedOperation = Objects.ByName<Entity>("ExecutedOperation");
            if (
                _entity.Fields.Any(
                    ef => ef.Name.ToLower() == "IdExecutedOperation".ToLower() && ef.EntityFieldType == EntityFieldType.Link && ef.IdEntityLink == entityExecutedOperation.Id ))
                return ", IRegistryWithOperation";

            return string.Empty;
            
        }

        private string GetIRegistryWithTermOperation()
        {
            if (_entity.EntityType != EntityType.Registry)
                return string.Empty;
            if (_entity.IdProject < (Int32)SolutionProject.BaseApp)
                return string.Empty;
            var entityExecutedOperation = Objects.ByName<Entity>("ExecutedOperation");
            if (
                _entity.Fields.Any(
                    ef => ef.Name.ToLower() == "IdTerminateOperation".ToLower() && ef.EntityFieldType == EntityFieldType.Link && ef.IdEntityLink == entityExecutedOperation.Id))
                return ", IRegistryWithTermOperation";

            return string.Empty;

        }


        private string GetIHasRegistrator()
        {
            return GetIHasRegistratorTerminator("Registrator" ,false);
        }

        private string GetIHasTerminator()
        {
            return GetIHasRegistratorTerminator("Terminator" ,true);
        }


        private string GetIHasRegistratorTerminator(string registratorOrTerminator , bool nullable)
        {
            if (_entity.EntityType != EntityType.Registry)
                return string.Empty;
            if (
                _entity.Fields.Any(
                    ef => ef.Name.ToLower() == ("Id" + registratorOrTerminator).ToLower() && ef.EntityFieldType == EntityFieldType.Link && (nullable==ef.AllowNull)))
                return ", IHas" + registratorOrTerminator;
            if (
                _entity.Fields.Any(
                    ef => (ef.Name.ToLower() == ("Id" + registratorOrTerminator).ToLower()) && ef.IsCommonReference() && (nullable == ef.AllowNull)))

                return ", IHasCommon" + registratorOrTerminator;

            return string.Empty;
        }


        private string GetIHierarhy()
        {
            if (_entity.Fields.Any(
                ef =>
                ef.Name.ToLower() == "IdParent".ToLower() && ef.EntityFieldType == EntityFieldType.Link &&
                ef.IdEntityLink == _entity.Id))
                return ", IHierarhy";

            return string.Empty;
        }


        private List<string> GetUsings()
        {
            List<string> result = new List<string>();
            var entityLinkes = _entity.Fields.Where(w => w.IdEntityLink != null).Select(s => s.IdEntityLink ?? -1);
            var entityTypes = _entities.Where(w => entityLinkes.Contains(w.Id)).Select(s => new { s.EntityType, s.IdProject });
            foreach (var entityType in entityTypes)
            {
                if (entityType.EntityType == EntityType.Enum && entityType.IdProject == (int)SolutionProject.Tools_MigrationHelper_Core)
                {
                    var addStr = "using " + SolutionHelper.GetProjectName(entityType.IdProject == (int)SolutionProject.Tools_MigrationHelper_Core ? (int)SolutionProject.Platform_PrimaryEntities : entityType.IdProject) + "." + "DbEnums" + ";";

                    if (!result.Contains(addStr))
                    {
                        result.Add(addStr);
                    }
                    var addStr2 = "using " + SolutionHelper.GetProjectName(entityType.IdProject == (int)SolutionProject.Tools_MigrationHelper_Core ? (int)SolutionProject.Platform_PrimaryEntities_Common : entityType.IdProject) + "." + "DbEnums" + ";";
                    if (!result.Contains(addStr2))
                    {
                        result.Add(addStr2);
                    }
                    continue;
                }
                if (entityType.IdProject == (int)SolutionProject.Tools_MigrationHelper_Core)
                {
                    var addStr = "using " + SolutionHelper.GetProjectName((int)SolutionProject.Platform_PrimaryEntities) + "." + entityType.EntityType + ";";
                    if (!result.Contains(addStr))
                    {
                        result.Add(addStr);
                    }
                }
            }
            result = result.Distinct().Except(new[] { "Platform.PrimaryEntities.Reference" }).ToList();

            return result;
        }

        /// <summary>
        /// Возвращает код свойства соответствующий полю сущности <paramref name="field"/>.
        /// Модификаторы (public, virtual, ...), тип, имя поля, аттрибуты (NotMapped, ...), get, set.
        /// </summary>
        /// <param name="field"></param>
        /// <returns></returns>
        public string GetProperty(EntityField field)
        {
            var linkedEntity = GeneratorHelper.GetEntity(field.IdEntityLink, _entities);
            var fieldName = GeneratorHelper.FirstToUp(field.Name);

            switch (field.EntityFieldType)
            {
                case EntityFieldType.Bool: 
                    return GetFieldString("bool", field);
                case EntityFieldType.String: 
                    return GetFieldString("string", field);
                case EntityFieldType.Int:
                    {
                        if (fieldName == "Id" && _entity.IdEntityType != (byte) EntityType.Registry)
                            return @"
        public override Int32 Id{get; set;}";

                        return "public " +
                               string.Format("{0} {1}", NullableFieldType("Int32", field.ColumnAllowNull), fieldName) +
                               "{get; set;}";
                    }
                case EntityFieldType.File: return GetFieldString("byte[]", field);
                case EntityFieldType.BigInt: return GetFieldString("System.Int64", field);
                
                case EntityFieldType.Numeric: 
                case EntityFieldType.Money: 
                                            return GetFieldString("decimal", field);
                case EntityFieldType.Date:
                case EntityFieldType.DateTime: 
                        return GetDateFieldString("DateTime", field);
                case EntityFieldType.FileLink:
                        return GetFieldString("Int32", field);
                case EntityFieldType.Link:
                    if (linkedEntity != null)
                    {
                        //перечисления
                        if (linkedEntity.IdEntityType == (byte) EntityType.Enum)
                        {
                            string type = linkedEntity.Fields.First(f => f.Name.ToLower() == "id").EntityFieldType == EntityFieldType.Int ? "int" : "byte";

                            return GetFieldString(type, field)
                                   + @"
                            /// <summary>
                            /// " + field.Caption + @"
                            /// </summary>
							[NotMapped] 
                            public virtual " + NullableFieldType(GetType(linkedEntity), !field.IsRefStatus() && field.ColumnAllowNull) + " " +
                                   (field.IsRefStatus() ? "RefStatus" : GeneratorHelper.ModifyName(fieldName)) +
                                   @" {
								get { return (" + NullableFieldType(GetType(linkedEntity), field.ColumnAllowNull) + ")this." + fieldName + @"; } 
								set { this." + fieldName + " = (" + NullableFieldType(type, field.ColumnAllowNull) + @") value; }
							}";

                        }

                        // если ссылка на самого себя
                        string childrenCollection = string.Empty;
                        if (linkedEntity.Id == _entity.Id)
                            childrenCollection = GetCollectionString(GetType(linkedEntity), GeneratorHelper.GetChildrenCollectionName(field), "_" + field.Name, field.Caption);

                        //не создаем ссылочное свойство если сущность на которую надо ссылаться не генерируемая
                        return GetFieldString("int", field, linkedEntity)
                               +
                               (((_entity.IdEntityType == (byte)EntityType.Tool
                               || _entity.IdEntityType == (byte)EntityType.Document)
                               && linkedEntity.Name == "DocStatus")
                               || (!linkedEntity.GenerateEntityClass && !GeneratorHelper.GetPrimaryProjects().Contains(linkedEntity.IdProject))
                               || (field.IdCalculatedFieldType != null)
                                    ? ""
                                    : @"
        /// <summary>
	    /// " + field.Caption + @"
	    /// </summary>
		public virtual " + GetType(linkedEntity) + " " + GeneratorHelper.ModifyName(fieldName) + @"{get; set;}
		")
                                + childrenCollection;
                    }
                    return "";

                case EntityFieldType.Multilink: return "\\мультилинки описываются ниже, если видете этот текст, нехорошо:)";
                case EntityFieldType.Tablepart: return GetTablePartString(field, linkedEntity);
                case EntityFieldType.DataEndpoint: return GetCollection(field, linkedEntity);
                case EntityFieldType.Guid: return GetFieldString("Guid", field);
                case EntityFieldType.TinyInt: return GetFieldString("byte", field);
                case EntityFieldType.SmallInt: return GetFieldString("Int16", field);
                case EntityFieldType.Text: return GetFieldString("string", field);
                case EntityFieldType.VirtualTablePart: 
                    {
                        if(!_entity.Fields.Any(a=> a.Id != field.Id && a.IdEntityLink == field.IdEntityLink) && _entity.EntityType != EntityType.Tablepart)
                            return GetTablePartString(field, linkedEntity);
                        return "";
                    }
                case EntityFieldType.Xml:
                    return string.Format(@"[Column(TypeName=""xml"")] 
		{0}
		[NotMapped]
		public XDocument {1}Wrapper 
		{{
			get {{ return XDocument.Parse({1}); }}
			set {{ {1} = value.ToString(); }}
		}}", GetFieldString("string", field), fieldName);

                case EntityFieldType.ReferenceEntity:
                case EntityFieldType.TablepartEntity:
                case EntityFieldType.ToolEntity:
                case EntityFieldType.DocumentEntity:
                    {
                        return GetFieldString("int", field);
                    }
                default:
                    return "/*Ошибка при генерации: тип сущности " + field.IdEntityFieldType + "*/";
            }
        }

        public string GetEntityType(Entity entity)
        {
            switch (entity.IdEntityType)
            {
                case 3: return " : ReferenceEntity";
                case 4: return " : TablePartEntity";
                case 5: return " : MultilinkEntity";
                case 6: return string.Format(" : DocumentEntity<{0}>", GeneratorHelper.GetEntityName(entity));
                case 7: return string.Format(" : ToolEntity<{0}>", GeneratorHelper.GetEntityName(entity));
                case 8: return " : RegistryEntity";
                case 9: return " : ReportEntity";
                default: return "/*Ошибка при генерации: тип сущности " + entity.IdEntityType + "*/";
            }
        }

        public string NullableFieldType(string fieldType, bool isNullable)
        {
            return isNullable && fieldType != "string" ? fieldType + "?" : fieldType;
        }

        public string Override(Entity linkedEntity, EntityField field)
        {
            if (((_entities.FirstOrDefault(f => f.Id == field.IdEntity).EntityType == EntityType.Document) && ((field.Name == "Number") || (field.Name == "Date"))) || (linkedEntity != null && linkedEntity.Name == "DocStatus" && (_entity.IdEntityType == (byte)EntityType.Document || _entity.IdEntityType == (byte)EntityType.Tool)))
                return "override ";
            else
                return "";
        }

        public string GetType(Entity entity)
        {
            if (entity.IdProject != (int)SolutionProject.Tools_MigrationHelper_Core)
                return string.Format("{0}.{1}.{2}", SolutionHelper.GetProjectName(entity.IdProject), (entity.EntityType == EntityType.Enum ? "DbEnums" : entity.EntityType.ToString()), GeneratorHelper.GetEntityName(entity));
            if (entity.IdProject == (int)SolutionProject.Tools_MigrationHelper_Core)
                return GeneratorHelper.GetEntityName(entity);
            return "";
        }

        private string GetFieldString(string type, EntityField field, Entity linkedEntity = (Entity) null)
        {
            return GetFieldString(type, field, Override(linkedEntity, field));
        }

        private string GetFieldString(string type, EntityField field, string @override)
        {
            return Regex.Replace(string.Format("public {0} {1} {2}{{get; set;}}", @override, NullableFieldType(type, field.ColumnAllowNull), GeneratorHelper.FirstToUp(field.Name)), " +", " ");
        }

        private string Id(EntityField field)
        {
            return field.AllowNull
                       ? String.Format("{0}!=null?{0}.Id:null", GeneratorHelper.ModifyName(field.Name))
                       : String.Format("{0}.Id", GeneratorHelper.ModifyName(field.Name));
            ;
        }


        private string GetMultilinkString(string fieldName, Entity multiLinkEntity, string fieldCaption)
        {
            var linkedEntity = GeneratorHelper.GetLinkedEntity(multiLinkEntity, _entities, _entity);
            if (linkedEntity != null)
            {
                return GetCollectionString(GetType(linkedEntity), GeneratorHelper.MultilinkPropertyName(fieldName), "_" + fieldName, fieldCaption);
            }
            return "";
        }

        private string GetTablePartString(EntityField field, Entity linkedEntity)
        {
            if (linkedEntity != null && linkedEntity.GenerateEntityClass)
            {
                return GetCollectionString(GetType(linkedEntity), GeneratorHelper.TablePartPropertyName(field), "_" + field.Name, field.Caption);
            }
            return "";
        }


        private string GetCollection(EntityField field, Entity linkedEntity)
        {
            if (linkedEntity != null && linkedEntity.GenerateEntityClass)
            {
                return GetCollectionString(GetType(linkedEntity), GeneratorHelper.FirstToUp(field.Name), "_" + field.Name, field.Caption);
            }
            return "";
        }

        private IEnumerable<string> GetTpChildCollection()
        {
            var slist = new List<string>();
            if (_entity.EntityType != EntityType.Tablepart)
                return slist;
            var links = GetTpChildCollectionsFields();
            foreach (var field in links)
            {
                var linkedEntity = GeneratorHelper.GetEntity(field.IdEntity, _entities);
                slist.Add(GetCollectionString(GetType(linkedEntity), GeneratorHelper.GetTpCollectionName(linkedEntity.Name), "_" + linkedEntity.Name, field.Caption));
            }
            return slist;
        }

        private readonly List<KeyValuePair<String, String>> _protectedMapping = new List<KeyValuePair<String, String>>();

        private string GetPrivateId()
        {
            _protectedMapping.Add(new KeyValuePair<string, string>("int", "Id"));
            return "protected int Id { get;  set; } ";
        }


        private string DocumentCaptionOverride()
        {
            if (_entity.EntityType == EntityType.Document &&
                _entity.Fields.Any(
                    f => (f.Name.ToLower() == "iddoctype") && (f.EntityLink != null) && (f.EntityLink.Name == "DocType")))
            {
                return @"
        /// <summary>
	    /// Наименование типа документа
	    /// </summary>
        public override string DocumentCaption 
        {
            get
            {
                if (DocType==null)
                    return base.DocumentCaption;
                else
                    return DocType.Caption;
                    
            }
        }

    ";
            }
            return "";
        }

        private string PropertyAccessExpressions()
        {
            if (_protectedMapping.Count == 0)
                return "";
            var sbuilder = new StringBuilder();
            sbuilder.AppendLine("public class PropertyAccessExpressions");
            sbuilder.AppendLine("        {");
            foreach (var pm in _protectedMapping)
            {
                sbuilder.AppendFormat("             public static readonly Expression<Func<{0}, {1}>> {2} = x => x.{2};",
                                      GeneratorHelper.GetEntityName(_entity), pm.Key, pm.Value);
                sbuilder.AppendLine();
            }
            sbuilder.AppendLine("        }");
            return sbuilder.ToString();
        }

        private IEnumerable<IEntityField> GetTpChildCollectionsFields()
        {
            return _entities.Where(w => w.Id != _entity.Id && w.EntityType == EntityType.Tablepart && w.GenerateEntityClass)
                            .SelectMany(s => s.Fields.Where(f => f.IdEntityLink == _entity.Id && f.EntityFieldType != EntityFieldType.VirtualTablePart));
        }

        private string GetCollectionString(string type, string publicName, string privateName, string fieldCaption)
        {
            return string.Format(@"private ICollection<{0}> {2}; 
        /// <summary>
        /// " + fieldCaption + @"
        /// </summary>
        [JsonIgnoreForException]
		public virtual ICollection<{0}> {1} 
		{{
			get{{ return {2} ?? ({2} = new List<{0}>()); }} 
			set{{ {2} = value; }}
		}}"
                                     , type
                                     , publicName
                                     , privateName);
        }

        private string GetDateFieldString(string type, EntityField field)
        {
            string str;

            if (field.ColumnAllowNull)
            {
                str = @"private {0} {2}; 
        /// <summary>
	    /// {4}
	    /// </summary>
		public {3} {0} {1} 
		{{
			get{{ return {2} != null ? ((DateTime){2}).Date : (DateTime?)null; }}
			set{{ {2} = value != null ? ((DateTime)value).Date : value; }}
		}}";
            }
            else
            {
                str = @"private {0} {2}; 
        /// <summary>
	    /// {4}
	    /// </summary>
		public {3} {0} {1} 
		{{
			get{{ return {2}.Date; }}
			set{{ {2} = value.Date; }}
		}}";
            }
            return string.Format(str
                                     , NullableFieldType(type, field.ColumnAllowNull)
                                     , field.Name
                                     , "_" + field.Name
                                     , Override(null, field)
                                     , field.Caption);
        }

        public List<string> GetMultiLinkRelationship()
        {
            var list = new List<string>();

            foreach (EntityField field in _multilinkFields)
            {
                //сущность мультиссылки
                var multiLinkEntity = GeneratorHelper.GetEntity(field.IdEntity, _entities);

                //если у сущности нет поля ссылающегося на данный мультилинк то называем свойство именем мультилинка
                var leftField = _entity.Fields.FirstOrDefault(f => f.IdEntityLink == multiLinkEntity.Id);
                var leftFieldName = leftField == null ? multiLinkEntity.Name : leftField.Name;

                list.Add(GetMultilinkString(leftFieldName,multiLinkEntity, field.Caption));
            }

            return list;
        }
    }
}
