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
	/// ТЧ Нормативные правовые акты (Расходные обязательства)
	/// </summary>
	public partial class ExpenditureObligations_RegulatoryAct : TablePartEntity      
	{
	
		/// <summary>
		/// Идентификатор
		/// </summary>
		
        public override Int32 Id{get; set;}

		/// <summary>
		/// Расходные обязательства
		/// </summary>
		public int IdOwner{get; set;}
        /// <summary>
	    /// Расходные обязательства
	    /// </summary>
		public virtual Sbor.Reference.ExpenditureObligations Owner{get; set;}
		

		/// <summary>
		/// НПА
		/// </summary>
		public int IdRegulatoryAct{get; set;}
        /// <summary>
	    /// НПА
	    /// </summary>
		public virtual Sbor.Reference.RegulatoryAct RegulatoryAct{get; set;}
		

			private ICollection<Sbor.Tablepart.ExpenditureObligations_RegulatoryAct_StructuralUnit> _ExpenditureObligations_RegulatoryAct_StructuralUnit; 
        /// <summary>
        /// Связь с ТЧ Нормативные правовые акты
        /// </summary>
        [JsonIgnoreForException]
		public virtual ICollection<Sbor.Tablepart.ExpenditureObligations_RegulatoryAct_StructuralUnit> ExpenditureObligations_RegulatoryAct_StructuralUnit 
		{
			get{ return _ExpenditureObligations_RegulatoryAct_StructuralUnit ?? (_ExpenditureObligations_RegulatoryAct_StructuralUnit = new List<Sbor.Tablepart.ExpenditureObligations_RegulatoryAct_StructuralUnit>()); } 
			set{ _ExpenditureObligations_RegulatoryAct_StructuralUnit = value; }
		}


		/// <summary>
		/// Конструктор по-умолчанию
		/// </summary>
		public ExpenditureObligations_RegulatoryAct()
		{
		}

		/// <summary>
		/// Идентификатор типа сущности
		/// </summary>
		public override int EntityId
		{
			get { return -1879048020; }
		}

		/// <summary>
		/// Идентификатор типа сущности
		/// </summary>
		public static int EntityIdStatic
		{
			get { return -1879048020; }
		}

		/// <summary>
		/// Русское наименование типа сущности
		/// </summary>
		public override string EntityCaption
		{
			get { return "ТЧ Нормативные правовые акты (Расходные обязательства)"; }
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
				Platform.BusinessLogic.EntityIds.Add(GetType().DeclaringType,-1879048020);
			}
		}


	}
}