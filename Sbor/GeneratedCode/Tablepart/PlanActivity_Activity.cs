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
	/// Мероприятия
	/// </summary>
	public partial class PlanActivity_Activity : TablePartEntity      
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
		/// Контингент
		/// </summary>
		public int? IdContingent{get; set;}
        /// <summary>
	    /// Контингент
	    /// </summary>
		public virtual Sbor.Reference.Contingent Contingent{get; set;}
		

		/// <summary>
		/// Показатель объема
		/// </summary>
		public int IdIndicatorActivity{get; set;}
        /// <summary>
	    /// Показатель объема
	    /// </summary>
		public virtual Sbor.Reference.IndicatorActivity IndicatorActivity{get; set;}
		

		/// <summary>
		/// Наименование
		/// </summary>
		public int IdActivity{get; set;}
        /// <summary>
	    /// Наименование
	    /// </summary>
		public virtual Sbor.Reference.Activity Activity{get; set;}
		

		/// <summary>
		/// Зарегистрировано в Деятельности ведомства
		/// </summary>
		public int? IdActivityOfSBP{get; set;}

		/// <summary>
		/// Деятельность ведомства
		/// </summary>
		public int? IdActivityOfSBP_A{get; set;}

			private ICollection<Sbor.Tablepart.PlanActivity_ActivityVolume> _PlanActivity_ActivityVolume; 
        /// <summary>
        /// Мероприятие
        /// </summary>
        [JsonIgnoreForException]
		public virtual ICollection<Sbor.Tablepart.PlanActivity_ActivityVolume> PlanActivity_ActivityVolume 
		{
			get{ return _PlanActivity_ActivityVolume ?? (_PlanActivity_ActivityVolume = new List<Sbor.Tablepart.PlanActivity_ActivityVolume>()); } 
			set{ _PlanActivity_ActivityVolume = value; }
		}
		private ICollection<Sbor.Tablepart.PlanActivity_IndicatorQualityActivity> _PlanActivity_IndicatorQualityActivity; 
        /// <summary>
        /// Мероприятие
        /// </summary>
        [JsonIgnoreForException]
		public virtual ICollection<Sbor.Tablepart.PlanActivity_IndicatorQualityActivity> PlanActivity_IndicatorQualityActivity 
		{
			get{ return _PlanActivity_IndicatorQualityActivity ?? (_PlanActivity_IndicatorQualityActivity = new List<Sbor.Tablepart.PlanActivity_IndicatorQualityActivity>()); } 
			set{ _PlanActivity_IndicatorQualityActivity = value; }
		}


		/// <summary>
		/// Конструктор по-умолчанию
		/// </summary>
		public PlanActivity_Activity()
		{
		}

		/// <summary>
		/// Идентификатор типа сущности
		/// </summary>
		public override int EntityId
		{
			get { return -2013265433; }
		}

		/// <summary>
		/// Идентификатор типа сущности
		/// </summary>
		public static int EntityIdStatic
		{
			get { return -2013265433; }
		}

		/// <summary>
		/// Русское наименование типа сущности
		/// </summary>
		public override string EntityCaption
		{
			get { return "Мероприятия"; }
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
				Platform.BusinessLogic.EntityIds.Add(GetType().DeclaringType,-2013265433);
			}
		}


	}
}