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
	/// ТЧ «Мероприятия для распределения»
	/// </summary>
	public partial class PublicInstitutionEstimate_DistributionActivity : TablePartEntity      
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
		public virtual Sbor.Document.PublicInstitutionEstimate Owner{get; set;}
		

		/// <summary>
		/// Метод
		/// </summary>
		public int IdMaster{get; set;}
        /// <summary>
	    /// Метод
	    /// </summary>
		public virtual Sbor.Tablepart.PublicInstitutionEstimate_DistributionMethod Master{get; set;}
		

		/// <summary>
		/// Мероприятие
		/// </summary>
		public int IdPublicInstitutionEstimate_Activity{get; set;}
        /// <summary>
	    /// Мероприятие
	    /// </summary>
		public virtual Sbor.Tablepart.PublicInstitutionEstimate_Activity PublicInstitutionEstimate_Activity{get; set;}
		

		/// <summary>
		/// `'Сумма прямых расходов ' + {BudgetYear} + ', руб'`
		/// </summary>
		public decimal? DirectOFG{get; set;}

		/// <summary>
		/// `'Сумма прямых расходов ' + ({BudgetYear} + 1) + ', руб'`
		/// </summary>
		public decimal? DirectPFG1{get; set;}

		/// <summary>
		/// `'Сумма прямых расходов ' + ({BudgetYear} + 2) + ', руб'`
		/// </summary>
		public decimal? DirectPFG2{get; set;}

		/// <summary>
		/// `'Объем мероприятия  ' + {BudgetYear} + ', руб'`
		/// </summary>
		public decimal? VolumeOFG{get; set;}

		/// <summary>
		/// `'Объем мероприятия  ' + ( {BudgetYear} + 1 ) + ', руб'`
		/// </summary>
		public decimal? VolumePFG1{get; set;}

		/// <summary>
		/// `'Объем мероприятия  ' + ({BudgetYear} + 2) + ', руб'`
		/// </summary>
		public decimal? VolumePFG2{get; set;}

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
		public PublicInstitutionEstimate_DistributionActivity()
		{
		}

		/// <summary>
		/// Идентификатор типа сущности
		/// </summary>
		public override int EntityId
		{
			get { return -1207959518; }
		}

		/// <summary>
		/// Идентификатор типа сущности
		/// </summary>
		public static int EntityIdStatic
		{
			get { return -1207959518; }
		}

		/// <summary>
		/// Русское наименование типа сущности
		/// </summary>
		public override string EntityCaption
		{
			get { return "ТЧ «Мероприятия для распределения»"; }
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
				Platform.BusinessLogic.EntityIds.Add(GetType().DeclaringType,-1207959518);
			}
		}


	}
}