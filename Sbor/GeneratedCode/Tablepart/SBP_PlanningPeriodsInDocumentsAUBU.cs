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
	/// Периоды планирования в документах АУ/БУ
	/// </summary>
	public partial class SBP_PlanningPeriodsInDocumentsAUBU : TablePartEntity      
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
		public virtual Sbor.Reference.SBP Owner{get; set;}
		

		/// <summary>
		/// Бюджет
		/// </summary>
		public int IdBudget{get; set;}
        /// <summary>
	    /// Бюджет
	    /// </summary>
		public virtual BaseApp.Reference.Budget Budget{get; set;}
		

		/// <summary>
		/// Тип периода в ОФГ
		/// </summary>
		public byte IdDocAUBUPeriodType_OFG{get; set;}
                            /// <summary>
                            /// Тип периода в ОФГ
                            /// </summary>
							[NotMapped] 
                            public virtual Sbor.DbEnums.DocAUBUPeriodType DocAUBUPeriodType_OFG {
								get { return (Sbor.DbEnums.DocAUBUPeriodType)this.IdDocAUBUPeriodType_OFG; } 
								set { this.IdDocAUBUPeriodType_OFG = (byte) value; }
							}

		/// <summary>
		/// Тип периода в ПФГ-1
		/// </summary>
		public byte IdDocAUBUPeriodType_PFG1{get; set;}
                            /// <summary>
                            /// Тип периода в ПФГ-1
                            /// </summary>
							[NotMapped] 
                            public virtual Sbor.DbEnums.DocAUBUPeriodType DocAUBUPeriodType_PFG1 {
								get { return (Sbor.DbEnums.DocAUBUPeriodType)this.IdDocAUBUPeriodType_PFG1; } 
								set { this.IdDocAUBUPeriodType_PFG1 = (byte) value; }
							}

		/// <summary>
		/// Тип периода в ПФГ-2
		/// </summary>
		public byte IdDocAUBUPeriodType_PFG2{get; set;}
                            /// <summary>
                            /// Тип периода в ПФГ-2
                            /// </summary>
							[NotMapped] 
                            public virtual Sbor.DbEnums.DocAUBUPeriodType DocAUBUPeriodType_PFG2 {
								get { return (Sbor.DbEnums.DocAUBUPeriodType)this.IdDocAUBUPeriodType_PFG2; } 
								set { this.IdDocAUBUPeriodType_PFG2 = (byte) value; }
							}

	

		/// <summary>
		/// Конструктор по-умолчанию
		/// </summary>
		public SBP_PlanningPeriodsInDocumentsAUBU()
		{
		}

		/// <summary>
		/// Идентификатор типа сущности
		/// </summary>
		public override int EntityId
		{
			get { return -2013265545; }
		}

		/// <summary>
		/// Идентификатор типа сущности
		/// </summary>
		public static int EntityIdStatic
		{
			get { return -2013265545; }
		}

		/// <summary>
		/// Русское наименование типа сущности
		/// </summary>
		public override string EntityCaption
		{
			get { return "Периоды планирования в документах АУ/БУ"; }
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
				Platform.BusinessLogic.EntityIds.Add(GetType().DeclaringType,-2013265545);
			}
		}


	}
}