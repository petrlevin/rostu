using System;
using System.Linq.Expressions;
using System.Collections.Generic;
using Platform.BusinessLogic.EntityTypes;
using Platform.BusinessLogic.Common.Interfaces;
using Platform.BusinessLogic.Interfaces;
using Platform.PrimaryEntities.Reference;
using System.ComponentModel.DataAnnotations.Schema;
using System.Xml.Linq;
using System.Data.Objects.DataClasses;
using Platform.Application.Common;
using Platform.Utils.Common;




namespace Platform.BusinessLogic.Reference
{
	// Это автоматически сгенерированный класс, изменения будут уничтожены при следующей генерации
	/// <summary>
	/// Шаблон импорта
	/// </summary>
	public partial class TamplateImport : ReferenceEntity      
	{
	
		/// <summary>
		/// Идентификатор
		/// </summary>
		public override Int32 Id{get; set;}

		/// <summary>
		/// Сущности
		/// </summary>
		private ICollection<Platform.BusinessLogic.Reference.TemplateImportXLS> _tpTemplatesByEntity; 
        [JsonIgnoreForException]
		public virtual ICollection<Platform.BusinessLogic.Reference.TemplateImportXLS> TemplatesByEntity 
		{
			get{ return _tpTemplatesByEntity ?? (_tpTemplatesByEntity = new List<Platform.BusinessLogic.Reference.TemplateImportXLS>()); } 
			set{ _tpTemplatesByEntity = value; }
		}

	

		public TamplateImport()
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
		public new static int EntityIdStatic
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
			public void Execute()
			{
				Platform.BusinessLogic.EntityIds.Add(GetType().DeclaringType,-1744830294);
			}
		}


	}
}