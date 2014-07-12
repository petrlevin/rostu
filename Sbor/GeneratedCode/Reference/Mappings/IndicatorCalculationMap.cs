using System.Data.Entity.ModelConfiguration;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sbor.Reference.Mappings
{
    /// <summary>
    /// Класс описывающий маппинг для сущности IndicatorCalculation
    /// </summary>
	public class IndicatorCalculationMap : EntityTypeConfiguration<IndicatorCalculation>
    {
        /// <summary>
        /// Дефолтный конструктор
        /// </summary>
		public IndicatorCalculationMap()
        {
            // Primary Key
            this.HasKey(t => t.Id);

            // Properties
            // Table & Column Mappings
            this.ToTable("IndicatorCalculation", "ref");
			this.Property(t => t.Id).HasColumnName("id");
			this.Property(t => t.IdPublicLegalFormation).HasColumnName("idPublicLegalFormation");
			this.Property(t => t.Caption).HasColumnName("Caption");
			this.Property(t => t.Symbol).HasColumnName("Symbol");
			this.Property(t => t.IdUnitDimension).HasColumnName("idUnitDimension");
			this.Property(t => t.DefaultValue).HasPrecision(16,3).HasColumnName("DefaultValue");
			this.Property(t => t.IdIndicatorCalculationValueType).HasColumnName("idIndicatorCalculationValueType");
			this.Property(t => t.IdRefStatus).HasColumnName("idRefStatus");
			
            // Relationships
			this.HasRequired(t => t.PublicLegalFormation).WithMany().HasForeignKey(d => d.IdPublicLegalFormation);
			this.HasOptional(t => t.UnitDimension).WithMany().HasForeignKey(d => d.IdUnitDimension);
			
        }
    }
}
