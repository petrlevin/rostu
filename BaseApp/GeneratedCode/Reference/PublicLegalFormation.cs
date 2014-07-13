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

namespace BaseApp.Reference
{
	// Это автоматически сгенерированный класс, изменения будут уничтожены при следующей генерации
	/// <summary>
	/// Публично-правовые образования
	/// </summary>
	public partial class PublicLegalFormation : ReferenceEntity, IHasRefStatus    , IHierarhy  
	{
	
		/// <summary>
		/// Идентификатор
		/// </summary>
		
        public override Int32 Id{get; set;}

		/// <summary>
		/// Вышестоящий
		/// </summary>
		public int? IdParent{get; set;}
        /// <summary>
	    /// Вышестоящий
	    /// </summary>
		public virtual BaseApp.Reference.PublicLegalFormation Parent{get; set;}
		private ICollection<BaseApp.Reference.PublicLegalFormation> _idParent; 
        /// <summary>
        /// Вышестоящий
        /// </summary>
        [JsonIgnoreForException]
		public virtual ICollection<BaseApp.Reference.PublicLegalFormation> ChildrenByidParent 
		{
			get{ return _idParent ?? (_idParent = new List<BaseApp.Reference.PublicLegalFormation>()); } 
			set{ _idParent = value; }
		}

		/// <summary>
		/// Наименование
		/// </summary>
		public string Caption{get; set;}

		/// <summary>
		/// Уровень
		/// </summary>
		public int IdBudgetLevel{get; set;}
        /// <summary>
	    /// Уровень
	    /// </summary>
		public virtual BaseApp.Reference.BudgetLevel BudgetLevel{get; set;}
		

		/// <summary>
		/// Группа доступа
		/// </summary>
		public int IdAccessGroup{get; set;}
        /// <summary>
	    /// Группа доступа
	    /// </summary>
		public virtual Platform.BusinessLogic.Reference.AccessGroup AccessGroup{get; set;}
		

		/// <summary>
		/// Код субъекта РФ
		/// </summary>
		public string Subject{get; set;}

		/// <summary>
		/// Способ формирования кода элемента СЦ
		/// </summary>
		public byte? IdMethodofFormingCode_GoalSetting{get; set;}
                            /// <summary>
                            /// Способ формирования кода элемента СЦ
                            /// </summary>
							[NotMapped] 
                            public virtual BaseApp.DbEnums.MethodofFormingCode? MethodofFormingCode_GoalSetting {
								get { return (BaseApp.DbEnums.MethodofFormingCode?)this.IdMethodofFormingCode_GoalSetting; } 
								set { this.IdMethodofFormingCode_GoalSetting = (byte?) value; }
							}

		/// <summary>
		/// Способ формирования кода целевого показателя
		/// </summary>
		public byte? IdMethodofFormingCode_TargetIndicator{get; set;}
                            /// <summary>
                            /// Способ формирования кода целевого показателя
                            /// </summary>
							[NotMapped] 
                            public virtual BaseApp.DbEnums.MethodofFormingCode? MethodofFormingCode_TargetIndicator {
								get { return (BaseApp.DbEnums.MethodofFormingCode?)this.IdMethodofFormingCode_TargetIndicator; } 
								set { this.IdMethodofFormingCode_TargetIndicator = (byte?) value; }
							}

		/// <summary>
		/// Способ формирования кода вида деятельности
		/// </summary>
		public byte? IdMethodofFormingCode_Activity{get; set;}
                            /// <summary>
                            /// Способ формирования кода вида деятельности
                            /// </summary>
							[NotMapped] 
                            public virtual BaseApp.DbEnums.MethodofFormingCode? MethodofFormingCode_Activity {
								get { return (BaseApp.DbEnums.MethodofFormingCode?)this.IdMethodofFormingCode_Activity; } 
								set { this.IdMethodofFormingCode_Activity = (byte?) value; }
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
		/// Используется ГМЗ
		/// </summary>
		public bool? UsedGMZ{get; set;}

	

		/// <summary>
		/// Конструктор по-умолчанию
		/// </summary>
		public PublicLegalFormation()
		{
		}

		/// <summary>
		/// Идентификатор типа сущности
		/// </summary>
		public override int EntityId
		{
			get { return -1946157026; }
		}

		/// <summary>
		/// Идентификатор типа сущности
		/// </summary>
		public static int EntityIdStatic
		{
			get { return -1946157026; }
		}

		/// <summary>
		/// Русское наименование типа сущности
		/// </summary>
		public override string EntityCaption
		{
			get { return "Публично-правовые образования"; }
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
				Platform.BusinessLogic.EntityIds.Add(GetType().DeclaringType,-1946157026);
			}
		}


	}
}