using System.Data.Entity.ModelConfiguration;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sbor.Tablepart.Mappings
{
    /// <summary>
    /// Класс описывающий маппинг для сущности SystemGoal_GoalIndicatorValue
    /// </summary>
	public class SystemGoal_GoalIndicatorValueMap : EntityTypeConfiguration<SystemGoal_GoalIndicatorValue>
    {
        /// <summary>
        /// Дефолтный конструктор
        /// </summary>
		public SystemGoal_GoalIndicatorValueMap()
        {
            // Primary Key
            this.HasKey(t => t.Id);

            // Properties
            // Table & Column Mappings
            this.ToTable("SystemGoal_GoalIndicatorValue", "tp");
			this.Property(t => t.IdHierarchyPeriod).HasColumnName("idHierarchyPeriod");
			this.Property(t => t.Value).HasPrecision(15,5).HasColumnName("Value");
			this.Property(t => t.Id).HasColumnName("id");
			this.Property(t => t.IdOwner).HasColumnName("idOwner");
			this.Property(t => t.IdMaster).HasColumnName("idMaster");
			
            // Relationships
			this.HasRequired(t => t.HierarchyPeriod).WithMany().HasForeignKey(d => d.IdHierarchyPeriod);
			this.HasRequired(t => t.Owner).WithMany(t => t.GoalIndicatorValue).HasForeignKey(d => d.IdOwner);
			this.HasRequired(t => t.Master).WithMany(t => t.SystemGoal_GoalIndicatorValue).HasForeignKey(d => d.IdMaster);
			
        }
    }
}
