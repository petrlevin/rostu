using System.Data.Entity.ModelConfiguration;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sbor.Tablepart.Mappings
{
    /// <summary>
    /// Класс описывающий маппинг для сущности ActivityOfSBP_ActivityDemandAndCapacity
    /// </summary>
	public class ActivityOfSBP_ActivityDemandAndCapacityMap : EntityTypeConfiguration<ActivityOfSBP_ActivityDemandAndCapacity>
    {
        /// <summary>
        /// Дефолтный конструктор
        /// </summary>
		public ActivityOfSBP_ActivityDemandAndCapacityMap()
        {
            // Primary Key
            this.HasKey(t => t.Id);

            // Properties
            // Table & Column Mappings
            this.ToTable("ActivityOfSBP_ActivityDemandAndCapacity", "tp");
			this.Property(t => t.Id).HasColumnName("id");
			this.Property(t => t.IdOwner).HasColumnName("idOwner");
			this.Property(t => t.IdActivity).HasColumnName("idActivity");
			
            // Relationships
			this.HasRequired(t => t.Owner).WithMany(t => t.ActivityDemandAndCapacity).HasForeignKey(d => d.IdOwner);
			this.HasRequired(t => t.Activity).WithMany(t => t.ActivityOfSBP_ActivityDemandAndCapacity).HasForeignKey(d => d.IdActivity);
			
        }
    }
}
