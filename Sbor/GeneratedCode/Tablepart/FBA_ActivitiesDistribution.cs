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
	/// Мероприятия для распределения
	/// </summary>
	public partial class FBA_ActivitiesDistribution : TablePartEntity      
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
		public virtual Sbor.Document.FinancialAndBusinessActivities Owner{get; set;}
		

		/// <summary>
		/// Метод
		/// </summary>
		public int IdMaster{get; set;}
        /// <summary>
	    /// Метод
	    /// </summary>
		public virtual Sbor.Tablepart.FBA_DistributionMethods Master{get; set;}
		

		/// <summary>
		/// Мероприятие
		/// </summary>
		public int IdFBA_Activity{get; set;}
        /// <summary>
	    /// Мероприятие
	    /// </summary>
		public virtual Sbor.Tablepart.FBA_Activity FBA_Activity{get; set;}
		

		/// <summary>
		/// `'Сумма прямых расходов ' + ({BudgetYear} + 0) + ', руб.'`
		/// </summary>
		public decimal? OFG_Direct{get; set;}

		/// <summary>
		/// `'Сумма прямых расходов ' + ({BudgetYear} + 1) + ', руб.'`
		/// </summary>
		public decimal? PFG1_Direct{get; set;}

		/// <summary>
		/// `'Сумма прямых расходов ' + ({BudgetYear} + 2) + ', руб.'`
		/// </summary>
		public decimal? PFG2_Direct{get; set;}

		/// <summary>
		/// `'Объем мероприятия ' + ({BudgetYear} + 0) + ', руб.'`
		/// </summary>
		public decimal? OFG_Activity{get; set;}

		/// <summary>
		/// `'Объем мероприятия ' + ({BudgetYear} + 1) + ', руб.'`
		/// </summary>
		public decimal? PFG1_Activity{get; set;}

		/// <summary>
		/// `'Объем мероприятия ' + ({BudgetYear} + 2) + ', руб.'`
		/// </summary>
		public decimal? PFG2_Activity{get; set;}

		/// <summary>
		/// `'Коэф. ' + ({BudgetYear}) + ', %'`
		/// </summary>
		public Int16? FactorOFG{get; set;}

		/// <summary>
		/// `'Коэф. ' + ({BudgetYear} + 1) + ', %'`
		/// </summary>
		public Int16? FactorPFG1{get; set;}

		/// <summary>
		/// `'Коэф. ' + ({BudgetYear} + 2) + ', %'`
		/// </summary>
		public Int16? FactorPFG2{get; set;}

	

		/// <summary>
		/// Конструктор по-умолчанию
		/// </summary>
		public FBA_ActivitiesDistribution()
		{
		}

		/// <summary>
		/// Идентификатор типа сущности
		/// </summary>
		public override int EntityId
		{
			get { return -1946156899; }
		}

		/// <summary>
		/// Идентификатор типа сущности
		/// </summary>
		public static int EntityIdStatic
		{
			get { return -1946156899; }
		}

		/// <summary>
		/// Русское наименование типа сущности
		/// </summary>
		public override string EntityCaption
		{
			get { return "Мероприятия для распределения"; }
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
				Platform.BusinessLogic.EntityIds.Add(GetType().DeclaringType,-1946156899);
			}
		}


	}
}