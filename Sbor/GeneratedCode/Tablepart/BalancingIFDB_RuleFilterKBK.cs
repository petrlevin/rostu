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
	/// Набор КБК для правила
	/// </summary>
	public partial class BalancingIFDB_RuleFilterKBK : TablePartEntity      
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
		/// Правило
		/// </summary>
		public int IdMaster{get; set;}
        /// <summary>
	    /// Правило
	    /// </summary>
		public virtual Sbor.Tablepart.BalancingIFDB_RuleIndex Master{get; set;}
		

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
		/// Тип фильтра по полю КВСР/КАДБ/КАИФ
		/// </summary>
		public byte? IdFilterFieldType_KVSR{get; set;}
                            /// <summary>
                            /// Тип фильтра по полю КВСР/КАДБ/КАИФ
                            /// </summary>
							[NotMapped] 
                            public virtual Sbor.DbEnums.FilterFieldType? FilterFieldType_KVSR {
								get { return (Sbor.DbEnums.FilterFieldType?)this.IdFilterFieldType_KVSR; } 
								set { this.IdFilterFieldType_KVSR = (byte?) value; }
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
		/// Тип фильтра по полю Территория
		/// </summary>
		public byte? IdFilterFieldType_OKATO{get; set;}
                            /// <summary>
                            /// Тип фильтра по полю Территория
                            /// </summary>
							[NotMapped] 
                            public virtual Sbor.DbEnums.FilterFieldType? FilterFieldType_OKATO {
								get { return (Sbor.DbEnums.FilterFieldType?)this.IdFilterFieldType_OKATO; } 
								set { this.IdFilterFieldType_OKATO = (byte?) value; }
							}

		/// <summary>
		/// Тип фильтра по полю Код РО
		/// </summary>
		public byte? IdFilterFieldType_AuthorityOfExpenseObligation{get; set;}
                            /// <summary>
                            /// Тип фильтра по полю Код РО
                            /// </summary>
							[NotMapped] 
                            public virtual Sbor.DbEnums.FilterFieldType? FilterFieldType_AuthorityOfExpenseObligation {
								get { return (Sbor.DbEnums.FilterFieldType?)this.IdFilterFieldType_AuthorityOfExpenseObligation; } 
								set { this.IdFilterFieldType_AuthorityOfExpenseObligation = (byte?) value; }
							}

	

			private ICollection<Sbor.Reference.FinanceSource> _mlFinanceSources; 
        /// <summary>
        /// Набор КБК для правила
        /// </summary>
        [JsonIgnoreForException]
		public virtual ICollection<Sbor.Reference.FinanceSource> FinanceSources 
		{
			get{ return _mlFinanceSources ?? (_mlFinanceSources = new List<Sbor.Reference.FinanceSource>()); } 
			set{ _mlFinanceSources = value; }
		}
			private ICollection<Sbor.Reference.KFO> _mlKFOs; 
        /// <summary>
        /// Набор КБК для правила
        /// </summary>
        [JsonIgnoreForException]
		public virtual ICollection<Sbor.Reference.KFO> KFOs 
		{
			get{ return _mlKFOs ?? (_mlKFOs = new List<Sbor.Reference.KFO>()); } 
			set{ _mlKFOs = value; }
		}
			private ICollection<Sbor.Reference.KVSR> _mlKVSRs; 
        /// <summary>
        /// Набор КБК для правила
        /// </summary>
        [JsonIgnoreForException]
		public virtual ICollection<Sbor.Reference.KVSR> KVSRs 
		{
			get{ return _mlKVSRs ?? (_mlKVSRs = new List<Sbor.Reference.KVSR>()); } 
			set{ _mlKVSRs = value; }
		}
			private ICollection<Sbor.Reference.RZPR> _mlRZPRs; 
        /// <summary>
        /// Набор КБК для правила
        /// </summary>
        [JsonIgnoreForException]
		public virtual ICollection<Sbor.Reference.RZPR> RZPRs 
		{
			get{ return _mlRZPRs ?? (_mlRZPRs = new List<Sbor.Reference.RZPR>()); } 
			set{ _mlRZPRs = value; }
		}
			private ICollection<Sbor.Reference.KCSR> _mlKCSRs; 
        /// <summary>
        /// Набор КБК для правила
        /// </summary>
        [JsonIgnoreForException]
		public virtual ICollection<Sbor.Reference.KCSR> KCSRs 
		{
			get{ return _mlKCSRs ?? (_mlKCSRs = new List<Sbor.Reference.KCSR>()); } 
			set{ _mlKCSRs = value; }
		}
			private ICollection<Sbor.Reference.KVR> _mlKVRs; 
        /// <summary>
        /// Набор КБК для правила
        /// </summary>
        [JsonIgnoreForException]
		public virtual ICollection<Sbor.Reference.KVR> KVRs 
		{
			get{ return _mlKVRs ?? (_mlKVRs = new List<Sbor.Reference.KVR>()); } 
			set{ _mlKVRs = value; }
		}
			private ICollection<Sbor.Reference.KOSGU> _mlKOSGUs; 
        /// <summary>
        /// Набор КБК для правила
        /// </summary>
        [JsonIgnoreForException]
		public virtual ICollection<Sbor.Reference.KOSGU> KOSGUs 
		{
			get{ return _mlKOSGUs ?? (_mlKOSGUs = new List<Sbor.Reference.KOSGU>()); } 
			set{ _mlKOSGUs = value; }
		}
			private ICollection<Sbor.Reference.DFK> _mlDFKs; 
        /// <summary>
        /// Набор КБК для правила
        /// </summary>
        [JsonIgnoreForException]
		public virtual ICollection<Sbor.Reference.DFK> DFKs 
		{
			get{ return _mlDFKs ?? (_mlDFKs = new List<Sbor.Reference.DFK>()); } 
			set{ _mlDFKs = value; }
		}
			private ICollection<Sbor.Reference.DKR> _mlDKRs; 
        /// <summary>
        /// Набор КБК для правила
        /// </summary>
        [JsonIgnoreForException]
		public virtual ICollection<Sbor.Reference.DKR> DKRs 
		{
			get{ return _mlDKRs ?? (_mlDKRs = new List<Sbor.Reference.DKR>()); } 
			set{ _mlDKRs = value; }
		}
			private ICollection<Sbor.Reference.DEK> _mlDEKs; 
        /// <summary>
        /// Набор КБК для правила
        /// </summary>
        [JsonIgnoreForException]
		public virtual ICollection<Sbor.Reference.DEK> DEKs 
		{
			get{ return _mlDEKs ?? (_mlDEKs = new List<Sbor.Reference.DEK>()); } 
			set{ _mlDEKs = value; }
		}
			private ICollection<Sbor.Reference.CodeSubsidy> _mlCodeSubsidys; 
        /// <summary>
        /// Набор КБК для правила
        /// </summary>
        [JsonIgnoreForException]
		public virtual ICollection<Sbor.Reference.CodeSubsidy> CodeSubsidys 
		{
			get{ return _mlCodeSubsidys ?? (_mlCodeSubsidys = new List<Sbor.Reference.CodeSubsidy>()); } 
			set{ _mlCodeSubsidys = value; }
		}
			private ICollection<Sbor.Reference.BranchCode> _mlBranchCodes; 
        /// <summary>
        /// Набор КБК для правила
        /// </summary>
        [JsonIgnoreForException]
		public virtual ICollection<Sbor.Reference.BranchCode> BranchCodes 
		{
			get{ return _mlBranchCodes ?? (_mlBranchCodes = new List<Sbor.Reference.BranchCode>()); } 
			set{ _mlBranchCodes = value; }
		}
			private ICollection<BaseApp.Reference.OKATO> _mlOKATOs; 
        /// <summary>
        /// Набор КБК для правила
        /// </summary>
        [JsonIgnoreForException]
		public virtual ICollection<BaseApp.Reference.OKATO> OKATOs 
		{
			get{ return _mlOKATOs ?? (_mlOKATOs = new List<BaseApp.Reference.OKATO>()); } 
			set{ _mlOKATOs = value; }
		}
			private ICollection<Sbor.Reference.AuthorityOfExpenseObligation> _mlAuthorityOfExpenseObligations; 
        /// <summary>
        /// Набор КБК для правила
        /// </summary>
        [JsonIgnoreForException]
		public virtual ICollection<Sbor.Reference.AuthorityOfExpenseObligation> AuthorityOfExpenseObligations 
		{
			get{ return _mlAuthorityOfExpenseObligations ?? (_mlAuthorityOfExpenseObligations = new List<Sbor.Reference.AuthorityOfExpenseObligation>()); } 
			set{ _mlAuthorityOfExpenseObligations = value; }
		}
			
		/// <summary>
		/// Конструктор по-умолчанию
		/// </summary>
		public BalancingIFDB_RuleFilterKBK()
		{
		}

		/// <summary>
		/// Идентификатор типа сущности
		/// </summary>
		public override int EntityId
		{
			get { return -2013265294; }
		}

		/// <summary>
		/// Идентификатор типа сущности
		/// </summary>
		public static int EntityIdStatic
		{
			get { return -2013265294; }
		}

		/// <summary>
		/// Русское наименование типа сущности
		/// </summary>
		public override string EntityCaption
		{
			get { return "Набор КБК для правила"; }
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
				Platform.BusinessLogic.EntityIds.Add(GetType().DeclaringType,-2013265294);
			}
		}


	}
}