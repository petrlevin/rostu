using System.Data.Entity.ModelConfiguration;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sbor.Tablepart.Mappings
{
    /// <summary>
    /// Класс описывающий маппинг для сущности DocumentsOfSED_GoalIndicator
    /// </summary>
	public class DocumentsOfSED_GoalIndicatorMap : EntityTypeConfiguration<DocumentsOfSED_GoalIndicator>
    {
        /// <summary>
        /// Дефолтный конструктор
        /// </summary>
		public DocumentsOfSED_GoalIndicatorMap()
        {
            // Primary Key
            this.HasKey(t => t.Id);

            // Properties
            // Table & Column Mappings
            this.ToTable("DocumentsOfSED_GoalIndicator", "tp");
			this.Property(t => t.IdOwner).HasColumnName("idOwner");
			this.Property(t => t.Id).HasColumnName("id");
			this.Property(t => t.IdMaster).HasColumnName("idMaster");
			this.Property(t => t.IdGoalIndicator).HasColumnName("idGoalIndicator");
			
            // Relationships
			this.HasRequired(t => t.Owner).WithMany(t => t.GoalIndicators).HasForeignKey(d => d.IdOwner);
			this.HasRequired(t => t.Master).WithMany(t => t.DocumentsOfSED_GoalIndicator).HasForeignKey(d => d.IdMaster);
			this.HasRequired(t => t.GoalIndicator).WithMany().HasForeignKey(d => d.IdGoalIndicator);
			
        }
    }
}
