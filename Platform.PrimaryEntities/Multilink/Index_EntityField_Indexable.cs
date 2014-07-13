using Platform.PrimaryEntities.Reference;

namespace Platform.PrimaryEntities.Multilink
{
	/// <summary>
	/// Класс описывающий мультилинк справочника Index хранящий индексируемые поля
	/// </summary>
	public class Index_EntityField_Indexable : Metadata
	{
		/// <summary>
		/// Идентификатор индекса
		/// </summary>
		public int IdIndex { get; set; }

		/// <summary>
		/// Идентификатор поля сущности
		/// </summary>
		public int IdEntityField { get; set; }

		/// <summary>
		/// Поле упорядочивания по полю сущности
		/// </summary>
		public int IdIndexOrder { get; set; }

		/// <summary>
		/// Поле упорядочивания по индексу
		/// </summary>
		public int IdEntityFieldOrder { get; set; }

		/// <summary>
		/// Индекс
		/// </summary>
		private Index _index;

		/// <summary>
		/// Индекс
		/// </summary>
		public Index Index
		{
			get { return _index ?? (_index = Objects.ById<Index>(IdIndex)); }
		}

		/// <summary>
		/// Поле сущности
		/// </summary>
		private EntityField _entityField;

		/// <summary>
		/// Поле сущности
		/// </summary>
		public EntityField EntityField
		{
			get { return _entityField ?? (_entityField = Objects.ById<EntityField>(IdEntityField)); }
		}
	}
}
