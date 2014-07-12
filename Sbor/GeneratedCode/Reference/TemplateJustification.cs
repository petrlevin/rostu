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
using BaseApp.Interfaces;

using Platform.PrimaryEntities.DbEnums;using Platform.PrimaryEntities.Common.DbEnums;

namespace Sbor.Reference
{
	// Это автоматически сгенерированный класс, изменения будут уничтожены при следующей генерации
	/// <summary>
	/// Шаблоны обоснований
	/// </summary>
	public partial class TemplateJustification : ReferenceEntity, IHasRefStatus      
	{
	
		/// <summary>
		/// Идентификатор
		/// </summary>
		
        public override Int32 Id{get; set;}

		/// <summary>
		/// ППО
		/// </summary>
		public int IdPublicLegalFormation{get; set;}
        /// <summary>
	    /// ППО
	    /// </summary>
		public virtual BaseApp.Reference.PublicLegalFormation PublicLegalFormation{get; set;}
		

		/// <summary>
		/// Бюджет
		/// </summary>
		public int? IdBudget{get; set;}
        /// <summary>
	    /// Бюджет
	    /// </summary>
		public virtual BaseApp.Reference.Budget Budget{get; set;}
		

		/// <summary>
		/// Наименование
		/// </summary>
		public string Caption{get; set;}

		/// <summary>
		/// Ведомство
		/// </summary>
		public int IdSBP{get; set;}
        /// <summary>
	    /// Ведомство
	    /// </summary>
		public virtual Sbor.Reference.SBP SBP{get; set;}
		

		/// <summary>
		/// Мероприятие
		/// </summary>
		public int IdActivity{get; set;}
        /// <summary>
	    /// Мероприятие
	    /// </summary>
		public virtual Sbor.Reference.Activity Activity{get; set;}
		

		/// <summary>
		/// Сметные строки
		/// </summary>
		

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
		public TemplateJustification()
		{
		}

		/// <summary>
		/// Идентификатор типа сущности
		/// </summary>
		public override int EntityId
		{
			get { return -1879048024; }
		}

		/// <summary>
		/// Идентификатор типа сущности
		/// </summary>
		public static int EntityIdStatic
		{
			get { return -1879048024; }
		}

		/// <summary>
		/// Русское наименование типа сущности
		/// </summary>
		public override string EntityCaption
		{
			get { return "Шаблоны обоснований"; }
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
				Platform.BusinessLogic.EntityIds.Add(GetType().DeclaringType,-1879048024);
			}
		}


	}
}