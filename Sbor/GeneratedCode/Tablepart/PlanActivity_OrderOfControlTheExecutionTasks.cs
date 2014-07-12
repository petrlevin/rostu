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
	/// Порядок контроля за исполнением задания
	/// </summary>
	public partial class PlanActivity_OrderOfControlTheExecutionTasks : TablePartEntity      
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
		public virtual Sbor.Document.PlanActivity Owner{get; set;}
		

		/// <summary>
		/// Требование к заданию 
		/// </summary>
		public int IdMaster{get; set;}
        /// <summary>
	    /// Требование к заданию 
	    /// </summary>
		public virtual Sbor.Tablepart.PlanActivity_RequirementsForTheTask Master{get; set;}
		

		/// <summary>
		/// Форма контроля
		/// </summary>
		public string FormOfControl{get; set;}

		/// <summary>
		/// Периодичность
		/// </summary>
		public string Periodicity{get; set;}

		/// <summary>
		/// Органы исполнительной власти, осуществляющие контроль
		/// </summary>
		public string OrgansOfExecutiveAuthoritiesInCharge{get; set;}

	

		/// <summary>
		/// Конструктор по-умолчанию
		/// </summary>
		public PlanActivity_OrderOfControlTheExecutionTasks()
		{
		}

		/// <summary>
		/// Идентификатор типа сущности
		/// </summary>
		public override int EntityId
		{
			get { return -2013265422; }
		}

		/// <summary>
		/// Идентификатор типа сущности
		/// </summary>
		public static int EntityIdStatic
		{
			get { return -2013265422; }
		}

		/// <summary>
		/// Русское наименование типа сущности
		/// </summary>
		public override string EntityCaption
		{
			get { return "Порядок контроля за исполнением задания"; }
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
				Platform.BusinessLogic.EntityIds.Add(GetType().DeclaringType,-2013265422);
			}
		}


	}
}