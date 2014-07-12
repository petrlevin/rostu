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
	/// Смета казенного учреждения
	/// </summary>
	public partial class PublicInstitutionEstimate : DocumentEntity<PublicInstitutionEstimate>    , IHierarhy  
	{
	
		/// <summary>
		/// Актуальный бланк формирования
		/// </summary>
		public int? IdSBP_BlankActual{get; set;}
        /// <summary>
	    /// Актуальный бланк формирования
	    /// </summary>
		public virtual Sbor.Tablepart.SBP_BlankHistory SBP_BlankActual{get; set;}
		

		/// <summary>
		/// Актуальный бланк формирования АУ/БУ
		/// </summary>
		public int? IdSBP_BlankActualAuBu{get; set;}
        /// <summary>
	    /// Актуальный бланк формирования АУ/БУ
	    /// </summary>
		public virtual Sbor.Tablepart.SBP_BlankHistory SBP_BlankActualAuBu{get; set;}
		

		/// <summary>
		/// Требует согласования
		/// </summary>
		public bool IsRequireCheck{get; set;}

		/// <summary>
		/// Идентификатор
		/// </summary>
		
        public override Int32 Id{get; set;}

		/// <summary>
		/// Наименование
		/// </summary>
		public string Caption{get; set;}

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
		/// Учреждение
		/// </summary>
		public int IdSBP{get; set;}
        /// <summary>
	    /// Учреждение
	    /// </summary>
		public virtual Sbor.Reference.SBP SBP{get; set;}
		

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
		public virtual Sbor.Document.PublicInstitutionEstimate Parent{get; set;}
		private ICollection<Sbor.Document.PublicInstitutionEstimate> _idParent; 
        /// <summary>
        /// Предыдущая редакция
        /// </summary>
        [JsonIgnoreForException]
		public virtual ICollection<Sbor.Document.PublicInstitutionEstimate> ChildrenByidParent 
		{
			get{ return _idParent ?? (_idParent = new List<Sbor.Document.PublicInstitutionEstimate>()); } 
			set{ _idParent = value; }
		}

		/// <summary>
		/// Причина отказа
		/// </summary>
		public string ReasonCancel{get; set;}

		/// <summary>
		/// Требует уточнения
		/// </summary>
		public bool IsRequireClarification{get; set;}

		/// <summary>
		/// Причина уточнения
		/// </summary>
		public string ReasonClarification{get; set;}

		/// <summary>
		/// Дата прекращения
		/// </summary>
		private DateTime? _DateTerminate; 
        /// <summary>
	    /// Дата прекращения
	    /// </summary>
		public  DateTime? DateTerminate 
		{
			get{ return _DateTerminate != null ? ((DateTime)_DateTerminate).Date : (DateTime?)null; }
			set{ _DateTerminate = value != null ? ((DateTime)value).Date : value; }
		}

		/// <summary>
		/// Причина прекращения
		/// </summary>
		public string ReasonTerminate{get; set;}

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
		/// ТЧ «Мероприятия»
		/// </summary>
		private ICollection<Sbor.Tablepart.PublicInstitutionEstimate_Activity> _tpActivities; 
        /// <summary>
        /// ТЧ «Мероприятия»
        /// </summary>
        [JsonIgnoreForException]
		public virtual ICollection<Sbor.Tablepart.PublicInstitutionEstimate_Activity> Activities 
		{
			get{ return _tpActivities ?? (_tpActivities = new List<Sbor.Tablepart.PublicInstitutionEstimate_Activity>()); } 
			set{ _tpActivities = value; }
		}

		/// <summary>
		/// ТЧ «Расходы»
		/// </summary>
		private ICollection<Sbor.Tablepart.PublicInstitutionEstimate_Expense> _tpExpenses; 
        /// <summary>
        /// ТЧ «Расходы»
        /// </summary>
        [JsonIgnoreForException]
		public virtual ICollection<Sbor.Tablepart.PublicInstitutionEstimate_Expense> Expenses 
		{
			get{ return _tpExpenses ?? (_tpExpenses = new List<Sbor.Tablepart.PublicInstitutionEstimate_Expense>()); } 
			set{ _tpExpenses = value; }
		}

		/// <summary>
		/// ТЧ «Методы распределения»
		/// </summary>
		private ICollection<Sbor.Tablepart.PublicInstitutionEstimate_DistributionMethod> _tpDistributionMethods; 
        /// <summary>
        /// ТЧ «Методы распределения»
        /// </summary>
        [JsonIgnoreForException]
		public virtual ICollection<Sbor.Tablepart.PublicInstitutionEstimate_DistributionMethod> DistributionMethods 
		{
			get{ return _tpDistributionMethods ?? (_tpDistributionMethods = new List<Sbor.Tablepart.PublicInstitutionEstimate_DistributionMethod>()); } 
			set{ _tpDistributionMethods = value; }
		}

		/// <summary>
		/// ТЧ «Дополнительный параметр распределения»
		/// </summary>
		private ICollection<Sbor.Tablepart.PublicInstitutionEstimate_DistributionAdditionalParam> _tpDistributionAdditionalParams; 
        /// <summary>
        /// ТЧ «Дополнительный параметр распределения»
        /// </summary>
        [JsonIgnoreForException]
		public virtual ICollection<Sbor.Tablepart.PublicInstitutionEstimate_DistributionAdditionalParam> DistributionAdditionalParams 
		{
			get{ return _tpDistributionAdditionalParams ?? (_tpDistributionAdditionalParams = new List<Sbor.Tablepart.PublicInstitutionEstimate_DistributionAdditionalParam>()); } 
			set{ _tpDistributionAdditionalParams = value; }
		}

		/// <summary>
		/// ТЧ «Мероприятия для распределения»
		/// </summary>
		private ICollection<Sbor.Tablepart.PublicInstitutionEstimate_DistributionActivity> _tpDistributionActivities; 
        /// <summary>
        /// ТЧ «Мероприятия для распределения»
        /// </summary>
        [JsonIgnoreForException]
		public virtual ICollection<Sbor.Tablepart.PublicInstitutionEstimate_DistributionActivity> DistributionActivities 
		{
			get{ return _tpDistributionActivities ?? (_tpDistributionActivities = new List<Sbor.Tablepart.PublicInstitutionEstimate_DistributionActivity>()); } 
			set{ _tpDistributionActivities = value; }
		}

		/// <summary>
		/// ТЧ «Косвенные расходы»
		/// </summary>
		private ICollection<Sbor.Tablepart.PublicInstitutionEstimate_IndirectExpenses> _tpIndirectExpenses; 
        /// <summary>
        /// ТЧ «Косвенные расходы»
        /// </summary>
        [JsonIgnoreForException]
		public virtual ICollection<Sbor.Tablepart.PublicInstitutionEstimate_IndirectExpenses> IndirectExpenses 
		{
			get{ return _tpIndirectExpenses ?? (_tpIndirectExpenses = new List<Sbor.Tablepart.PublicInstitutionEstimate_IndirectExpenses>()); } 
			set{ _tpIndirectExpenses = value; }
		}

		/// <summary>
		/// ТЧ «Мероприятия АУ/БУ»
		/// </summary>
		private ICollection<Sbor.Tablepart.PublicInstitutionEstimate_ActivityAUBU> _tpActivitiesAUBU; 
        /// <summary>
        /// ТЧ «Мероприятия АУ/БУ»
        /// </summary>
        [JsonIgnoreForException]
		public virtual ICollection<Sbor.Tablepart.PublicInstitutionEstimate_ActivityAUBU> ActivitiesAUBU 
		{
			get{ return _tpActivitiesAUBU ?? (_tpActivitiesAUBU = new List<Sbor.Tablepart.PublicInstitutionEstimate_ActivityAUBU>()); } 
			set{ _tpActivitiesAUBU = value; }
		}

		/// <summary>
		/// ТЧ «Расходы учредителя по мероприятиям АУ/БУ»
		/// </summary>
		private ICollection<Sbor.Tablepart.PublicInstitutionEstimate_FounderAUBUExpense> _tpFounderAUBUExpenses; 
        /// <summary>
        /// ТЧ «Расходы учредителя по мероприятиям АУ/БУ»
        /// </summary>
        [JsonIgnoreForException]
		public virtual ICollection<Sbor.Tablepart.PublicInstitutionEstimate_FounderAUBUExpense> FounderAUBUExpenses 
		{
			get{ return _tpFounderAUBUExpenses ?? (_tpFounderAUBUExpenses = new List<Sbor.Tablepart.PublicInstitutionEstimate_FounderAUBUExpense>()); } 
			set{ _tpFounderAUBUExpenses = value; }
		}

		/// <summary>
		/// ТЧ «Расходы автономных и бюджетных учреждений»
		/// </summary>
		private ICollection<Sbor.Tablepart.PublicInstitutionEstimate_AloneAndBudgetInstitutionExpense> _tpAloneAndBudgetInstitutionExpenses; 
        /// <summary>
        /// ТЧ «Расходы автономных и бюджетных учреждений»
        /// </summary>
        [JsonIgnoreForException]
		public virtual ICollection<Sbor.Tablepart.PublicInstitutionEstimate_AloneAndBudgetInstitutionExpense> AloneAndBudgetInstitutionExpenses 
		{
			get{ return _tpAloneAndBudgetInstitutionExpenses ?? (_tpAloneAndBudgetInstitutionExpenses = new List<Sbor.Tablepart.PublicInstitutionEstimate_AloneAndBudgetInstitutionExpense>()); } 
			set{ _tpAloneAndBudgetInstitutionExpenses = value; }
		}

		/// <summary>
		/// Утвержден
		/// </summary>
		public bool IsApproved{get; set;}

		/// <summary>
		/// Статус
		/// </summary>
		public override int IdDocStatus{get; set;}

		/// <summary>
		/// Вести доп. потребности
		/// </summary>
		public bool? HasAdditionalNeed{get; set;}

	

		/// <summary>
		/// Конструктор по-умолчанию
		/// </summary>
		public PublicInstitutionEstimate()
		{
		}

		/// <summary>
		/// Идентификатор типа сущности
		/// </summary>
		public override int EntityId
		{
			get { return -1207959525; }
		}

		/// <summary>
		/// Идентификатор типа сущности
		/// </summary>
		public static int EntityIdStatic
		{
			get { return -1207959525; }
		}

		/// <summary>
		/// Русское наименование типа сущности
		/// </summary>
		public override string EntityCaption
		{
			get { return "Смета казенного учреждения"; }
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
				Platform.BusinessLogic.EntityIds.Add(GetType().DeclaringType,-1207959525);
			}
		}


	}
}