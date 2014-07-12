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



namespace Sbor.Reports.Tablepart
{
	// Это автоматически сгенерированный класс, изменения будут уничтожены при следующей генерации
	/// <summary>
	/// Правила фильтрации
	/// </summary>
	public partial class ConsolidatedExpenditure_BaseFilter : TablePartEntity      
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
		public virtual Sbor.Reports.Report.ConsolidatedExpenditure Owner{get; set;}
		

		/// <summary>
		/// Тип фильтра по полю ДКР
		/// </summary>
		public byte? IdFilterFieldType_DKR{get; set;}
                            /// <summary>
                            /// Тип фильтра по полю ДКР
                            /// </summary>
							[NotMapped] 
                            public virtual Sbor.DbEnums.FilterFieldType? FilterFieldType_DKR {
								get { return (Sbor.DbEnums.FilterFieldType?)this.IdFilterFieldType_DKR; } 
								set { this.IdFilterFieldType_DKR = (byte?) value; }
							}

		/// <summary>
		/// Тип фильтра по полю ДФК
		/// </summary>
		public byte? IdFilterFieldType_DFK{get; set;}
                            /// <summary>
                            /// Тип фильтра по полю ДФК
                            /// </summary>
							[NotMapped] 
                            public virtual Sbor.DbEnums.FilterFieldType? FilterFieldType_DFK {
								get { return (Sbor.DbEnums.FilterFieldType?)this.IdFilterFieldType_DFK; } 
								set { this.IdFilterFieldType_DFK = (byte?) value; }
							}

		/// <summary>
		/// Тип фильтра по полю ДЭК
		/// </summary>
		public byte? IdFilterFieldType_DEK{get; set;}
                            /// <summary>
                            /// Тип фильтра по полю ДЭК
                            /// </summary>
							[NotMapped] 
                            public virtual Sbor.DbEnums.FilterFieldType? FilterFieldType_DEK {
								get { return (Sbor.DbEnums.FilterFieldType?)this.IdFilterFieldType_DEK; } 
								set { this.IdFilterFieldType_DEK = (byte?) value; }
							}

		/// <summary>
		/// Тип фильтра по полю Источники финансирования
		/// </summary>
		public byte? IdFilterFieldType_FinanceSource{get; set;}
                            /// <summary>
                            /// Тип фильтра по полю Источники финансирования
                            /// </summary>
							[NotMapped] 
                            public virtual Sbor.DbEnums.FilterFieldType? FilterFieldType_FinanceSource {
								get { return (Sbor.DbEnums.FilterFieldType?)this.IdFilterFieldType_FinanceSource; } 
								set { this.IdFilterFieldType_FinanceSource = (byte?) value; }
							}

		/// <summary>
		/// Тип фильтра по полю КВР
		/// </summary>
		public byte? IdFilterFieldType_KVR{get; set;}
                            /// <summary>
                            /// Тип фильтра по полю КВР
                            /// </summary>
							[NotMapped] 
                            public virtual Sbor.DbEnums.FilterFieldType? FilterFieldType_KVR {
								get { return (Sbor.DbEnums.FilterFieldType?)this.IdFilterFieldType_KVR; } 
								set { this.IdFilterFieldType_KVR = (byte?) value; }
							}

		/// <summary>
		/// Тип фильтра по полю  КВСР/КАДБ/КАИФ
		/// </summary>
		public byte? IdFilterFieldType_KVSR{get; set;}
                            /// <summary>
                            /// Тип фильтра по полю  КВСР/КАДБ/КАИФ
                            /// </summary>
							[NotMapped] 
                            public virtual Sbor.DbEnums.FilterFieldType? FilterFieldType_KVSR {
								get { return (Sbor.DbEnums.FilterFieldType?)this.IdFilterFieldType_KVSR; } 
								set { this.IdFilterFieldType_KVSR = (byte?) value; }
							}

		/// <summary>
		/// Тип фильтра по полю Коды субсидий
		/// </summary>
		public byte? IdFilterFieldType_CodeSubsidy{get; set;}
                            /// <summary>
                            /// Тип фильтра по полю Коды субсидий
                            /// </summary>
							[NotMapped] 
                            public virtual Sbor.DbEnums.FilterFieldType? FilterFieldType_CodeSubsidy {
								get { return (Sbor.DbEnums.FilterFieldType?)this.IdFilterFieldType_CodeSubsidy; } 
								set { this.IdFilterFieldType_CodeSubsidy = (byte?) value; }
							}

		/// <summary>
		/// Тип фильтра по полю КОСГУ
		/// </summary>
		public byte? IdFilterFieldType_KOSGU{get; set;}
                            /// <summary>
                            /// Тип фильтра по полю КОСГУ
                            /// </summary>
							[NotMapped] 
                            public virtual Sbor.DbEnums.FilterFieldType? FilterFieldType_KOSGU {
								get { return (Sbor.DbEnums.FilterFieldType?)this.IdFilterFieldType_KOSGU; } 
								set { this.IdFilterFieldType_KOSGU = (byte?) value; }
							}

		/// <summary>
		/// Тип фильтра по полю КФО
		/// </summary>
		public byte? IdFilterFieldType_KFO{get; set;}
                            /// <summary>
                            /// Тип фильтра по полю КФО
                            /// </summary>
							[NotMapped] 
                            public virtual Sbor.DbEnums.FilterFieldType? FilterFieldType_KFO {
								get { return (Sbor.DbEnums.FilterFieldType?)this.IdFilterFieldType_KFO; } 
								set { this.IdFilterFieldType_KFO = (byte?) value; }
							}

		/// <summary>
		/// Тип фильтра по полю КЦСР
		/// </summary>
		public byte? IdFilterFieldType_KCSR{get; set;}
                            /// <summary>
                            /// Тип фильтра по полю КЦСР
                            /// </summary>
							[NotMapped] 
                            public virtual Sbor.DbEnums.FilterFieldType? FilterFieldType_KCSR {
								get { return (Sbor.DbEnums.FilterFieldType?)this.IdFilterFieldType_KCSR; } 
								set { this.IdFilterFieldType_KCSR = (byte?) value; }
							}

		/// <summary>
		/// Тип фильтра по полю Отраслевые коды
		/// </summary>
		public byte? IdFilterFieldType_BranchCode{get; set;}
                            /// <summary>
                            /// Тип фильтра по полю Отраслевые коды
                            /// </summary>
							[NotMapped] 
                            public virtual Sbor.DbEnums.FilterFieldType? FilterFieldType_BranchCode {
								get { return (Sbor.DbEnums.FilterFieldType?)this.IdFilterFieldType_BranchCode; } 
								set { this.IdFilterFieldType_BranchCode = (byte?) value; }
							}

		/// <summary>
		/// Тип фильтра по полю РзПР
		/// </summary>
		public byte? IdFilterFieldType_RZPR{get; set;}
                            /// <summary>
                            /// Тип фильтра по полю РзПР
                            /// </summary>
							[NotMapped] 
                            public virtual Sbor.DbEnums.FilterFieldType? FilterFieldType_RZPR {
								get { return (Sbor.DbEnums.FilterFieldType?)this.IdFilterFieldType_RZPR; } 
								set { this.IdFilterFieldType_RZPR = (byte?) value; }
							}

		/// <summary>
		/// Тип фильтра по полю Типы РО
		/// </summary>
		public byte? IdFilterFieldType_ExpenseObligationType{get; set;}
                            /// <summary>
                            /// Тип фильтра по полю Типы РО
                            /// </summary>
							[NotMapped] 
                            public virtual Sbor.DbEnums.FilterFieldType? FilterFieldType_ExpenseObligationType {
								get { return (Sbor.DbEnums.FilterFieldType?)this.IdFilterFieldType_ExpenseObligationType; } 
								set { this.IdFilterFieldType_ExpenseObligationType = (byte?) value; }
							}

		/// <summary>
		/// Типы РО
		/// </summary>
		private ICollection<Sbor.Reports.Tablepart.ConsolidatedExpenditure_BaseFilter_ExpenseObligationType> _tpExpenseObligationType; 
        /// <summary>
        /// Типы РО
        /// </summary>
        [JsonIgnoreForException]
		public virtual ICollection<Sbor.Reports.Tablepart.ConsolidatedExpenditure_BaseFilter_ExpenseObligationType> ExpenseObligationType 
		{
			get{ return _tpExpenseObligationType ?? (_tpExpenseObligationType = new List<Sbor.Reports.Tablepart.ConsolidatedExpenditure_BaseFilter_ExpenseObligationType>()); } 
			set{ _tpExpenseObligationType = value; }
		}

			private ICollection<Sbor.Reports.Tablepart.ConsolidatedExpenditure_BaseFilter_ExpenseObligationType> _ConsolidatedExpenditure_BaseFilter_ExpenseObligationType; 
        /// <summary>
        /// Ссылка на владельца
        /// </summary>
        [JsonIgnoreForException]
		public virtual ICollection<Sbor.Reports.Tablepart.ConsolidatedExpenditure_BaseFilter_ExpenseObligationType> ConsolidatedExpenditure_BaseFilter_ExpenseObligationType 
		{
			get{ return _ConsolidatedExpenditure_BaseFilter_ExpenseObligationType ?? (_ConsolidatedExpenditure_BaseFilter_ExpenseObligationType = new List<Sbor.Reports.Tablepart.ConsolidatedExpenditure_BaseFilter_ExpenseObligationType>()); } 
			set{ _ConsolidatedExpenditure_BaseFilter_ExpenseObligationType = value; }
		}

private ICollection<Sbor.Reference.DKR> _mlDKR; 
        /// <summary>
        /// Правила фильтрации
        /// </summary>
        [JsonIgnoreForException]
		public virtual ICollection<Sbor.Reference.DKR> DKR 
		{
			get{ return _mlDKR ?? (_mlDKR = new List<Sbor.Reference.DKR>()); } 
			set{ _mlDKR = value; }
		}
			private ICollection<Sbor.Reference.DFK> _mlDFK; 
        /// <summary>
        /// Правила фильтрации
        /// </summary>
        [JsonIgnoreForException]
		public virtual ICollection<Sbor.Reference.DFK> DFK 
		{
			get{ return _mlDFK ?? (_mlDFK = new List<Sbor.Reference.DFK>()); } 
			set{ _mlDFK = value; }
		}
			private ICollection<Sbor.Reference.DEK> _mlDEK; 
        /// <summary>
        /// Правила фильтрации
        /// </summary>
        [JsonIgnoreForException]
		public virtual ICollection<Sbor.Reference.DEK> DEK 
		{
			get{ return _mlDEK ?? (_mlDEK = new List<Sbor.Reference.DEK>()); } 
			set{ _mlDEK = value; }
		}
			private ICollection<Sbor.Reference.FinanceSource> _mlFinanceSource; 
        /// <summary>
        /// Правила фильтрации
        /// </summary>
        [JsonIgnoreForException]
		public virtual ICollection<Sbor.Reference.FinanceSource> FinanceSource 
		{
			get{ return _mlFinanceSource ?? (_mlFinanceSource = new List<Sbor.Reference.FinanceSource>()); } 
			set{ _mlFinanceSource = value; }
		}
			private ICollection<Sbor.Reference.KVR> _mlKVR; 
        /// <summary>
        /// Правила фильтрации
        /// </summary>
        [JsonIgnoreForException]
		public virtual ICollection<Sbor.Reference.KVR> KVR 
		{
			get{ return _mlKVR ?? (_mlKVR = new List<Sbor.Reference.KVR>()); } 
			set{ _mlKVR = value; }
		}
			private ICollection<Sbor.Reference.KVSR> _mlKVSR; 
        /// <summary>
        /// Правила фильтрации
        /// </summary>
        [JsonIgnoreForException]
		public virtual ICollection<Sbor.Reference.KVSR> KVSR 
		{
			get{ return _mlKVSR ?? (_mlKVSR = new List<Sbor.Reference.KVSR>()); } 
			set{ _mlKVSR = value; }
		}
			private ICollection<Sbor.Reference.CodeSubsidy> _mlCodeSubsidy; 
        /// <summary>
        /// Правила фильтрации
        /// </summary>
        [JsonIgnoreForException]
		public virtual ICollection<Sbor.Reference.CodeSubsidy> CodeSubsidy 
		{
			get{ return _mlCodeSubsidy ?? (_mlCodeSubsidy = new List<Sbor.Reference.CodeSubsidy>()); } 
			set{ _mlCodeSubsidy = value; }
		}
			private ICollection<Sbor.Reference.KOSGU> _mlKOSGU; 
        /// <summary>
        /// Правила фильтрации
        /// </summary>
        [JsonIgnoreForException]
		public virtual ICollection<Sbor.Reference.KOSGU> KOSGU 
		{
			get{ return _mlKOSGU ?? (_mlKOSGU = new List<Sbor.Reference.KOSGU>()); } 
			set{ _mlKOSGU = value; }
		}
			private ICollection<Sbor.Reference.KFO> _mlKFO; 
        /// <summary>
        /// Правила фильтрации
        /// </summary>
        [JsonIgnoreForException]
		public virtual ICollection<Sbor.Reference.KFO> KFO 
		{
			get{ return _mlKFO ?? (_mlKFO = new List<Sbor.Reference.KFO>()); } 
			set{ _mlKFO = value; }
		}
			private ICollection<Sbor.Reference.KCSR> _mlKCSR; 
        /// <summary>
        /// Правила фильтрации
        /// </summary>
        [JsonIgnoreForException]
		public virtual ICollection<Sbor.Reference.KCSR> KCSR 
		{
			get{ return _mlKCSR ?? (_mlKCSR = new List<Sbor.Reference.KCSR>()); } 
			set{ _mlKCSR = value; }
		}
			private ICollection<Sbor.Reference.BranchCode> _mlBranchCode; 
        /// <summary>
        /// Правила фильтрации
        /// </summary>
        [JsonIgnoreForException]
		public virtual ICollection<Sbor.Reference.BranchCode> BranchCode 
		{
			get{ return _mlBranchCode ?? (_mlBranchCode = new List<Sbor.Reference.BranchCode>()); } 
			set{ _mlBranchCode = value; }
		}
			private ICollection<Sbor.Reference.RZPR> _mlRZPR; 
        /// <summary>
        /// Правила фильтрации
        /// </summary>
        [JsonIgnoreForException]
		public virtual ICollection<Sbor.Reference.RZPR> RZPR 
		{
			get{ return _mlRZPR ?? (_mlRZPR = new List<Sbor.Reference.RZPR>()); } 
			set{ _mlRZPR = value; }
		}
			
		/// <summary>
		/// Конструктор по-умолчанию
		/// </summary>
		public ConsolidatedExpenditure_BaseFilter()
		{
		}

		/// <summary>
		/// Идентификатор типа сущности
		/// </summary>
		public override int EntityId
		{
			get { return -1610612484; }
		}

		/// <summary>
		/// Идентификатор типа сущности
		/// </summary>
		public static int EntityIdStatic
		{
			get { return -1610612484; }
		}

		/// <summary>
		/// Русское наименование типа сущности
		/// </summary>
		public override string EntityCaption
		{
			get { return "Правила фильтрации"; }
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
				Platform.BusinessLogic.EntityIds.Add(GetType().DeclaringType,-1610612484);
			}
		}


	}
}