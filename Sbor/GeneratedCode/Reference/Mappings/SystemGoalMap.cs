using System.Data.Entity.ModelConfiguration;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sbor.Reference.Mappings
{
    /// <summary>
    /// Класс описывающий маппинг для сущности SystemGoal
    /// </summary>
	public class SystemGoalMap : EntityTypeConfiguration<SystemGoal>
    {
        /// <summary>
        /// Дефолтный конструктор
        /// </summary>
		public SystemGoalMap()
        {
            // Primary Key
            this.HasKey(t => t.Id);

            // Properties
            // Table & Column Mappings
            this.ToTable("SystemGoal", "ref");
			this.Property(t => t.Id).HasColumnName("id");
			this.Property(t => t.IdPublicLegalFormation).HasColumnName("idPublicLegalFormation");
			this.Property(t => t.IdParent).HasColumnName("idParent");
			this.Property(t => t.Code).HasColumnName("Code");
			this.Property(t => t.Caption).HasColumnName("Caption");
			this.Property(t => t.DateStart).HasColumnName("DateStart");
			this.Property(t => t.DateEnd).HasColumnName("DateEnd");
			this.Property(t => t.IdElementTypeSystemGoal).HasColumnName("idElementTypeSystemGoal");
			this.Property(t => t.IdSBP).HasColumnName("idSBP");
			this.Property(t => t.IdDocType_CommitDoc).HasColumnName("idDocType_CommitDoc");
			this.Property(t => t.IdRefStatus).HasColumnName("idRefStatus");
			this.Property(t => t.DateCommit).HasColumnName("DateCommit");
			this.Property(t => t.IsChanged).HasColumnName("isChanged");
			this.Property(t => t.IdCommitDoc).HasColumnName("idCommitDoc");
			this.Property(t => t.IdCommitDocEntity).HasColumnName("idCommitDocEntity");
			this.Property(t => t.IdImplementDoc).HasColumnName("idImplementDoc");
			this.Property(t => t.IdImplementDocEntity).HasColumnName("idImplementDocEntity");
			this.Property(t => t.IdDocType_ImplementDoc).HasColumnName("idDocType_ImplementDoc");
			
            // Relationships
			this.HasRequired(t => t.PublicLegalFormation).WithMany().HasForeignKey(d => d.IdPublicLegalFormation);
			this.HasOptional(t => t.Parent).WithMany(t => t.ChildrenByidParent).HasForeignKey(d => d.IdParent);
			this.HasRequired(t => t.ElementTypeSystemGoal).WithMany().HasForeignKey(d => d.IdElementTypeSystemGoal);
			this.HasOptional(t => t.SBP).WithMany().HasForeignKey(d => d.IdSBP);
			this.HasRequired(t => t.DocType_CommitDoc).WithMany().HasForeignKey(d => d.IdDocType_CommitDoc);
			this.HasOptional(t => t.CommitDocEntity).WithMany().HasForeignKey(d => d.IdCommitDocEntity);
			this.HasOptional(t => t.ImplementDocEntity).WithMany().HasForeignKey(d => d.IdImplementDocEntity);
			this.HasOptional(t => t.DocType_ImplementDoc).WithMany().HasForeignKey(d => d.IdDocType_ImplementDoc);
			
        }
    }
}
