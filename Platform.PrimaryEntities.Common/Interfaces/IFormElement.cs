using Platform.PrimaryEntities.Common.DbEnums;

namespace Platform.PrimaryEntities.Common.Interfaces
{
	public interface IFormElement 
	{
		 int Id { get; set; }

		 int? IdParent { get; set; }

		 FormElementType ElementType { get; }

		 EntityFieldType EntityFieldType { get; }

		 string Name { get; set; }
	}
}
