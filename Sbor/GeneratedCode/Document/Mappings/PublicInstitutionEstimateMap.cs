using System.Data.Entity.ModelConfiguration;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sbor.Document.Mappings
{
    /// <summary>
    /// Класс описывающий маппинг для сущности PublicInstitutionEstimate
    /// </summary>
	public class PublicInstitutionEstimateMap : EntityTypeConfiguration<PublicInstitutionEstimate>
    {
        /// <summary>
        /// Дефолтный конструктор
        /// </summary>
		public PublicInstitutionEstimateMap()
        {
            // Primary Key
            this.HasKey(t => t.Id);

            // Properties
            // Table & Column Mappings
            this.ToTable("PublicInstitutionEstimate", "doc");
			this.Property(t => t.IdSBP_BlankActual).HasColumnName("idSBP_BlankActual");
			this.Property(t => t.IdSBP_BlankActualAuBu).HasColumnName("idSBP_BlankActualAuBu");
			this.Property(t => t.IsRequireCheck).HasColumnName("isRequireCheck");
			this.Property(t => t.Id).HasColumnName("id");
			this.Property(t => t.Caption).HasColumnName("Caption");
			this.Property(t => t.IdPublicLegalFormation).HasColumnName("idPublicLegalFormation");
			this.Property(t => t.IdBudget).HasColumnName("idBudget");
			this.Property(t => t.IdVersion).HasColumnName("idVersion");
			this.Property(t => t.Number).HasColumnName("Number");
			this.Property(t => t.Date).HasColumnName("Date");
			this.Property(t => t.IdSBP).HasColumnName("idSBP");
			this.Property(t => t.Description).HasColumnName("Description");
			this.Property(t => t.IdParent).HasColumnName("idParent");
			this.Property(t => t.ReasonCancel).HasColumnName("ReasonCancel");
			this.Property(t => t.IsRequireClarification).HasColumnName("isRequireClarification");
			this.Property(t => t.ReasonClarification).HasColumnName("ReasonClarification");
			this.Property(t => t.DateTerminate).HasColumnName("DateTerminate");
			this.Property(t => t.ReasonTerminate).HasColumnName("ReasonTerminate");
			this.Property(t => t.DateCommit).HasColumnName("DateCommit");
			this.Property(t => t.DateLastEdit).HasColumnName("DateLastEdit");
			this.Property(t => t.IsApproved).HasColumnName("isApproved");
			this.Property(t => t.IdDocStatus).HasColumnName("idDocStatus");
			this.Property(t => t.HasAdditionalNeed).HasColumnName("HasAdditionalNeed");
			
            // Relationships
			this.HasOptional(t => t.SBP_BlankActual).WithMany().HasForeignKey(d => d.IdSBP_BlankActual);
			this.HasOptional(t => t.SBP_BlankActualAuBu).WithMany().HasForeignKey(d => d.IdSBP_BlankActualAuBu);
			this.HasRequired(t => t.PublicLegalFormation).WithMany().HasForeignKey(d => d.IdPublicLegalFormation);
			this.HasRequired(t => t.Budget).WithMany().HasForeignKey(d => d.IdBudget);
			this.HasRequired(t => t.Version).WithMany().HasForeignKey(d => d.IdVersion);
			this.HasRequired(t => t.SBP).WithMany().HasForeignKey(d => d.IdSBP);
			this.HasOptional(t => t.Parent).WithMany(t => t.ChildrenByidParent).HasForeignKey(d => d.IdParent);
			this.HasRequired(t => t.DocStatus).WithMany().HasForeignKey(d => d.IdDocStatus);
			
        }
    }
}
