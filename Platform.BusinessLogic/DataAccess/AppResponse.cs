using System.Collections.Generic;
using Platform.ClientInteraction;

namespace Platform.BusinessLogic.DataAccess
{
    /// <summary>
    /// Данные отсылаемые клиенту
    /// </summary>
    public class AppResponse
    {
        #region Свойства, актуальные только для модели

        /// <summary>
        /// Идентификатор сущности
        /// </summary>
        public int EntityId;

        /// <summary>
        /// Наименование сущности
        /// </summary>
        public string EntityName;

        /// <summary>
        /// Наименование поля Caption
        /// </summary>
        public string CaptionField;

        /// <summary>
        /// Наименование поля Description
        /// </summary>
        public string DescriptionField;

        #endregion

        /// <summary>
        /// Результат выборки
        /// </summary>
        public List<IDictionary<string, object>> Result;

		/// <summary>
		/// Количество элементов в ответе
		/// </summary>
		public int Count { get; set; }

	    /// <summary>
	    /// Действия, которые необходимо совершить на клиенте
	    /// </summary>
		public ClientActionList Actions;

        /// <summary>
        /// признак только чтения
        /// </summary>
        public bool ReadOnly { get; set; }



	}
}