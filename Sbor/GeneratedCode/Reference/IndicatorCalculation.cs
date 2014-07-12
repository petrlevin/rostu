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
	/// Показатели расчета
	/// </summary>
	public partial class IndicatorCalculation : ReferenceEntity, IHasRefStatus      
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
		/// Наименование
		/// </summary>
		public string Caption{get; set;}

		/// <summary>
		/// Условное обозначение
		/// </summary>
		public string Symbol{get; set;}

		/// <summary>
		/// Единица измерения
		/// </summary>
		public int? IdUnitDimension{get; set;}
        /// <summary>
	    /// Единица измерения
	    /// </summary>
		public virtual Sbor.Reference.UnitDimension UnitDimension{get; set;}
		

		/// <summary>
		/// Значение по-умолчанию
		/// </summary>
		public decimal? DefaultValue{get; set;}

		/// <summary>
		/// Тип значения
		/// </summary>
		public byte IdIndicatorCalculationValueType{get; set;}
                            /// <summary>
                            /// Тип значения
                            /// </summary>
							[NotMapped] 
                            public virtual Sbor.DbEnums.IndicatorCalculationValueType IndicatorCalculationValueType {
								get { return (Sbor.DbEnums.IndicatorCalculationValueType)this.IdIndicatorCalculationValueType; } 
								set { this.IdIndicatorCalculationValueType = (byte) value; }
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
		/// Конструктор по-умолчанию
		/// </summary>
		public IndicatorCalculation()
		{
		}

		/// <summary>
		/// Идентификатор типа сущности
		/// </summary>
		public override int EntityId
		{
			get { return -1543503626; }
		}

		/// <summary>
		/// Идентификатор типа сущности
		/// </summary>
		public static int EntityIdStatic
		{
			get { return -1543503626; }
		}

		/// <summary>
		/// Русское наименование типа сущности
		/// </summary>
		public override string EntityCaption
		{
			get { return "Показатели расчета"; }
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
				Platform.BusinessLogic.EntityIds.Add(GetType().DeclaringType,-1543503626);
			}
		}


	}
}