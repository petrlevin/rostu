using System.Data.Entity.ModelConfiguration;

namespace BaseApp.GeneratedCode.Mappings
{
    public class BudgetMap : EntityTypeConfiguration<Budget>
    {
        public BudgetMap()
        {
            // Primary Key
            this.HasKey(t => t.id);

            // Properties
            // Table & Column Mappings
            this.ToTable("Budget", "ref");
			            this.Property(t => t.id).HasColumnName("id");
			            this.Property(t => t.idPublicLegalFormation).HasColumnName("idPublicLegalFormation");
			            this.Property(t => t.Year).HasColumnName("Year");
			            this.Property(t => t.Caption).HasColumnName("Caption");
			
            // Relationships
			            this.HasRequired(t => t._idPublicLegalFormation)
                .WithMany()
                .HasForeignKey(d => d.idPublicLegalFormation);
			 
        }
    }
}