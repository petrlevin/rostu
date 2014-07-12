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
	/// Расходные обязательства
	/// </summary>
	public partial class ExpenditureObligations : ReferenceEntity, IHasRefStatus      
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
		/// Ведомство
		/// </summary>
		public int IdSBP{get; set;}
        /// <summary>
	    /// Ведомство
	    /// </summary>
		public virtual Sbor.Reference.SBP SBP{get; set;}
		

		/// <summary>
		/// Полномочие РО
		/// </summary>
		public int IdAuthorityOfExpenseObligation{get; set;}
        /// <summary>
	    /// Полномочие РО
	    /// </summary>
		public virtual Sbor.Reference.AuthorityOfExpenseObligation AuthorityOfExpenseObligation{get; set;}
		

		/// <summary>
		/// Наименование
		/// </summary>
		public string Caption{get; set;}

		/// <summary>
		/// Описание
		/// </summary>
		public string Description{get; set;}

		/// <summary>
		/// ТЧ Нормативные правовые акты
		/// </summary>
		private ICollection<Sbor.Tablepart.ExpenditureObligations_RegulatoryAct> _tpRegulatoryAct; 
        /// <summary>
        /// ТЧ Нормативные правовые акты
        /// </summary>
        [JsonIgnoreForException]
		public virtual ICollection<Sbor.Tablepart.ExpenditureObligations_RegulatoryAct> RegulatoryAct 
		{
			get{ return _tpRegulatoryAct ?? (_tpRegulatoryAct = new List<Sbor.Tablepart.ExpenditureObligations_RegulatoryAct>()); } 
			set{ _tpRegulatoryAct = value; }
		}

		/// <summary>
		/// ТЧ Структурные единицы НПА
		/// </summary>
		private ICollection<Sbor.Tablepart.ExpenditureObligations_RegulatoryAct_StructuralUnit> _tpRegulatoryAct_StructuralUnit; 
        /// <summary>
        /// ТЧ Структурные единицы НПА
        /// </summary>
        [JsonIgnoreForException]
		public virtual ICollection<Sbor.Tablepart.ExpenditureObligations_RegulatoryAct_StructuralUnit> RegulatoryAct_StructuralUnit 
		{
			get{ return _tpRegulatoryAct_StructuralUnit ?? (_tpRegulatoryAct_StructuralUnit = new List<Sbor.Tablepart.ExpenditureObligations_RegulatoryAct_StructuralUnit>()); } 
			set{ _tpRegulatoryAct_StructuralUnit = value; }
		}

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

	

		/// <summary>
		/// Конструктор по-умолчанию
		/// </summary>
		public ExpenditureObligations()
		{
		}

		/// <summary>
		/// Идентификатор типа сущности
		/// </summary>
		public override int EntityId
		{
			get { return -1879048021; }
		}

		/// <summary>
		/// Идентификатор типа сущности
		/// </summary>
		public static int EntityIdStatic
		{
			get { return -1879048021; }
		}

		/// <summary>
		/// Русское наименование типа сущности
		/// </summary>
		public override string EntityCaption
		{
			get { return "Расходные обязательства"; }
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
				Platform.BusinessLogic.EntityIds.Add(GetType().DeclaringType,-1879048021);
			}
		}


	}
}