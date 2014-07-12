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
	/// Спрос и мощность мероприятий - значения
	/// </summary>
	public partial class ActivityOfSBP_ActivityDemandAndCapacity_Value : TablePartEntity      
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
		/// строка-владелец
		/// </summary>
		public int IdMaster{get; set;}
        /// <summary>
	    /// строка-владелец
	    /// </summary>
		public virtual Sbor.Tablepart.ActivityOfSBP_ActivityDemandAndCapacity Master{get; set;}
		

		/// <summary>
		/// Год
		/// </summary>
		public int IdHierarchyPeriod{get; set;}
        /// <summary>
	    /// Год
	    /// </summary>
		public virtual Platform.BusinessLogic.Reference.HierarchyPeriod HierarchyPeriod{get; set;}
		

		/// <summary>
		/// Спрос
		/// </summary>
		public decimal? Demand{get; set;}

		/// <summary>
		/// Мощность
		/// </summary>
		public decimal? Capacity{get; set;}

	

		/// <summary>
		/// Конструктор по-умолчанию
		/// </summary>
		public ActivityOfSBP_ActivityDemandAndCapacity_Value()
		{
		}

		/// <summary>
		/// Идентификатор типа сущности
		/// </summary>
		public override int EntityId
		{
			get { return -1543503633; }
		}

		/// <summary>
		/// Идентификатор типа сущности
		/// </summary>
		public static int EntityIdStatic
		{
			get { return -1543503633; }
		}

		/// <summary>
		/// Русское наименование типа сущности
		/// </summary>
		public override string EntityCaption
		{
			get { return "Спрос и мощность мероприятий - значения"; }
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
				Platform.BusinessLogic.EntityIds.Add(GetType().DeclaringType,-1543503633);
			}
		}


	}
}