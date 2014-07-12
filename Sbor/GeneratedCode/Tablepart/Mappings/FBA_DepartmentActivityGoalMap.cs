using System.Data.Entity.ModelConfiguration;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sbor.Tablepart.Mappings
{
    /// <summary>
    /// Класс описывающий маппинг для сущности FBA_DepartmentActivityGoal
    /// </summary>
	public class FBA_DepartmentActivityGoalMap : EntityTypeConfiguration<FBA_DepartmentActivityGoal>
    {
        /// <summary>
        /// Дефолтный конструктор
        /// </summary>
		public FBA_DepartmentActivityGoalMap()
        {
            // Primary Key
            this.HasKey(t => t.Id);

            // Properties
            // Table & Column Mappings
            this.ToTable("FBA_DepartmentActivityGoal", "tp");
			this.Property(t => t.Id).HasColumnName("id");
			this.Property(t => t.IdOwner).HasColumnName("idOwner");
			this.Property(t => t.DepartmentGoal).HasColumnName("DepartmentGoal");
			
            // Relationships
			this.HasRequired(t => t.Owner).WithMany(t => t.DepartmentActivityGoals).HasForeignKey(d => d.IdOwner);
			
        }
    }
}
