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
	/// Бланки
	/// </summary>
	public partial class ApprovalBalancingIFDB_Blank : TablePartEntity      
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
		public virtual Sbor.Tool.ApprovalBalancingIFDB Owner{get; set;}
		

		/// <summary>
		/// Тип бланка
		/// </summary>
		public byte IdBlankType{get; set;}
                            /// <summary>
                            /// Тип бланка
                            /// </summary>
							[NotMapped] 
                            public virtual Sbor.DbEnums.BlankType BlankType {
								get { return (Sbor.DbEnums.BlankType)this.IdBlankType; } 
								set { this.IdBlankType = (byte) value; }
							}

		/// <summary>
		/// Тип РО
		/// </summary>
		public byte? IdBlankValueType_ExpenseObligationType{get; set;}
                            /// <summary>
                            /// Тип РО
                            /// </summary>
							[NotMapped] 
                            public virtual Sbor.DbEnums.BlankValueType? BlankValueType_ExpenseObligationType {
								get { return (Sbor.DbEnums.BlankValueType?)this.IdBlankValueType_ExpenseObligationType; } 
								set { this.IdBlankValueType_ExpenseObligationType = (byte?) value; }
							}

		/// <summary>
		/// Источник
		/// </summary>
		public byte IdBlankValueType_FinanceSource{get; set;}
                            /// <summary>
                            /// Источник
                            /// </summary>
							[NotMapped] 
                            public virtual Sbor.DbEnums.BlankValueType BlankValueType_FinanceSource {
								get { return (Sbor.DbEnums.BlankValueType)this.IdBlankValueType_FinanceSource; } 
								set { this.IdBlankValueType_FinanceSource = (byte) value; }
							}

		/// <summary>
		/// КФО
		/// </summary>
		public byte? IdBlankValueType_KFO{get; set;}
                            /// <summary>
                            /// КФО
                            /// </summary>
							[NotMapped] 
                            public virtual Sbor.DbEnums.BlankValueType? BlankValueType_KFO {
								get { return (Sbor.DbEnums.BlankValueType?)this.IdBlankValueType_KFO; } 
								set { this.IdBlankValueType_KFO = (byte?) value; }
							}

		/// <summary>
		/// КВСР/КАДБ/КАИФ
		/// </summary>
		public byte? IdBlankValueType_KVSR{get; set;}
                            /// <summary>
                            /// КВСР/КАДБ/КАИФ
                            /// </summary>
							[NotMapped] 
                            public virtual Sbor.DbEnums.BlankValueType? BlankValueType_KVSR {
								get { return (Sbor.DbEnums.BlankValueType?)this.IdBlankValueType_KVSR; } 
								set { this.IdBlankValueType_KVSR = (byte?) value; }
							}

		/// <summary>
		/// РзПр
		/// </summary>
		public byte? IdBlankValueType_RZPR{get; set;}
                            /// <summary>
                            /// РзПр
                            /// </summary>
							[NotMapped] 
                            public virtual Sbor.DbEnums.BlankValueType? BlankValueType_RZPR {
								get { return (Sbor.DbEnums.BlankValueType?)this.IdBlankValueType_RZPR; } 
								set { this.IdBlankValueType_RZPR = (byte?) value; }
							}

		/// <summary>
		/// КЦСР
		/// </summary>
		public byte? IdBlankValueType_KCSR{get; set;}
                            /// <summary>
                            /// КЦСР
                            /// </summary>
							[NotMapped] 
                            public virtual Sbor.DbEnums.BlankValueType? BlankValueType_KCSR {
								get { return (Sbor.DbEnums.BlankValueType?)this.IdBlankValueType_KCSR; } 
								set { this.IdBlankValueType_KCSR = (byte?) value; }
							}

		/// <summary>
		/// КВР
		/// </summary>
		public byte? IdBlankValueType_KVR{get; set;}
                            /// <summary>
                            /// КВР
                            /// </summary>
							[NotMapped] 
                            public virtual Sbor.DbEnums.BlankValueType? BlankValueType_KVR {
								get { return (Sbor.DbEnums.BlankValueType?)this.IdBlankValueType_KVR; } 
								set { this.IdBlankValueType_KVR = (byte?) value; }
							}

		/// <summary>
		/// КОСГУ
		/// </summary>
		public byte? IdBlankValueType_KOSGU{get; set;}
                            /// <summary>
                            /// КОСГУ
                            /// </summary>
							[NotMapped] 
                            public virtual Sbor.DbEnums.BlankValueType? BlankValueType_KOSGU {
								get { return (Sbor.DbEnums.BlankValueType?)this.IdBlankValueType_KOSGU; } 
								set { this.IdBlankValueType_KOSGU = (byte?) value; }
							}

		/// <summary>
		/// ДФК
		/// </summary>
		public byte? IdBlankValueType_DFK{get; set;}
                            /// <summary>
                            /// ДФК
                            /// </summary>
							[NotMapped] 
                            public virtual Sbor.DbEnums.BlankValueType? BlankValueType_DFK {
								get { return (Sbor.DbEnums.BlankValueType?)this.IdBlankValueType_DFK; } 
								set { this.IdBlankValueType_DFK = (byte?) value; }
							}

		/// <summary>
		/// ДКР
		/// </summary>
		public byte? IdBlankValueType_DKR{get; set;}
                            /// <summary>
                            /// ДКР
                            /// </summary>
							[NotMapped] 
                            public virtual Sbor.DbEnums.BlankValueType? BlankValueType_DKR {
								get { return (Sbor.DbEnums.BlankValueType?)this.IdBlankValueType_DKR; } 
								set { this.IdBlankValueType_DKR = (byte?) value; }
							}

		/// <summary>
		/// ДЭК
		/// </summary>
		public byte? IdBlankValueType_DEK{get; set;}
                            /// <summary>
                            /// ДЭК
                            /// </summary>
							[NotMapped] 
                            public virtual Sbor.DbEnums.BlankValueType? BlankValueType_DEK {
								get { return (Sbor.DbEnums.BlankValueType?)this.IdBlankValueType_DEK; } 
								set { this.IdBlankValueType_DEK = (byte?) value; }
							}

		/// <summary>
		/// Код субсидии
		/// </summary>
		public byte? IdBlankValueType_CodeSubsidy{get; set;}
                            /// <summary>
                            /// Код субсидии
                            /// </summary>
							[NotMapped] 
                            public virtual Sbor.DbEnums.BlankValueType? BlankValueType_CodeSubsidy {
								get { return (Sbor.DbEnums.BlankValueType?)this.IdBlankValueType_CodeSubsidy; } 
								set { this.IdBlankValueType_CodeSubsidy = (byte?) value; }
							}

		/// <summary>
		/// Отраслевой код
		/// </summary>
		public byte? IdBlankValueType_BranchCode{get; set;}
                            /// <summary>
                            /// Отраслевой код
                            /// </summary>
							[NotMapped] 
                            public virtual Sbor.DbEnums.BlankValueType? BlankValueType_BranchCode {
								get { return (Sbor.DbEnums.BlankValueType?)this.IdBlankValueType_BranchCode; } 
								set { this.IdBlankValueType_BranchCode = (byte?) value; }
							}

	

		/// <summary>
		/// Конструктор по-умолчанию
		/// </summary>
		public ApprovalBalancingIFDB_Blank()
		{
		}

		/// <summary>
		/// Идентификатор типа сущности
		/// </summary>
		public override int EntityId
		{
			get { return -2013265062; }
		}

		/// <summary>
		/// Идентификатор типа сущности
		/// </summary>
		public static int EntityIdStatic
		{
			get { return -2013265062; }
		}

		/// <summary>
		/// Русское наименование типа сущности
		/// </summary>
		public override string EntityCaption
		{
			get { return "Бланки"; }
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
				Platform.BusinessLogic.EntityIds.Add(GetType().DeclaringType,-2013265062);
			}
		}


	}
}