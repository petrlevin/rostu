using System.Collections.Generic;
using Platform.BusinessLogic.AppServices;

namespace Platform.BusinessLogic.Denormalizer
{
	/// <summary>
	/// Сервис для методов денормализатора
	/// </summary>
	[AppService]
	public class DenormalizerService
	{
        /// <summary>
        /// Поля денормализованной ТЧ
        /// </summary>
		public class DenormalizedFields
		{
			/// <summary>
			/// Поля-периоды
			/// </summary>
			public IEnumerable<int> Periods { get; set; }

            /// <summary>
            /// Поля-значения
            /// </summary>
            public IEnumerable<IDictionary<string, object>> Fields { get; set; }
		}

		/// <summary>
		/// Получить модель значимых колонок
		/// </summary>
		/// <param name="entityId">id сущности-владельца</param>
		/// <param name="itemId">id элемента сущности-владельца</param>
        /// <param name="tpEntityId">Идентификатор сущности денормализованной ТЧ</param>
		/// <returns></returns>
        public DenormalizedFields GetFields(int entityId, int itemId, int tpEntityId)
		{
            var provider = new PeriodsProvider(entityId, itemId, tpEntityId);
			return new DenormalizedFields
				{
					Periods = provider.PeriodIds,
					Fields = provider.ColumnsModel
				};
		}
	}
}