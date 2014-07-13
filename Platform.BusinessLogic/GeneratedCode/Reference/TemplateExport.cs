using System;
using System.Collections.Generic;
using Platform.BusinessLogic.EntityTypes;
using Platform.BusinessLogic.Common.Interfaces;
using Platform.BusinessLogic.Interfaces;
using Platform.PrimaryEntities.Reference;
using System.Xml.Linq;
using System.ComponentModel.DataAnnotations.Schema;
using Platform.Application.Common;
using Platform.Utils.Common;




namespace Platform.BusinessLogic.Reference
{
	// Это автоматически сгенерированный класс, изменения будут уничтожены при следующей генерации
	/// <summary>
	/// Шаблон экспорта
	/// </summary>
	public partial class TemplateExport : ReferenceEntity      
	{
	
		/// <summary>
		/// Идентификатор
		/// </summary>
		
        public override Int32 Id{get; set;}

		/// <summary>
		/// Наименование
		/// </summary>
		public string Caption{get; set;}

		/// <summary>
		/// Тип выборки
		/// </summary>
		public byte IdSelectionType{get; set;}
                            /// <summary>
                            /// Тип выборки
                            /// </summary>
							[NotMapped] 
                            public virtual Platform.BusinessLogic.DbEnums.SelectionType SelectionType {
								get { return (Platform.BusinessLogic.DbEnums.SelectionType)this.IdSelectionType; } 
								set { this.IdSelectionType = (byte) value; }
							}

		/// <summary>
		/// Сущности
		/// </summary>
		private ICollection<Platform.BusinessLogic.Tablepart.TemplateExport_Entity> _tpEntities; 
        /// <summary>
        /// Сущности
        /// </summary>
        [JsonIgnoreForException]
		public virtual ICollection<Platform.BusinessLogic.Tablepart.TemplateExport_Entity> Entities 
		{
			get{ return _tpEntities ?? (_tpEntities = new List<Platform.BusinessLogic.Tablepart.TemplateExport_Entity>()); } 
			set{ _tpEntities = value; }
		}

		/// <summary>
		/// Запрос
		/// </summary>
		public string EntitiesSql{get; set;}

		/// <summary>
		/// Запрос на получение связанных сущностей
		/// </summary>
		public string LinkedEntitiesSql{get; set;}

		/// <summary>
		/// Тип выборки связанных
		/// </summary>
		public byte IdLinkedSelectionType{get; set;}
                            /// <summary>
                            /// Тип выборки связанных
                            /// </summary>
							[NotMapped] 
                            public virtual Platform.BusinessLogic.DbEnums.SelectionType LinkedSelectionType {
								get { return (Platform.BusinessLogic.DbEnums.SelectionType)this.IdLinkedSelectionType; } 
								set { this.IdLinkedSelectionType = (byte) value; }
							}

	
private ICollection<Entity> _mlLinkedEntities; 
        /// <summary>
        /// Шаблон выгрузки
        /// </summary>
        [JsonIgnoreForException]
		public virtual ICollection<Entity> LinkedEntities 
		{
			get{ return _mlLinkedEntities ?? (_mlLinkedEntities = new List<Entity>()); } 
			set{ _mlLinkedEntities = value; }
		}
			
		/// <summary>
		/// Конструктор по-умолчанию
		/// </summary>
		public TemplateExport()
		{
		}

		/// <summary>
		/// Идентификатор типа сущности
		/// </summary>
		public override int EntityId
		{
			get { return -1744830191; }
		}

		/// <summary>
		/// Идентификатор типа сущности
		/// </summary>
		public static int EntityIdStatic
		{
			get { return -1744830191; }
		}

		/// <summary>
		/// Русское наименование типа сущности
		/// </summary>
		public override string EntityCaption
		{
			get { return "Шаблон экспорта"; }
		}

		

		

		/// <summary>
		/// Регистрация идентфикатора сущности
		/// </summary>
		public class EntityIdRegistrator:IBeforeAplicationStart
		{
			/// <summary>
			/// Зарегистрировать
			/// </summary>
			public void Execute()
			{
				Platform.BusinessLogic.EntityIds.Add(GetType().DeclaringType,-1744830191);
			}
		}


	}
}