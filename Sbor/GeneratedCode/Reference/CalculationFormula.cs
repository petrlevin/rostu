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
	/// Формулы расчета
	/// </summary>
	public partial class CalculationFormula : ReferenceEntity, IHasRefStatus      
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
		/// Описание
		/// </summary>
		public string Description{get; set;}

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

	
private ICollection<Sbor.Reference.IndicatorCalculation> _idIndicatorOfCalculationFormula; 
        /// <summary>
        /// Формула расчета
        /// </summary>
        [JsonIgnoreForException]
		public virtual ICollection<Sbor.Reference.IndicatorCalculation> IdIndicatorOfCalculationFormula 
		{
			get{ return _idIndicatorOfCalculationFormula ?? (_idIndicatorOfCalculationFormula = new List<Sbor.Reference.IndicatorCalculation>()); } 
			set{ _idIndicatorOfCalculationFormula = value; }
		}
			
		/// <summary>
		/// Конструктор по-умолчанию
		/// </summary>
		public CalculationFormula()
		{
		}

		/// <summary>
		/// Идентификатор типа сущности
		/// </summary>
		public override int EntityId
		{
			get { return -1543503627; }
		}

		/// <summary>
		/// Идентификатор типа сущности
		/// </summary>
		public static int EntityIdStatic
		{
			get { return -1543503627; }
		}

		/// <summary>
		/// Русское наименование типа сущности
		/// </summary>
		public override string EntityCaption
		{
			get { return "Формулы расчета"; }
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
				Platform.BusinessLogic.EntityIds.Add(GetType().DeclaringType,-1543503627);
			}
		}


	}
}