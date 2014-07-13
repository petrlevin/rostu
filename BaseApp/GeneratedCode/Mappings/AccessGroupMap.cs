using System.Data.Entity.ModelConfiguration;

namespace BaseApp.GeneratedCode.Mappings
{
    public class AccessGroupMap : EntityTypeConfiguration<AccessGroup>
    {
        public AccessGroupMap()
        {
            // Primary Key
            this.HasKey(t => t.id);

            // Properties
            // Table & Column Mappings
            this.ToTable("AccessGroup", "ref");
			            this.Property(t => t.id).HasColumnName("id");
			            this.Property(t => t.idParent).HasColumnName("idParent");
			            this.Property(t => t.Caption).HasColumnName("Caption");
			
            // Relationships
			            this.HasOptional(t => t._idParent)
                .WithMany()
                .HasForeignKey(d => d.idParent);
			 
        }
    }
}