using System.Data.Entity.ModelConfiguration;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sbor.Tablepart.Mappings
{
    /// <summary>
    /// Класс описывающий маппинг для сущности Activity_Indicator
    /// </summary>
	public class Activity_IndicatorMap : EntityTypeConfiguration<Activity_Indicator>
    {
        /// <summary>
        /// Дефолтный конструктор
        /// </summary>
		public Activity_IndicatorMap()
        {
            // Primary Key
            this.HasKey(t => t.Id);

            // Properties
            // Table & Column Mappings
            this.ToTable("Activity_Indicator", "tp");
			this.Property(t => t.IdOwner).HasColumnName("idOwner");
			this.Property(t => t.Id).HasColumnName("id");
			this.Property(t => t.IdIndicatorActivity).HasColumnName("idIndicatorActivity");
			this.Property(t => t.IdSBP).HasColumnName("idSBP");
			
            // Relationships
			this.HasRequired(t => t.Owner).WithMany(t => t.Indicator).HasForeignKey(d => d.IdOwner);
			this.HasRequired(t => t.IndicatorActivity).WithMany().HasForeignKey(d => d.IdIndicatorActivity);
			this.HasRequired(t => t.SBP).WithMany().HasForeignKey(d => d.IdSBP);
			
        }
    }
}
