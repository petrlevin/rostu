using Platform.PrimaryEntities.Common.DbEnums;
using Platform.PrimaryEntities.Common.Interfaces;
using Platform.PrimaryEntities.DbEnums;
using System;
using System.Collections.Generic;
using System.Linq;
using Platform.PrimaryEntities.Interfaces;
using Platform.Utils;
using Platform.Utils.Common;

namespace Platform.PrimaryEntities.Reference
{
    /// <summary>
    /// Класс сущности
    /// </summary>
    [Serializable()]
    public class Entity : Metadata, IEntity
    {
        /// <summary>
        /// Соедиение с базой данных
        /// </summary>
        //[Dependency("DbConnection")]
        //public SqlConnection DbConnection { get; set; }

        #region Данные из БД

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
        /// Описание
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Идентификатор типа сущности
        /// </summary>
        public byte IdEntityType { get; set; }

		/// <summary>
		/// Признак необходимости генерации класса
		/// </summary>
        public bool GenerateEntityClass { get; set; }

        /// <summary>
        /// Системные сущности нельзя удалять через интерфейс системы. 
        /// Системные сущности создаются системой (триггерами, контролями и т.д.) и ею же удаляются.
        /// </summary>
        /// <remarks>
        /// Пример системных сущностей: 
        /// * Автоматически создаваемый справочник для каждого поля типа "ссылка в иерархии".
        /// </remarks>
        public bool? IsSystem { get; set; }


        /// <summary>
        /// 
        /// </summary>
        public int? IdEntityGroup { get; set; }

    

        /// <summary>
        /// Идентификатор проекта к которому относится сущность
        /// </summary>
		public int IdProject { get; set; }

		/// <summary>
		/// Признак версионности сущности
		/// </summary>
		public bool IsVersioning { get; set; }

		/// <summary>
		/// Признак наличия поля упорядочивания
		/// </summary>
		public bool? Ordered { get; set; }

		/// <summary>
		/// Признак допустимости общих ссылок на эту сушность
		/// </summary>
		public bool AllowGenericLinks { get; set; }
		#endregion

        #region Вычисляемая информация, не требующая ображения к БД
        /// <summary>
        /// Тип сущности
        /// </summary>
        public EntityType EntityType
        {
            get { return (EntityType)IdEntityType; }
            set { IdEntityType = (byte)value; }
        }

        /// <summary>
        /// Наименование схемы в БД которой принадлежит сущность
        /// </summary>
        public string Schema
        {
            get { return Schemas.ByEntityType(EntityType); }
        }


        #endregion

        

        #region Вычисляемая информация, требующая обращение к БД (кэшу)

        
        //private void FillEntityField()
        //{
        //    var childs = Objects.ChildsById<EntityField, Entity>(Id);
        //    if (childs!=null)
        //        _fields = childs.Cast<IEntityField>();
        //}

        private IEnumerable<IEntityField> _fields;
        private FieldStrategyBase _fieldAccessStrategy;

        /// <summary>
        /// Список полей сущности
        /// </summary>
        /// <exception cref="InvalidOperationException" ></exception>
        [JsonIgnoreForException]
        public IEnumerable<IEntityField> Fields
        {
            
            get { return _fieldAccessStrategy.Get(this); }
            set {  _fieldAccessStrategy.Set(this,value); }
        }




        //public static IDisposable UseFieldAccesState(FieldAccesStates state)
        //{
        //    if (state == FieldAccesStates.Dynamic)
        //        return new Dynamic();
        //    else if (state == FieldAccesStates.Static)
        //        return  new Static();
        //    else
        //        return  new Mixed();
        //}




        #region FieldStrategies

        //public enum FieldAccesStates
        //{
        //    Dynamic,
        //    Static ,
        //    Mixed
        //}

        public abstract class FieldStrategyBase 
        {

            public abstract IEnumerable<IEntityField> Get(Entity entity);
            public abstract void Set(Entity entity ,IEnumerable<IEntityField> value);
            public static FieldStrategyBase Instance { get; set; }

            static FieldStrategyBase()
            {
                Instance = new Mixed();
            }

        }

        public class Dynamic :FieldStrategyBase
        {
            public override IEnumerable<IEntityField> Get(Entity entity)
            {
                var childs = entity.Objects.ChildsById<EntityField, Entity>(entity.Id);
                if (childs != null)
                    return childs.Cast<IEntityField>();
                else
                    return null;

            }

            public override void Set(Entity entity, IEnumerable<IEntityField> value)
            {
                throw new InvalidOperationException("Сущности находятся в состоянии динамического получения полей. Установка полей в ручную не возможна. Для изменение состояние используйте конструкцию ' using   Entity.UseFieldAccesState(state) '. ");
            }
        }

        public class Static : FieldStrategyBase
        {
            public override IEnumerable<IEntityField> Get(Entity entity)
            {
                return entity._fields;

            }

            public override void Set(Entity entity, IEnumerable<IEntityField> value)
            {
                entity._fields = value;
            }
        }

        public class Mixed : Dynamic
        {
            
            public override IEnumerable<IEntityField> Get(Entity entity)
            {
                if (entity._fields == null)
                {
                    entity._fields = base.Get(entity);
                }
                return entity._fields;
            }

            public override void Set(Entity entity, IEnumerable<IEntityField> value)
            {
                entity._fields = value;
            }


        }



        #endregion

        /// <summary>
		/// Список полей сущности реально существующих в таблице
		/// </summary>
        [JsonIgnoreForException]
		public IEnumerable<IEntityField> RealFields
		{
			get
			{
				return Fields.ToList().Where(f => f.IsRealField());
			}
		}

		/// <summary>
        /// Поле наименования сущности
        /// </summary>

        public IEntityField CaptionField
        {
            get { return Fields.SingleOrDefault(a => a.IsCaption ?? false) ?? Fields.SingleOrDefault(a => a.Name == "id"); }
        }

        /// <summary>
        /// Поле описания сущности
        /// </summary>

        public IEntityField DescriptionField
        {
            get { return Fields.SingleOrDefault(a => a.IsDescription ?? false); }
        }

        #endregion



        #region Protected Members

        ///// <summary>
        ///// Перечисление, содержащее все допустимые схемы для сущностей
        ///// </summary>
        //protected Dictionary<EntityType, string> Schemas = new Dictionary<EntityType, string>()
        //    {
        //        {EntityType.Enum, "enm"},
        //        {EntityType.Reference, "ref"},
        //        {EntityType.Multilink, "ml"},
        //        {EntityType.Tablepart, "tp"},
        //        {EntityType.Registry, "reg"},
        //        {EntityType.Tool, "tool"},
        //        {EntityType.Document, "doc"},
        //        {EntityType.Report, "rep"}
        //    };



        #endregion

        /// <summary>
        /// Возвращает список наименований полей сущности
        /// </summary>
        /// <returns></returns>
        public List<IEntityField> GetFields()
        {
            return this.Fields.ToList();
        }


        public Entity()
            : base()
        {
            _fieldAccessStrategy = FieldStrategyBase.Instance;
        }


        public override int EntityId
        {
            get { return -2147483615; }
        }

        public static int EntityIdStatic
        {
            get { return -2147483615; }
        }
    }
}
