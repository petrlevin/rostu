using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Platform.BusinessLogic.Common.Interfaces;

namespace Platform.BusinessLogic.FormsLogic
{
	/// <summary>
	/// Элемент формы. Контейнер или поле.
	/// </summary>
    public class FormItem : IEntityFieldInfo
	{
		/// <summary>
		/// Имя поля сущности.
		/// Если не указано, то компонент не имеет связи с полем сущности. Компонент может являться контейнером.
		/// </summary>
		public string EntityFieldName { get; set; }

		/// <summary>
		/// Метка поля
		/// </summary>
		public string Label { get; set; }
		
		/// <summary>
		/// Свойства данного компонента.
		/// Могут перекрывать свойства по умолчанию.
		/// </summary>
		public string Properties { get; set; }

		/// <summary>
		/// Дочерние компоненты контейнера.
		/// </summary>
		public List<FormItem> Items { get; set; }

		#region Свойства преконфигурированного элемента управления. Следует вынести в отдельный класс

		/// <summary>
		/// Свойства по умолчанию для данного типа компонента
		/// </summary>
		public string DefaultProperties { get; set; }

		#endregion

		#region Свойства элемента управления. Следует вынести в отельный класс.

		/// <summary>
		/// Имя класса компонента. 
		/// Если указано, то компонент будет создан инструкцией Ext.create(ControlName, {...})
		/// </summary>
		public string ControlName { get; set; }

		/// <summary>
		/// Псевдоним компонента. 
		/// Если не указано имя компонента, то компонент будет описан конфиг-объектом. 
		/// Если при этом указан псевдоним, то он будет указан в качестве значения свойства xtype концигурационного объекта компонента.
		/// </summary>
		public string ControlAlias { get; set; }

		/// <summary>
		/// Имя свойства, в которое следует подставить значение из Label
		/// </summary>
		public string LabelProperty { get; set; }

		#endregion

	}
}
