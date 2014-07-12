using System.Data.Entity.ModelConfiguration;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sbor.Reference.Mappings
{
    /// <summary>
    /// Класс описывающий маппинг для сущности ModelSystemGoal
    /// </summary>
	public class ModelSystemGoalMap : EntityTypeConfiguration<ModelSystemGoal>
    {
        /// <summary>
        /// Дефолтный конструктор
        /// </summary>
		public ModelSystemGoalMap()
        {
            // Primary Key
            this.HasKey(t => t.Id);

            // Properties
            // Table & Column Mappings
            this.ToTable("ModelSystemGoal", "ref");
			this.Property(t => t.Id).HasColumnName("id");
			this.Property(t => t.IdPublicLegalFormation).HasColumnName("idPublicLegalFormation");
			this.Property(t => t.IdParent).HasColumnName("idParent");
			this.Property(t => t.IdElementTypeSystemGoal).HasColumnName("idElementTypeSystemGoal");
			this.Property(t => t.IdRefStatus).HasColumnName("idRefStatus");
			
            // Relationships
			this.HasRequired(t => t.PublicLegalFormation).WithMany().HasForeignKey(d => d.IdPublicLegalFormation);
			this.HasOptional(t => t.Parent).WithMany(t => t.ChildrenByidParent).HasForeignKey(d => d.IdParent);
			this.HasRequired(t => t.ElementTypeSystemGoal).WithMany().HasForeignKey(d => d.IdElementTypeSystemGoal);
			
        }
    }
}
