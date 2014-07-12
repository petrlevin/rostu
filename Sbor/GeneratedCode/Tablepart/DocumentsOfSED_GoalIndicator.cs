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
	public partial class DocumentsOfSED_GoalIndicator : TablePartEntity      
	{
	
		/// <summary>
		/// Ссылка на владельца
		/// </summary>
		public int IdOwner{get; set;}
        /// <summary>
	    /// Ссылка на владельца
	    /// </summary>
		public virtual Sbor.Document.DocumentsOfSED Owner{get; set;}
		

		/// <summary>
		/// Идентификатор
		/// </summary>
		
        public override Int32 Id{get; set;}

		/// <summary>
		/// Ссылка на Элементы СЦ
		/// </summary>
		public int IdMaster{get; set;}
        /// <summary>
	    /// Ссылка на Элементы СЦ
	    /// </summary>
		public virtual Sbor.Tablepart.DocumentsOfSED_ItemsSystemGoal Master{get; set;}
		

		/// <summary>
		/// Наименование
		/// </summary>
		public int IdGoalIndicator{get; set;}
        /// <summary>
	    /// Наименование
	    /// </summary>
		public virtual Sbor.Reference.GoalIndicator GoalIndicator{get; set;}
		

			private ICollection<Sbor.Tablepart.DocumentsOfSED_GoalIndicatorValue> _DocumentsOfSED_GoalIndicatorValue; 
        /// <summary>
        /// Ссылка на Целевые показатели
        /// </summary>
        [JsonIgnoreForException]
		public virtual ICollection<Sbor.Tablepart.DocumentsOfSED_GoalIndicatorValue> DocumentsOfSED_GoalIndicatorValue 
		{
			get{ return _DocumentsOfSED_GoalIndicatorValue ?? (_DocumentsOfSED_GoalIndicatorValue = new List<Sbor.Tablepart.DocumentsOfSED_GoalIndicatorValue>()); } 
			set{ _DocumentsOfSED_GoalIndicatorValue = value; }
		}


		/// <summary>
		/// Конструктор по-умолчанию
		/// </summary>
		public DocumentsOfSED_GoalIndicator()
		{
		}

		/// <summary>
		/// Идентификатор типа сущности
		/// </summary>
		public override int EntityId
		{
			get { return -2013265859; }
		}

		/// <summary>
		/// Идентификатор типа сущности
		/// </summary>
		public static int EntityIdStatic
		{
			get { return -2013265859; }
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
				Platform.BusinessLogic.EntityIds.Add(GetType().DeclaringType,-2013265859);
			}
		}


	}
}