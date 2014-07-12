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
	/// Плановые объемы поступлений
	/// </summary>
	public partial class FBA_PlannedVolumeIncome : TablePartEntity      
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
		/// Мероприятие
		/// </summary>
		public int? IdFBA_Activity{get; set;}
        /// <summary>
	    /// Мероприятие
	    /// </summary>
		public virtual Sbor.Tablepart.FBA_Activity FBA_Activity{get; set;}
		

		/// <summary>
		/// Источник
		/// </summary>
		public int IdFinanceSource{get; set;}
        /// <summary>
	    /// Источник
	    /// </summary>
		public virtual Sbor.Reference.FinanceSource FinanceSource{get; set;}
		

		/// <summary>
		/// КФО
		/// </summary>
		public int IdKFO{get; set;}
        /// <summary>
	    /// КФО
	    /// </summary>
		public virtual Sbor.Reference.KFO KFO{get; set;}
		

		/// <summary>
		/// Код субсидии
		/// </summary>
		public int? IdCodeSubsidy{get; set;}
        /// <summary>
	    /// Код субсидии
	    /// </summary>
		public virtual Sbor.Reference.CodeSubsidy CodeSubsidy{get; set;}
		

			private ICollection<Sbor.Tablepart.FBA_PlannedVolumeIncome_value> _FBA_PlannedVolumeIncome_value; 
        /// <summary>
        /// КБК
        /// </summary>
        [JsonIgnoreForException]
		public virtual ICollection<Sbor.Tablepart.FBA_PlannedVolumeIncome_value> FBA_PlannedVolumeIncome_value 
		{
			get{ return _FBA_PlannedVolumeIncome_value ?? (_FBA_PlannedVolumeIncome_value = new List<Sbor.Tablepart.FBA_PlannedVolumeIncome_value>()); } 
			set{ _FBA_PlannedVolumeIncome_value = value; }
		}


		/// <summary>
		/// Конструктор по-умолчанию
		/// </summary>
		public FBA_PlannedVolumeIncome()
		{
		}

		/// <summary>
		/// Идентификатор типа сущности
		/// </summary>
		public override int EntityId
		{
			get { return -1946156910; }
		}

		/// <summary>
		/// Идентификатор типа сущности
		/// </summary>
		public static int EntityIdStatic
		{
			get { return -1946156910; }
		}

		/// <summary>
		/// Русское наименование типа сущности
		/// </summary>
		public override string EntityCaption
		{
			get { return "Плановые объемы поступлений"; }
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
				Platform.BusinessLogic.EntityIds.Add(GetType().DeclaringType,-1946156910);
			}
		}


	}
}