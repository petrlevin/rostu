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
	/// Расходы
	/// </summary>
	public partial class BalancingIFDB_Expense : TablePartEntity      
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
		public virtual Sbor.Tool.BalancingIFDB Owner{get; set;}
		

		/// <summary>
		/// Наименование
		/// </summary>
		public int IdMaster{get; set;}
        /// <summary>
	    /// Наименование
	    /// </summary>
		public virtual Sbor.Tablepart.BalancingIFDB_Program Master{get; set;}
		

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
		/// Отраслевой код
		/// </summary>
		public int? IdBranchCode{get; set;}
        /// <summary>
	    /// Отраслевой код
	    /// </summary>
		public virtual Sbor.Reference.BranchCode BranchCode{get; set;}
		

		/// <summary>
		/// `'Актуальная базовая ' + ({BudgetYear})`
		/// </summary>
		public decimal? OFG{get; set;}

		/// <summary>
		/// `'Актуальная доп. потребностей ' + ({BudgetYear})`
		/// </summary>
		public decimal? AdditionalOFG{get; set;}

		/// <summary>
		/// `'Измененная базовая ' + ({BudgetYear})`
		/// </summary>
		public decimal? ChangeOFG{get; set;}

		/// <summary>
		/// `'Измененная доп. потребностей ' + ({BudgetYear})`
		/// </summary>
		public decimal? ChangeAdditionalOFG{get; set;}

		/// <summary>
		/// `'Актуальная базовая ' + ({BudgetYear} + 1)`
		/// </summary>
		public decimal? PFG1{get; set;}

		/// <summary>
		/// `'Актуальная доп. потребностей ' + ({BudgetYear} + 1)`
		/// </summary>
		public decimal? AdditionalPFG1{get; set;}

		/// <summary>
		/// `'Измененная базовая ' + ({BudgetYear} + 1)`
		/// </summary>
		public decimal? ChangePFG1{get; set;}

		/// <summary>
		/// `'Измененная доп. потребностей ' + ({BudgetYear} + 1)`
		/// </summary>
		public decimal? ChangeAdditionalPFG1{get; set;}

		/// <summary>
		/// `'Актуальная базовая ' + ({BudgetYear} + 2)`
		/// </summary>
		public decimal? PFG2{get; set;}

		/// <summary>
		/// `'Актуальная доп. потребностей ' + ({BudgetYear} + 2)`
		/// </summary>
		public decimal? AdditionalPFG2{get; set;}

		/// <summary>
		/// `'Измененная базовая ' + ({BudgetYear} + 2)`
		/// </summary>
		public decimal? ChangePFG2{get; set;}

		/// <summary>
		/// `'Измененная доп. потребностей ' + ({BudgetYear} + 2)`
		/// </summary>
		public decimal? ChangeAdditionalPFG2{get; set;}

		/// <summary>
		/// `'Разница базовая ' + ({BudgetYear})`
		/// </summary>
		public decimal? DifferenceOFG{get; set;}

		/// <summary>
		/// `'Разница доп. потребностей ' + ({BudgetYear})`
		/// </summary>
		public decimal? DifferenceAdditionalOFG{get; set;}

		/// <summary>
		/// `'Разница базовая ' + ({BudgetYear}+1)`
		/// </summary>
		public decimal? DifferencePFG1{get; set;}

		/// <summary>
		/// `'Разница доп. потребностей ' + ({BudgetYear}+1)`
		/// </summary>
		public decimal? DifferenceAdditionalPFG1{get; set;}

		/// <summary>
		/// `'Разница базовая ' + ({BudgetYear}+2)`
		/// </summary>
		public decimal? DifferencePFG2{get; set;}

		/// <summary>
		/// `'Разница доп. потребностей ' + ({BudgetYear}+2)`
		/// </summary>
		public decimal? DifferenceAdditionalPFG2{get; set;}

		/// <summary>
		/// Территория
		/// </summary>
		public int? IdOKATO{get; set;}
        /// <summary>
	    /// Территория
	    /// </summary>
		public virtual BaseApp.Reference.OKATO OKATO{get; set;}
		

		/// <summary>
		/// Код РО
		/// </summary>
		public int? IdAuthorityOfExpenseObligation{get; set;}
        /// <summary>
	    /// Код РО
	    /// </summary>
		public virtual Sbor.Reference.AuthorityOfExpenseObligation AuthorityOfExpenseObligation{get; set;}
		

			private ICollection<Sbor.Tablepart.BalancingIFDB_EstimatedLine> _BalancingIFDB_EstimatedLine; 
        /// <summary>
        /// Расход
        /// </summary>
        [JsonIgnoreForException]
		public virtual ICollection<Sbor.Tablepart.BalancingIFDB_EstimatedLine> BalancingIFDB_EstimatedLine 
		{
			get{ return _BalancingIFDB_EstimatedLine ?? (_BalancingIFDB_EstimatedLine = new List<Sbor.Tablepart.BalancingIFDB_EstimatedLine>()); } 
			set{ _BalancingIFDB_EstimatedLine = value; }
		}


		/// <summary>
		/// Конструктор по-умолчанию
		/// </summary>
		public BalancingIFDB_Expense()
		{
		}

		/// <summary>
		/// Идентификатор типа сущности
		/// </summary>
		public override int EntityId
		{
			get { return -2013265409; }
		}

		/// <summary>
		/// Идентификатор типа сущности
		/// </summary>
		public static int EntityIdStatic
		{
			get { return -2013265409; }
		}

		/// <summary>
		/// Русское наименование типа сущности
		/// </summary>
		public override string EntityCaption
		{
			get { return "Расходы"; }
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
				Platform.BusinessLogic.EntityIds.Add(GetType().DeclaringType,-2013265409);
			}
		}


	}
}