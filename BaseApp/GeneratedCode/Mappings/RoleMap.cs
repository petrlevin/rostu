using System.Data.Entity.ModelConfiguration;

namespace BaseApp.GeneratedCode.Mappings
{
    public class RoleMap : EntityTypeConfiguration<Role>
    {
        public RoleMap()
        {
            // Primary Key
            this.HasKey(t => t.id);

            // Properties
            // Table & Column Mappings
            this.ToTable("Role", "ref");
			            this.Property(t => t.id).HasColumnName("id");
			            this.Property(t => t.Caption).HasColumnName("Caption");
			            this.Property(t => t.Name).HasColumnName("Name");
			            this.Property(t => t.Description).HasColumnName("Description");
			
            // Relationships
			 
        }
    }
}