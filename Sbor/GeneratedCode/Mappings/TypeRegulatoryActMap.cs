using System.Data.Entity.ModelConfiguration;

namespace Sbor.GeneratedCode.Mappings
{
    public class TypeRegulatoryActMap : EntityTypeConfiguration<TypeRegulatoryAct>
    {
        public TypeRegulatoryActMap()
        {
            // Primary Key
            this.HasKey(t => t.id);

            // Properties
            // Table & Column Mappings
            this.ToTable("TypeRegulatoryAct", "ref");
			            this.Property(t => t.id).HasColumnName("id");
			            this.Property(t => t.idAccessGroup).HasColumnName("idAccessGroup");
			            this.Property(t => t.Caption).HasColumnName("Caption");
			
            // Relationships
			            this.HasOptional(t => t._idAccessGroup)
                .WithMany()
                .HasForeignKey(d => d.idAccessGroup);
			 
        }
    }
}