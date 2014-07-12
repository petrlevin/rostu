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
	/// Методы распределения
	/// </summary>
	public partial class FBA_DistributionMethods : TablePartEntity      
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
		/// Метод распределения
		/// </summary>
		public byte IdIndirectCostsDistributionMethod{get; set;}
                            /// <summary>
                            /// Метод распределения
                            /// </summary>
							[NotMapped] 
                            public virtual Sbor.DbEnums.IndirectCostsDistributionMethod IndirectCostsDistributionMethod {
								get { return (Sbor.DbEnums.IndirectCostsDistributionMethod)this.IdIndirectCostsDistributionMethod; } 
								set { this.IdIndirectCostsDistributionMethod = (byte) value; }
							}

			private ICollection<Sbor.Tablepart.FBA_CostActivities> _FBA_CostActivities; 
        /// <summary>
        /// Метод
        /// </summary>
        [JsonIgnoreForException]
		public virtual ICollection<Sbor.Tablepart.FBA_CostActivities> FBA_CostActivities 
		{
			get{ return _FBA_CostActivities ?? (_FBA_CostActivities = new List<Sbor.Tablepart.FBA_CostActivities>()); } 
			set{ _FBA_CostActivities = value; }
		}
		private ICollection<Sbor.Tablepart.FBA_DistributionAdditionalParameter> _FBA_DistributionAdditionalParameter; 
        /// <summary>
        /// Метод
        /// </summary>
        [JsonIgnoreForException]
		public virtual ICollection<Sbor.Tablepart.FBA_DistributionAdditionalParameter> FBA_DistributionAdditionalParameter 
		{
			get{ return _FBA_DistributionAdditionalParameter ?? (_FBA_DistributionAdditionalParameter = new List<Sbor.Tablepart.FBA_DistributionAdditionalParameter>()); } 
			set{ _FBA_DistributionAdditionalParameter = value; }
		}
		private ICollection<Sbor.Tablepart.FBA_ActivitiesDistribution> _FBA_ActivitiesDistribution; 
        /// <summary>
        /// Метод
        /// </summary>
        [JsonIgnoreForException]
		public virtual ICollection<Sbor.Tablepart.FBA_ActivitiesDistribution> FBA_ActivitiesDistribution 
		{
			get{ return _FBA_ActivitiesDistribution ?? (_FBA_ActivitiesDistribution = new List<Sbor.Tablepart.FBA_ActivitiesDistribution>()); } 
			set{ _FBA_ActivitiesDistribution = value; }
		}
		private ICollection<Sbor.Tablepart.FBA_IndirectCosts> _FBA_IndirectCosts; 
        /// <summary>
        /// Метод
        /// </summary>
        [JsonIgnoreForException]
		public virtual ICollection<Sbor.Tablepart.FBA_IndirectCosts> FBA_IndirectCosts 
		{
			get{ return _FBA_IndirectCosts ?? (_FBA_IndirectCosts = new List<Sbor.Tablepart.FBA_IndirectCosts>()); } 
			set{ _FBA_IndirectCosts = value; }
		}


		/// <summary>
		/// Конструктор по-умолчанию
		/// </summary>
		public FBA_DistributionMethods()
		{
		}

		/// <summary>
		/// Идентификатор типа сущности
		/// </summary>
		public override int EntityId
		{
			get { return -1946156903; }
		}

		/// <summary>
		/// Идентификатор типа сущности
		/// </summary>
		public static int EntityIdStatic
		{
			get { return -1946156903; }
		}

		/// <summary>
		/// Русское наименование типа сущности
		/// </summary>
		public override string EntityCaption
		{
			get { return "Методы распределения"; }
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
				Platform.BusinessLogic.EntityIds.Add(GetType().DeclaringType,-1946156903);
			}
		}


	}
}