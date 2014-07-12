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
	/// Просмотр изменений
	/// </summary>
	public partial class LimitBudgetAllocations_ShowChanges : TablePartEntity      
	{
	
		/// <summary>
		/// Отраслевой код
		/// </summary>
		public int? IdBranchCode{get; set;}
        /// <summary>
	    /// Отраслевой код
	    /// </summary>
		public virtual Sbor.Reference.BranchCode BranchCode{get; set;}
		

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
		public virtual Sbor.Document.LimitBudgetAllocations Owner{get; set;}
		

		/// <summary>
		/// Тип РО
		/// </summary>
		public byte? IdExpenseObligationType{get; set;}
                            /// <summary>
                            /// Тип РО
                            /// </summary>
							[NotMapped] 
                            public virtual Sbor.DbEnums.ExpenseObligationType? ExpenseObligationType {
								get { return (Sbor.DbEnums.ExpenseObligationType?)this.IdExpenseObligationType; } 
								set { this.IdExpenseObligationType = (byte?) value; }
							}

		/// <summary>
		/// Источник финансирования
		/// </summary>
		public int? IdFinanceSource{get; set;}
        /// <summary>
	    /// Источник финансирования
	    /// </summary>
		public virtual Sbor.Reference.FinanceSource FinanceSource{get; set;}
		

		/// <summary>
		/// КФО
		/// </summary>
		public int? IdKFO{get; set;}
        /// <summary>
	    /// КФО
	    /// </summary>
		public virtual Sbor.Reference.KFO KFO{get; set;}
		

		/// <summary>
		/// КВСР/КАДБ/КАИФ
		/// </summary>
		public int? IdKVSR{get; set;}
        /// <summary>
	    /// КВСР/КАДБ/КАИФ
	    /// </summary>
		public virtual Sbor.Reference.KVSR KVSR{get; set;}
		

		/// <summary>
		/// РзПР
		/// </summary>
		public int? IdRZPR{get; set;}
        /// <summary>
	    /// РзПР
	    /// </summary>
		public virtual Sbor.Reference.RZPR RZPR{get; set;}
		

		/// <summary>
		/// КЦСР
		/// </summary>
		public int? IdKCSR{get; set;}
        /// <summary>
	    /// КЦСР
	    /// </summary>
		public virtual Sbor.Reference.KCSR KCSR{get; set;}
		

		/// <summary>
		/// КВР
		/// </summary>
		public int? IdKVR{get; set;}
        /// <summary>
	    /// КВР
	    /// </summary>
		public virtual Sbor.Reference.KVR KVR{get; set;}
		

		/// <summary>
		/// КОСГУ
		/// </summary>
		public int? IdKOSGU{get; set;}
        /// <summary>
	    /// КОСГУ
	    /// </summary>
		public virtual Sbor.Reference.KOSGU KOSGU{get; set;}
		

		/// <summary>
		/// ДФК
		/// </summary>
		public int? IdDFK{get; set;}
        /// <summary>
	    /// ДФК
	    /// </summary>
		public virtual Sbor.Reference.DFK DFK{get; set;}
		

		/// <summary>
		/// ДКР
		/// </summary>
		public int? IdDKR{get; set;}
        /// <summary>
	    /// ДКР
	    /// </summary>
		public virtual Sbor.Reference.DKR DKR{get; set;}
		

		/// <summary>
		/// ДЭК
		/// </summary>
		public int? IdDEK{get; set;}
        /// <summary>
	    /// ДЭК
	    /// </summary>
		public virtual Sbor.Reference.DEK DEK{get; set;}
		

		/// <summary>
		/// Код субсидии
		/// </summary>
		public int? IdCodeSubsidy{get; set;}
        /// <summary>
	    /// Код субсидии
	    /// </summary>
		public virtual Sbor.Reference.CodeSubsidy CodeSubsidy{get; set;}
		

		/// <summary>
		/// Год
		/// </summary>
		public Int32? Year{get; set;}

		/// <summary>
		/// Изменено, руб.
		/// </summary>
		public decimal? ChangedAllocations{get; set;}

	

		/// <summary>
		/// Конструктор по-умолчанию
		/// </summary>
		public LimitBudgetAllocations_ShowChanges()
		{
		}

		/// <summary>
		/// Идентификатор типа сущности
		/// </summary>
		public override int EntityId
		{
			get { return -1207959527; }
		}

		/// <summary>
		/// Идентификатор типа сущности
		/// </summary>
		public static int EntityIdStatic
		{
			get { return -1207959527; }
		}

		/// <summary>
		/// Русское наименование типа сущности
		/// </summary>
		public override string EntityCaption
		{
			get { return "Просмотр изменений"; }
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
				Platform.BusinessLogic.EntityIds.Add(GetType().DeclaringType,-1207959527);
			}
		}


	}
}