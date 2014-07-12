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



namespace Sbor.Reports.Tablepart
{
	// Это автоматически сгенерированный класс, изменения будут уничтожены при следующей генерации
	/// <summary>
	/// Наименование колонок
	/// </summary>
	public partial class BudgetExpenseStructure_CustomColumn : TablePartEntity      
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
		public virtual Sbor.Reports.Report.BudgetExpenseStructure Owner{get; set;}
		

		/// <summary>
		/// Наименование
		/// </summary>
		public string Name{get; set;}

			private ICollection<Sbor.Reports.Tablepart.BudgetExpenseStructure_CustomFilter> _BudgetExpenseStructure_CustomFilter; 
        /// <summary>
        /// Колонка
        /// </summary>
        [JsonIgnoreForException]
		public virtual ICollection<Sbor.Reports.Tablepart.BudgetExpenseStructure_CustomFilter> BudgetExpenseStructure_CustomFilter 
		{
			get{ return _BudgetExpenseStructure_CustomFilter ?? (_BudgetExpenseStructure_CustomFilter = new List<Sbor.Reports.Tablepart.BudgetExpenseStructure_CustomFilter>()); } 
			set{ _BudgetExpenseStructure_CustomFilter = value; }
		}


		/// <summary>
		/// Конструктор по-умолчанию
		/// </summary>
		public BudgetExpenseStructure_CustomColumn()
		{
		}

		/// <summary>
		/// Идентификатор типа сущности
		/// </summary>
		public override int EntityId
		{
			get { return -1207959383; }
		}

		/// <summary>
		/// Идентификатор типа сущности
		/// </summary>
		public static int EntityIdStatic
		{
			get { return -1207959383; }
		}

		/// <summary>
		/// Русское наименование типа сущности
		/// </summary>
		public override string EntityCaption
		{
			get { return "Наименование колонок"; }
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
				Platform.BusinessLogic.EntityIds.Add(GetType().DeclaringType,-1207959383);
			}
		}


	}
}