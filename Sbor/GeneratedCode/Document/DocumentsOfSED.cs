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
	/// Документы СЭР
	/// </summary>
	public partial class DocumentsOfSED : DocumentEntity<DocumentsOfSED>    , IHierarhy  
	{
	
		/// <summary>
		/// ППО
		/// </summary>
		public int IdPublicLegalFormation{get; set;}
        /// <summary>
	    /// ППО
	    /// </summary>
		public virtual BaseApp.Reference.PublicLegalFormation PublicLegalFormation{get; set;}
		

		/// <summary>
		/// Версия
		/// </summary>
		public int IdVersion{get; set;}
        /// <summary>
	    /// Версия
	    /// </summary>
		public virtual BaseApp.Reference.Version Version{get; set;}
		

		/// <summary>
		/// Тип документа
		/// </summary>
		public int IdDocType{get; set;}
        /// <summary>
	    /// Тип документа
	    /// </summary>
		public virtual Sbor.Reference.DocType DocType{get; set;}
		

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
		/// Элементы СЦ
		/// </summary>
		private ICollection<Sbor.Tablepart.DocumentsOfSED_ItemsSystemGoal> _tpItemsSystemGoals; 
        /// <summary>
        /// Элементы СЦ
        /// </summary>
        [JsonIgnoreForException]
		public virtual ICollection<Sbor.Tablepart.DocumentsOfSED_ItemsSystemGoal> ItemsSystemGoals 
		{
			get{ return _tpItemsSystemGoals ?? (_tpItemsSystemGoals = new List<Sbor.Tablepart.DocumentsOfSED_ItemsSystemGoal>()); } 
			set{ _tpItemsSystemGoals = value; }
		}

		/// <summary>
		/// Целевые показатели
		/// </summary>
		private ICollection<Sbor.Tablepart.DocumentsOfSED_GoalIndicator> _tpGoalIndicators; 
        /// <summary>
        /// Целевые показатели
        /// </summary>
        [JsonIgnoreForException]
		public virtual ICollection<Sbor.Tablepart.DocumentsOfSED_GoalIndicator> GoalIndicators 
		{
			get{ return _tpGoalIndicators ?? (_tpGoalIndicators = new List<Sbor.Tablepart.DocumentsOfSED_GoalIndicator>()); } 
			set{ _tpGoalIndicators = value; }
		}

		/// <summary>
		/// Описание
		/// </summary>
		public string Description{get; set;}

		/// <summary>
		/// Статус
		/// </summary>
		public override int IdDocStatus{get; set;}

		/// <summary>
		/// Целевые показатели
		/// </summary>
		private ICollection<Sbor.Tablepart.DocumentsOfSED_GoalIndicatorValue> _tpGoalIndicatorValues; 
        /// <summary>
        /// Целевые показатели
        /// </summary>
        [JsonIgnoreForException]
		public virtual ICollection<Sbor.Tablepart.DocumentsOfSED_GoalIndicatorValue> GoalIndicatorValues 
		{
			get{ return _tpGoalIndicatorValues ?? (_tpGoalIndicatorValues = new List<Sbor.Tablepart.DocumentsOfSED_GoalIndicatorValue>()); } 
			set{ _tpGoalIndicatorValues = value; }
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
		/// Предыдущая редакция
		/// </summary>
		public int? IdParent{get; set;}
        /// <summary>
	    /// Предыдущая редакция
	    /// </summary>
		public virtual Sbor.Document.DocumentsOfSED Parent{get; set;}
		private ICollection<Sbor.Document.DocumentsOfSED> _idParent; 
        /// <summary>
        /// Предыдущая редакция
        /// </summary>
        [JsonIgnoreForException]
		public virtual ICollection<Sbor.Document.DocumentsOfSED> ChildrenByidParent 
		{
			get{ return _idParent ?? (_idParent = new List<Sbor.Document.DocumentsOfSED>()); } 
			set{ _idParent = value; }
		}

		/// <summary>
		/// Идентификатор
		/// </summary>
		
        public override Int32 Id{get; set;}

		/// <summary>
		/// Наименование
		/// </summary>
		public string Caption{get; set;}

	

		/// <summary>
		/// Конструктор по-умолчанию
		/// </summary>
		public DocumentsOfSED()
		{
		}

		/// <summary>
		/// Идентификатор типа сущности
		/// </summary>
		public override int EntityId
		{
			get { return -1677721568; }
		}

		/// <summary>
		/// Идентификатор типа сущности
		/// </summary>
		public static int EntityIdStatic
		{
			get { return -1677721568; }
		}

		/// <summary>
		/// Русское наименование типа сущности
		/// </summary>
		public override string EntityCaption
		{
			get { return "Документы СЭР"; }
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
				Platform.BusinessLogic.EntityIds.Add(GetType().DeclaringType,-1677721568);
			}
		}


	}
}