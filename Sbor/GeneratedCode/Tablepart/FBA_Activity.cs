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
	public partial class FBA_Activity : TablePartEntity      
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
		public virtual Sbor.Document.FinancialAndBusinessActivities Owner{get; set;}
		

		/// <summary>
		/// Собственная деятельность
		/// </summary>
		public bool IsOwnActivity{get; set;}

		/// <summary>
		/// Мероприятие
		/// </summary>
		public int IdActivity{get; set;}
        /// <summary>
	    /// Мероприятие
	    /// </summary>
		public virtual Sbor.Reference.Activity Activity{get; set;}
		

		/// <summary>
		/// Контингент
		/// </summary>
		public int? IdContingent{get; set;}
        /// <summary>
	    /// Контингент
	    /// </summary>
		public virtual Sbor.Reference.Contingent Contingent{get; set;}
		

			private ICollection<Sbor.Tablepart.FBA_PlannedVolumeIncome> _FBA_PlannedVolumeIncome; 
        /// <summary>
        /// Мероприятие
        /// </summary>
        [JsonIgnoreForException]
		public virtual ICollection<Sbor.Tablepart.FBA_PlannedVolumeIncome> FBA_PlannedVolumeIncome 
		{
			get{ return _FBA_PlannedVolumeIncome ?? (_FBA_PlannedVolumeIncome = new List<Sbor.Tablepart.FBA_PlannedVolumeIncome>()); } 
			set{ _FBA_PlannedVolumeIncome = value; }
		}
		private ICollection<Sbor.Tablepart.FBA_CostActivities> _FBA_CostActivities; 
        /// <summary>
        /// Мероприятие
        /// </summary>
        [JsonIgnoreForException]
		public virtual ICollection<Sbor.Tablepart.FBA_CostActivities> FBA_CostActivities 
		{
			get{ return _FBA_CostActivities ?? (_FBA_CostActivities = new List<Sbor.Tablepart.FBA_CostActivities>()); } 
			set{ _FBA_CostActivities = value; }
		}
		private ICollection<Sbor.Tablepart.FBA_ActivitiesDistribution> _FBA_ActivitiesDistribution; 
        /// <summary>
        /// Мероприятие
        /// </summary>
        [JsonIgnoreForException]
		public virtual ICollection<Sbor.Tablepart.FBA_ActivitiesDistribution> FBA_ActivitiesDistribution 
		{
			get{ return _FBA_ActivitiesDistribution ?? (_FBA_ActivitiesDistribution = new List<Sbor.Tablepart.FBA_ActivitiesDistribution>()); } 
			set{ _FBA_ActivitiesDistribution = value; }
		}


		/// <summary>
		/// Конструктор по-умолчанию
		/// </summary>
		public FBA_Activity()
		{
		}

		/// <summary>
		/// Идентификатор типа сущности
		/// </summary>
		public override int EntityId
		{
			get { return -1946156912; }
		}

		/// <summary>
		/// Идентификатор типа сущности
		/// </summary>
		public static int EntityIdStatic
		{
			get { return -1946156912; }
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
				Platform.BusinessLogic.EntityIds.Add(GetType().DeclaringType,-1946156912);
			}
		}


	}
}