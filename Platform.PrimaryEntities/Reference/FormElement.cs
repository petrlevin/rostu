using System;
using System.Collections.Generic;
using Platform.PrimaryEntities.Common.DbEnums;
using Platform.PrimaryEntities.Common.Interfaces;
using Platform.Utils.Common;

namespace Platform.PrimaryEntities.Reference
{
	/// <summary>
	/// Элементы формы
	/// </summary>
	public class FormElement : Metadata, IFormElement
	{
		public FormElement(): base()
		{
		}

		#region Implementation of IFormElement

		public int Id { get; set; }
		public int? IdParent { get; set; }
		public FormElementType ElementType { get; private set; }
		public EntityFieldType EntityFieldType { get; private set; }
		public string Name { get; set; }

		#endregion

		public int IdOwner { get; set; }
		public int? IdEntityField { get; set; }
		public string Caption { get; set; }
		public int? IdInterfaceControl { get; set; }
		public int? IdEntityFieldType { get; set; }
		public int? IdCalculatedFieldType { get; set; }
		public string Formula { get; set; }
		public string Properties { get; set; }
		public int Order { get; set; }

		public virtual EntityField EntityField { get; set; }
		public virtual InterfaceControl Control { get; set; }

        public virtual FormElement Parent { get; set; }

        private ICollection<FormElement> _children;
        [JsonIgnoreForException]
        public virtual ICollection<FormElement> ChildrenByidParent
        {
            get { return _children ?? (_children = new List<FormElement>()); }
            set { _children = value; }
        }

		/// <summary>
		/// Форма, которой принадлежит поле
		/// </summary>
		public Form Form
		{
			get { return Form.ById(IdOwner); }
		}

		public static FormElement ById(int id)
		{
			throw new NotImplementedException();
		}
	}
}
