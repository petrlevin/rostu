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
	public partial class ActivityOfSBP_Activity : TablePartEntity      
	{
	
		/// <summary>
		/// Ссылка на владельца
		/// </summary>
		public int IdOwner{get; set;}
        /// <summary>
	    /// Ссылка на владельца
	    /// </summary>
		public virtual Sbor.Document.ActivityOfSBP Owner{get; set;}
		

		/// <summary>
		/// Идентификатор
		/// </summary>
		
        public override Int32 Id{get; set;}

		/// <summary>
		/// Элемент СЦ
		/// </summary>
		public int IdMaster{get; set;}
        /// <summary>
	    /// Элемент СЦ
	    /// </summary>
		public virtual Sbor.Tablepart.ActivityOfSBP_SystemGoalElement Master{get; set;}
		

		/// <summary>
		/// Исполнитель
		/// </summary>
		public int IdSBP{get; set;}
        /// <summary>
	    /// Исполнитель
	    /// </summary>
		public virtual Sbor.Reference.SBP SBP{get; set;}
		

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
		public int IdIndicatorActivity_Volume{get; set;}
        /// <summary>
	    /// Показатель объема
	    /// </summary>
		public virtual Sbor.Reference.IndicatorActivity IndicatorActivity_Volume{get; set;}
		

		/// <summary>
		/// Наименование
		/// </summary>
		public int IdActivity{get; set;}
        /// <summary>
	    /// Наименование
	    /// </summary>
		public virtual Sbor.Reference.Activity Activity{get; set;}
		

			private ICollection<Sbor.Tablepart.ActivityOfSBP_ActivityResourceMaintenance> _ActivityOfSBP_ActivityResourceMaintenance; 
        /// <summary>
        /// Мероприятие
        /// </summary>
        [JsonIgnoreForException]
		public virtual ICollection<Sbor.Tablepart.ActivityOfSBP_ActivityResourceMaintenance> ActivityOfSBP_ActivityResourceMaintenance 
		{
			get{ return _ActivityOfSBP_ActivityResourceMaintenance ?? (_ActivityOfSBP_ActivityResourceMaintenance = new List<Sbor.Tablepart.ActivityOfSBP_ActivityResourceMaintenance>()); } 
			set{ _ActivityOfSBP_ActivityResourceMaintenance = value; }
		}
		private ICollection<Sbor.Tablepart.ActivityOfSBP_IndicatorQualityActivity> _ActivityOfSBP_IndicatorQualityActivity; 
        /// <summary>
        /// Мероприятие
        /// </summary>
        [JsonIgnoreForException]
		public virtual ICollection<Sbor.Tablepart.ActivityOfSBP_IndicatorQualityActivity> ActivityOfSBP_IndicatorQualityActivity 
		{
			get{ return _ActivityOfSBP_IndicatorQualityActivity ?? (_ActivityOfSBP_IndicatorQualityActivity = new List<Sbor.Tablepart.ActivityOfSBP_IndicatorQualityActivity>()); } 
			set{ _ActivityOfSBP_IndicatorQualityActivity = value; }
		}
		private ICollection<Sbor.Tablepart.ActivityOfSBP_Activity_Value> _ActivityOfSBP_Activity_Value; 
        /// <summary>
        /// Ссылка на главную ТЧ
        /// </summary>
        [JsonIgnoreForException]
		public virtual ICollection<Sbor.Tablepart.ActivityOfSBP_Activity_Value> ActivityOfSBP_Activity_Value 
		{
			get{ return _ActivityOfSBP_Activity_Value ?? (_ActivityOfSBP_Activity_Value = new List<Sbor.Tablepart.ActivityOfSBP_Activity_Value>()); } 
			set{ _ActivityOfSBP_Activity_Value = value; }
		}
		private ICollection<Sbor.Tablepart.ActivityOfSBP_ActivityDemandAndCapacity> _ActivityOfSBP_ActivityDemandAndCapacity; 
        /// <summary>
        /// Наименование
        /// </summary>
        [JsonIgnoreForException]
		public virtual ICollection<Sbor.Tablepart.ActivityOfSBP_ActivityDemandAndCapacity> ActivityOfSBP_ActivityDemandAndCapacity 
		{
			get{ return _ActivityOfSBP_ActivityDemandAndCapacity ?? (_ActivityOfSBP_ActivityDemandAndCapacity = new List<Sbor.Tablepart.ActivityOfSBP_ActivityDemandAndCapacity>()); } 
			set{ _ActivityOfSBP_ActivityDemandAndCapacity = value; }
		}


		/// <summary>
		/// Конструктор по-умолчанию
		/// </summary>
		public ActivityOfSBP_Activity()
		{
		}

		/// <summary>
		/// Идентификатор типа сущности
		/// </summary>
		public override int EntityId
		{
			get { return -1543503775; }
		}

		/// <summary>
		/// Идентификатор типа сущности
		/// </summary>
		public static int EntityIdStatic
		{
			get { return -1543503775; }
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
				Platform.BusinessLogic.EntityIds.Add(GetType().DeclaringType,-1543503775);
			}
		}


	}
}