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



namespace Sbor.Tablepart
{
	// Это автоматически сгенерированный класс, изменения будут уничтожены при следующей генерации
	/// <summary>
	/// Элементы СЦ
	/// </summary>
	public partial class LongTermGoalProgram_SystemGoalElement : TablePartEntity    , IHierarhy  
	{
	
		/// <summary>
		/// Ссылка на владельца
		/// </summary>
		public int IdOwner{get; set;}
        /// <summary>
	    /// Ссылка на владельца
	    /// </summary>
		public virtual Sbor.Document.LongTermGoalProgram Owner{get; set;}
		

		/// <summary>
		/// Идентификатор
		/// </summary>
		
        public override Int32 Id{get; set;}

		/// <summary>
		/// Из другого документа СЦ
		/// </summary>
		public bool FromAnotherDocumentSE{get; set;}

		/// <summary>
		/// Наименование
		/// </summary>
		public int IdSystemGoal{get; set;}
        /// <summary>
	    /// Наименование
	    /// </summary>
		public virtual Sbor.Reference.SystemGoal SystemGoal{get; set;}
		

		/// <summary>
		/// Основная цель
		/// </summary>
		public bool IsMainGoal{get; set;}

		/// <summary>
		/// Код
		/// </summary>
		public string Code{get; set;}

		/// <summary>
		/// Тип
		/// </summary>
		public int? IdElementTypeSystemGoal{get; set;}
        /// <summary>
	    /// Тип
	    /// </summary>
		public virtual Sbor.Reference.ElementTypeSystemGoal ElementTypeSystemGoal{get; set;}
		

		/// <summary>
		/// Срок реализации с
		/// </summary>
		private DateTime? _DateStart; 
        /// <summary>
	    /// Срок реализации с
	    /// </summary>
		public  DateTime? DateStart 
		{
			get{ return _DateStart != null ? ((DateTime)_DateStart).Date : (DateTime?)null; }
			set{ _DateStart = value != null ? ((DateTime)value).Date : value; }
		}

		/// <summary>
		/// Срок реализации по
		/// </summary>
		private DateTime? _DateEnd; 
        /// <summary>
	    /// Срок реализации по
	    /// </summary>
		public  DateTime? DateEnd 
		{
			get{ return _DateEnd != null ? ((DateTime)_DateEnd).Date : (DateTime?)null; }
			set{ _DateEnd = value != null ? ((DateTime)value).Date : value; }
		}

		/// <summary>
		/// Ответственный исполнитель
		/// </summary>
		public int? IdSBP{get; set;}
        /// <summary>
	    /// Ответственный исполнитель
	    /// </summary>
		public virtual Sbor.Reference.SBP SBP{get; set;}
		

		/// <summary>
		/// Вышестоящий
		/// </summary>
		public int? IdParent{get; set;}
        /// <summary>
	    /// Вышестоящий
	    /// </summary>
		public virtual Sbor.Tablepart.LongTermGoalProgram_SystemGoalElement Parent{get; set;}
		private ICollection<Sbor.Tablepart.LongTermGoalProgram_SystemGoalElement> _idParent; 
        /// <summary>
        /// Вышестоящий
        /// </summary>
        [JsonIgnoreForException]
		public virtual ICollection<Sbor.Tablepart.LongTermGoalProgram_SystemGoalElement> ChildrenByidParent 
		{
			get{ return _idParent ?? (_idParent = new List<Sbor.Tablepart.LongTermGoalProgram_SystemGoalElement>()); } 
			set{ _idParent = value; }
		}

			private ICollection<Sbor.Tablepart.LongTermGoalProgram_GoalIndicator> _LongTermGoalProgram_GoalIndicator; 
        /// <summary>
        /// Элемент СЦ
        /// </summary>
        [JsonIgnoreForException]
		public virtual ICollection<Sbor.Tablepart.LongTermGoalProgram_GoalIndicator> LongTermGoalProgram_GoalIndicator 
		{
			get{ return _LongTermGoalProgram_GoalIndicator ?? (_LongTermGoalProgram_GoalIndicator = new List<Sbor.Tablepart.LongTermGoalProgram_GoalIndicator>()); } 
			set{ _LongTermGoalProgram_GoalIndicator = value; }
		}
		private ICollection<Sbor.Tablepart.LongTermGoalProgram_Activity> _LongTermGoalProgram_Activity; 
        /// <summary>
        /// Элемент СЦ
        /// </summary>
        [JsonIgnoreForException]
		public virtual ICollection<Sbor.Tablepart.LongTermGoalProgram_Activity> LongTermGoalProgram_Activity 
		{
			get{ return _LongTermGoalProgram_Activity ?? (_LongTermGoalProgram_Activity = new List<Sbor.Tablepart.LongTermGoalProgram_Activity>()); } 
			set{ _LongTermGoalProgram_Activity = value; }
		}


		/// <summary>
		/// Конструктор по-умолчанию
		/// </summary>
		public LongTermGoalProgram_SystemGoalElement()
		{
		}

		/// <summary>
		/// Идентификатор типа сущности
		/// </summary>
		public override int EntityId
		{
			get { return -1543503789; }
		}

		/// <summary>
		/// Идентификатор типа сущности
		/// </summary>
		public static int EntityIdStatic
		{
			get { return -1543503789; }
		}

		/// <summary>
		/// Русское наименование типа сущности
		/// </summary>
		public override string EntityCaption
		{
			get { return "Элементы СЦ"; }
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
				Platform.BusinessLogic.EntityIds.Add(GetType().DeclaringType,-1543503789);
			}
		}


	}
}