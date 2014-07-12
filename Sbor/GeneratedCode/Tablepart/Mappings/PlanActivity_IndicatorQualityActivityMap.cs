using System.Data.Entity.ModelConfiguration;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sbor.Tablepart.Mappings
{
    /// <summary>
    /// Класс описывающий маппинг для сущности PlanActivity_IndicatorQualityActivity
    /// </summary>
	public class PlanActivity_IndicatorQualityActivityMap : EntityTypeConfiguration<PlanActivity_IndicatorQualityActivity>
    {
        /// <summary>
        /// Дефолтный конструктор
        /// </summary>
		public PlanActivity_IndicatorQualityActivityMap()
        {
            // Primary Key
            this.HasKey(t => t.Id);

            // Properties
            // Table & Column Mappings
            this.ToTable("PlanActivity_IndicatorQualityActivity", "tp");
			this.Property(t => t.Id).HasColumnName("id");
			this.Property(t => t.IdOwner).HasColumnName("idOwner");
			this.Property(t => t.IdMaster).HasColumnName("idMaster");
			this.Property(t => t.IdIndicatorActivity).HasColumnName("idIndicatorActivity");
			
            // Relationships
			this.HasRequired(t => t.Owner).WithMany(t => t.IndicatorQualityActivity).HasForeignKey(d => d.IdOwner);
			this.HasOptional(t => t.Master).WithMany(t => t.PlanActivity_IndicatorQualityActivity).HasForeignKey(d => d.IdMaster);
			this.HasRequired(t => t.IndicatorActivity).WithMany().HasForeignKey(d => d.IdIndicatorActivity);
			
        }
    }
}
