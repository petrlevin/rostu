using System;
using System.Collections.Generic;

namespace Platform.BusinessLogic.DataAccess
{
	/// <summary>
	/// Класс опиисывающий возвращение результата для клиентского грида
	/// </summary>
	public class GridResult
	{
		/// <summary>
		/// Количество строк до пэйджинга
		/// </summary>
		public int Count { get; set; }

        /// <summary>
        /// Количество корневых элементов в иерархическом гриде
        /// </summary>
        public int RootCount { get; set; }
		

		/// <summary>
		/// Возвращаемый набор строк
		/// </summary>
		public List<IDictionary<string, object>> Rows {get; set; }


        public Dictionary<String, Object> Aggregates
        {
            get; set;
        }


	}
}
