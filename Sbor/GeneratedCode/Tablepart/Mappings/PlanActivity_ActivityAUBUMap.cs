using System.Data.Entity.ModelConfiguration;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sbor.Tablepart.Mappings
{
    /// <summary>
    /// Класс описывающий маппинг для сущности PlanActivity_ActivityAUBU
    /// </summary>
	public class PlanActivity_ActivityAUBUMap : EntityTypeConfiguration<PlanActivity_ActivityAUBU>
    {
        /// <summary>
        /// Дефолтный конструктор
        /// </summary>
		public PlanActivity_ActivityAUBUMap()
        {
            // Primary Key
            this.HasKey(t => t.Id);

            // Properties
            // Table & Column Mappings
            this.ToTable("PlanActivity_ActivityAUBU", "tp");
			this.Property(t => t.Id).HasColumnName("id");
			this.Property(t => t.IdOwner).HasColumnName("idOwner");
			this.Property(t => t.IdActivity).HasColumnName("idActivity");
			this.Property(t => t.IdContingent).HasColumnName("idContingent");
			this.Property(t => t.IdIndicatorActivity).HasColumnName("idIndicatorActivity");
			
            // Relationships
			this.HasRequired(t => t.Owner).WithMany(t => t.ActivityAUBU).HasForeignKey(d => d.IdOwner);
			this.HasRequired(t => t.Activity).WithMany().HasForeignKey(d => d.IdActivity);
			this.HasOptional(t => t.Contingent).WithMany().HasForeignKey(d => d.IdContingent);
			this.HasRequired(t => t.IndicatorActivity).WithMany().HasForeignKey(d => d.IdIndicatorActivity);
			
        }
    }
}
