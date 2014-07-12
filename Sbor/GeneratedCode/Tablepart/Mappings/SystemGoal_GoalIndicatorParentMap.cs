using System.Data.Entity.ModelConfiguration;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sbor.Tablepart.Mappings
{
    /// <summary>
    /// Класс описывающий маппинг для сущности SystemGoal_GoalIndicatorParent
    /// </summary>
	public class SystemGoal_GoalIndicatorParentMap : EntityTypeConfiguration<SystemGoal_GoalIndicatorParent>
    {
        /// <summary>
        /// Дефолтный конструктор
        /// </summary>
		public SystemGoal_GoalIndicatorParentMap()
        {
            // Primary Key
            this.HasKey(t => t.Id);

            // Properties
            // Table & Column Mappings
            this.ToTable("SystemGoal_GoalIndicatorParent", "tp");
			this.Property(t => t.Id).HasColumnName("id");
			this.Property(t => t.IdOwner).HasColumnName("idOwner");
			this.Property(t => t.IdGoalIndicator).HasColumnName("idGoalIndicator");
			
            // Relationships
			this.HasRequired(t => t.Owner).WithMany(t => t.GoalIndicatorParent).HasForeignKey(d => d.IdOwner);
			this.HasRequired(t => t.GoalIndicator).WithMany().HasForeignKey(d => d.IdGoalIndicator);
			
        }
    }
}
