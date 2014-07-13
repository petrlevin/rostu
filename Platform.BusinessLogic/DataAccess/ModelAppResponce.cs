using System.Collections.Generic;
using Platform.BusinessLogic.AppServices;
using Platform.Client;

namespace Platform.BusinessLogic.DataAccess
{
    /// <summary>
    /// Объект-информация о модели сущности
    /// </summary>
    public class ModelAppResponse : AppResponse
    {
        /// <summary>
        /// Шаблоны импорта
        /// </summary>
		public Dictionary<int, string> ImportTemplates;

        /// <summary>
        /// Описание табличных частей
        /// </summary>
        public List<object> TableParts{ get; set;}

		/// <summary>
		/// Поля, по которым имеется иерархия
		/// </summary>
		public List<string> HierarchyFields { get; set; }

		/// <summary>
		/// Сущность является табличной частью, подлежащей денормализации
		/// </summary>
		public bool IsDenormalizedTablepart { get; set; }

		/// <summary>
		/// Сущность содержит денормализованную ТЧ (одну или более).
		/// </summary>
		public bool HasDenormalizedTableparts { get; set; }

        /// <summary>
        /// Печатные формы (имена классов)
        /// </summary>
        public IEnumerable<ExtMenuItem> PrintForms { get; set; }

        /// <summary>
        /// Список пунктов меню "Действия"
        /// </summary>
        public new IEnumerable<ListFormActionsInfo.MenuItem> Actions { get; set; }
    }
}
