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
	/// ТЧ «Методы распределения»
	/// </summary>
	public partial class PublicInstitutionEstimate_DistributionMethod : TablePartEntity      
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
		/// Наименование метода распределения
		/// </summary>
		public byte IdIndirectCostsDistributionMethod{get; set;}
                            /// <summary>
                            /// Наименование метода распределения
                            /// </summary>
							[NotMapped] 
                            public virtual Sbor.DbEnums.IndirectCostsDistributionMethod IndirectCostsDistributionMethod {
								get { return (Sbor.DbEnums.IndirectCostsDistributionMethod)this.IdIndirectCostsDistributionMethod; } 
								set { this.IdIndirectCostsDistributionMethod = (byte) value; }
							}

			private ICollection<Sbor.Tablepart.PublicInstitutionEstimate_DistributionAdditionalParam> _PublicInstitutionEstimate_DistributionAdditionalParam; 
        /// <summary>
        /// Метод
        /// </summary>
        [JsonIgnoreForException]
		public virtual ICollection<Sbor.Tablepart.PublicInstitutionEstimate_DistributionAdditionalParam> PublicInstitutionEstimate_DistributionAdditionalParam 
		{
			get{ return _PublicInstitutionEstimate_DistributionAdditionalParam ?? (_PublicInstitutionEstimate_DistributionAdditionalParam = new List<Sbor.Tablepart.PublicInstitutionEstimate_DistributionAdditionalParam>()); } 
			set{ _PublicInstitutionEstimate_DistributionAdditionalParam = value; }
		}
		private ICollection<Sbor.Tablepart.PublicInstitutionEstimate_DistributionActivity> _PublicInstitutionEstimate_DistributionActivity; 
        /// <summary>
        /// Метод
        /// </summary>
        [JsonIgnoreForException]
		public virtual ICollection<Sbor.Tablepart.PublicInstitutionEstimate_DistributionActivity> PublicInstitutionEstimate_DistributionActivity 
		{
			get{ return _PublicInstitutionEstimate_DistributionActivity ?? (_PublicInstitutionEstimate_DistributionActivity = new List<Sbor.Tablepart.PublicInstitutionEstimate_DistributionActivity>()); } 
			set{ _PublicInstitutionEstimate_DistributionActivity = value; }
		}
		private ICollection<Sbor.Tablepart.PublicInstitutionEstimate_IndirectExpenses> _PublicInstitutionEstimate_IndirectExpenses; 
        /// <summary>
        /// Метод
        /// </summary>
        [JsonIgnoreForException]
		public virtual ICollection<Sbor.Tablepart.PublicInstitutionEstimate_IndirectExpenses> PublicInstitutionEstimate_IndirectExpenses 
		{
			get{ return _PublicInstitutionEstimate_IndirectExpenses ?? (_PublicInstitutionEstimate_IndirectExpenses = new List<Sbor.Tablepart.PublicInstitutionEstimate_IndirectExpenses>()); } 
			set{ _PublicInstitutionEstimate_IndirectExpenses = value; }
		}


		/// <summary>
		/// Конструктор по-умолчанию
		/// </summary>
		public PublicInstitutionEstimate_DistributionMethod()
		{
		}

		/// <summary>
		/// Идентификатор типа сущности
		/// </summary>
		public override int EntityId
		{
			get { return -1207959520; }
		}

		/// <summary>
		/// Идентификатор типа сущности
		/// </summary>
		public static int EntityIdStatic
		{
			get { return -1207959520; }
		}

		/// <summary>
		/// Русское наименование типа сущности
		/// </summary>
		public override string EntityCaption
		{
			get { return "ТЧ «Методы распределения»"; }
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
				Platform.BusinessLogic.EntityIds.Add(GetType().DeclaringType,-1207959520);
			}
		}


	}
}