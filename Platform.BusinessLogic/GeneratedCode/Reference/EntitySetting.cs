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


using Platform.PrimaryEntities.Reference;

namespace Platform.BusinessLogic.Reference
{
	// Это автоматически сгенерированный класс, изменения будут уничтожены при следующей генерации
	/// <summary>
	/// Настройки сущности
	/// </summary>
	public partial class EntitySetting : ReferenceEntity      
	{
	
		/// <summary>
		/// Идентификатор
		/// </summary>
		
        public override Int32 Id{get; set;}

		/// <summary>
		/// Сущность
		/// </summary>
		public int IdEntity{get; set;}
        /// <summary>
	    /// Сущность
	    /// </summary>
		public virtual Entity Entity{get; set;}
		

		/// <summary>
		/// Форма элемента
		/// </summary>
		public int? IdItemForm{get; set;}
        /// <summary>
	    /// Форма элемента
	    /// </summary>
		public virtual Platform.PrimaryEntities.Reference.Form ItemForm{get; set;}
		

		/// <summary>
		/// Форма списка
		/// </summary>
		public int? IdListForm{get; set;}
        /// <summary>
	    /// Форма списка
	    /// </summary>
		public virtual Platform.PrimaryEntities.Reference.Form ListForm{get; set;}
		

		/// <summary>
		/// Форма выбора
		/// </summary>
		public int? IdSelectionForm{get; set;}
        /// <summary>
	    /// Форма выбора
	    /// </summary>
		public virtual Platform.PrimaryEntities.Reference.Form SelectionForm{get; set;}
		

		/// <summary>
		/// Всегда показывать линейно
		/// </summary>
		public bool AlwaysShowLinearly{get; set;}

		/// <summary>
		/// Поле для иерархии по-умолчанию
		/// </summary>
		public int? IdEntityField_Hierarchy{get; set;}
        /// <summary>
	    /// Поле для иерархии по-умолчанию
	    /// </summary>
		public virtual EntityField EntityField_Hierarchy{get; set;}
		

		/// <summary>
		/// Наименования сущностей для панели навигации
		/// </summary>
		private ICollection<Platform.BusinessLogic.Reference.ItemsCaptionsForNavigationPanel> _tpItemsCaptionsForNavigationPanel; 
        /// <summary>
        /// Наименования сущностей для панели навигации
        /// </summary>
        [JsonIgnoreForException]
		public virtual ICollection<Platform.BusinessLogic.Reference.ItemsCaptionsForNavigationPanel> ItemsCaptionsForNavigationPanel 
		{
			get{ return _tpItemsCaptionsForNavigationPanel ?? (_tpItemsCaptionsForNavigationPanel = new List<Platform.BusinessLogic.Reference.ItemsCaptionsForNavigationPanel>()); } 
			set{ _tpItemsCaptionsForNavigationPanel = value; }
		}

		/// <summary>
		/// Класс для выбора наименования
		/// </summary>
		public string ClassSelectionCaption{get; set;}

	

		/// <summary>
		/// Конструктор по-умолчанию
		/// </summary>
		public EntitySetting()
		{
		}

		/// <summary>
		/// Идентификатор типа сущности
		/// </summary>
		public override int EntityId
		{
			get { return -1811939300; }
		}

		/// <summary>
		/// Идентификатор типа сущности
		/// </summary>
		public static int EntityIdStatic
		{
			get { return -1811939300; }
		}

		/// <summary>
		/// Русское наименование типа сущности
		/// </summary>
		public override string EntityCaption
		{
			get { return "Настройки сущности"; }
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
				Platform.BusinessLogic.EntityIds.Add(GetType().DeclaringType,-1811939300);
			}
		}


	}
}