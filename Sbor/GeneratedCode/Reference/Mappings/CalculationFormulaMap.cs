using System.Data.Entity.ModelConfiguration;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sbor.Reference.Mappings
{
    /// <summary>
    /// Класс описывающий маппинг для сущности CalculationFormula
    /// </summary>
	public class CalculationFormulaMap : EntityTypeConfiguration<CalculationFormula>
    {
        /// <summary>
        /// Дефолтный конструктор
        /// </summary>
		public CalculationFormulaMap()
        {
            // Primary Key
            this.HasKey(t => t.Id);

            // Properties
            // Table & Column Mappings
            this.ToTable("CalculationFormula", "ref");
			this.Property(t => t.Id).HasColumnName("id");
			this.Property(t => t.IdPublicLegalFormation).HasColumnName("idPublicLegalFormation");
			this.Property(t => t.Caption).HasColumnName("Caption");
			this.Property(t => t.Description).HasColumnName("Description");
			this.Property(t => t.IdRefStatus).HasColumnName("idRefStatus");
			
            // Relationships
			this.HasRequired(t => t.PublicLegalFormation).WithMany().HasForeignKey(d => d.IdPublicLegalFormation);
			this.HasMany(t => t.IdIndicatorOfCalculationFormula).WithMany().Map(m => m.MapLeftKey("idCalculationFormula").MapRightKey("idIndicatorCalculation").ToTable("IndicatorOfCalculationFormula", "ml"));
			
        }
    }
}
