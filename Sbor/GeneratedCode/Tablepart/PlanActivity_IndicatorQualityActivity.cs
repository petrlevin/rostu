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
	/// Показатели качества мероприятий
	/// </summary>
	public partial class PlanActivity_IndicatorQualityActivity : TablePartEntity      
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
		public virtual Sbor.Document.PlanActivity Owner{get; set;}
		

		/// <summary>
		/// Мероприятие
		/// </summary>
		public int? IdMaster{get; set;}
        /// <summary>
	    /// Мероприятие
	    /// </summary>
		public virtual Sbor.Tablepart.PlanActivity_Activity Master{get; set;}
		

		/// <summary>
		/// Показатель
		/// </summary>
		public int IdIndicatorActivity{get; set;}
        /// <summary>
	    /// Показатель
	    /// </summary>
		public virtual Sbor.Reference.IndicatorActivity IndicatorActivity{get; set;}
		

			private ICollection<Sbor.Tablepart.PlanActivity_IndicatorQualityActivityValue> _PlanActivity_IndicatorQualityActivityValue; 
        /// <summary>
        /// Показатель
        /// </summary>
        [JsonIgnoreForException]
		public virtual ICollection<Sbor.Tablepart.PlanActivity_IndicatorQualityActivityValue> PlanActivity_IndicatorQualityActivityValue 
		{
			get{ return _PlanActivity_IndicatorQualityActivityValue ?? (_PlanActivity_IndicatorQualityActivityValue = new List<Sbor.Tablepart.PlanActivity_IndicatorQualityActivityValue>()); } 
			set{ _PlanActivity_IndicatorQualityActivityValue = value; }
		}


		/// <summary>
		/// Конструктор по-умолчанию
		/// </summary>
		public PlanActivity_IndicatorQualityActivity()
		{
		}

		/// <summary>
		/// Идентификатор типа сущности
		/// </summary>
		public override int EntityId
		{
			get { return -2013265430; }
		}

		/// <summary>
		/// Идентификатор типа сущности
		/// </summary>
		public static int EntityIdStatic
		{
			get { return -2013265430; }
		}

		/// <summary>
		/// Русское наименование типа сущности
		/// </summary>
		public override string EntityCaption
		{
			get { return "Показатели качества мероприятий"; }
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
				Platform.BusinessLogic.EntityIds.Add(GetType().DeclaringType,-2013265430);
			}
		}


	}
}