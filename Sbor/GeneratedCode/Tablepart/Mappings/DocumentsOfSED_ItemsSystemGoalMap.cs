using System.Data.Entity.ModelConfiguration;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sbor.Tablepart.Mappings
{
    /// <summary>
    /// Класс описывающий маппинг для сущности DocumentsOfSED_ItemsSystemGoal
    /// </summary>
	public class DocumentsOfSED_ItemsSystemGoalMap : EntityTypeConfiguration<DocumentsOfSED_ItemsSystemGoal>
    {
        /// <summary>
        /// Дефолтный конструктор
        /// </summary>
		public DocumentsOfSED_ItemsSystemGoalMap()
        {
            // Primary Key
            this.HasKey(t => t.Id);

            // Properties
            // Table & Column Mappings
            this.ToTable("DocumentsOfSED_ItemsSystemGoal", "tp");
			this.Property(t => t.IdOwner).HasColumnName("idOwner");
			this.Property(t => t.Id).HasColumnName("id");
			this.Property(t => t.IdSystemGoal).HasColumnName("idSystemGoal");
			this.Property(t => t.Code).HasColumnName("Code");
			this.Property(t => t.IdElementTypeSystemGoal).HasColumnName("idElementTypeSystemGoal");
			this.Property(t => t.DateStart).HasColumnName("DateStart");
			this.Property(t => t.DateEnd).HasColumnName("DateEnd");
			this.Property(t => t.IdSBP).HasColumnName("idSBP");
			this.Property(t => t.IsOtherDocSG).HasColumnName("isOtherDocSG");
			this.Property(t => t.IdParent).HasColumnName("idParent");
			
            // Relationships
			this.HasRequired(t => t.Owner).WithMany(t => t.ItemsSystemGoals).HasForeignKey(d => d.IdOwner);
			this.HasRequired(t => t.SystemGoal).WithMany().HasForeignKey(d => d.IdSystemGoal);
			this.HasOptional(t => t.ElementTypeSystemGoal).WithMany().HasForeignKey(d => d.IdElementTypeSystemGoal);
			this.HasOptional(t => t.SBP).WithMany().HasForeignKey(d => d.IdSBP);
			this.HasOptional(t => t.Parent).WithMany(t => t.ChildrenByidParent).HasForeignKey(d => d.IdParent);
			
        }
    }
}
