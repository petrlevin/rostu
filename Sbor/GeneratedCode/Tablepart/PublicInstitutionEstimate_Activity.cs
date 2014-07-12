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
	/// ТЧ «Мероприятия»
	/// </summary>
	public partial class PublicInstitutionEstimate_Activity : TablePartEntity      
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
		public virtual Sbor.Document.PublicInstitutionEstimate Owner{get; set;}
		

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
		

		/// <summary>
		/// Показатель объема
		/// </summary>
		public int IdIndicatorActivity{get; set;}
        /// <summary>
	    /// Показатель объема
	    /// </summary>
		public virtual Sbor.Reference.IndicatorActivity IndicatorActivity{get; set;}
		

		/// <summary>
		/// Единица измерения
		/// </summary>
		public int IdUnitDimension{get; set;}
        /// <summary>
	    /// Единица измерения
	    /// </summary>
		public virtual Sbor.Reference.UnitDimension UnitDimension{get; set;}
		

			private ICollection<Sbor.Tablepart.PublicInstitutionEstimate_Expense> _PublicInstitutionEstimate_Expense; 
        /// <summary>
        /// Мероприятие
        /// </summary>
        [JsonIgnoreForException]
		public virtual ICollection<Sbor.Tablepart.PublicInstitutionEstimate_Expense> PublicInstitutionEstimate_Expense 
		{
			get{ return _PublicInstitutionEstimate_Expense ?? (_PublicInstitutionEstimate_Expense = new List<Sbor.Tablepart.PublicInstitutionEstimate_Expense>()); } 
			set{ _PublicInstitutionEstimate_Expense = value; }
		}
		private ICollection<Sbor.Tablepart.PublicInstitutionEstimate_DistributionActivity> _PublicInstitutionEstimate_DistributionActivity; 
        /// <summary>
        /// Мероприятие
        /// </summary>
        [JsonIgnoreForException]
		public virtual ICollection<Sbor.Tablepart.PublicInstitutionEstimate_DistributionActivity> PublicInstitutionEstimate_DistributionActivity 
		{
			get{ return _PublicInstitutionEstimate_DistributionActivity ?? (_PublicInstitutionEstimate_DistributionActivity = new List<Sbor.Tablepart.PublicInstitutionEstimate_DistributionActivity>()); } 
			set{ _PublicInstitutionEstimate_DistributionActivity = value; }
		}


		/// <summary>
		/// Конструктор по-умолчанию
		/// </summary>
		public PublicInstitutionEstimate_Activity()
		{
		}

		/// <summary>
		/// Идентификатор типа сущности
		/// </summary>
		public override int EntityId
		{
			get { return -1207959523; }
		}

		/// <summary>
		/// Идентификатор типа сущности
		/// </summary>
		public static int EntityIdStatic
		{
			get { return -1207959523; }
		}

		/// <summary>
		/// Русское наименование типа сущности
		/// </summary>
		public override string EntityCaption
		{
			get { return "ТЧ «Мероприятия»"; }
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
				Platform.BusinessLogic.EntityIds.Add(GetType().DeclaringType,-1207959523);
			}
		}


	}
}