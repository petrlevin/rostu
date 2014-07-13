using System;
using System.Collections.Generic;
using Platform.PrimaryEntities.Common.DbEnums;
using Platform.PrimaryEntities.Common.Interfaces;
using Platform.PrimaryEntities.Factoring;

namespace Platform.PrimaryEntities.Reference
{
	/// <summary>
	/// Класс описывающий фильтры
	/// </summary>
	public class Filter : Metadata, IIdentitied
	{
		#region Значения из БД
		/// <summary>
		/// Идентифкатор
		/// </summary>
		public int Id { get; set; }


        /// <summary>
        /// Признак отключен
        /// </summary>
        public bool Disabled { get; set; }

		/// <summary>
		/// Идентификатор поля, для которого создан фильтр
		/// </summary>
		public int IdEntityField { get; set; }

		/// <summary>
		/// Идентифкатор логического оператора
		/// </summary>
		public byte IdLogicOperator { get; set; }

		/// <summary>
		/// Отрицание выражения.
		/// </summary>
		public bool Not { get; set; }

		/// <summary>
		/// Идентифкатор поля - левый операнд условного выражения
		/// </summary>
		public int? IdLeftEntityField { get; set; }

		/// <summary>
		/// Идентифкатор оператора сравнения
		/// </summary>
		public byte? IdComparisionOperator { get; set; }

		/// <summary>
		/// Значение
		/// </summary>
		public string RightValue { get; set; }

		/// <summary>
		/// Значение как поле
		/// </summary>
		public int? IdRightEntityField { get; set; }

		/// <summary>
		/// Выражение SQL
		/// </summary>
		public string RightSqlExpression { get; set; }

		/// <summary>
		/// Ссылка на родителя
		/// </summary>
		public int? IdParent { get; set; }

        /// <summary>
        /// Описание
        /// </summary>
        public string Description { get; set; }

        public bool WithParents { get; set; }

	    #endregion

		/// <summary>
		/// Значение как поле
		/// </summary>
		public IEntityField RightEntityField
		{
			get {
				return IdRightEntityField.HasValue ? Objects.ById<EntityField>(IdRightEntityField.Value) : null;
			}
		}

		/// <summary>
		/// Дочерние фильтры
		/// </summary>
		public List<Filter> ChildFilter { get; set; }
		
		/// <summary>
		/// Логический оператор
		/// </summary>
		public LogicOperator LogicOperator
		{
			get { return (LogicOperator) IdLogicOperator; }
		}

		/// <summary>
		/// Оператор сравнения
		/// </summary>
		public ComparisionOperator ComparisionOperator
		{
			get
			{
				if (IdComparisionOperator.HasValue) 
					return (ComparisionOperator) IdComparisionOperator;
				else 
					throw new Exception("IdComparisionOperator == null");
			}
		}


        public override int EntityId
        {
			get { return -2080374748; }
        }

	}
}
