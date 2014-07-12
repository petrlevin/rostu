using System.Data.Entity.ModelConfiguration;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sbor.Tablepart.Mappings
{
    /// <summary>
    /// Класс описывающий маппинг для сущности ActivityOfSBP_Activity
    /// </summary>
	public class ActivityOfSBP_ActivityMap : EntityTypeConfiguration<ActivityOfSBP_Activity>
    {
        /// <summary>
        /// Дефолтный конструктор
        /// </summary>
		public ActivityOfSBP_ActivityMap()
        {
            // Primary Key
            this.HasKey(t => t.Id);

            // Properties
            // Table & Column Mappings
            this.ToTable("ActivityOfSBP_Activity", "tp");
			this.Property(t => t.IdOwner).HasColumnName("idOwner");
			this.Property(t => t.Id).HasColumnName("id");
			this.Property(t => t.IdMaster).HasColumnName("idMaster");
			this.Property(t => t.IdSBP).HasColumnName("idSBP");
			this.Property(t => t.IdContingent).HasColumnName("idContingent");
			this.Property(t => t.IdIndicatorActivity_Volume).HasColumnName("idIndicatorActivity_Volume");
			this.Property(t => t.IdActivity).HasColumnName("idActivity");
			
            // Relationships
			this.HasRequired(t => t.Owner).WithMany(t => t.Activity).HasForeignKey(d => d.IdOwner);
			this.HasRequired(t => t.Master).WithMany(t => t.ActivityOfSBP_Activity).HasForeignKey(d => d.IdMaster);
			this.HasRequired(t => t.SBP).WithMany().HasForeignKey(d => d.IdSBP);
			this.HasOptional(t => t.Contingent).WithMany().HasForeignKey(d => d.IdContingent);
			this.HasRequired(t => t.IndicatorActivity_Volume).WithMany().HasForeignKey(d => d.IdIndicatorActivity_Volume);
			this.HasRequired(t => t.Activity).WithMany().HasForeignKey(d => d.IdActivity);
			
        }
    }
}
