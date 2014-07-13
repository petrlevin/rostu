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
	/// Шаблон импорта
	/// </summary>
	public partial class TemplateImport : ReferenceEntity      
	{
	
		/// <summary>
		/// Идентификатор
		/// </summary>
		
        public override Int32 Id{get; set;}

		/// <summary>
		/// Сущности
		/// </summary>
		private ICollection<Platform.BusinessLogic.Reference.TemplateImportXLS> _tpTemplatesByEntity; 
        /// <summary>
        /// Сущности
        /// </summary>
        [JsonIgnoreForException]
		public virtual ICollection<Platform.BusinessLogic.Reference.TemplateImportXLS> TemplatesByEntity 
		{
			get{ return _tpTemplatesByEntity ?? (_tpTemplatesByEntity = new List<Platform.BusinessLogic.Reference.TemplateImportXLS>()); } 
			set{ _tpTemplatesByEntity = value; }
		}

		/// <summary>
		/// Наименование
		/// </summary>
		public string Caption{get; set;}

	

		/// <summary>
		/// Конструктор по-умолчанию
		/// </summary>
		public TemplateImport()
		{
		}

		/// <summary>
		/// Идентификатор типа сущности
		/// </summary>
		public override int EntityId
		{
			get { return -1744830294; }
		}

		/// <summary>
		/// Идентификатор типа сущности
		/// </summary>
		public static int EntityIdStatic
		{
			get { return -1744830294; }
		}

		/// <summary>
		/// Русское наименование типа сущности
		/// </summary>
		public override string EntityCaption
		{
			get { return "Шаблон импорта"; }
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
				Platform.BusinessLogic.EntityIds.Add(GetType().DeclaringType,-1744830294);
			}
		}


	}
}