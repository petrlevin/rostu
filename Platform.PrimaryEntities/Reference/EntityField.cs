using System;
using System.Collections.Generic;
using System.Linq;
using Platform.PrimaryEntities.Common.DbEnums;
using Platform.PrimaryEntities.Common.Interfaces;
using Platform.PrimaryEntities.DbEnums;
using Platform.Utils.Common;

namespace Platform.PrimaryEntities.Reference
{
	/// <summary>
	/// Класс полей сущности
	/// </summary>
    public class EntityField : Metadata, IEntityField, IIdentitied
	{
		#region Поля БД
		/// <summary>
		/// Идентификатор элемента
		/// </summary>
		public int Id { get; set; }

		/// <summary>
		/// Системное наименование
		/// </summary>
		public string Name { get; set; }

		/// <summary>
		/// Наименование
		/// </summary>
		public string Caption { get; set; }

		/// <summary>
		/// Ссылка на сущность, которой принадлежит поле
		/// </summary>
		public int IdEntity { get; set; }
		
		/// <summary>
		/// Тип поля
		/// </summary>
		public byte IdEntityFieldType { get; set; }

		/// <summary>
		/// Размер
		/// </summary>
        public Int16? Size{ get; set; }

	    /// <summary>
		/// Точность
		/// </summary>
		public byte? Precision { get; set; }

        /// <summary>
        /// Валидатор
        /// </summary>
        public string RegExpValidator { get; set; }

	    public string EnityName
	    {
	        get { return Entity.Name; }
	    }

	    /// <summary>
		/// Допускается ли NULL значение
		/// </summary>
		public bool AllowNull { get; set; }

        /// <summary>
        /// Допускается ли NULL значение для колонки таблицы.
        /// При создании поля для сущности типа "Отчет" колонка в таблице всегда должна быть nullable. Т.к. при выборе отчета в панели навигации создается запись в таблице отчета с пустыми значениями.
        /// </summary>
        public bool ColumnAllowNull 
        {
            get
            {
                return (Entity.EntityType == EntityType.Report && !Name.Equals("id", StringComparison.OrdinalIgnoreCase)) || this.AllowNull;
            }
        }

		/// <summary>
	    /// Тип значения по умолчанию для поля (Sql или Application)
	    /// </summary>
		public FieldDefaultValueType FieldDefaultValueType
	    {
            get { return (FieldDefaultValueType)IdFieldDefaultValueType; }
	    }

        /// <summary>
        /// Идентификатор типа значения по умолчанию
        /// </summary>
        public byte IdFieldDefaultValueType { get; set; }

	    /// <summary>
		/// Значение по умолчанию
		/// </summary>
		public string DefaultValue { get; set; }

		/// <summary>
		/// Системные поля нельзя удалять через интерфейс системы. 
		/// Системные поля создаются системой (триггерами, контролями и т.д.) и ею же удаляются.
		/// </summary>
		public bool? IsSystem { get; set; }

		/// <summary>
		/// Для ссылочных полей - ссылка, на которое ссылается поле.
		/// </summary>
		public int? IdEntityLink { get; set; }

		/// <summary>
		/// Поле в IdEntityLink, указывающее на элемент данной сущности
		/// </summary>
		public int? IdOwnerField { get; set; }

		/// <summary>
		/// Выражение для вычисляемых полей.
		/// </summary>
		public string Expression { get; set; }

		/// <summary>
		/// Идентификатор типа поддержки ссылочного поля
		/// </summary>
		public byte? IdForeignKeyType { get; set; }

		/// <summary>
		/// Поле является вычисляемым на стороне БД
		/// </summary>
		public bool IsDbComputed
		{
			get
			{
				return (CalculatedFieldType == Platform.PrimaryEntities.Common.DbEnums.CalculatedFieldType.DbComputed ||
				        CalculatedFieldType == Platform.PrimaryEntities.Common.DbEnums.CalculatedFieldType.DbComputedPersisted);
			}
		}

		/// <summary>
		/// Признак, что поле является Caption для сущности
		/// </summary>
		public bool? IsCaption { get; set; }

		/// <summary>
		/// Признак, что поле является Description для сущности
		/// </summary>
		public bool? IsDescription { get; set; }

		/// <summary>
		/// Признак сокрытия поля
		/// </summary>
		public bool? IsHidden { get; set; }

		/// <summary>
		/// Признак только для чтения
		/// </summary>
		public bool? ReadOnly { get; set; }

		/// <summary>
		/// Идентификатор типа виртуального поля
		/// </summary>
		public byte? IdCalculatedFieldType { get; set; }

        /// <summary>
        /// Всплывающая подсказка
        /// </summary>
        public string Tooltip { get; set; }

		#endregion

		private Entity _entity;

		private Entity _entityLink;
		/// <summary>
		/// Сущность которой принадлежит поле
		/// </summary>
		public Entity Entity
		{
			get
			{
				if (_entity==null)
					_entity=Objects.ById<Entity>(IdEntity);
				return _entity;
			}
		}

		/// <summary>
		/// Тип поля
		/// </summary>
		public EntityFieldType EntityFieldType
		{
			get { return (EntityFieldType) IdEntityFieldType; }
		}

		/// <summary>
		/// Сущность на которую ссылается поле
		/// </summary>
        [JsonIgnoreForException]
		public IEntity EntityLink
		{
			get
			{
				if (_entityLink==null && IdEntityLink.HasValue)
					_entityLink = Objects.ById<Entity>(IdEntityLink.Value);
				return _entityLink;
			}

			//get { return IdEntityLink.HasValue ? Objects.ById<Entity>(IdEntityLink.Value) : null; }
		}

		/// <summary>
		/// Тип виртуального поля
		/// </summary>
		public CalculatedFieldType? CalculatedFieldType
		{
			get
			{
			    if (IdCalculatedFieldType == null)
			        return null;
				return (CalculatedFieldType)IdCalculatedFieldType;
			}
		}

		/// <summary>
		/// Тип поддержки ссылочного поля
		/// </summary>
		public ForeignKeyType ForeignKeyType
		{
			get { return (ForeignKeyType) (IdForeignKeyType ?? 1); }
		}

		/// <summary>
		/// Тип поля в виде конструкции SQL
		/// </summary>
		public string SqlType
		{
			get
			{
				switch (EntityFieldType)
				{
					case EntityFieldType.Bool:
						return "BIT";
					case EntityFieldType.Text:
						return String.Format("NVARCHAR(MAX)");
					case EntityFieldType.String:
						return String.Format("NVARCHAR({0})", Size == -1 ? "MAX" : Size.ToString());
					case EntityFieldType.Int:
						return "INT";
					case EntityFieldType.BigInt:
						return "BIGINT";
					case EntityFieldType.Numeric:
                        return String.Format("NUMERIC({0}, {1})", Size, Precision);
                    case EntityFieldType.Money:
						return String.Format("NUMERIC({0}, {1})", MoneySize, MoneyPrecision);
					case EntityFieldType.DateTime:
						return "DATETIME";
					case EntityFieldType.Date:
						return "DATE";
                    case EntityFieldType.FileLink:
					case EntityFieldType.Link:
						{
							if (Name.ToLower() == "idowner" && !IdEntityLink.HasValue)
								return "INT";
							if (!IdEntityLink.HasValue)
								throw new Exception(String.Format("ToSqlType: у поля '{0}' не заполнено свойство IdEntityLink", Name));
							EntityField entityLinkFieldId =
								(EntityField) Objects.ById<Entity>(IdEntityLink.Value).Fields.SingleOrDefault(a => a.Name == "id");
							if (entityLinkFieldId == null)
								throw new Exception(
									String.Format(
										"У сущности на которую ссылается поле '{0}' отсутствует обязательное поле [id]",
										Name));
							return entityLinkFieldId.SqlType;
						}
					case EntityFieldType.Guid:
						return "UNIQUEIDENTIFIER";
					case EntityFieldType.TinyInt:
						return "TINYINT";
					case EntityFieldType.SmallInt:
						return "SMALLINT";
					case EntityFieldType.Xml:
						return "XML";
                    case EntityFieldType.TablepartEntity:
                    case EntityFieldType.ToolEntity:
                    case EntityFieldType.ReferenceEntity:
                    case EntityFieldType.DocumentEntity:
				        return "INT";
                    case EntityFieldType.File:
                        return "varbinary(max)";
                    default:
						return null;
				}
			}
		}

		public void DeserialiseXml(Dictionary<string, string> xml)
		{

		}

        public override String ToString()
        {
            return this.Name;
        }

        public override int EntityId
        {
            get { return -2147483614; }
        }

        /// <summary>
        /// 
        /// </summary>
        public new static int EntityIdStatic
        {
            get { return -2147483614; }
        }

        /// <summary>
        /// Табличное поле?
        /// </summary>
	    public bool IsTableField
	    {
	        get
	        {
	            return
	                this.IdEntityFieldType == (byte) EntityFieldType.Tablepart
	                || this.IdEntityFieldType == (byte) EntityFieldType.VirtualTablePart
	                || this.IdEntityFieldType == (byte) EntityFieldType.Multilink;
	        }
	    }


        IEntity IEntityField.Entity
        {
            get { return Entity; }
        }

	    private const short MoneySize = 22;
        private const short MoneyPrecision = 2;
	}
}
