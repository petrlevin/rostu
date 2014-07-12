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
	/// Спрос и мощность мероприятий
	/// </summary>
	public partial class ActivityOfSBP_ActivityDemandAndCapacity : TablePartEntity      
	{
	
		/// <summary>
		/// Идентификатор
		/// </summary>
		
        public override Int32 Id{get; set;}

		/// <summary>
		/// Ссылка на владельца
		/// </summary>
		public int IdOwner{get; set;}
        /// <summary>
	    /// Ссылка на владельца
	    /// </summary>
		public virtual Sbor.Document.ActivityOfSBP Owner{get; set;}
		

		/// <summary>
		/// Наименование
		/// </summary>
		public int IdActivity{get; set;}
        /// <summary>
	    /// Наименование
	    /// </summary>
		public virtual Sbor.Tablepart.ActivityOfSBP_Activity Activity{get; set;}
		

			private ICollection<Sbor.Tablepart.ActivityOfSBP_ActivityDemandAndCapacity_Value> _ActivityOfSBP_ActivityDemandAndCapacity_Value; 
        /// <summary>
        /// строка-владелец
        /// </summary>
        [JsonIgnoreForException]
		public virtual ICollection<Sbor.Tablepart.ActivityOfSBP_ActivityDemandAndCapacity_Value> ActivityOfSBP_ActivityDemandAndCapacity_Value 
		{
			get{ return _ActivityOfSBP_ActivityDemandAndCapacity_Value ?? (_ActivityOfSBP_ActivityDemandAndCapacity_Value = new List<Sbor.Tablepart.ActivityOfSBP_ActivityDemandAndCapacity_Value>()); } 
			set{ _ActivityOfSBP_ActivityDemandAndCapacity_Value = value; }
		}


		/// <summary>
		/// Конструктор по-умолчанию
		/// </summary>
		public ActivityOfSBP_ActivityDemandAndCapacity()
		{
		}

		/// <summary>
		/// Идентификатор типа сущности
		/// </summary>
		public override int EntityId
		{
			get { return -1543503634; }
		}

		/// <summary>
		/// Идентификатор типа сущности
		/// </summary>
		public static int EntityIdStatic
		{
			get { return -1543503634; }
		}

		/// <summary>
		/// Русское наименование типа сущности
		/// </summary>
		public override string EntityCaption
		{
			get { return "Спрос и мощность мероприятий"; }
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
				Platform.BusinessLogic.EntityIds.Add(GetType().DeclaringType,-1543503634);
			}
		}


	}
}