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
	/// Долгосрочная целевая программа
	/// </summary>
	public partial class LongTermGoalProgram : DocumentEntity<LongTermGoalProgram>    , IHierarhy  
	{
	
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
		/// Статус
		/// </summary>
		public override int IdDocStatus{get; set;}

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
		public bool? HasMasterDoc{get; set;}

		/// <summary>
		/// Государственная программа
		/// </summary>
		public int? IdMasterStateProgram{get; set;}
        /// <summary>
	    /// Государственная программа
	    /// </summary>
		public virtual Sbor.Document.StateProgram MasterStateProgram{get; set;}
		

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
		public virtual Sbor.Document.LongTermGoalProgram Parent{get; set;}
		private ICollection<Sbor.Document.LongTermGoalProgram> _idParent; 
        /// <summary>
        /// Предыдущая редакция
        /// </summary>
        [JsonIgnoreForException]
		public virtual ICollection<Sbor.Document.LongTermGoalProgram> ChildrenByidParent 
		{
			get{ return _idParent ?? (_idParent = new List<Sbor.Document.LongTermGoalProgram>()); } 
			set{ _idParent = value; }
		}

		/// <summary>
		/// Элементы СЦ
		/// </summary>
		private ICollection<Sbor.Tablepart.LongTermGoalProgram_SystemGoalElement> _tpSystemGoalElement; 
        /// <summary>
        /// Элементы СЦ
        /// </summary>
        [JsonIgnoreForException]
		public virtual ICollection<Sbor.Tablepart.LongTermGoalProgram_SystemGoalElement> SystemGoalElement 
		{
			get{ return _tpSystemGoalElement ?? (_tpSystemGoalElement = new List<Sbor.Tablepart.LongTermGoalProgram_SystemGoalElement>()); } 
			set{ _tpSystemGoalElement = value; }
		}

		/// <summary>
		/// Целевые показатели
		/// </summary>
		private ICollection<Sbor.Tablepart.LongTermGoalProgram_GoalIndicator> _tpGoalIndicator; 
        /// <summary>
        /// Целевые показатели
        /// </summary>
        [JsonIgnoreForException]
		public virtual ICollection<Sbor.Tablepart.LongTermGoalProgram_GoalIndicator> GoalIndicator 
		{
			get{ return _tpGoalIndicator ?? (_tpGoalIndicator = new List<Sbor.Tablepart.LongTermGoalProgram_GoalIndicator>()); } 
			set{ _tpGoalIndicator = value; }
		}

		/// <summary>
		/// Соисполнители
		/// </summary>
		private ICollection<Sbor.Tablepart.LongTermGoalProgram_CoExecutor> _tpSBPs; 
        /// <summary>
        /// Соисполнители
        /// </summary>
        [JsonIgnoreForException]
		public virtual ICollection<Sbor.Tablepart.LongTermGoalProgram_CoExecutor> SBPs 
		{
			get{ return _tpSBPs ?? (_tpSBPs = new List<Sbor.Tablepart.LongTermGoalProgram_CoExecutor>()); } 
			set{ _tpSBPs = value; }
		}

		/// <summary>
		/// Ресурсное обеспечение
		/// </summary>
		private ICollection<Sbor.Tablepart.LongTermGoalProgram_ResourceMaintenance> _tpResourceMaintenance; 
        /// <summary>
        /// Ресурсное обеспечение
        /// </summary>
        [JsonIgnoreForException]
		public virtual ICollection<Sbor.Tablepart.LongTermGoalProgram_ResourceMaintenance> ResourceMaintenance 
		{
			get{ return _tpResourceMaintenance ?? (_tpResourceMaintenance = new List<Sbor.Tablepart.LongTermGoalProgram_ResourceMaintenance>()); } 
			set{ _tpResourceMaintenance = value; }
		}

		/// <summary>
		/// Перечень подпрограмм
		/// </summary>
		private ICollection<Sbor.Tablepart.LongTermGoalProgram_ListSubProgram> _tpListSubProgram; 
        /// <summary>
        /// Перечень подпрограмм
        /// </summary>
        [JsonIgnoreForException]
		public virtual ICollection<Sbor.Tablepart.LongTermGoalProgram_ListSubProgram> ListSubProgram 
		{
			get{ return _tpListSubProgram ?? (_tpListSubProgram = new List<Sbor.Tablepart.LongTermGoalProgram_ListSubProgram>()); } 
			set{ _tpListSubProgram = value; }
		}

		/// <summary>
		/// Ресурсное обеспечение подпрограмм
		/// </summary>
		private ICollection<Sbor.Tablepart.LongTermGoalProgram_SubProgramResourceMaintenance> _tpSubProgramResourceMaintenance; 
        /// <summary>
        /// Ресурсное обеспечение подпрограмм
        /// </summary>
        [JsonIgnoreForException]
		public virtual ICollection<Sbor.Tablepart.LongTermGoalProgram_SubProgramResourceMaintenance> SubProgramResourceMaintenance 
		{
			get{ return _tpSubProgramResourceMaintenance ?? (_tpSubProgramResourceMaintenance = new List<Sbor.Tablepart.LongTermGoalProgram_SubProgramResourceMaintenance>()); } 
			set{ _tpSubProgramResourceMaintenance = value; }
		}

		/// <summary>
		/// Мероприятия
		/// </summary>
		private ICollection<Sbor.Tablepart.LongTermGoalProgram_Activity> _tpActivity; 
        /// <summary>
        /// Мероприятия
        /// </summary>
        [JsonIgnoreForException]
		public virtual ICollection<Sbor.Tablepart.LongTermGoalProgram_Activity> Activity 
		{
			get{ return _tpActivity ?? (_tpActivity = new List<Sbor.Tablepart.LongTermGoalProgram_Activity>()); } 
			set{ _tpActivity = value; }
		}

		/// <summary>
		/// Ресурсное обеспечение мероприятий
		/// </summary>
		private ICollection<Sbor.Tablepart.LongTermGoalProgram_ActivityResourceMaintenance> _tpActivityResourceMaintenance; 
        /// <summary>
        /// Ресурсное обеспечение мероприятий
        /// </summary>
        [JsonIgnoreForException]
		public virtual ICollection<Sbor.Tablepart.LongTermGoalProgram_ActivityResourceMaintenance> ActivityResourceMaintenance 
		{
			get{ return _tpActivityResourceMaintenance ?? (_tpActivityResourceMaintenance = new List<Sbor.Tablepart.LongTermGoalProgram_ActivityResourceMaintenance>()); } 
			set{ _tpActivityResourceMaintenance = value; }
		}

		/// <summary>
		/// Показатели качества мероприятий
		/// </summary>
		private ICollection<Sbor.Tablepart.LongTermGoalProgram_IndicatorActivity> _tpIndicatorActivity; 
        /// <summary>
        /// Показатели качества мероприятий
        /// </summary>
        [JsonIgnoreForException]
		public virtual ICollection<Sbor.Tablepart.LongTermGoalProgram_IndicatorActivity> IndicatorActivity 
		{
			get{ return _tpIndicatorActivity ?? (_tpIndicatorActivity = new List<Sbor.Tablepart.LongTermGoalProgram_IndicatorActivity>()); } 
			set{ _tpIndicatorActivity = value; }
		}

		/// <summary>
		/// Целевые показатели
		/// </summary>
		private ICollection<Sbor.Tablepart.LongTermGoalProgram_GoalIndicator_Value> _tpGoalIndicator_Value; 
        /// <summary>
        /// Целевые показатели
        /// </summary>
        [JsonIgnoreForException]
		public virtual ICollection<Sbor.Tablepart.LongTermGoalProgram_GoalIndicator_Value> GoalIndicator_Value 
		{
			get{ return _tpGoalIndicator_Value ?? (_tpGoalIndicator_Value = new List<Sbor.Tablepart.LongTermGoalProgram_GoalIndicator_Value>()); } 
			set{ _tpGoalIndicator_Value = value; }
		}

		/// <summary>
		/// Ресурсное обеспечение
		/// </summary>
		private ICollection<Sbor.Tablepart.LongTermGoalProgram_ResourceMaintenance_Value> _tpResourceMaintenance_Value; 
        /// <summary>
        /// Ресурсное обеспечение
        /// </summary>
        [JsonIgnoreForException]
		public virtual ICollection<Sbor.Tablepart.LongTermGoalProgram_ResourceMaintenance_Value> ResourceMaintenance_Value 
		{
			get{ return _tpResourceMaintenance_Value ?? (_tpResourceMaintenance_Value = new List<Sbor.Tablepart.LongTermGoalProgram_ResourceMaintenance_Value>()); } 
			set{ _tpResourceMaintenance_Value = value; }
		}

		/// <summary>
		/// Ресурсное обеспечение подпрограмм
		/// </summary>
		private ICollection<Sbor.Tablepart.LongTermGoalProgram_SubProgramResourceMaintenance_Value> _tpSubProgramResourceMaintenance_Value; 
        /// <summary>
        /// Ресурсное обеспечение подпрограмм
        /// </summary>
        [JsonIgnoreForException]
		public virtual ICollection<Sbor.Tablepart.LongTermGoalProgram_SubProgramResourceMaintenance_Value> SubProgramResourceMaintenance_Value 
		{
			get{ return _tpSubProgramResourceMaintenance_Value ?? (_tpSubProgramResourceMaintenance_Value = new List<Sbor.Tablepart.LongTermGoalProgram_SubProgramResourceMaintenance_Value>()); } 
			set{ _tpSubProgramResourceMaintenance_Value = value; }
		}

		/// <summary>
		/// Мероприятия - значения объемов
		/// </summary>
		private ICollection<Sbor.Tablepart.LongTermGoalProgram_Activity_Value> _tpActivity_Value; 
        /// <summary>
        /// Мероприятия - значения объемов
        /// </summary>
        [JsonIgnoreForException]
		public virtual ICollection<Sbor.Tablepart.LongTermGoalProgram_Activity_Value> Activity_Value 
		{
			get{ return _tpActivity_Value ?? (_tpActivity_Value = new List<Sbor.Tablepart.LongTermGoalProgram_Activity_Value>()); } 
			set{ _tpActivity_Value = value; }
		}

		/// <summary>
		/// Ресурсное обеспечение мероприятий
		/// </summary>
		private ICollection<Sbor.Tablepart.LongTermGoalProgram_ActivityResourceMaintenance_Value> _tpActivityResourceMaintenance_Value; 
        /// <summary>
        /// Ресурсное обеспечение мероприятий
        /// </summary>
        [JsonIgnoreForException]
		public virtual ICollection<Sbor.Tablepart.LongTermGoalProgram_ActivityResourceMaintenance_Value> ActivityResourceMaintenance_Value 
		{
			get{ return _tpActivityResourceMaintenance_Value ?? (_tpActivityResourceMaintenance_Value = new List<Sbor.Tablepart.LongTermGoalProgram_ActivityResourceMaintenance_Value>()); } 
			set{ _tpActivityResourceMaintenance_Value = value; }
		}

		/// <summary>
		/// Показатели качества мероприятий
		/// </summary>
		private ICollection<Sbor.Tablepart.LongTermGoalProgram_IndicatorActivity_Value> _tpIndicatorActivity_Value; 
        /// <summary>
        /// Показатели качества мероприятий
        /// </summary>
        [JsonIgnoreForException]
		public virtual ICollection<Sbor.Tablepart.LongTermGoalProgram_IndicatorActivity_Value> IndicatorActivity_Value 
		{
			get{ return _tpIndicatorActivity_Value ?? (_tpIndicatorActivity_Value = new List<Sbor.Tablepart.LongTermGoalProgram_IndicatorActivity_Value>()); } 
			set{ _tpIndicatorActivity_Value = value; }
		}

		/// <summary>
		/// Причина отказа
		/// </summary>
		public string ReasonCancel{get; set;}

		/// <summary>
		/// Долгосрочная целевая программа
		/// </summary>
		public int? IdMasterLongTermGoalProgram{get; set;}
        /// <summary>
	    /// Долгосрочная целевая программа
	    /// </summary>
		public virtual Sbor.Document.LongTermGoalProgram MasterLongTermGoalProgram{get; set;}
		private ICollection<Sbor.Document.LongTermGoalProgram> _idMasterLongTermGoalProgram; 
        /// <summary>
        /// Долгосрочная целевая программа
        /// </summary>
        [JsonIgnoreForException]
		public virtual ICollection<Sbor.Document.LongTermGoalProgram> ChildrenByidMasterLongTermGoalProgram 
		{
			get{ return _idMasterLongTermGoalProgram ?? (_idMasterLongTermGoalProgram = new List<Sbor.Document.LongTermGoalProgram>()); } 
			set{ _idMasterLongTermGoalProgram = value; }
		}

		/// <summary>
		/// Заголовок
		/// </summary>
		public string Header{get; set;}

		/// <summary>
		/// Вести доп. потребности
		/// </summary>
		public bool HasAdditionalNeed{get; set;}

	

		/// <summary>
		/// Конструктор по-умолчанию
		/// </summary>
		public LongTermGoalProgram()
		{
		}

		/// <summary>
		/// Идентификатор типа сущности
		/// </summary>
		public override int EntityId
		{
			get { return -1543503791; }
		}

		/// <summary>
		/// Идентификатор типа сущности
		/// </summary>
		public static int EntityIdStatic
		{
			get { return -1543503791; }
		}

		/// <summary>
		/// Русское наименование типа сущности
		/// </summary>
		public override string EntityCaption
		{
			get { return "Долгосрочная целевая программа"; }
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
				Platform.BusinessLogic.EntityIds.Add(GetType().DeclaringType,-1543503791);
			}
		}


	}
}