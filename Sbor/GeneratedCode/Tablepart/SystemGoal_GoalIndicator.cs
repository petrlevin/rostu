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
	public partial class SystemGoal_GoalIndicator : TablePartEntity      
	{
	
		/// <summary>
		/// Ссылка на владельца
		/// </summary>
		public int IdOwner{get; set;}
        /// <summary>
	    /// Ссылка на владельца
	    /// </summary>
		public virtual Sbor.Reference.SystemGoal Owner{get; set;}
		

		/// <summary>
		/// Идентификатор
		/// </summary>
		
        public override Int32 Id{get; set;}

		/// <summary>
		/// Наименование
		/// </summary>
		public int IdGoalIndicator{get; set;}
        /// <summary>
	    /// Наименование
	    /// </summary>
		public virtual Sbor.Reference.GoalIndicator GoalIndicator{get; set;}
		

		/// <summary>
		/// Версия
		/// </summary>
		public int IdVersion{get; set;}
        /// <summary>
	    /// Версия
	    /// </summary>
		public virtual BaseApp.Reference.Version Version{get; set;}
		

			private ICollection<Sbor.Tablepart.SystemGoal_GoalIndicatorValue> _SystemGoal_GoalIndicatorValue; 
        /// <summary>
        /// Целевой показатель
        /// </summary>
        [JsonIgnoreForException]
		public virtual ICollection<Sbor.Tablepart.SystemGoal_GoalIndicatorValue> SystemGoal_GoalIndicatorValue 
		{
			get{ return _SystemGoal_GoalIndicatorValue ?? (_SystemGoal_GoalIndicatorValue = new List<Sbor.Tablepart.SystemGoal_GoalIndicatorValue>()); } 
			set{ _SystemGoal_GoalIndicatorValue = value; }
		}


		/// <summary>
		/// Конструктор по-умолчанию
		/// </summary>
		public SystemGoal_GoalIndicator()
		{
		}

		/// <summary>
		/// Идентификатор типа сущности
		/// </summary>
		public override int EntityId
		{
			get { return -2013265857; }
		}

		/// <summary>
		/// Идентификатор типа сущности
		/// </summary>
		public static int EntityIdStatic
		{
			get { return -2013265857; }
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
				Platform.BusinessLogic.EntityIds.Add(GetType().DeclaringType,-2013265857);
			}
		}


	}
}