using System.Data.Entity.ModelConfiguration;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sbor.Tablepart.Mappings
{
    /// <summary>
    /// Класс описывающий маппинг для сущности ActivityOfSBP_IndicatorQualityActivity_Value
    /// </summary>
	public class ActivityOfSBP_IndicatorQualityActivity_ValueMap : EntityTypeConfiguration<ActivityOfSBP_IndicatorQualityActivity_Value>
    {
        /// <summary>
        /// Дефолтный конструктор
        /// </summary>
		public ActivityOfSBP_IndicatorQualityActivity_ValueMap()
        {
            // Primary Key
            this.HasKey(t => t.Id);

            // Properties
            // Table & Column Mappings
            this.ToTable("ActivityOfSBP_IndicatorQualityActivity_Value", "tp");
			this.Property(t => t.IdHierarchyPeriod).HasColumnName("idHierarchyPeriod");
			this.Property(t => t.Value).HasPrecision(20,5).HasColumnName("Value");
			this.Property(t => t.IdOwner).HasColumnName("idOwner");
			this.Property(t => t.Id).HasColumnName("id");
			this.Property(t => t.IdMaster).HasColumnName("idMaster");
			this.Property(t => t.AdditionalValue).HasPrecision(20,5).HasColumnName("AdditionalValue");
			
            // Relationships
			this.HasRequired(t => t.HierarchyPeriod).WithMany().HasForeignKey(d => d.IdHierarchyPeriod);
			this.HasRequired(t => t.Owner).WithMany(t => t.IndicatorQualityActivity_Value).HasForeignKey(d => d.IdOwner);
			this.HasRequired(t => t.Master).WithMany(t => t.ActivityOfSBP_IndicatorQualityActivity_Value).HasForeignKey(d => d.IdMaster);
			
        }
    }
}
