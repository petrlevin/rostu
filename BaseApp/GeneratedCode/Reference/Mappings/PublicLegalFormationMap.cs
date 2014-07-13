using System.Data.Entity.ModelConfiguration;
using System.ComponentModel.DataAnnotations.Schema;

namespace BaseApp.Reference.Mappings
{
    /// <summary>
    /// Класс описывающий маппинг для сущности PublicLegalFormation
    /// </summary>
	public class PublicLegalFormationMap : EntityTypeConfiguration<PublicLegalFormation>
    {
        /// <summary>
        /// Дефолтный конструктор
        /// </summary>
		public PublicLegalFormationMap()
        {
            // Primary Key
            this.HasKey(t => t.Id);

            // Properties
            // Table & Column Mappings
            this.ToTable("PublicLegalFormation", "ref");
			this.Property(t => t.Id).HasColumnName("id");
			this.Property(t => t.IdParent).HasColumnName("idParent");
			this.Property(t => t.Caption).HasColumnName("Caption");
			this.Property(t => t.IdBudgetLevel).HasColumnName("idBudgetLevel");
			this.Property(t => t.IdAccessGroup).HasColumnName("idAccessGroup");
			this.Property(t => t.Subject).HasColumnName("Subject");
			this.Property(t => t.IdMethodofFormingCode_GoalSetting).HasColumnName("idMethodofFormingCode_GoalSetting");
			this.Property(t => t.IdMethodofFormingCode_TargetIndicator).HasColumnName("idMethodofFormingCode_TargetIndicator");
			this.Property(t => t.IdMethodofFormingCode_Activity).HasColumnName("idMethodofFormingCode_Activity");
			this.Property(t => t.IdRefStatus).HasColumnName("idRefStatus");
			this.Property(t => t.UsedGMZ).HasColumnName("UsedGMZ");
			
            // Relationships
			this.HasOptional(t => t.Parent).WithMany(t => t.ChildrenByidParent).HasForeignKey(d => d.IdParent);
			this.HasRequired(t => t.BudgetLevel).WithMany().HasForeignKey(d => d.IdBudgetLevel);
			this.HasRequired(t => t.AccessGroup).WithMany().HasForeignKey(d => d.IdAccessGroup);
			
        }
    }
}
