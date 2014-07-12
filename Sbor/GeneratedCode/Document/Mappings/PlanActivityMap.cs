using System.Data.Entity.ModelConfiguration;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sbor.Document.Mappings
{
    /// <summary>
    /// Класс описывающий маппинг для сущности PlanActivity
    /// </summary>
	public class PlanActivityMap : EntityTypeConfiguration<PlanActivity>
    {
        /// <summary>
        /// Дефолтный конструктор
        /// </summary>
		public PlanActivityMap()
        {
            // Primary Key
            this.HasKey(t => t.Id);

            // Properties
            // Table & Column Mappings
            this.ToTable("PlanActivity", "doc");
			this.Property(t => t.Id).HasColumnName("id");
			this.Property(t => t.IdPublicLegalFormation).HasColumnName("idPublicLegalFormation");
			this.Property(t => t.IdBudget).HasColumnName("idBudget");
			this.Property(t => t.IdVersion).HasColumnName("idVersion");
			this.Property(t => t.Number).HasColumnName("Number");
			this.Property(t => t.Date).HasColumnName("Date");
			this.Property(t => t.IsApproved).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Computed).HasColumnName("isApproved");
			this.Property(t => t.Description).HasColumnName("Description");
			this.Property(t => t.IdParent).HasColumnName("idParent");
			this.Property(t => t.IsRequireClarification).HasColumnName("isRequireClarification");
			this.Property(t => t.ReasonClarification).HasColumnName("ReasonClarification");
			this.Property(t => t.IdDocStatus).HasColumnName("idDocStatus");
			this.Property(t => t.Caption).HasColumnName("Caption");
			this.Property(t => t.DateCommit).HasColumnName("DateCommit");
			this.Property(t => t.IdSBP).HasColumnName("idSBP");
			this.Property(t => t.DateTerminate).HasColumnName("DateTerminate");
			this.Property(t => t.ReasonTerminate).HasColumnName("ReasonTerminate");
			this.Property(t => t.ReasonCancel).HasColumnName("ReasonCancel");
			this.Property(t => t.DateLastEdit).HasColumnName("DateLastEdit");
			this.Property(t => t.IdDocAUBUPeriodType_OFG).HasColumnName("idDocAUBUPeriodType_OFG");
			this.Property(t => t.IdDocAUBUPeriodType_PFG1).HasColumnName("idDocAUBUPeriodType_PFG1");
			this.Property(t => t.IdDocAUBUPeriodType_PFG2).HasColumnName("idDocAUBUPeriodType_PFG2");
			this.Property(t => t.IsAdditionalNeed).HasColumnName("isAdditionalNeed");
			this.Property(t => t.IdSBP_BlankActual).HasColumnName("idSBP_BlankActual");
			this.Property(t => t.IsRequireCheck).HasColumnName("isRequireCheck");
			
            // Relationships
			this.HasRequired(t => t.PublicLegalFormation).WithMany().HasForeignKey(d => d.IdPublicLegalFormation);
			this.HasRequired(t => t.Budget).WithMany().HasForeignKey(d => d.IdBudget);
			this.HasRequired(t => t.Version).WithMany().HasForeignKey(d => d.IdVersion);
			this.HasOptional(t => t.Parent).WithMany(t => t.ChildrenByidParent).HasForeignKey(d => d.IdParent);
			this.HasRequired(t => t.DocStatus).WithMany().HasForeignKey(d => d.IdDocStatus);
			this.HasRequired(t => t.SBP).WithMany().HasForeignKey(d => d.IdSBP);
			this.HasOptional(t => t.SBP_BlankActual).WithMany().HasForeignKey(d => d.IdSBP_BlankActual);
			
        }
    }
}
