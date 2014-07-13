using System.Data.Entity.ModelConfiguration;
using Platform.PrimaryEntities.Reference;

namespace Platform.BusinessLogic.GeneratedCode.Reference.Mappings
{
	public class IndexMap : EntityTypeConfiguration<Index>
	{
		public IndexMap()
		{
			// Primary Key
			this.HasKey(t => t.Id);

			this.ToTable("Index", "ref");
			this.Property(t => t.Id).HasColumnName("id");
			this.Property(t => t.IdEntity).HasColumnName("idEntity");
			this.Property(t => t.Name).HasColumnName("Name");
			this.Property(t => t.IdIndexType).HasColumnName("idIndexType");
			this.Property(t => t.IsClustered).HasColumnName("isClustered");
		}
	}
}
