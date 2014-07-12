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
	/// Целевые показатели
	/// </summary>
	public partial class StateProgram_GoalIndicator : TablePartEntity      
	{
	
		/// <summary>
		/// Ссылка на владельца
		/// </summary>
		public int IdOwner{get; set;}
        /// <summary>
	    /// Ссылка на владельца
	    /// </summary>
		public virtual Sbor.Document.StateProgram Owner{get; set;}
		

		/// <summary>
		/// Идентификатор
		/// </summary>
		
        public override Int32 Id{get; set;}

		/// <summary>
		/// Элемент СЦ
		/// </summary>
		public int IdMaster{get; set;}
        /// <summary>
	    /// Элемент СЦ
	    /// </summary>
		public virtual Sbor.Tablepart.StateProgram_SystemGoalElement Master{get; set;}
		

		/// <summary>
		/// Наименование
		/// </summary>
		public int IdGoalIndicator{get; set;}
        /// <summary>
	    /// Наименование
	    /// </summary>
		public virtual Sbor.Reference.GoalIndicator GoalIndicator{get; set;}
		

			private ICollection<Sbor.Tablepart.StateProgram_GoalIndicator_Value> _StateProgram_GoalIndicator_Value; 
        /// <summary>
        /// Ссылка на главную ТЧ
        /// </summary>
        [JsonIgnoreForException]
		public virtual ICollection<Sbor.Tablepart.StateProgram_GoalIndicator_Value> StateProgram_GoalIndicator_Value 
		{
			get{ return _StateProgram_GoalIndicator_Value ?? (_StateProgram_GoalIndicator_Value = new List<Sbor.Tablepart.StateProgram_GoalIndicator_Value>()); } 
			set{ _StateProgram_GoalIndicator_Value = value; }
		}


		/// <summary>
		/// Конструктор по-умолчанию
		/// </summary>
		public StateProgram_GoalIndicator()
		{
		}

		/// <summary>
		/// Идентификатор типа сущности
		/// </summary>
		public override int EntityId
		{
			get { return -1543503811; }
		}

		/// <summary>
		/// Идентификатор типа сущности
		/// </summary>
		public static int EntityIdStatic
		{
			get { return -1543503811; }
		}

		/// <summary>
		/// Русское наименование типа сущности
		/// </summary>
		public override string EntityCaption
		{
			get { return "Целевые показатели"; }
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
				Platform.BusinessLogic.EntityIds.Add(GetType().DeclaringType,-1543503811);
			}
		}


	}
}