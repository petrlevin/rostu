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
	/// ТЧ Структурные единицы НПА (Расходные обязательства)
	/// </summary>
	public partial class ExpenditureObligations_RegulatoryAct_StructuralUnit : TablePartEntity      
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
		/// Связь с ТЧ Нормативные правовые акты
		/// </summary>
		public int IdMaster{get; set;}
        /// <summary>
	    /// Связь с ТЧ Нормативные правовые акты
	    /// </summary>
		public virtual Sbor.Tablepart.ExpenditureObligations_RegulatoryAct Master{get; set;}
		

		/// <summary>
		/// Наименование
		/// </summary>
		public int IdRegulatoryAct_StructuralUnit{get; set;}
        /// <summary>
	    /// Наименование
	    /// </summary>
		public virtual Sbor.Tablepart.RegulatoryAct_StructuralUnit RegulatoryAct_StructuralUnit{get; set;}
		

	

		/// <summary>
		/// Конструктор по-умолчанию
		/// </summary>
		public ExpenditureObligations_RegulatoryAct_StructuralUnit()
		{
		}

		/// <summary>
		/// Идентификатор типа сущности
		/// </summary>
		public override int EntityId
		{
			get { return -1879048019; }
		}

		/// <summary>
		/// Идентификатор типа сущности
		/// </summary>
		public static int EntityIdStatic
		{
			get { return -1879048019; }
		}

		/// <summary>
		/// Русское наименование типа сущности
		/// </summary>
		public override string EntityCaption
		{
			get { return "ТЧ Структурные единицы НПА (Расходные обязательства)"; }
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
				Platform.BusinessLogic.EntityIds.Add(GetType().DeclaringType,-1879048019);
			}
		}


	}
}