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
	/// Периоды финансового обеспечения
	/// </summary>
	public partial class PlanActivity_PeriodsOfFinancialProvision : TablePartEntity      
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
		/// Период
		/// </summary>
		public int? IdHierarchyPeriod{get; set;}
        /// <summary>
	    /// Период
	    /// </summary>
		public virtual Platform.BusinessLogic.Reference.HierarchyPeriod HierarchyPeriod{get; set;}
		

		/// <summary>
		/// Сумма
		/// </summary>
		public decimal? Value{get; set;}

		/// <summary>
		/// Строка КБК
		/// </summary>
		public int IdMaster{get; set;}
        /// <summary>
	    /// Строка КБК
	    /// </summary>
		public virtual Sbor.Tablepart.PlanActivity_KBKOfFinancialProvision Master{get; set;}
		

		/// <summary>
		/// Доп. потребность
		/// </summary>
		public decimal? AdditionalValue{get; set;}

	

		/// <summary>
		/// Конструктор по-умолчанию
		/// </summary>
		public PlanActivity_PeriodsOfFinancialProvision()
		{
		}

		/// <summary>
		/// Идентификатор типа сущности
		/// </summary>
		public override int EntityId
		{
			get { return -2013265435; }
		}

		/// <summary>
		/// Идентификатор типа сущности
		/// </summary>
		public static int EntityIdStatic
		{
			get { return -2013265435; }
		}

		/// <summary>
		/// Русское наименование типа сущности
		/// </summary>
		public override string EntityCaption
		{
			get { return "Периоды финансового обеспечения"; }
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
				Platform.BusinessLogic.EntityIds.Add(GetType().DeclaringType,-2013265435);
			}
		}


	}
}