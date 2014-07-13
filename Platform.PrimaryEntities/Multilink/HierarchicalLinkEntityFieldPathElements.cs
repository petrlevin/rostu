namespace Platform.PrimaryEntities.Multilink
{
	/// <summary>
	/// Класс описывающий мультилинк для сущности описанной классом HierarchicalLink
	/// </summary>
	public class HierarchicalLinkEntityFieldPathElements
	{
		/// <summary>
		/// Ссылка на элемент HierarchicalLink
		/// </summary>
		public int IdHierarchicalLink { get; set; }

		/// <summary>
		/// Ссылка на элемент EntityField
		/// </summary>
		public int IdEntityField { get; set; }

		/// <summary>
		/// Порядок элементов EntityField в пределах одного элемента HierarchicalLink
		/// </summary>
		public int OrdEntityField { get; set; }
	}
}
