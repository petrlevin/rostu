using System.Data.Entity.ModelConfiguration;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sbor.Tablepart.Mappings
{
    /// <summary>
    /// Класс описывающий маппинг для сущности SystemGoal_GoalIndicator
    /// </summary>
	public class SystemGoal_GoalIndicatorMap : EntityTypeConfiguration<SystemGoal_GoalIndicator>
    {
        /// <summary>
        /// Дефолтный конструктор
        /// </summary>
		public SystemGoal_GoalIndicatorMap()
        {
            // Primary Key
            this.HasKey(t => t.Id);

            // Properties
            // Table & Column Mappings
            this.ToTable("SystemGoal_GoalIndicator", "tp");
			this.Property(t => t.IdOwner).HasColumnName("idOwner");
			this.Property(t => t.Id).HasColumnName("id");
			this.Property(t => t.IdGoalIndicator).HasColumnName("idGoalIndicator");
			this.Property(t => t.IdVersion).HasColumnName("idVersion");
			
            // Relationships
			this.HasRequired(t => t.Owner).WithMany(t => t.GoalIndicator).HasForeignKey(d => d.IdOwner);
			this.HasRequired(t => t.GoalIndicator).WithMany().HasForeignKey(d => d.IdGoalIndicator);
			this.HasRequired(t => t.Version).WithMany().HasForeignKey(d => d.IdVersion);
			
        }
    }
}
