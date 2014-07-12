using System.Data.Entity.ModelConfiguration;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sbor.Tablepart.Mappings
{
    /// <summary>
    /// Класс описывающий маппинг для сущности ActivityOfSBP_ActivityResourceMaintenance_Value
    /// </summary>
	public class ActivityOfSBP_ActivityResourceMaintenance_ValueMap : EntityTypeConfiguration<ActivityOfSBP_ActivityResourceMaintenance_Value>
    {
        /// <summary>
        /// Дефолтный конструктор
        /// </summary>
		public ActivityOfSBP_ActivityResourceMaintenance_ValueMap()
        {
            // Primary Key
            this.HasKey(t => t.Id);

            // Properties
            // Table & Column Mappings
            this.ToTable("ActivityOfSBP_ActivityResourceMaintenance_Value", "tp");
			this.Property(t => t.IdHierarchyPeriod).HasColumnName("idHierarchyPeriod");
			this.Property(t => t.Value).HasPrecision(18,2).HasColumnName("Value");
			this.Property(t => t.IdOwner).HasColumnName("idOwner");
			this.Property(t => t.Id).HasColumnName("id");
			this.Property(t => t.IdMaster).HasColumnName("idMaster");
			this.Property(t => t.AdditionalValue).HasPrecision(18,2).HasColumnName("AdditionalValue");
			
            // Relationships
			this.HasOptional(t => t.HierarchyPeriod).WithMany().HasForeignKey(d => d.IdHierarchyPeriod);
			this.HasRequired(t => t.Owner).WithMany(t => t.ActivityResourceMaintenance_Value).HasForeignKey(d => d.IdOwner);
			this.HasRequired(t => t.Master).WithMany(t => t.ActivityOfSBP_ActivityResourceMaintenance_Value).HasForeignKey(d => d.IdMaster);
			
        }
    }
}
