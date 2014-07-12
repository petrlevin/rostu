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

using Platform.PrimaryEntities.Reference;

namespace Sbor.Reports.Tablepart
{
	// Это автоматически сгенерированный класс, изменения будут уничтожены при следующей генерации
	/// <summary>
	/// Перечень выводимых колонок
	/// </summary>
	public partial class BudgetExpenseStructure_Columns : TablePartEntity      
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
		/// КБК
		/// </summary>
		public int IdKBKEntity{get; set;}
        /// <summary>
	    /// КБК
	    /// </summary>
		public virtual Entity KBKEntity{get; set;}
		

		/// <summary>
		/// Порядок в отчете
		/// </summary>
		public Int16? Order{get; set;}

		/// <summary>
		/// Вывод группировочных итогов
		/// </summary>
		public bool IsGroupResult{get; set;}

		/// <summary>
		/// Уровень группировки
		/// </summary>
		public Int16? MinLevel{get; set;}

		/// <summary>
		/// Уровень детализации
		/// </summary>
		public Int16? MaxLevel{get; set;}

	

		/// <summary>
		/// Конструктор по-умолчанию
		/// </summary>
		public BudgetExpenseStructure_Columns()
		{
		}

		/// <summary>
		/// Идентификатор типа сущности
		/// </summary>
		public override int EntityId
		{
			get { return -1207959402; }
		}

		/// <summary>
		/// Идентификатор типа сущности
		/// </summary>
		public static int EntityIdStatic
		{
			get { return -1207959402; }
		}

		/// <summary>
		/// Русское наименование типа сущности
		/// </summary>
		public override string EntityCaption
		{
			get { return "Перечень выводимых колонок"; }
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
				Platform.BusinessLogic.EntityIds.Add(GetType().DeclaringType,-1207959402);
			}
		}


	}
}