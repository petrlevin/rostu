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

namespace Platform.BusinessLogic.Tablepart
{
	// Это автоматически сгенерированный класс, изменения будут уничтожены при следующей генерации
	/// <summary>
	/// Сопоставление полей
	/// </summary>
	public partial class TemplateImportXLS_FieldsMap : TablePartEntity      
	{
	
		/// <summary>
		/// Идентификатор
		/// </summary>
		
        public override Int32 Id{get; set;}

		/// <summary>
		/// Ссылка на владельца
		/// </summary>
		public int IdOwner{get; set;}
        /// <summary>
	    /// Ссылка на владельца
	    /// </summary>
		public virtual Platform.BusinessLogic.Reference.TemplateImportXLS Owner{get; set;}
		

		/// <summary>
		/// Поле сущности
		/// </summary>
		public int IdEntityField{get; set;}
        /// <summary>
	    /// Поле сущности
	    /// </summary>
		public virtual EntityField EntityField{get; set;}
		

		/// <summary>
		/// Имя столбца
		/// </summary>
		public string NameColumn{get; set;}

		/// <summary>
		/// Значение поля
		/// </summary>
		public string ValueColumn{get; set;}

		/// <summary>
		/// Маска поиска
		/// </summary>
		public string MaskFinding{get; set;}

		/// <summary>
		/// Маска замены
		/// </summary>
		public string MaskReplacing{get; set;}

	

		/// <summary>
		/// Конструктор по-умолчанию
		/// </summary>
		public TemplateImportXLS_FieldsMap()
		{
		}

		/// <summary>
		/// Идентификатор типа сущности
		/// </summary>
		public override int EntityId
		{
			get { return -1543503747; }
		}

		/// <summary>
		/// Идентификатор типа сущности
		/// </summary>
		public static int EntityIdStatic
		{
			get { return -1543503747; }
		}

		/// <summary>
		/// Русское наименование типа сущности
		/// </summary>
		public override string EntityCaption
		{
			get { return "Сопоставление полей"; }
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
				Platform.BusinessLogic.EntityIds.Add(GetType().DeclaringType,-1543503747);
			}
		}


	}
}