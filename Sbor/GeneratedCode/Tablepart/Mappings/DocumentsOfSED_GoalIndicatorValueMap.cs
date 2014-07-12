using System.Data.Entity.ModelConfiguration;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sbor.Tablepart.Mappings
{
    /// <summary>
    /// Класс описывающий маппинг для сущности DocumentsOfSED_GoalIndicatorValue
    /// </summary>
	public class DocumentsOfSED_GoalIndicatorValueMap : EntityTypeConfiguration<DocumentsOfSED_GoalIndicatorValue>
    {
        /// <summary>
        /// Дефолтный конструктор
        /// </summary>
		public DocumentsOfSED_GoalIndicatorValueMap()
        {
            // Primary Key
            this.HasKey(t => t.Id);

            // Properties
            // Table & Column Mappings
            this.ToTable("DocumentsOfSED_GoalIndicatorValue", "tp");
			this.Property(t => t.IdHierarchyPeriod).HasColumnName("idHierarchyPeriod");
			this.Property(t => t.Value).HasPrecision(15,5).HasColumnName("Value");
			this.Property(t => t.Id).HasColumnName("id");
			this.Property(t => t.IdOwner).HasColumnName("idOwner");
			this.Property(t => t.IdMaster).HasColumnName("idMaster");
			
            // Relationships
			this.HasRequired(t => t.HierarchyPeriod).WithMany().HasForeignKey(d => d.IdHierarchyPeriod);
			this.HasRequired(t => t.Owner).WithMany(t => t.GoalIndicatorValues).HasForeignKey(d => d.IdOwner);
			this.HasRequired(t => t.Master).WithMany(t => t.DocumentsOfSED_GoalIndicatorValue).HasForeignKey(d => d.IdMaster);
			
        }
    }
}
