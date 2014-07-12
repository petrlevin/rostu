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
	/// План деятельности
	/// </summary>
	public partial class PlanActivity : DocumentEntity<PlanActivity>    , IHierarhy  
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
		/// Утвержден
		/// </summary>
		public bool IsApproved{get; set;}

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
		public virtual Sbor.Document.PlanActivity Parent{get; set;}
		private ICollection<Sbor.Document.PlanActivity> _idParent; 
        /// <summary>
        /// Предыдущая редакция
        /// </summary>
        [JsonIgnoreForException]
		public virtual ICollection<Sbor.Document.PlanActivity> ChildrenByidParent 
		{
			get{ return _idParent ?? (_idParent = new List<Sbor.Document.PlanActivity>()); } 
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
		/// Статус
		/// </summary>
		public override int IdDocStatus{get; set;}

		/// <summary>
		/// Наименование
		/// </summary>
		public string Caption{get; set;}

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
		/// Учреждение
		/// </summary>
		public int IdSBP{get; set;}
        /// <summary>
	    /// Учреждение
	    /// </summary>
		public virtual Sbor.Reference.SBP SBP{get; set;}
		

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
		/// Мероприятия
		/// </summary>
		private ICollection<Sbor.Tablepart.PlanActivity_Activity> _tpActivity; 
        /// <summary>
        /// Мероприятия
        /// </summary>
        [JsonIgnoreForException]
		public virtual ICollection<Sbor.Tablepart.PlanActivity_Activity> Activity 
		{
			get{ return _tpActivity ?? (_tpActivity = new List<Sbor.Tablepart.PlanActivity_Activity>()); } 
			set{ _tpActivity = value; }
		}

		/// <summary>
		/// Объемы мероприятий
		/// </summary>
		private ICollection<Sbor.Tablepart.PlanActivity_ActivityVolume> _tpActivityVolumes; 
        /// <summary>
        /// Объемы мероприятий
        /// </summary>
        [JsonIgnoreForException]
		public virtual ICollection<Sbor.Tablepart.PlanActivity_ActivityVolume> ActivityVolumes 
		{
			get{ return _tpActivityVolumes ?? (_tpActivityVolumes = new List<Sbor.Tablepart.PlanActivity_ActivityVolume>()); } 
			set{ _tpActivityVolumes = value; }
		}

		/// <summary>
		/// Показатели качества мероприятий
		/// </summary>
		private ICollection<Sbor.Tablepart.PlanActivity_IndicatorQualityActivity> _tpIndicatorQualityActivity; 
        /// <summary>
        /// Показатели качества мероприятий
        /// </summary>
        [JsonIgnoreForException]
		public virtual ICollection<Sbor.Tablepart.PlanActivity_IndicatorQualityActivity> IndicatorQualityActivity 
		{
			get{ return _tpIndicatorQualityActivity ?? (_tpIndicatorQualityActivity = new List<Sbor.Tablepart.PlanActivity_IndicatorQualityActivity>()); } 
			set{ _tpIndicatorQualityActivity = value; }
		}

		/// <summary>
		/// Значения показателей качества
		/// </summary>
		private ICollection<Sbor.Tablepart.PlanActivity_IndicatorQualityActivityValue> _tpIndicatorQualityActivityValues; 
        /// <summary>
        /// Значения показателей качества
        /// </summary>
        [JsonIgnoreForException]
		public virtual ICollection<Sbor.Tablepart.PlanActivity_IndicatorQualityActivityValue> IndicatorQualityActivityValues 
		{
			get{ return _tpIndicatorQualityActivityValues ?? (_tpIndicatorQualityActivityValues = new List<Sbor.Tablepart.PlanActivity_IndicatorQualityActivityValue>()); } 
			set{ _tpIndicatorQualityActivityValues = value; }
		}

		/// <summary>
		/// Мероприятия АУ/БУ
		/// </summary>
		private ICollection<Sbor.Tablepart.PlanActivity_ActivityAUBU> _tpActivityAUBU; 
        /// <summary>
        /// Мероприятия АУ/БУ
        /// </summary>
        [JsonIgnoreForException]
		public virtual ICollection<Sbor.Tablepart.PlanActivity_ActivityAUBU> ActivityAUBU 
		{
			get{ return _tpActivityAUBU ?? (_tpActivityAUBU = new List<Sbor.Tablepart.PlanActivity_ActivityAUBU>()); } 
			set{ _tpActivityAUBU = value; }
		}

		/// <summary>
		/// Объемы мероприятий АУ/БУ
		/// </summary>
		private ICollection<Sbor.Tablepart.PlanActivity_ActivityVolumeAUBU> _tpActivityVolumeAUBU; 
        /// <summary>
        /// Объемы мероприятий АУ/БУ
        /// </summary>
        [JsonIgnoreForException]
		public virtual ICollection<Sbor.Tablepart.PlanActivity_ActivityVolumeAUBU> ActivityVolumeAUBU 
		{
			get{ return _tpActivityVolumeAUBU ?? (_tpActivityVolumeAUBU = new List<Sbor.Tablepart.PlanActivity_ActivityVolumeAUBU>()); } 
			set{ _tpActivityVolumeAUBU = value; }
		}

		/// <summary>
		/// КБК финансового обеспечения
		/// </summary>
		private ICollection<Sbor.Tablepart.PlanActivity_KBKOfFinancialProvision> _tpKBKOfFinancialProvisions; 
        /// <summary>
        /// КБК финансового обеспечения
        /// </summary>
        [JsonIgnoreForException]
		public virtual ICollection<Sbor.Tablepart.PlanActivity_KBKOfFinancialProvision> KBKOfFinancialProvisions 
		{
			get{ return _tpKBKOfFinancialProvisions ?? (_tpKBKOfFinancialProvisions = new List<Sbor.Tablepart.PlanActivity_KBKOfFinancialProvision>()); } 
			set{ _tpKBKOfFinancialProvisions = value; }
		}

		/// <summary>
		/// Периоды финансового обеспечения
		/// </summary>
		private ICollection<Sbor.Tablepart.PlanActivity_PeriodsOfFinancialProvision> _tpPeriodsOfFinancialProvisions; 
        /// <summary>
        /// Периоды финансового обеспечения
        /// </summary>
        [JsonIgnoreForException]
		public virtual ICollection<Sbor.Tablepart.PlanActivity_PeriodsOfFinancialProvision> PeriodsOfFinancialProvisions 
		{
			get{ return _tpPeriodsOfFinancialProvisions ?? (_tpPeriodsOfFinancialProvisions = new List<Sbor.Tablepart.PlanActivity_PeriodsOfFinancialProvision>()); } 
			set{ _tpPeriodsOfFinancialProvisions = value; }
		}

		/// <summary>
		/// Причина отказа 	
		/// </summary>
		public string ReasonCancel{get; set;}

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
		/// Тип периода ОФГ
		/// </summary>
		public byte? IdDocAUBUPeriodType_OFG{get; set;}
                            /// <summary>
                            /// Тип периода ОФГ
                            /// </summary>
							[NotMapped] 
                            public virtual Sbor.DbEnums.DocAUBUPeriodType? DocAUBUPeriodType_OFG {
								get { return (Sbor.DbEnums.DocAUBUPeriodType?)this.IdDocAUBUPeriodType_OFG; } 
								set { this.IdDocAUBUPeriodType_OFG = (byte?) value; }
							}

		/// <summary>
		/// Тип периода ПФГ-1
		/// </summary>
		public byte? IdDocAUBUPeriodType_PFG1{get; set;}
                            /// <summary>
                            /// Тип периода ПФГ-1
                            /// </summary>
							[NotMapped] 
                            public virtual Sbor.DbEnums.DocAUBUPeriodType? DocAUBUPeriodType_PFG1 {
								get { return (Sbor.DbEnums.DocAUBUPeriodType?)this.IdDocAUBUPeriodType_PFG1; } 
								set { this.IdDocAUBUPeriodType_PFG1 = (byte?) value; }
							}

		/// <summary>
		/// Тип периода ПФГ-2
		/// </summary>
		public byte? IdDocAUBUPeriodType_PFG2{get; set;}
                            /// <summary>
                            /// Тип периода ПФГ-2
                            /// </summary>
							[NotMapped] 
                            public virtual Sbor.DbEnums.DocAUBUPeriodType? DocAUBUPeriodType_PFG2 {
								get { return (Sbor.DbEnums.DocAUBUPeriodType?)this.IdDocAUBUPeriodType_PFG2; } 
								set { this.IdDocAUBUPeriodType_PFG2 = (byte?) value; }
							}

		/// <summary>
		/// Вести доп. потребности
		/// </summary>
		public bool? IsAdditionalNeed{get; set;}

		/// <summary>
		/// Требования к заданию
		/// </summary>
		private ICollection<Sbor.Tablepart.PlanActivity_RequirementsForTheTask> _tpRequirementsForTheTasks; 
        /// <summary>
        /// Требования к заданию
        /// </summary>
        [JsonIgnoreForException]
		public virtual ICollection<Sbor.Tablepart.PlanActivity_RequirementsForTheTask> RequirementsForTheTasks 
		{
			get{ return _tpRequirementsForTheTasks ?? (_tpRequirementsForTheTasks = new List<Sbor.Tablepart.PlanActivity_RequirementsForTheTask>()); } 
			set{ _tpRequirementsForTheTasks = value; }
		}

		/// <summary>
		/// Порядок контроля за исполнением задания
		/// </summary>
		private ICollection<Sbor.Tablepart.PlanActivity_OrderOfControlTheExecutionTasks> _tpOrderOfControlTheExecutionTaskss; 
        /// <summary>
        /// Порядок контроля за исполнением задания
        /// </summary>
        [JsonIgnoreForException]
		public virtual ICollection<Sbor.Tablepart.PlanActivity_OrderOfControlTheExecutionTasks> OrderOfControlTheExecutionTaskss 
		{
			get{ return _tpOrderOfControlTheExecutionTaskss ?? (_tpOrderOfControlTheExecutionTaskss = new List<Sbor.Tablepart.PlanActivity_OrderOfControlTheExecutionTasks>()); } 
			set{ _tpOrderOfControlTheExecutionTaskss = value; }
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
		/// Требует согласования
		/// </summary>
		public bool IsRequireCheck{get; set;}

	

		/// <summary>
		/// Конструктор по-умолчанию
		/// </summary>
		public PlanActivity()
		{
		}

		/// <summary>
		/// Идентификатор типа сущности
		/// </summary>
		public override int EntityId
		{
			get { return -2013265436; }
		}

		/// <summary>
		/// Идентификатор типа сущности
		/// </summary>
		public static int EntityIdStatic
		{
			get { return -2013265436; }
		}

		/// <summary>
		/// Русское наименование типа сущности
		/// </summary>
		public override string EntityCaption
		{
			get { return "План деятельности"; }
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
				Platform.BusinessLogic.EntityIds.Add(GetType().DeclaringType,-2013265436);
			}
		}


	}
}