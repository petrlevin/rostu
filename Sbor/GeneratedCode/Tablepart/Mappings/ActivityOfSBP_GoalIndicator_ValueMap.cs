using System.Data.Entity.ModelConfiguration;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sbor.Tablepart.Mappings
{
    /// <summary>
    /// Класс описывающий маппинг для сущности ActivityOfSBP_GoalIndicator_Value
    /// </summary>
	public class ActivityOfSBP_GoalIndicator_ValueMap : EntityTypeConfiguration<ActivityOfSBP_GoalIndicator_Value>
    {
        /// <summary>
        /// Дефолтный конструктор
        /// </summary>
		public ActivityOfSBP_GoalIndicator_ValueMap()
        {
            // Primary Key
            this.HasKey(t => t.Id);

            // Properties
            // Table & Column Mappings
            this.ToTable("ActivityOfSBP_GoalIndicator_Value", "tp");
			this.Property(t => t.IdHierarchyPeriod).HasColumnName("idHierarchyPeriod");
			this.Property(t => t.Value).HasPrecision(20,5).HasColumnName("Value");
			this.Property(t => t.IdOwner).HasColumnName("idOwner");
			this.Property(t => t.Id).HasColumnName("id");
			this.Property(t => t.IdMaster).HasColumnName("idMaster");
			
            // Relationships
			this.HasRequired(t => t.HierarchyPeriod).WithMany().HasForeignKey(d => d.IdHierarchyPeriod);
			this.HasRequired(t => t.Owner).WithMany(t => t.GoalIndicator_Value).HasForeignKey(d => d.IdOwner);
			this.HasRequired(t => t.Master).WithMany(t => t.ActivityOfSBP_GoalIndicator_Value).HasForeignKey(d => d.IdMaster);
			
        }
    }
}
