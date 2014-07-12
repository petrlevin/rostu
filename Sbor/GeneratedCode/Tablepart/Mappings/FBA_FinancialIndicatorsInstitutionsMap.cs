using System.Data.Entity.ModelConfiguration;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sbor.Tablepart.Mappings
{
    /// <summary>
    /// Класс описывающий маппинг для сущности FBA_FinancialIndicatorsInstitutions
    /// </summary>
	public class FBA_FinancialIndicatorsInstitutionsMap : EntityTypeConfiguration<FBA_FinancialIndicatorsInstitutions>
    {
        /// <summary>
        /// Дефолтный конструктор
        /// </summary>
		public FBA_FinancialIndicatorsInstitutionsMap()
        {
            // Primary Key
            this.HasKey(t => t.Id);

            // Properties
            // Table & Column Mappings
            this.ToTable("FBA_FinancialIndicatorsInstitutions", "tp");
			this.Property(t => t.Id).HasColumnName("id");
			this.Property(t => t.IdOwner).HasColumnName("idOwner");
			this.Property(t => t.IdFinancialIndicator).HasColumnName("idFinancialIndicator");
			this.Property(t => t.IdFinancialIndicatorCaption).HasColumnName("idFinancialIndicatorCaption");
			this.Property(t => t.Value).HasPrecision(20,2).HasColumnName("Value");
			
            // Relationships
			this.HasRequired(t => t.Owner).WithMany(t => t.FinancialIndicatorsInstitutionss).HasForeignKey(d => d.IdOwner);
			this.HasRequired(t => t.FinancialIndicator).WithMany().HasForeignKey(d => d.IdFinancialIndicator);
			
        }
    }
}
