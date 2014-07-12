using System.Data.Entity.ModelConfiguration;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sbor.Tablepart.Mappings
{
    /// <summary>
    /// Класс описывающий маппинг для сущности PlanActivity_IndicatorQualityActivityValue
    /// </summary>
	public class PlanActivity_IndicatorQualityActivityValueMap : EntityTypeConfiguration<PlanActivity_IndicatorQualityActivityValue>
    {
        /// <summary>
        /// Дефолтный конструктор
        /// </summary>
		public PlanActivity_IndicatorQualityActivityValueMap()
        {
            // Primary Key
            this.HasKey(t => t.Id);

            // Properties
            // Table & Column Mappings
            this.ToTable("PlanActivity_IndicatorQualityActivityValue", "tp");
			this.Property(t => t.Id).HasColumnName("id");
			this.Property(t => t.IdOwner).HasColumnName("idOwner");
			this.Property(t => t.IdMaster).HasColumnName("idMaster");
			this.Property(t => t.IdHierarchyPeriod).HasColumnName("idHierarchyPeriod");
			this.Property(t => t.Value).HasPrecision(20,5).HasColumnName("Value");
			this.Property(t => t.AdditionalValue).HasPrecision(20,5).HasColumnName("AdditionalValue");
			
            // Relationships
			this.HasRequired(t => t.Owner).WithMany(t => t.IndicatorQualityActivityValues).HasForeignKey(d => d.IdOwner);
			this.HasRequired(t => t.Master).WithMany(t => t.PlanActivity_IndicatorQualityActivityValue).HasForeignKey(d => d.IdMaster);
			this.HasRequired(t => t.HierarchyPeriod).WithMany().HasForeignKey(d => d.IdHierarchyPeriod);
			
        }
    }
}
