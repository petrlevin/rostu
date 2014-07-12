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
	/// ТЧ «Мероприятия АУ/БУ»
	/// </summary>
	public partial class PublicInstitutionEstimate_ActivityAUBU : TablePartEntity      
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
		/// Единица измерения
		/// </summary>
		public int? IdUnitDimension{get; set;}
        /// <summary>
	    /// Единица измерения
	    /// </summary>
		public virtual Sbor.Reference.UnitDimension UnitDimension{get; set;}
		

		/// <summary>
		/// Показатель объема
		/// </summary>
		public int? IdIndicatorActivity{get; set;}
        /// <summary>
	    /// Показатель объема
	    /// </summary>
		public virtual Sbor.Reference.IndicatorActivity IndicatorActivity{get; set;}
		

			private ICollection<Sbor.Tablepart.PublicInstitutionEstimate_FounderAUBUExpense> _PublicInstitutionEstimate_FounderAUBUExpense; 
        /// <summary>
        /// Мероприятие
        /// </summary>
        [JsonIgnoreForException]
		public virtual ICollection<Sbor.Tablepart.PublicInstitutionEstimate_FounderAUBUExpense> PublicInstitutionEstimate_FounderAUBUExpense 
		{
			get{ return _PublicInstitutionEstimate_FounderAUBUExpense ?? (_PublicInstitutionEstimate_FounderAUBUExpense = new List<Sbor.Tablepart.PublicInstitutionEstimate_FounderAUBUExpense>()); } 
			set{ _PublicInstitutionEstimate_FounderAUBUExpense = value; }
		}
		private ICollection<Sbor.Tablepart.PublicInstitutionEstimate_AloneAndBudgetInstitutionExpense> _PublicInstitutionEstimate_AloneAndBudgetInstitutionExpense; 
        /// <summary>
        /// Мероприятие
        /// </summary>
        [JsonIgnoreForException]
		public virtual ICollection<Sbor.Tablepart.PublicInstitutionEstimate_AloneAndBudgetInstitutionExpense> PublicInstitutionEstimate_AloneAndBudgetInstitutionExpense 
		{
			get{ return _PublicInstitutionEstimate_AloneAndBudgetInstitutionExpense ?? (_PublicInstitutionEstimate_AloneAndBudgetInstitutionExpense = new List<Sbor.Tablepart.PublicInstitutionEstimate_AloneAndBudgetInstitutionExpense>()); } 
			set{ _PublicInstitutionEstimate_AloneAndBudgetInstitutionExpense = value; }
		}


		/// <summary>
		/// Конструктор по-умолчанию
		/// </summary>
		public PublicInstitutionEstimate_ActivityAUBU()
		{
		}

		/// <summary>
		/// Идентификатор типа сущности
		/// </summary>
		public override int EntityId
		{
			get { return -1207959516; }
		}

		/// <summary>
		/// Идентификатор типа сущности
		/// </summary>
		public static int EntityIdStatic
		{
			get { return -1207959516; }
		}

		/// <summary>
		/// Русское наименование типа сущности
		/// </summary>
		public override string EntityCaption
		{
			get { return "ТЧ «Мероприятия АУ/БУ»"; }
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
				Platform.BusinessLogic.EntityIds.Add(GetType().DeclaringType,-1207959516);
			}
		}


	}
}