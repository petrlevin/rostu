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


using Platform.PrimaryEntities.Reference;using Platform.PrimaryEntities.DbEnums;using Platform.PrimaryEntities.Common.DbEnums;

namespace Platform.BusinessLogic.Reference
{
	// Это автоматически сгенерированный класс, изменения будут уничтожены при следующей генерации
	/// <summary>
	/// Шаблоны импорта из Excel
	/// </summary>
	public partial class TemplateImportXLS : ReferenceEntity, IHasRefStatus      
	{
	
		/// <summary>
		/// Владелец
		/// </summary>
		public int? IdOwner{get; set;}
        /// <summary>
	    /// Владелец
	    /// </summary>
		public virtual Platform.BusinessLogic.Reference.TemplateImport Owner{get; set;}
		

		/// <summary>
		/// Идентификатор
		/// </summary>
		
        public override Int32 Id{get; set;}

		/// <summary>
		/// Наименование
		/// </summary>
		public string Caption{get; set;}

		/// <summary>
		/// Сущность
		/// </summary>
		public int IdEntity{get; set;}
        /// <summary>
	    /// Сущность
	    /// </summary>
		public virtual Entity Entity{get; set;}
		

		/// <summary>
		/// Тип импорта
		/// </summary>
		public byte IdImportType{get; set;}
                            /// <summary>
                            /// Тип импорта
                            /// </summary>
							[NotMapped] 
                            public virtual Platform.BusinessLogic.DbEnums.ImportType ImportType {
								get { return (Platform.BusinessLogic.DbEnums.ImportType)this.IdImportType; } 
								set { this.IdImportType = (byte) value; }
							}

		/// <summary>
		/// Выполнять одной транзакцией
		/// </summary>
		public bool IsPerformSingleTransaction{get; set;}

		/// <summary>
		/// Игнорировать мягкие контроли
		/// </summary>
		public bool IsIgnoreSoftControl{get; set;}

		/// <summary>
		/// Описание
		/// </summary>
		public string Description{get; set;}

		/// <summary>
		/// Группа доступа
		/// </summary>
		public int? IdAccessGroup{get; set;}
        /// <summary>
	    /// Группа доступа
	    /// </summary>
		public virtual Platform.BusinessLogic.Reference.AccessGroup AccessGroup{get; set;}
		

		/// <summary>
		/// Способ выполнения импорта
		/// </summary>
		public byte IdExecImportMode{get; set;}
                            /// <summary>
                            /// Способ выполнения импорта
                            /// </summary>
							[NotMapped] 
                            public virtual Platform.BusinessLogic.DbEnums.ExecImportMode ExecImportMode {
								get { return (Platform.BusinessLogic.DbEnums.ExecImportMode)this.IdExecImportMode; } 
								set { this.IdExecImportMode = (byte) value; }
							}

		/// <summary>
		/// Сопоставление полей
		/// </summary>
		private ICollection<Platform.BusinessLogic.Tablepart.TemplateImportXLS_FieldsMap> _tpFieldsMap; 
        /// <summary>
        /// Сопоставление полей
        /// </summary>
        [JsonIgnoreForException]
		public virtual ICollection<Platform.BusinessLogic.Tablepart.TemplateImportXLS_FieldsMap> FieldsMap 
		{
			get{ return _tpFieldsMap ?? (_tpFieldsMap = new List<Platform.BusinessLogic.Tablepart.TemplateImportXLS_FieldsMap>()); } 
			set{ _tpFieldsMap = value; }
		}

		/// <summary>
		/// Ключевые поля
		/// </summary>
		private ICollection<Platform.BusinessLogic.Tablepart.TemplateImportXLS_KeyField> _tpKeyField; 
        /// <summary>
        /// Ключевые поля
        /// </summary>
        [JsonIgnoreForException]
		public virtual ICollection<Platform.BusinessLogic.Tablepart.TemplateImportXLS_KeyField> KeyField 
		{
			get{ return _tpKeyField ?? (_tpKeyField = new List<Platform.BusinessLogic.Tablepart.TemplateImportXLS_KeyField>()); } 
			set{ _tpKeyField = value; }
		}

		/// <summary>
		/// Статус
		/// </summary>
		public byte IdRefStatus{get; set;}
                            /// <summary>
                            /// Статус
                            /// </summary>
							[NotMapped] 
                            public virtual RefStatus RefStatus {
								get { return (RefStatus)this.IdRefStatus; } 
								set { this.IdRefStatus = (byte) value; }
							}

	

		/// <summary>
		/// Конструктор по-умолчанию
		/// </summary>
		public TemplateImportXLS()
		{
		}

		/// <summary>
		/// Идентификатор типа сущности
		/// </summary>
		public override int EntityId
		{
			get { return -1543503748; }
		}

		/// <summary>
		/// Идентификатор типа сущности
		/// </summary>
		public static int EntityIdStatic
		{
			get { return -1543503748; }
		}

		/// <summary>
		/// Русское наименование типа сущности
		/// </summary>
		public override string EntityCaption
		{
			get { return "Шаблоны импорта из Excel"; }
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
				Platform.BusinessLogic.EntityIds.Add(GetType().DeclaringType,-1543503748);
			}
		}


	}
}