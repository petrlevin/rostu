using System.Data.Entity.ModelConfiguration;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sbor.Tablepart.Mappings
{
    /// <summary>
    /// Класс описывающий маппинг для сущности ActivityOfSBP_SystemGoalElement
    /// </summary>
	public class ActivityOfSBP_SystemGoalElementMap : EntityTypeConfiguration<ActivityOfSBP_SystemGoalElement>
    {
        /// <summary>
        /// Дефолтный конструктор
        /// </summary>
		public ActivityOfSBP_SystemGoalElementMap()
        {
            // Primary Key
            this.HasKey(t => t.Id);

            // Properties
            // Table & Column Mappings
            this.ToTable("ActivityOfSBP_SystemGoalElement", "tp");
			this.Property(t => t.IdOwner).HasColumnName("idOwner");
			this.Property(t => t.Id).HasColumnName("id");
			this.Property(t => t.FromAnotherDocumentSE).HasColumnName("FromAnotherDocumentSE");
			this.Property(t => t.IdSystemGoal).HasColumnName("idSystemGoal");
			this.Property(t => t.IsMainGoal).HasColumnName("IsMainGoal");
			this.Property(t => t.Code).HasColumnName("Code");
			this.Property(t => t.IdElementTypeSystemGoal).HasColumnName("idElementTypeSystemGoal");
			this.Property(t => t.DateStart).HasColumnName("DateStart");
			this.Property(t => t.DateEnd).HasColumnName("DateEnd");
			this.Property(t => t.IdSBP).HasColumnName("idSBP");
			this.Property(t => t.IdParent).HasColumnName("idParent");
			
            // Relationships
			this.HasRequired(t => t.Owner).WithMany(t => t.SystemGoalElement).HasForeignKey(d => d.IdOwner);
			this.HasRequired(t => t.SystemGoal).WithMany().HasForeignKey(d => d.IdSystemGoal);
			this.HasOptional(t => t.ElementTypeSystemGoal).WithMany().HasForeignKey(d => d.IdElementTypeSystemGoal);
			this.HasOptional(t => t.SBP).WithMany().HasForeignKey(d => d.IdSBP);
			this.HasOptional(t => t.Parent).WithMany(t => t.ChildrenByidParent).HasForeignKey(d => d.IdParent);
			
        }
    }
}
