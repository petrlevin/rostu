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
	/// Сметные строки
	/// </summary>
	public partial class BalancingIFDB_EstimatedLine : TablePartEntity      
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
		public virtual Sbor.Tool.BalancingIFDB Owner{get; set;}
		

		/// <summary>
		/// Расход
		/// </summary>
		public int? IdMaster{get; set;}
        /// <summary>
	    /// Расход
	    /// </summary>
		public virtual Sbor.Tablepart.BalancingIFDB_Expense Master{get; set;}
		

		/// <summary>
		/// Сметная строка
		/// </summary>
		public int IdEstimatedLine{get; set;}
        /// <summary>
	    /// Сметная строка
	    /// </summary>
		public virtual Sbor.Registry.EstimatedLine EstimatedLine{get; set;}
		

		/// <summary>
		/// Старое значение
		/// </summary>
		public decimal? OldValue{get; set;}

		/// <summary>
		/// Новое значение
		/// </summary>
		public decimal? NewValue{get; set;}

		/// <summary>
		/// Набор задач
		/// </summary>
		public int IdTaskCollection{get; set;}
        /// <summary>
	    /// Набор задач
	    /// </summary>
		public virtual Sbor.Registry.TaskCollection TaskCollection{get; set;}
		

		/// <summary>
		/// Период
		/// </summary>
		public int IdHierarchyPeriod{get; set;}
        /// <summary>
	    /// Период
	    /// </summary>
		public virtual Platform.BusinessLogic.Reference.HierarchyPeriod HierarchyPeriod{get; set;}
		

		/// <summary>
		/// Доп. потребность 
		/// </summary>
		public bool IsAdditionalNeed{get; set;}

		/// <summary>
		/// Территория
		/// </summary>
		public int? IdOKATO{get; set;}
        /// <summary>
	    /// Территория
	    /// </summary>
		public virtual BaseApp.Reference.OKATO OKATO{get; set;}
		

		/// <summary>
		/// Полномочия расходных обязательств
		/// </summary>
		public int? IdAuthorityOfExpenseObligation{get; set;}
        /// <summary>
	    /// Полномочия расходных обязательств
	    /// </summary>
		public virtual Sbor.Reference.AuthorityOfExpenseObligation AuthorityOfExpenseObligation{get; set;}
		

			private ICollection<Sbor.Tablepart.BalancingIFDB_ChangeHistory> _BalancingIFDB_ChangeHistory; 
        /// <summary>
        /// Сметная строка
        /// </summary>
        [JsonIgnoreForException]
		public virtual ICollection<Sbor.Tablepart.BalancingIFDB_ChangeHistory> BalancingIFDB_ChangeHistory 
		{
			get{ return _BalancingIFDB_ChangeHistory ?? (_BalancingIFDB_ChangeHistory = new List<Sbor.Tablepart.BalancingIFDB_ChangeHistory>()); } 
			set{ _BalancingIFDB_ChangeHistory = value; }
		}


		/// <summary>
		/// Конструктор по-умолчанию
		/// </summary>
		public BalancingIFDB_EstimatedLine()
		{
		}

		/// <summary>
		/// Идентификатор типа сущности
		/// </summary>
		public override int EntityId
		{
			get { return -2013265299; }
		}

		/// <summary>
		/// Идентификатор типа сущности
		/// </summary>
		public static int EntityIdStatic
		{
			get { return -2013265299; }
		}

		/// <summary>
		/// Русское наименование типа сущности
		/// </summary>
		public override string EntityCaption
		{
			get { return "Сметные строки"; }
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
				Platform.BusinessLogic.EntityIds.Add(GetType().DeclaringType,-2013265299);
			}
		}


	}
}