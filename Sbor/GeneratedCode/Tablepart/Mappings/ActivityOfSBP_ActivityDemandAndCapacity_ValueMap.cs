using System.Data.Entity.ModelConfiguration;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sbor.Tablepart.Mappings
{
    /// <summary>
    /// Класс описывающий маппинг для сущности ActivityOfSBP_ActivityDemandAndCapacity_Value
    /// </summary>
	public class ActivityOfSBP_ActivityDemandAndCapacity_ValueMap : EntityTypeConfiguration<ActivityOfSBP_ActivityDemandAndCapacity_Value>
    {
        /// <summary>
        /// Дефолтный конструктор
        /// </summary>
		public ActivityOfSBP_ActivityDemandAndCapacity_ValueMap()
        {
            // Primary Key
            this.HasKey(t => t.Id);

            // Properties
            // Table & Column Mappings
            this.ToTable("ActivityOfSBP_ActivityDemandAndCapacity_Value", "tp");
			this.Property(t => t.Id).HasColumnName("id");
			this.Property(t => t.IdOwner).HasColumnName("idOwner");
			this.Property(t => t.IdMaster).HasColumnName("idMaster");
			this.Property(t => t.IdHierarchyPeriod).HasColumnName("idHierarchyPeriod");
			this.Property(t => t.Demand).HasPrecision(20,5).HasColumnName("Demand");
			this.Property(t => t.Capacity).HasPrecision(20,5).HasColumnName("Capacity");
			
            // Relationships
			this.HasRequired(t => t.Owner).WithMany(t => t.ActivityDemandAndCapacity_Value).HasForeignKey(d => d.IdOwner);
			this.HasRequired(t => t.Master).WithMany(t => t.ActivityOfSBP_ActivityDemandAndCapacity_Value).HasForeignKey(d => d.IdMaster);
			this.HasRequired(t => t.HierarchyPeriod).WithMany().HasForeignKey(d => d.IdHierarchyPeriod);
			
        }
    }
}
