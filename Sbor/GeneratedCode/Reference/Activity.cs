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
	/// Перечень мероприятий
	/// </summary>
	public partial class Activity : ReferenceEntity, IHasRefStatus      
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
		/// Код
		/// </summary>
		public string Code{get; set;}

		/// <summary>
		/// Тип
		/// </summary>
		public byte IdActivityType{get; set;}
                            /// <summary>
                            /// Тип
                            /// </summary>
							[NotMapped] 
                            public virtual Sbor.DbEnums.ActivityType ActivityType {
								get { return (Sbor.DbEnums.ActivityType)this.IdActivityType; } 
								set { this.IdActivityType = (byte) value; }
							}

		/// <summary>
		/// Наименование
		/// </summary>
		public string Caption{get; set;}

		/// <summary>
		/// Полное наименование
		/// </summary>
		public string FullCaption{get; set;}

		/// <summary>
		/// Орган, устанавливающий цены (тарифы)
		/// </summary>
		public string OrganSetPrice{get; set;}

		/// <summary>
		/// НПА
		/// </summary>
		private ICollection<Sbor.Tablepart.Activity_RegulatoryAct> _tpRegulatoryAct; 
        /// <summary>
        /// НПА
        /// </summary>
        [JsonIgnoreForException]
		public virtual ICollection<Sbor.Tablepart.Activity_RegulatoryAct> RegulatoryAct 
		{
			get{ return _tpRegulatoryAct ?? (_tpRegulatoryAct = new List<Sbor.Tablepart.Activity_RegulatoryAct>()); } 
			set{ _tpRegulatoryAct = value; }
		}

		/// <summary>
		/// Дополнительная информация
		/// </summary>
		private ICollection<Sbor.Tablepart.Activity_ExtInfo> _tpExtInfo; 
        /// <summary>
        /// Дополнительная информация
        /// </summary>
        [JsonIgnoreForException]
		public virtual ICollection<Sbor.Tablepart.Activity_ExtInfo> ExtInfo 
		{
			get{ return _tpExtInfo ?? (_tpExtInfo = new List<Sbor.Tablepart.Activity_ExtInfo>()); } 
			set{ _tpExtInfo = value; }
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
		/// Платность
		/// </summary>
		public byte? IdPaidType{get; set;}
                            /// <summary>
                            /// Платность
                            /// </summary>
							[NotMapped] 
                            public virtual Sbor.DbEnums.PaidType? PaidType {
								get { return (Sbor.DbEnums.PaidType?)this.IdPaidType; } 
								set { this.IdPaidType = (byte?) value; }
							}

		/// <summary>
		/// Коды РО
		/// </summary>
		private ICollection<Sbor.Tablepart.Activity_CodeAuthority> _tpCodeAuthority; 
        /// <summary>
        /// Коды РО
        /// </summary>
        [JsonIgnoreForException]
		public virtual ICollection<Sbor.Tablepart.Activity_CodeAuthority> CodeAuthority 
		{
			get{ return _tpCodeAuthority ?? (_tpCodeAuthority = new List<Sbor.Tablepart.Activity_CodeAuthority>()); } 
			set{ _tpCodeAuthority = value; }
		}

		/// <summary>
		/// Показатели
		/// </summary>
		private ICollection<Sbor.Tablepart.Activity_Indicator> _tpIndicator; 
        /// <summary>
        /// Показатели
        /// </summary>
        [JsonIgnoreForException]
		public virtual ICollection<Sbor.Tablepart.Activity_Indicator> Indicator 
		{
			get{ return _tpIndicator ?? (_tpIndicator = new List<Sbor.Tablepart.Activity_Indicator>()); } 
			set{ _tpIndicator = value; }
		}

	
private ICollection<Sbor.Reference.SBP> _mlActivity_SBP; 
        /// <summary>
        /// Перечень мероприятий
        /// </summary>
        [JsonIgnoreForException]
		public virtual ICollection<Sbor.Reference.SBP> Activity_SBP 
		{
			get{ return _mlActivity_SBP ?? (_mlActivity_SBP = new List<Sbor.Reference.SBP>()); } 
			set{ _mlActivity_SBP = value; }
		}
			private ICollection<Sbor.Reference.Contingent> _mlActivity_Contingent; 
        /// <summary>
        /// Мероприятие
        /// </summary>
        [JsonIgnoreForException]
		public virtual ICollection<Sbor.Reference.Contingent> Activity_Contingent 
		{
			get{ return _mlActivity_Contingent ?? (_mlActivity_Contingent = new List<Sbor.Reference.Contingent>()); } 
			set{ _mlActivity_Contingent = value; }
		}
			
		/// <summary>
		/// Конструктор по-умолчанию
		/// </summary>
		public Activity()
		{
		}

		/// <summary>
		/// Идентификатор типа сущности
		/// </summary>
		public override int EntityId
		{
			get { return -1543503842; }
		}

		/// <summary>
		/// Идентификатор типа сущности
		/// </summary>
		public static int EntityIdStatic
		{
			get { return -1543503842; }
		}

		/// <summary>
		/// Русское наименование типа сущности
		/// </summary>
		public override string EntityCaption
		{
			get { return "Перечень мероприятий"; }
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
				Platform.BusinessLogic.EntityIds.Add(GetType().DeclaringType,-1543503842);
			}
		}


	}
}