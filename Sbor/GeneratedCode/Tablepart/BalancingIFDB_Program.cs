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

namespace Sbor.Tablepart
{
	// Это автоматически сгенерированный класс, изменения будут уничтожены при следующей генерации
	/// <summary>
	/// Программы/мероприятия
	/// </summary>
	public partial class BalancingIFDB_Program : TablePartEntity    , IHierarhy  
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
		/// Вышестоящая
		/// </summary>
		public int? IdParent{get; set;}
        /// <summary>
	    /// Вышестоящая
	    /// </summary>
		public virtual Sbor.Tablepart.BalancingIFDB_Program Parent{get; set;}
		private ICollection<Sbor.Tablepart.BalancingIFDB_Program> _idParent; 
        /// <summary>
        /// Вышестоящая
        /// </summary>
        [JsonIgnoreForException]
		public virtual ICollection<Sbor.Tablepart.BalancingIFDB_Program> ChildrenByidParent 
		{
			get{ return _idParent ?? (_idParent = new List<Sbor.Tablepart.BalancingIFDB_Program>()); } 
			set{ _idParent = value; }
		}

		/// <summary>
		/// `'Актуальная базовая ' + ({BudgetYear})`
		/// </summary>
		public decimal? OFG{get; set;}

		/// <summary>
		/// `'Актуальная доп. потребностей ' + ({BudgetYear})`
		/// </summary>
		public decimal? AdditionalOFG{get; set;}

		/// <summary>
		/// `'Измененная базовая ' + ({BudgetYear})`
		/// </summary>
		public decimal? ChangeOFG{get; set;}

		/// <summary>
		/// `'Измененная доп. потребностей ' + ({BudgetYear})`
		/// </summary>
		public decimal? ChangeAdditionalOFG{get; set;}

		/// <summary>
		/// `'Актуальная базовая ' + ({BudgetYear} + 1)`
		/// </summary>
		public decimal? PFG1{get; set;}

		/// <summary>
		/// `'Актуальная доп. потребностей ' + ({BudgetYear} + 1)`
		/// </summary>
		public decimal? AdditionalPFG1{get; set;}

		/// <summary>
		/// `'Измененная базовая ' + ({BudgetYear} + 1)`
		/// </summary>
		public decimal? ChangePFG1{get; set;}

		/// <summary>
		/// `'Измененная доп. потребностей ' + ({BudgetYear} + 1)`
		/// </summary>
		public decimal? ChangeAdditionalPFG1{get; set;}

		/// <summary>
		/// `'Актуальная базовая ' + ({BudgetYear} + 2)`
		/// </summary>
		public decimal? PFG2{get; set;}

		/// <summary>
		/// `'Актуальная доп. потребностей ' + ({BudgetYear} + 2)`
		/// </summary>
		public decimal? AdditionalPFG2{get; set;}

		/// <summary>
		/// `'Измененная базовая ' + ({BudgetYear} + 2)`
		/// </summary>
		public decimal? ChangePFG2{get; set;}

		/// <summary>
		/// `'Измененная доп. потребностей ' + ({BudgetYear} + 2)`
		/// </summary>
		public decimal? ChangeAdditionalPFG2{get; set;}

		/// <summary>
		/// Наименование
		/// </summary>
		public int IdProgramOrActivity{get; set;}

		/// <summary>
		/// Ссылка на сущность
		/// </summary>
		public int IdProgramOrActivityEntity{get; set;}
        /// <summary>
	    /// Ссылка на сущность
	    /// </summary>
		public virtual Entity ProgramOrActivityEntity{get; set;}
		

		/// <summary>
		/// Тип
		/// </summary>
		public int? IdType{get; set;}

		/// <summary>
		/// Ссылка на сущность
		/// </summary>
		public int? IdTypeEntity{get; set;}
        /// <summary>
	    /// Ссылка на сущность
	    /// </summary>
		public virtual Entity TypeEntity{get; set;}
		

		/// <summary>
		/// `'Разница базовая ' + ({BudgetYear})`
		/// </summary>
		public decimal? DifferenceOFG{get; set;}

		/// <summary>
		/// `'Разница доп. потребностей ' + ({BudgetYear})`
		/// </summary>
		public decimal? DifferenceAdditionalOFG{get; set;}

		/// <summary>
		/// `'Разница базовая ' + ({BudgetYear}+1)`
		/// </summary>
		public decimal? DifferencePFG1{get; set;}

		/// <summary>
		/// `'Разница доп. потребностей ' + ({BudgetYear}+1)`
		/// </summary>
		public decimal? DifferenceAdditionalPFG1{get; set;}

		/// <summary>
		/// `'Разница базовая ' + ({BudgetYear}+2)`
		/// </summary>
		public decimal? DifferencePFG2{get; set;}

		/// <summary>
		/// `'Разница доп. потребностей ' + ({BudgetYear}+2)`
		/// </summary>
		public decimal? DifferenceAdditionalPFG2{get; set;}

			private ICollection<Sbor.Tablepart.BalancingIFDB_Expense> _BalancingIFDB_Expense; 
        /// <summary>
        /// Наименование
        /// </summary>
        [JsonIgnoreForException]
		public virtual ICollection<Sbor.Tablepart.BalancingIFDB_Expense> BalancingIFDB_Expense 
		{
			get{ return _BalancingIFDB_Expense ?? (_BalancingIFDB_Expense = new List<Sbor.Tablepart.BalancingIFDB_Expense>()); } 
			set{ _BalancingIFDB_Expense = value; }
		}


		/// <summary>
		/// Конструктор по-умолчанию
		/// </summary>
		public BalancingIFDB_Program()
		{
		}

		/// <summary>
		/// Идентификатор типа сущности
		/// </summary>
		public override int EntityId
		{
			get { return -2013265411; }
		}

		/// <summary>
		/// Идентификатор типа сущности
		/// </summary>
		public static int EntityIdStatic
		{
			get { return -2013265411; }
		}

		/// <summary>
		/// Русское наименование типа сущности
		/// </summary>
		public override string EntityCaption
		{
			get { return "Программы/мероприятия"; }
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
				Platform.BusinessLogic.EntityIds.Add(GetType().DeclaringType,-2013265411);
			}
		}


	}
}