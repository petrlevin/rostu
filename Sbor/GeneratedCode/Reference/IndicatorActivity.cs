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

using Platform.PrimaryEntities.DbEnums;using Platform.PrimaryEntities.Common.DbEnums;

namespace Sbor.Reference
{
	// Это автоматически сгенерированный класс, изменения будут уничтожены при следующей генерации
	/// <summary>
	/// Показатели мероприятий
	/// </summary>
	public partial class IndicatorActivity : ReferenceEntity, IHasRefStatus      
	{
	
		/// <summary>
		/// Идентификатор
		/// </summary>
		
        public override Int32 Id{get; set;}

		/// <summary>
		/// ППО
		/// </summary>
		public int IdPublicLegalFormation{get; set;}
        /// <summary>
	    /// ППО
	    /// </summary>
		public virtual BaseApp.Reference.PublicLegalFormation PublicLegalFormation{get; set;}
		

		/// <summary>
		/// Тип показателя
		/// </summary>
		public byte IdIndicatorActivityType{get; set;}
                            /// <summary>
                            /// Тип показателя
                            /// </summary>
							[NotMapped] 
                            public virtual Sbor.DbEnums.IndicatorActivityType IndicatorActivityType {
								get { return (Sbor.DbEnums.IndicatorActivityType)this.IdIndicatorActivityType; } 
								set { this.IdIndicatorActivityType = (byte) value; }
							}

		/// <summary>
		/// Наименование
		/// </summary>
		public string Caption{get; set;}

		/// <summary>
		/// Единица измерения
		/// </summary>
		public int IdUnitDimension{get; set;}
        /// <summary>
	    /// Единица измерения
	    /// </summary>
		public virtual Sbor.Reference.UnitDimension UnitDimension{get; set;}
		

		/// <summary>
		/// Ведомство
		/// </summary>
		public int? IdSBP{get; set;}
        /// <summary>
	    /// Ведомство
	    /// </summary>
		public virtual Sbor.Reference.SBP SBP{get; set;}
		

		/// <summary>
		/// Условия восприятия
		/// </summary>
		public byte IdTermsOfPerception{get; set;}
                            /// <summary>
                            /// Условия восприятия
                            /// </summary>
							[NotMapped] 
                            public virtual Sbor.DbEnums.TermsOfPerception TermsOfPerception {
								get { return (Sbor.DbEnums.TermsOfPerception)this.IdTermsOfPerception; } 
								set { this.IdTermsOfPerception = (byte) value; }
							}

		/// <summary>
		/// Статус
		/// </summary>
		public byte IdRefStatus{get; set;}
                            /// <summary>
                            /// Статус
                            /// </summary>
							[NotMapped] 
                            public virtual RefStatus RefStatus {
								get { return (RefStatus)this.IdRefStatus; } 
								set { this.IdRefStatus = (byte) value; }
							}

		/// <summary>
		/// Методика расчета
		/// </summary>
		public int? IdCalculationFormula{get; set;}
        /// <summary>
	    /// Методика расчета
	    /// </summary>
		public virtual Sbor.Reference.CalculationFormula CalculationFormula{get; set;}
		

		/// <summary>
		/// Нормативный правовой акт
		/// </summary>
		public int? IdRegulatoryAct{get; set;}
        /// <summary>
	    /// Нормативный правовой акт
	    /// </summary>
		public virtual Sbor.Reference.RegulatoryAct RegulatoryAct{get; set;}
		

	

		/// <summary>
		/// Конструктор по-умолчанию
		/// </summary>
		public IndicatorActivity()
		{
		}

		/// <summary>
		/// Идентификатор типа сущности
		/// </summary>
		public override int EntityId
		{
			get { return -1543503839; }
		}

		/// <summary>
		/// Идентификатор типа сущности
		/// </summary>
		public static int EntityIdStatic
		{
			get { return -1543503839; }
		}

		/// <summary>
		/// Русское наименование типа сущности
		/// </summary>
		public override string EntityCaption
		{
			get { return "Показатели мероприятий"; }
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
				Platform.BusinessLogic.EntityIds.Add(GetType().DeclaringType,-1543503839);
			}
		}


	}
}