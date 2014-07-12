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
	/// Деятельность ведомства
	/// </summary>
	public partial class ActivityOfSBP : DocumentEntity<ActivityOfSBP>    , IHierarhy  
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
		/// Тип документа
		/// </summary>
		public int IdDocType{get; set;}
        /// <summary>
	    /// Тип документа
	    /// </summary>
		public virtual Sbor.Reference.DocType DocType{get; set;}
		

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
		/// Код
		/// </summary>
		public int? IdAnalyticalCodeStateProgram{get; set;}
        /// <summary>
	    /// Код
	    /// </summary>
		public virtual Sbor.Reference.AnalyticalCodeStateProgram AnalyticalCodeStateProgram{get; set;}
		

		/// <summary>
		/// Срок реализации с
		/// </summary>
		private DateTime _DateStart; 
        /// <summary>
	    /// Срок реализации с
	    /// </summary>
		public  DateTime DateStart 
		{
			get{ return _DateStart.Date; }
			set{ _DateStart = value.Date; }
		}

		/// <summary>
		/// Срок реализации по
		/// </summary>
		private DateTime _DateEnd; 
        /// <summary>
	    /// Срок реализации по
	    /// </summary>
		public  DateTime DateEnd 
		{
			get{ return _DateEnd.Date; }
			set{ _DateEnd = value.Date; }
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
		public bool IsApproved{get; set;}

		/// <summary>
		/// Наименование
		/// </summary>
		public string Caption{get; set;}

		/// <summary>
		/// Тип ответственного исполнителя
		/// </summary>
		public int IdResponsibleExecutantType{get; set;}
        /// <summary>
	    /// Тип ответственного исполнителя
	    /// </summary>
		public virtual Sbor.Reference.ResponsibleExecutantType ResponsibleExecutantType{get; set;}
		

		/// <summary>
		/// Ответственный исполнитель
		/// </summary>
		public int IdSBP{get; set;}
        /// <summary>
	    /// Ответственный исполнитель
	    /// </summary>
		public virtual Sbor.Reference.SBP SBP{get; set;}
		

		/// <summary>
		/// В составе ГП
		/// </summary>
		public bool HasMasterDoc{get; set;}

		/// <summary>
		/// Государственная программа / подпрограмма ГП 
		/// </summary>
		public int? IdMasterDoc{get; set;}
        /// <summary>
	    /// Государственная программа / подпрограмма ГП 
	    /// </summary>
		public virtual Sbor.Document.StateProgram MasterDoc{get; set;}
		

		/// <summary>
		/// Описание
		/// </summary>
		public string Description{get; set;}

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
		/// Предыдущая редакция
		/// </summary>
		public int? IdParent{get; set;}
        /// <summary>
	    /// Предыдущая редакция
	    /// </summary>
		public virtual Sbor.Document.ActivityOfSBP Parent{get; set;}
		private ICollection<Sbor.Document.ActivityOfSBP> _idParent; 
        /// <summary>
        /// Предыдущая редакция
        /// </summary>
        [JsonIgnoreForException]
		public virtual ICollection<Sbor.Document.ActivityOfSBP> ChildrenByidParent 
		{
			get{ return _idParent ?? (_idParent = new List<Sbor.Document.ActivityOfSBP>()); } 
			set{ _idParent = value; }
		}

		/// <summary>
		/// Элементы СЦ
		/// </summary>
		private ICollection<Sbor.Tablepart.ActivityOfSBP_SystemGoalElement> _tpSystemGoalElement; 
        /// <summary>
        /// Элементы СЦ
        /// </summary>
        [JsonIgnoreForException]
		public virtual ICollection<Sbor.Tablepart.ActivityOfSBP_SystemGoalElement> SystemGoalElement 
		{
			get{ return _tpSystemGoalElement ?? (_tpSystemGoalElement = new List<Sbor.Tablepart.ActivityOfSBP_SystemGoalElement>()); } 
			set{ _tpSystemGoalElement = value; }
		}

		/// <summary>
		/// Целевые показатели
		/// </summary>
		private ICollection<Sbor.Tablepart.ActivityOfSBP_GoalIndicator> _tpGoalIndicator; 
        /// <summary>
        /// Целевые показатели
        /// </summary>
        [JsonIgnoreForException]
		public virtual ICollection<Sbor.Tablepart.ActivityOfSBP_GoalIndicator> GoalIndicator 
		{
			get{ return _tpGoalIndicator ?? (_tpGoalIndicator = new List<Sbor.Tablepart.ActivityOfSBP_GoalIndicator>()); } 
			set{ _tpGoalIndicator = value; }
		}

		/// <summary>
		/// Ресурсное обеспечение
		/// </summary>
		private ICollection<Sbor.Tablepart.ActivityOfSBP_ResourceMaintenance> _tpResourceMaintenance; 
        /// <summary>
        /// Ресурсное обеспечение
        /// </summary>
        [JsonIgnoreForException]
		public virtual ICollection<Sbor.Tablepart.ActivityOfSBP_ResourceMaintenance> ResourceMaintenance 
		{
			get{ return _tpResourceMaintenance ?? (_tpResourceMaintenance = new List<Sbor.Tablepart.ActivityOfSBP_ResourceMaintenance>()); } 
			set{ _tpResourceMaintenance = value; }
		}

		/// <summary>
		/// Мероприятия
		/// </summary>
		private ICollection<Sbor.Tablepart.ActivityOfSBP_Activity> _tpActivity; 
        /// <summary>
        /// Мероприятия
        /// </summary>
        [JsonIgnoreForException]
		public virtual ICollection<Sbor.Tablepart.ActivityOfSBP_Activity> Activity 
		{
			get{ return _tpActivity ?? (_tpActivity = new List<Sbor.Tablepart.ActivityOfSBP_Activity>()); } 
			set{ _tpActivity = value; }
		}

		/// <summary>
		/// Ресурсное обеспечение мероприятий
		/// </summary>
		private ICollection<Sbor.Tablepart.ActivityOfSBP_ActivityResourceMaintenance> _tpActivityResourceMaintenance; 
        /// <summary>
        /// Ресурсное обеспечение мероприятий
        /// </summary>
        [JsonIgnoreForException]
		public virtual ICollection<Sbor.Tablepart.ActivityOfSBP_ActivityResourceMaintenance> ActivityResourceMaintenance 
		{
			get{ return _tpActivityResourceMaintenance ?? (_tpActivityResourceMaintenance = new List<Sbor.Tablepart.ActivityOfSBP_ActivityResourceMaintenance>()); } 
			set{ _tpActivityResourceMaintenance = value; }
		}

		/// <summary>
		/// Показатели качества мероприятий
		/// </summary>
		private ICollection<Sbor.Tablepart.ActivityOfSBP_IndicatorQualityActivity> _tpIndicatorActivity; 
        /// <summary>
        /// Показатели качества мероприятий
        /// </summary>
        [JsonIgnoreForException]
		public virtual ICollection<Sbor.Tablepart.ActivityOfSBP_IndicatorQualityActivity> IndicatorActivity 
		{
			get{ return _tpIndicatorActivity ?? (_tpIndicatorActivity = new List<Sbor.Tablepart.ActivityOfSBP_IndicatorQualityActivity>()); } 
			set{ _tpIndicatorActivity = value; }
		}

		/// <summary>
		/// Ресурсное обеспечение мероприятий
		/// </summary>
		private ICollection<Sbor.Tablepart.ActivityOfSBP_ActivityResourceMaintenance_Value> _tpActivityResourceMaintenance_Value; 
        /// <summary>
        /// Ресурсное обеспечение мероприятий
        /// </summary>
        [JsonIgnoreForException]
		public virtual ICollection<Sbor.Tablepart.ActivityOfSBP_ActivityResourceMaintenance_Value> ActivityResourceMaintenance_Value 
		{
			get{ return _tpActivityResourceMaintenance_Value ?? (_tpActivityResourceMaintenance_Value = new List<Sbor.Tablepart.ActivityOfSBP_ActivityResourceMaintenance_Value>()); } 
			set{ _tpActivityResourceMaintenance_Value = value; }
		}

		/// <summary>
		/// Мероприятия
		/// </summary>
		private ICollection<Sbor.Tablepart.ActivityOfSBP_Activity_Value> _tpActivity_Value; 
        /// <summary>
        /// Мероприятия
        /// </summary>
        [JsonIgnoreForException]
		public virtual ICollection<Sbor.Tablepart.ActivityOfSBP_Activity_Value> Activity_Value 
		{
			get{ return _tpActivity_Value ?? (_tpActivity_Value = new List<Sbor.Tablepart.ActivityOfSBP_Activity_Value>()); } 
			set{ _tpActivity_Value = value; }
		}

		/// <summary>
		/// Ресурсное обеспечение
		/// </summary>
		private ICollection<Sbor.Tablepart.ActivityOfSBP_ResourceMaintenance_Value> _tpResourceMaintenance_Value; 
        /// <summary>
        /// Ресурсное обеспечение
        /// </summary>
        [JsonIgnoreForException]
		public virtual ICollection<Sbor.Tablepart.ActivityOfSBP_ResourceMaintenance_Value> ResourceMaintenance_Value 
		{
			get{ return _tpResourceMaintenance_Value ?? (_tpResourceMaintenance_Value = new List<Sbor.Tablepart.ActivityOfSBP_ResourceMaintenance_Value>()); } 
			set{ _tpResourceMaintenance_Value = value; }
		}

		/// <summary>
		/// Целевые показатели
		/// </summary>
		private ICollection<Sbor.Tablepart.ActivityOfSBP_GoalIndicator_Value> _tpGoalIndicator_Value; 
        /// <summary>
        /// Целевые показатели
        /// </summary>
        [JsonIgnoreForException]
		public virtual ICollection<Sbor.Tablepart.ActivityOfSBP_GoalIndicator_Value> GoalIndicator_Value 
		{
			get{ return _tpGoalIndicator_Value ?? (_tpGoalIndicator_Value = new List<Sbor.Tablepart.ActivityOfSBP_GoalIndicator_Value>()); } 
			set{ _tpGoalIndicator_Value = value; }
		}

		/// <summary>
		/// Показатели качества мероприятий
		/// </summary>
		private ICollection<Sbor.Tablepart.ActivityOfSBP_IndicatorQualityActivity_Value> _tpIndicatorQualityActivity_Value; 
        /// <summary>
        /// Показатели качества мероприятий
        /// </summary>
        [JsonIgnoreForException]
		public virtual ICollection<Sbor.Tablepart.ActivityOfSBP_IndicatorQualityActivity_Value> IndicatorQualityActivity_Value 
		{
			get{ return _tpIndicatorQualityActivity_Value ?? (_tpIndicatorQualityActivity_Value = new List<Sbor.Tablepart.ActivityOfSBP_IndicatorQualityActivity_Value>()); } 
			set{ _tpIndicatorQualityActivity_Value = value; }
		}

		/// <summary>
		/// Причина отказа
		/// </summary>
		public string ReasonCancel{get; set;}

		/// <summary>
		/// Статус
		/// </summary>
		public override int IdDocStatus{get; set;}

		/// <summary>
		/// Заголовок
		/// </summary>
		public string Header{get; set;}

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
		/// Вести доп. потребности
		/// </summary>
		public bool HasAdditionalNeed{get; set;}

		/// <summary>
		/// Спрос и мощность мероприятий
		/// </summary>
		private ICollection<Sbor.Tablepart.ActivityOfSBP_ActivityDemandAndCapacity> _tpActivityDemandAndCapacity; 
        /// <summary>
        /// Спрос и мощность мероприятий
        /// </summary>
        [JsonIgnoreForException]
		public virtual ICollection<Sbor.Tablepart.ActivityOfSBP_ActivityDemandAndCapacity> ActivityDemandAndCapacity 
		{
			get{ return _tpActivityDemandAndCapacity ?? (_tpActivityDemandAndCapacity = new List<Sbor.Tablepart.ActivityOfSBP_ActivityDemandAndCapacity>()); } 
			set{ _tpActivityDemandAndCapacity = value; }
		}

		/// <summary>
		/// Спрос и мощность мероприятий - значения
		/// </summary>
		private ICollection<Sbor.Tablepart.ActivityOfSBP_ActivityDemandAndCapacity_Value> _tpActivityDemandAndCapacity_Value; 
        /// <summary>
        /// Спрос и мощность мероприятий - значения
        /// </summary>
        [JsonIgnoreForException]
		public virtual ICollection<Sbor.Tablepart.ActivityOfSBP_ActivityDemandAndCapacity_Value> ActivityDemandAndCapacity_Value 
		{
			get{ return _tpActivityDemandAndCapacity_Value ?? (_tpActivityDemandAndCapacity_Value = new List<Sbor.Tablepart.ActivityOfSBP_ActivityDemandAndCapacity_Value>()); } 
			set{ _tpActivityDemandAndCapacity_Value = value; }
		}

		/// <summary>
		/// Актуальные бланки формирования
		/// </summary>
		private ICollection<Sbor.Tablepart.ActivityOfSBP_SBPBlankActual> _tpSBPBlankActuals; 
        /// <summary>
        /// Актуальные бланки формирования
        /// </summary>
        [JsonIgnoreForException]
		public virtual ICollection<Sbor.Tablepart.ActivityOfSBP_SBPBlankActual> SBPBlankActuals 
		{
			get{ return _tpSBPBlankActuals ?? (_tpSBPBlankActuals = new List<Sbor.Tablepart.ActivityOfSBP_SBPBlankActual>()); } 
			set{ _tpSBPBlankActuals = value; }
		}

	

		/// <summary>
		/// Конструктор по-умолчанию
		/// </summary>
		public ActivityOfSBP()
		{
		}

		/// <summary>
		/// Идентификатор типа сущности
		/// </summary>
		public override int EntityId
		{
			get { return -1543503797; }
		}

		/// <summary>
		/// Идентификатор типа сущности
		/// </summary>
		public static int EntityIdStatic
		{
			get { return -1543503797; }
		}

		/// <summary>
		/// Русское наименование типа сущности
		/// </summary>
		public override string EntityCaption
		{
			get { return "Деятельность ведомства"; }
		}

		
        /// <summary>
	    /// Наименование типа документа
	    /// </summary>
        public override string DocumentCaption 
        {
            get
            {
                if (DocType==null)
                    return base.DocumentCaption;
                else
                    return DocType.Caption;
                    
            }
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
				Platform.BusinessLogic.EntityIds.Add(GetType().DeclaringType,-1543503797);
			}
		}


	}
}