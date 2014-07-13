using System.Data.Entity.ModelConfiguration;

namespace BaseApp.GeneratedCode.Mappings
{
    public class PublicLegalFormationModuleMap : EntityTypeConfiguration<PublicLegalFormationModule>
    {
        public PublicLegalFormationModuleMap()
        {
            // Primary Key
            this.HasKey(t => t.id);

            // Properties
            // Table & Column Mappings
            this.ToTable("PublicLegalFormationModule", "ref");
			            this.Property(t => t.id).HasColumnName("id");
			            this.Property(t => t.idPublicLegalFormation).HasColumnName("idPublicLegalFormation");
			
            // Relationships
			            this.HasRequired(t => t._idPublicLegalFormation)
                .WithMany()
                .HasForeignKey(d => d.idPublicLegalFormation);
			 
        }
    }
}