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



namespace Sbor.Document
{
	// Это автоматически сгенерированный класс, изменения будут уничтожены при следующей генерации
	/// <summary>
	/// Предельные объемы бюджетных ассигнований
	/// </summary>
	public partial class LimitBudgetAllocations : DocumentEntity<LimitBudgetAllocations>    , IHierarhy  
	{
	
		/// <summary>
		/// Вести доп. потребности
		/// </summary>
		public bool? IsAdditionalNeed{get; set;}

		/// <summary>
		/// Сравнить с документом
		/// </summary>
		public int? IdCompareWithDocument{get; set;}
        /// <summary>
	    /// Сравнить с документом
	    /// </summary>
		public virtual Sbor.Document.LimitBudgetAllocations CompareWithDocument{get; set;}
		private ICollection<Sbor.Document.LimitBudgetAllocations> _IdCompareWithDocument; 
        /// <summary>
        /// Сравнить с документом
        /// </summary>
        [JsonIgnoreForException]
		public virtual ICollection<Sbor.Document.LimitBudgetAllocations> ChildrenByIdCompareWithDocument 
		{
			get{ return _IdCompareWithDocument ?? (_IdCompareWithDocument = new List<Sbor.Document.LimitBudgetAllocations>()); } 
			set{ _IdCompareWithDocument = value; }
		}

		/// <summary>
		/// Актуальный бланк доведения
		/// </summary>
		public int? IdSBP_BlankActual{get; set;}
        /// <summary>
	    /// Актуальный бланк доведения
	    /// </summary>
		public virtual Sbor.Tablepart.SBP_BlankHistory SBP_BlankActual{get; set;}
		

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
		public int IdBudget{get; set;}
        /// <summary>
	    /// Бюджет
	    /// </summary>
		public virtual BaseApp.Reference.Budget Budget{get; set;}
		

		/// <summary>
		/// Версия
		/// </summary>
		public int IdVersion{get; set;}
        /// <summary>
	    /// Версия
	    /// </summary>
		public virtual BaseApp.Reference.Version Version{get; set;}
		

		/// <summary>
		/// Номер
		/// </summary>
		public override string Number{get; set;}

		/// <summary>
		/// Дата
		/// </summary>
		private DateTime _Date; 
        /// <summary>
	    /// Дата
	    /// </summary>
		public override  DateTime Date 
		{
			get{ return _Date.Date; }
			set{ _Date = value.Date; }
		}

		/// <summary>
		/// Системная дата утверждения
		/// </summary>
		private DateTime? _DateCommit; 
        /// <summary>
	    /// Системная дата утверждения
	    /// </summary>
		public  DateTime? DateCommit 
		{
			get{ return _DateCommit != null ? ((DateTime)_DateCommit).Date : (DateTime?)null; }
			set{ _DateCommit = value != null ? ((DateTime)value).Date : value; }
		}

		/// <summary>
		/// Утвержден
		/// </summary>
		public bool? IsApproved{get; set;}

		/// <summary>
		/// Субъект бюджетного планирования
		/// </summary>
		public int IdSBP{get; set;}
        /// <summary>
	    /// Субъект бюджетного планирования
	    /// </summary>
		public virtual Sbor.Reference.SBP SBP{get; set;}
		

		/// <summary>
		/// Тип СБП
		/// </summary>
		public byte? IdSBPType{get; set;}
                            /// <summary>
                            /// Тип СБП
                            /// </summary>
							[NotMapped] 
                            public virtual Sbor.DbEnums.SBPType? SBPType {
								get { return (Sbor.DbEnums.SBPType?)this.IdSBPType; } 
								set { this.IdSBPType = (byte?) value; }
							}

		/// <summary>
		/// Последнее редактирование
		/// </summary>
		private DateTime? _DateLastEdit; 
        /// <summary>
	    /// Последнее редактирование
	    /// </summary>
		public  DateTime? DateLastEdit 
		{
			get{ return _DateLastEdit != null ? ((DateTime)_DateLastEdit).Date : (DateTime?)null; }
			set{ _DateLastEdit = value != null ? ((DateTime)value).Date : value; }
		}

		/// <summary>
		/// Описание
		/// </summary>
		public string Description{get; set;}

		/// <summary>
		/// Предыдущая редакция
		/// </summary>
		public int? IdParent{get; set;}
        /// <summary>
	    /// Предыдущая редакция
	    /// </summary>
		public virtual Sbor.Document.LimitBudgetAllocations Parent{get; set;}
		private ICollection<Sbor.Document.LimitBudgetAllocations> _idParent; 
        /// <summary>
        /// Предыдущая редакция
        /// </summary>
        [JsonIgnoreForException]
		public virtual ICollection<Sbor.Document.LimitBudgetAllocations> ChildrenByidParent 
		{
			get{ return _idParent ?? (_idParent = new List<Sbor.Document.LimitBudgetAllocations>()); } 
			set{ _idParent = value; }
		}

		/// <summary>
		/// Требует уточнения
		/// </summary>
		public bool IsRequireClarification{get; set;}

		/// <summary>
		/// Причина уточнения
		/// </summary>
		public string ReasonClarification{get; set;}

		/// <summary>
		/// Предельные объемы бюджетных ассигнований
		/// </summary>
		private ICollection<Sbor.Tablepart.LimitBudgetAllocations_LimitAllocations> _tpLimitAllocations; 
        /// <summary>
        /// Предельные объемы бюджетных ассигнований
        /// </summary>
        [JsonIgnoreForException]
		public virtual ICollection<Sbor.Tablepart.LimitBudgetAllocations_LimitAllocations> LimitAllocations 
		{
			get{ return _tpLimitAllocations ?? (_tpLimitAllocations = new List<Sbor.Tablepart.LimitBudgetAllocations_LimitAllocations>()); } 
			set{ _tpLimitAllocations = value; }
		}

		/// <summary>
		/// Контрольные соотношения
		/// </summary>
		private ICollection<Sbor.Tablepart.LimitBudgetAllocations_ControlRelation> _tpControlRelation; 
        /// <summary>
        /// Контрольные соотношения
        /// </summary>
        [JsonIgnoreForException]
		public virtual ICollection<Sbor.Tablepart.LimitBudgetAllocations_ControlRelation> ControlRelation 
		{
			get{ return _tpControlRelation ?? (_tpControlRelation = new List<Sbor.Tablepart.LimitBudgetAllocations_ControlRelation>()); } 
			set{ _tpControlRelation = value; }
		}

		/// <summary>
		/// Просмотр изменений
		/// </summary>
		private ICollection<Sbor.Tablepart.LimitBudgetAllocations_ShowChanges> _tpShowChanges; 
        /// <summary>
        /// Просмотр изменений
        /// </summary>
        [JsonIgnoreForException]
		public virtual ICollection<Sbor.Tablepart.LimitBudgetAllocations_ShowChanges> ShowChanges 
		{
			get{ return _tpShowChanges ?? (_tpShowChanges = new List<Sbor.Tablepart.LimitBudgetAllocations_ShowChanges>()); } 
			set{ _tpShowChanges = value; }
		}

		/// <summary>
		/// Статус
		/// </summary>
		public override int IdDocStatus{get; set;}

		/// <summary>
		/// Наименование
		/// </summary>
		public string Caption{get; set;}

	

		/// <summary>
		/// Конструктор по-умолчанию
		/// </summary>
		public LimitBudgetAllocations()
		{
		}

		/// <summary>
		/// Идентификатор типа сущности
		/// </summary>
		public override int EntityId
		{
			get { return -1207959532; }
		}

		/// <summary>
		/// Идентификатор типа сущности
		/// </summary>
		public static int EntityIdStatic
		{
			get { return -1207959532; }
		}

		/// <summary>
		/// Русское наименование типа сущности
		/// </summary>
		public override string EntityCaption
		{
			get { return "Предельные объемы бюджетных ассигнований"; }
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
				Platform.BusinessLogic.EntityIds.Add(GetType().DeclaringType,-1207959532);
			}
		}


	}
}