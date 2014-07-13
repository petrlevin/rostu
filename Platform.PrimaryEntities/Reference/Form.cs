using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Platform.PrimaryEntities.Common.DbEnums;
using Platform.PrimaryEntities.Common.Interfaces;

namespace Platform.PrimaryEntities.Reference
{
	/// <summary>
	/// Класс описывающий форму
	/// </summary>
	public class Form : BaseEntity, IForm, IIdentitied
	{
		#region Поля БД
		/// <summary>
		/// Идентификатор элемента
		/// </summary>
		public int Id { get; set; }

		/// <summary>
		/// Системное наименование
		/// </summary>
		public string Name { get; set; }

		/// <summary>
		/// Наименование
		/// </summary>
		public string Caption { get; set; }

		/// <summary>
		/// Описание
		/// </summary>
		public string Description;

		/// <summary>
		/// Ссылка на сущность
		/// </summary>
        public int? IdEntity { get; set; }

        public int? IdHierarchyViewField { get; set; }

		#endregion

		/// <summary>
		/// Конструктор
		/// </summary>
		public Form()
		{
			//IsDefault = true;
		}

		#region Implementation of IForm

		/// <summary>
		/// Сущность, на которой основана форма
		/// </summary>
		public IEntity Entity { get; set; }

        ///// <summary>
        ///// Признак, того что форма стоится на основе полей сущности
        ///// </summary>
        //public bool IsDefault { get; set; }

		/// <summary>
		/// Поля формы
		/// </summary>
        public IEnumerable<IFormElement> FormElements { get; set; }

		public string Schema
		{
			get { return "gen"; }
		}

	    public static Form ById(int id)
		{
			throw new NotImplementedException();
		}


		#endregion


	}
}
