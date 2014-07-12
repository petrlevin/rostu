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

using Platform.PrimaryEntities.Reference;using Platform.PrimaryEntities.DbEnums;using Platform.PrimaryEntities.Common.DbEnums;

namespace Sbor.Reference
{
	// Это автоматически сгенерированный класс, изменения будут уничтожены при следующей генерации
	/// <summary>
	/// Система целеполагания
	/// </summary>
	public partial class SystemGoal : ReferenceEntity, IHasRefStatus    , IHierarhy  
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
		/// Вышестоящий
		/// </summary>
		public int? IdParent{get; set;}
        /// <summary>
	    /// Вышестоящий
	    /// </summary>
		public virtual Sbor.Reference.SystemGoal Parent{get; set;}
		private ICollection<Sbor.Reference.SystemGoal> _idParent; 
        /// <summary>
        /// Вышестоящий
        /// </summary>
        [JsonIgnoreForException]
		public virtual ICollection<Sbor.Reference.SystemGoal> ChildrenByidParent 
		{
			get{ return _idParent ?? (_idParent = new List<Sbor.Reference.SystemGoal>()); } 
			set{ _idParent = value; }
		}

		/// <summary>
		/// Код
		/// </summary>
		public string Code{get; set;}

		/// <summary>
		/// Наименование
		/// </summary>
		public string Caption{get; set;}

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
		/// Тип элемента СЦ
		/// </summary>
		public int IdElementTypeSystemGoal{get; set;}
        /// <summary>
	    /// Тип элемента СЦ
	    /// </summary>
		public virtual Sbor.Reference.ElementTypeSystemGoal ElementTypeSystemGoal{get; set;}
		

		/// <summary>
		/// Ответственный исполнитель
		/// </summary>
		public int? IdSBP{get; set;}
        /// <summary>
	    /// Ответственный исполнитель
	    /// </summary>
		public virtual Sbor.Reference.SBP SBP{get; set;}
		

		/// <summary>
		/// Тип утверждающего документа
		/// </summary>
		public int IdDocType_CommitDoc{get; set;}
        /// <summary>
	    /// Тип утверждающего документа
	    /// </summary>
		public virtual Sbor.Reference.DocType DocType_CommitDoc{get; set;}
		

		/// <summary>
		/// Целевые показатели
		/// </summary>
		private ICollection<Sbor.Tablepart.SystemGoal_GoalIndicator> _tpGoalIndicator; 
        /// <summary>
        /// Целевые показатели
        /// </summary>
        [JsonIgnoreForException]
		public virtual ICollection<Sbor.Tablepart.SystemGoal_GoalIndicator> GoalIndicator 
		{
			get{ return _tpGoalIndicator ?? (_tpGoalIndicator = new List<Sbor.Tablepart.SystemGoal_GoalIndicator>()); } 
			set{ _tpGoalIndicator = value; }
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
		/// Целевые показатели
		/// </summary>
		private ICollection<Sbor.Tablepart.SystemGoal_GoalIndicatorValue> _tpGoalIndicatorValue; 
        /// <summary>
        /// Целевые показатели
        /// </summary>
        [JsonIgnoreForException]
		public virtual ICollection<Sbor.Tablepart.SystemGoal_GoalIndicatorValue> GoalIndicatorValue 
		{
			get{ return _tpGoalIndicatorValue ?? (_tpGoalIndicatorValue = new List<Sbor.Tablepart.SystemGoal_GoalIndicatorValue>()); } 
			set{ _tpGoalIndicatorValue = value; }
		}

		/// <summary>
		/// Дата утверждения
		/// </summary>
		private DateTime? _DateCommit; 
        /// <summary>
	    /// Дата утверждения
	    /// </summary>
		public  DateTime? DateCommit 
		{
			get{ return _DateCommit != null ? ((DateTime)_DateCommit).Date : (DateTime?)null; }
			set{ _DateCommit = value != null ? ((DateTime)value).Date : value; }
		}

		/// <summary>
		/// Изменен после утверждения
		/// </summary>
		public bool? IsChanged{get; set;}

		/// <summary>
		/// Утверждающий документ
		/// </summary>
		public int? IdCommitDoc{get; set;}

		/// <summary>
		/// Ссылка на сущность
		/// </summary>
		public int? IdCommitDocEntity{get; set;}
        /// <summary>
	    /// Ссылка на сущность
	    /// </summary>
		public virtual Entity CommitDocEntity{get; set;}
		

		/// <summary>
		/// Реализующий документ
		/// </summary>
		public int? IdImplementDoc{get; set;}

		/// <summary>
		/// Ссылка на сущность
		/// </summary>
		public int? IdImplementDocEntity{get; set;}
        /// <summary>
	    /// Ссылка на сущность
	    /// </summary>
		public virtual Entity ImplementDocEntity{get; set;}
		

		/// <summary>
		/// Тип реализующего документа
		/// </summary>
		public int? IdDocType_ImplementDoc{get; set;}
        /// <summary>
	    /// Тип реализующего документа
	    /// </summary>
		public virtual Sbor.Reference.DocType DocType_ImplementDoc{get; set;}
		

		/// <summary>
		/// Показатели вышестоящей цели
		/// </summary>
		private ICollection<Sbor.Tablepart.SystemGoal_GoalIndicatorParent> _tpGoalIndicatorParent; 
        /// <summary>
        /// Показатели вышестоящей цели
        /// </summary>
        [JsonIgnoreForException]
		public virtual ICollection<Sbor.Tablepart.SystemGoal_GoalIndicatorParent> GoalIndicatorParent 
		{
			get{ return _tpGoalIndicatorParent ?? (_tpGoalIndicatorParent = new List<Sbor.Tablepart.SystemGoal_GoalIndicatorParent>()); } 
			set{ _tpGoalIndicatorParent = value; }
		}

	

		/// <summary>
		/// Конструктор по-умолчанию
		/// </summary>
		public SystemGoal()
		{
		}

		/// <summary>
		/// Идентификатор типа сущности
		/// </summary>
		public override int EntityId
		{
			get { return -2013265862; }
		}

		/// <summary>
		/// Идентификатор типа сущности
		/// </summary>
		public static int EntityIdStatic
		{
			get { return -2013265862; }
		}

		/// <summary>
		/// Русское наименование типа сущности
		/// </summary>
		public override string EntityCaption
		{
			get { return "Система целеполагания"; }
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
				Platform.BusinessLogic.EntityIds.Add(GetType().DeclaringType,-2013265862);
			}
		}


	}
}