using System.Data.Entity.ModelConfiguration;

namespace Platform.BusinessLogic.Reference.Mappings
{
    public class ControlsMap : EntityTypeConfiguration<Controls>
    {
        public ControlsMap()
        {
            // Primary Key
            this.HasKey(t =>  t.Id);

            // Properties
            // Table & Column Mappings
            this.ToTable("Controls", "ref");
			            this.Property(t => t.Id).HasColumnName("id");
			            this.Property(t => t.Enabled).HasColumnName("Enabled");
			            this.Property(t => t.Skippable).HasColumnName("Skippable");
			
            // Relationships
			        }
    }
}
