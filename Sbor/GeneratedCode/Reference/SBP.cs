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
	/// Субъекты бюджетного планирования
	/// </summary>
	public partial class SBP : ReferenceEntity, IHasRefStatus , IVersioning   , IHierarhy  
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
		/// Организация-владелец
		/// </summary>
		public int IdOrganization{get; set;}
        /// <summary>
	    /// Организация-владелец
	    /// </summary>
		public virtual BaseApp.Reference.Organization Organization{get; set;}
		

		/// <summary>
		/// Наименование
		/// </summary>
		public string Caption{get; set;}

		/// <summary>
		/// Тип СБП
		/// </summary>
		public byte IdSBPType{get; set;}
                            /// <summary>
                            /// Тип СБП
                            /// </summary>
							[NotMapped] 
                            public virtual Sbor.DbEnums.SBPType SBPType {
								get { return (Sbor.DbEnums.SBPType)this.IdSBPType; } 
								set { this.IdSBPType = (byte) value; }
							}

		/// <summary>
		/// Учредитель
		/// </summary>
		public bool IsFounder{get; set;}

		/// <summary>
		/// Вышестоящий
		/// </summary>
		public int? IdParent{get; set;}
        /// <summary>
	    /// Вышестоящий
	    /// </summary>
		public virtual Sbor.Reference.SBP Parent{get; set;}
		private ICollection<Sbor.Reference.SBP> _idParent; 
        /// <summary>
        /// Вышестоящий
        /// </summary>
        [JsonIgnoreForException]
		public virtual ICollection<Sbor.Reference.SBP> ChildrenByidParent 
		{
			get{ return _idParent ?? (_idParent = new List<Sbor.Reference.SBP>()); } 
			set{ _idParent = value; }
		}

		/// <summary>
		/// КВСР/КАДБ/КАИФ
		/// </summary>
		public int? IdKVSR{get; set;}
        /// <summary>
	    /// КВСР/КАДБ/КАИФ
	    /// </summary>
		public virtual Sbor.Reference.KVSR KVSR{get; set;}
		

		/// <summary>
		/// Дата начала действия
		/// </summary>
		private DateTime? _ValidityFrom; 
        /// <summary>
	    /// Дата начала действия
	    /// </summary>
		public  DateTime? ValidityFrom 
		{
			get{ return _ValidityFrom != null ? ((DateTime)_ValidityFrom).Date : (DateTime?)null; }
			set{ _ValidityFrom = value != null ? ((DateTime)value).Date : value; }
		}

		/// <summary>
		/// Дата окончания действия
		/// </summary>
		private DateTime? _ValidityTo; 
        /// <summary>
	    /// Дата окончания действия
	    /// </summary>
		public  DateTime? ValidityTo 
		{
			get{ return _ValidityTo != null ? ((DateTime)_ValidityTo).Date : (DateTime?)null; }
			set{ _ValidityTo = value != null ? ((DateTime)value).Date : value; }
		}

		/// <summary>
		/// Cсылка на корень
		/// </summary>
		public int? IdRoot{get; set;}
        /// <summary>
	    /// Cсылка на корень
	    /// </summary>
		public virtual Sbor.Reference.SBP Root{get; set;}
		private ICollection<Sbor.Reference.SBP> _idRoot; 
        /// <summary>
        /// Cсылка на корень
        /// </summary>
        [JsonIgnoreForException]
		public virtual ICollection<Sbor.Reference.SBP> ChildrenByidRoot 
		{
			get{ return _idRoot ?? (_idRoot = new List<Sbor.Reference.SBP>()); } 
			set{ _idRoot = value; }
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
		/// Бланки доведения и формирования
		/// </summary>
		private ICollection<Sbor.Tablepart.SBP_Blank> _tpSBP_Blank; 
        /// <summary>
        /// Бланки доведения и формирования
        /// </summary>
        [JsonIgnoreForException]
		public virtual ICollection<Sbor.Tablepart.SBP_Blank> SBP_Blank 
		{
			get{ return _tpSBP_Blank ?? (_tpSBP_Blank = new List<Sbor.Tablepart.SBP_Blank>()); } 
			set{ _tpSBP_Blank = value; }
		}

		/// <summary>
		/// Периоды планирования в документах АУ/БУ
		/// </summary>
		private ICollection<Sbor.Tablepart.SBP_PlanningPeriodsInDocumentsAUBU> _tpSBP_PlanningPeriodsInDocumentsAUBU; 
        /// <summary>
        /// Периоды планирования в документах АУ/БУ
        /// </summary>
        [JsonIgnoreForException]
		public virtual ICollection<Sbor.Tablepart.SBP_PlanningPeriodsInDocumentsAUBU> SBP_PlanningPeriodsInDocumentsAUBU 
		{
			get{ return _tpSBP_PlanningPeriodsInDocumentsAUBU ?? (_tpSBP_PlanningPeriodsInDocumentsAUBU = new List<Sbor.Tablepart.SBP_PlanningPeriodsInDocumentsAUBU>()); } 
			set{ _tpSBP_PlanningPeriodsInDocumentsAUBU = value; }
		}

		/// <summary>
		/// История создания бланков доведения и формирования
		/// </summary>
		private ICollection<Sbor.Tablepart.SBP_BlankHistory> _tpBlankHistorys; 
        /// <summary>
        /// История создания бланков доведения и формирования
        /// </summary>
        [JsonIgnoreForException]
		public virtual ICollection<Sbor.Tablepart.SBP_BlankHistory> BlankHistorys 
		{
			get{ return _tpBlankHistorys ?? (_tpBlankHistorys = new List<Sbor.Tablepart.SBP_BlankHistory>()); } 
			set{ _tpBlankHistorys = value; }
		}

	

		/// <summary>
		/// Конструктор по-умолчанию
		/// </summary>
		public SBP()
		{
		}

		/// <summary>
		/// Идентификатор типа сущности
		/// </summary>
		public override int EntityId
		{
			get { return -2013265882; }
		}

		/// <summary>
		/// Идентификатор типа сущности
		/// </summary>
		public static int EntityIdStatic
		{
			get { return -2013265882; }
		}

		/// <summary>
		/// Русское наименование типа сущности
		/// </summary>
		public override string EntityCaption
		{
			get { return "Субъекты бюджетного планирования"; }
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
				Platform.BusinessLogic.EntityIds.Add(GetType().DeclaringType,-2013265882);
			}
		}


	}
}