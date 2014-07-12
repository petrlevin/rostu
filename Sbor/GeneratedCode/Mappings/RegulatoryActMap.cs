using System.Data.Entity.ModelConfiguration;

namespace Sbor.GeneratedCode.Mappings
{
    public class RegulatoryActMap : EntityTypeConfiguration<RegulatoryAct>
    {
        public RegulatoryActMap()
        {
            // Primary Key
            this.HasKey(t => t.id);

            // Properties
            // Table & Column Mappings
            this.ToTable("RegulatoryAct", "ref");
			            this.Property(t => t.id).HasColumnName("id");
			            this.Property(t => t.idPublicLegalFormation).HasColumnName("idPublicLegalFormation");
			            this.Property(t => t.idBudgetLevel).HasColumnName("idBudgetLevel");
			            this.Property(t => t.idTypeRegulatoryAct).HasColumnName("idTypeRegulatoryAct");
			            this.Property(t => t.Number).HasColumnName("Number");
			            this.Property(t => t.Date).HasColumnName("Date");
			            this.Property(t => t.DateStart).HasColumnName("DateStart");
			            this.Property(t => t.DateEnd).HasColumnName("DateEnd");
			            this.Property(t => t.AuthorityRegulatoryAct).HasColumnName("AuthorityRegulatoryAct");
			            this.Property(t => t.Caption).HasColumnName("Caption");
			
            // Relationships
			            this.HasRequired(t => t._idPublicLegalFormation)
                .WithMany()
                .HasForeignKey(d => d.idPublicLegalFormation);
			            this.HasRequired(t => t._idBudgetLevel)
                .WithMany()
                .HasForeignKey(d => d.idBudgetLevel);
			            this.HasRequired(t => t._idTypeRegulatoryAct)
                .WithMany()
                .HasForeignKey(d => d.idTypeRegulatoryAct);
			 
        }
    }
}