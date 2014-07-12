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



namespace Sbor.Document
{
	// Это автоматически сгенерированный класс, изменения будут уничтожены при следующей генерации
	/// <summary>
	/// План финансово-хозяйственной деятельности
	/// </summary>
	public partial class FinancialAndBusinessActivities : DocumentEntity<FinancialAndBusinessActivities>    , IHierarhy  
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
		/// Бюджет
		/// </summary>
		public int IdBudget{get; set;}
        /// <summary>
	    /// Бюджет
	    /// </summary>
		public virtual BaseApp.Reference.Budget Budget{get; set;}
		

		/// <summary>
		/// Версия
		/// </summary>
		public int IdVersion{get; set;}
        /// <summary>
	    /// Версия
	    /// </summary>
		public virtual BaseApp.Reference.Version Version{get; set;}
		

		/// <summary>
		/// Номер
		/// </summary>
		public override string Number{get; set;}

		/// <summary>
		/// Дата
		/// </summary>
		private DateTime _Date; 
        /// <summary>
	    /// Дата
	    /// </summary>
		public override  DateTime Date 
		{
			get{ return _Date.Date; }
			set{ _Date = value.Date; }
		}

		/// <summary>
		/// Системная дата утверждения
		/// </summary>
		private DateTime? _DateCommit; 
        /// <summary>
	    /// Системная дата утверждения
	    /// </summary>
		public  DateTime? DateCommit 
		{
			get{ return _DateCommit != null ? ((DateTime)_DateCommit).Date : (DateTime?)null; }
			set{ _DateCommit = value != null ? ((DateTime)value).Date : value; }
		}

		/// <summary>
		/// Утвержден
		/// </summary>
		public bool IsApproved{get; set;}

		/// <summary>
		/// Последнее редактирование
		/// </summary>
		private DateTime? _DateLastEdit; 
        /// <summary>
	    /// Последнее редактирование
	    /// </summary>
		public  DateTime? DateLastEdit 
		{
			get{ return _DateLastEdit != null ? ((DateTime)_DateLastEdit).Date : (DateTime?)null; }
			set{ _DateLastEdit = value != null ? ((DateTime)value).Date : value; }
		}

		/// <summary>
		/// Учреждение
		/// </summary>
		public int IdSBP{get; set;}
        /// <summary>
	    /// Учреждение
	    /// </summary>
		public virtual Sbor.Reference.SBP SBP{get; set;}
		

		/// <summary>
		/// Требует уточнения
		/// </summary>
		public bool IsRequireClarification{get; set;}

		/// <summary>
		/// Причина уточнения
		/// </summary>
		public string ReasonClarification{get; set;}

		/// <summary>
		/// Причина отказа
		/// </summary>
		public string ReasonCancel{get; set;}

		/// <summary>
		/// Описание
		/// </summary>
		public string Description{get; set;}

		/// <summary>
		/// Предыдущая редакция
		/// </summary>
		public int? IdParent{get; set;}
        /// <summary>
	    /// Предыдущая редакция
	    /// </summary>
		public virtual Sbor.Document.FinancialAndBusinessActivities Parent{get; set;}
		private ICollection<Sbor.Document.FinancialAndBusinessActivities> _idParent; 
        /// <summary>
        /// Предыдущая редакция
        /// </summary>
        [JsonIgnoreForException]
		public virtual ICollection<Sbor.Document.FinancialAndBusinessActivities> ChildrenByidParent 
		{
			get{ return _idParent ?? (_idParent = new List<Sbor.Document.FinancialAndBusinessActivities>()); } 
			set{ _idParent = value; }
		}

		/// <summary>
		/// Дата прекращения
		/// </summary>
		private DateTime? _DateTerminate; 
        /// <summary>
	    /// Дата прекращения
	    /// </summary>
		public  DateTime? DateTerminate 
		{
			get{ return _DateTerminate != null ? ((DateTime)_DateTerminate).Date : (DateTime?)null; }
			set{ _DateTerminate = value != null ? ((DateTime)value).Date : value; }
		}

		/// <summary>
		/// Причина прекращения
		/// </summary>
		public string ReasonTerminate{get; set;}

		/// <summary>
		/// Вести доп. потребности
		/// </summary>
		public bool IsExtraNeed{get; set;}

		/// <summary>
		/// Объем публичных обязательств, всего
		/// </summary>
		public decimal? VolumePublicObligations{get; set;}

		/// <summary>
		/// Иная информация
		/// </summary>
		public string OtherInformation{get; set;}

		/// <summary>
		/// Мероприятия
		/// </summary>
		private ICollection<Sbor.Tablepart.FBA_Activity> _tpActivity; 
        /// <summary>
        /// Мероприятия
        /// </summary>
        [JsonIgnoreForException]
		public virtual ICollection<Sbor.Tablepart.FBA_Activity> Activity 
		{
			get{ return _tpActivity ?? (_tpActivity = new List<Sbor.Tablepart.FBA_Activity>()); } 
			set{ _tpActivity = value; }
		}

		/// <summary>
		/// Плановые объемы поступлений
		/// </summary>
		private ICollection<Sbor.Tablepart.FBA_PlannedVolumeIncome> _tpPlannedVolumeIncomes; 
        /// <summary>
        /// Плановые объемы поступлений
        /// </summary>
        [JsonIgnoreForException]
		public virtual ICollection<Sbor.Tablepart.FBA_PlannedVolumeIncome> PlannedVolumeIncomes 
		{
			get{ return _tpPlannedVolumeIncomes ?? (_tpPlannedVolumeIncomes = new List<Sbor.Tablepart.FBA_PlannedVolumeIncome>()); } 
			set{ _tpPlannedVolumeIncomes = value; }
		}

		/// <summary>
		/// Плановые объемы поступлений - значения
		/// </summary>
		private ICollection<Sbor.Tablepart.FBA_PlannedVolumeIncome_value> _tpPlannedVolumeIncome_values; 
        /// <summary>
        /// Плановые объемы поступлений - значения
        /// </summary>
        [JsonIgnoreForException]
		public virtual ICollection<Sbor.Tablepart.FBA_PlannedVolumeIncome_value> PlannedVolumeIncome_values 
		{
			get{ return _tpPlannedVolumeIncome_values ?? (_tpPlannedVolumeIncome_values = new List<Sbor.Tablepart.FBA_PlannedVolumeIncome_value>()); } 
			set{ _tpPlannedVolumeIncome_values = value; }
		}

		/// <summary>
		/// Расходы по мероприятиям
		/// </summary>
		private ICollection<Sbor.Tablepart.FBA_CostActivities> _tpCostActivitiess; 
        /// <summary>
        /// Расходы по мероприятиям
        /// </summary>
        [JsonIgnoreForException]
		public virtual ICollection<Sbor.Tablepart.FBA_CostActivities> CostActivitiess 
		{
			get{ return _tpCostActivitiess ?? (_tpCostActivitiess = new List<Sbor.Tablepart.FBA_CostActivities>()); } 
			set{ _tpCostActivitiess = value; }
		}

		/// <summary>
		/// Расходы по мероприятиям - значения
		/// </summary>
		private ICollection<Sbor.Tablepart.FBA_CostActivities_value> _tpCostActivities_values; 
        /// <summary>
        /// Расходы по мероприятиям - значения
        /// </summary>
        [JsonIgnoreForException]
		public virtual ICollection<Sbor.Tablepart.FBA_CostActivities_value> CostActivities_values 
		{
			get{ return _tpCostActivities_values ?? (_tpCostActivities_values = new List<Sbor.Tablepart.FBA_CostActivities_value>()); } 
			set{ _tpCostActivities_values = value; }
		}

		/// <summary>
		/// Методы распределения
		/// </summary>
		private ICollection<Sbor.Tablepart.FBA_DistributionMethods> _tpDistributionMethodss; 
        /// <summary>
        /// Методы распределения
        /// </summary>
        [JsonIgnoreForException]
		public virtual ICollection<Sbor.Tablepart.FBA_DistributionMethods> DistributionMethodss 
		{
			get{ return _tpDistributionMethodss ?? (_tpDistributionMethodss = new List<Sbor.Tablepart.FBA_DistributionMethods>()); } 
			set{ _tpDistributionMethodss = value; }
		}

		/// <summary>
		/// Дополнительный параметр распределения
		/// </summary>
		private ICollection<Sbor.Tablepart.FBA_DistributionAdditionalParameter> _tpDistributionAdditionalParameters; 
        /// <summary>
        /// Дополнительный параметр распределения
        /// </summary>
        [JsonIgnoreForException]
		public virtual ICollection<Sbor.Tablepart.FBA_DistributionAdditionalParameter> DistributionAdditionalParameters 
		{
			get{ return _tpDistributionAdditionalParameters ?? (_tpDistributionAdditionalParameters = new List<Sbor.Tablepart.FBA_DistributionAdditionalParameter>()); } 
			set{ _tpDistributionAdditionalParameters = value; }
		}

		/// <summary>
		/// Мероприятия для распределения
		/// </summary>
		private ICollection<Sbor.Tablepart.FBA_ActivitiesDistribution> _tpActivitiesDistributions; 
        /// <summary>
        /// Мероприятия для распределения
        /// </summary>
        [JsonIgnoreForException]
		public virtual ICollection<Sbor.Tablepart.FBA_ActivitiesDistribution> ActivitiesDistributions 
		{
			get{ return _tpActivitiesDistributions ?? (_tpActivitiesDistributions = new List<Sbor.Tablepart.FBA_ActivitiesDistribution>()); } 
			set{ _tpActivitiesDistributions = value; }
		}

		/// <summary>
		/// Косвенные расходы
		/// </summary>
		private ICollection<Sbor.Tablepart.FBA_IndirectCosts> _tpIndirectCosts; 
        /// <summary>
        /// Косвенные расходы
        /// </summary>
        [JsonIgnoreForException]
		public virtual ICollection<Sbor.Tablepart.FBA_IndirectCosts> IndirectCosts 
		{
			get{ return _tpIndirectCosts ?? (_tpIndirectCosts = new List<Sbor.Tablepart.FBA_IndirectCosts>()); } 
			set{ _tpIndirectCosts = value; }
		}

		/// <summary>
		/// Косвенные расходы - значения
		/// </summary>
		private ICollection<Sbor.Tablepart.FBA_IndirectCosts_value> _tpIndirectCosts_values; 
        /// <summary>
        /// Косвенные расходы - значения
        /// </summary>
        [JsonIgnoreForException]
		public virtual ICollection<Sbor.Tablepart.FBA_IndirectCosts_value> IndirectCosts_values 
		{
			get{ return _tpIndirectCosts_values ?? (_tpIndirectCosts_values = new List<Sbor.Tablepart.FBA_IndirectCosts_value>()); } 
			set{ _tpIndirectCosts_values = value; }
		}

		/// <summary>
		/// Цели деятельности учреждения
		/// </summary>
		private ICollection<Sbor.Tablepart.FBA_DepartmentActivityGoal> _tpDepartmentActivityGoals; 
        /// <summary>
        /// Цели деятельности учреждения
        /// </summary>
        [JsonIgnoreForException]
		public virtual ICollection<Sbor.Tablepart.FBA_DepartmentActivityGoal> DepartmentActivityGoals 
		{
			get{ return _tpDepartmentActivityGoals ?? (_tpDepartmentActivityGoals = new List<Sbor.Tablepart.FBA_DepartmentActivityGoal>()); } 
			set{ _tpDepartmentActivityGoals = value; }
		}

		/// <summary>
		/// Показатели финансового состояния учреждения
		/// </summary>
		private ICollection<Sbor.Tablepart.FBA_FinancialIndicatorsInstitutions> _tpFinancialIndicatorsInstitutionss; 
        /// <summary>
        /// Показатели финансового состояния учреждения
        /// </summary>
        [JsonIgnoreForException]
		public virtual ICollection<Sbor.Tablepart.FBA_FinancialIndicatorsInstitutions> FinancialIndicatorsInstitutionss 
		{
			get{ return _tpFinancialIndicatorsInstitutionss ?? (_tpFinancialIndicatorsInstitutionss = new List<Sbor.Tablepart.FBA_FinancialIndicatorsInstitutions>()); } 
			set{ _tpFinancialIndicatorsInstitutionss = value; }
		}

		/// <summary>
		/// Статус
		/// </summary>
		public override int IdDocStatus{get; set;}

		/// <summary>
		/// Наименование
		/// </summary>
		public string Caption{get; set;}

		/// <summary>
		/// Актуальный бланк формирования
		/// </summary>
		public int? IdSBP_BlankActual{get; set;}
        /// <summary>
	    /// Актуальный бланк формирования
	    /// </summary>
		public virtual Sbor.Tablepart.SBP_BlankHistory SBP_BlankActual{get; set;}
		

		/// <summary>
		/// Требует согласования
		/// </summary>
		public bool IsRequireCheck{get; set;}

	

		/// <summary>
		/// Конструктор по-умолчанию
		/// </summary>
		public FinancialAndBusinessActivities()
		{
		}

		/// <summary>
		/// Идентификатор типа сущности
		/// </summary>
		public override int EntityId
		{
			get { return -1946156914; }
		}

		/// <summary>
		/// Идентификатор типа сущности
		/// </summary>
		public static int EntityIdStatic
		{
			get { return -1946156914; }
		}

		/// <summary>
		/// Русское наименование типа сущности
		/// </summary>
		public override string EntityCaption
		{
			get { return "План финансово-хозяйственной деятельности"; }
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
				Platform.BusinessLogic.EntityIds.Add(GetType().DeclaringType,-1946156914);
			}
		}


	}
}