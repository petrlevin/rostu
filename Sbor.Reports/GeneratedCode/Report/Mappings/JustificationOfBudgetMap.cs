using System.Data.Entity.ModelConfiguration;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sbor.Reports.Report.Mappings
{
    /// <summary>
    /// Класс описывающий маппинг для сущности JustificationOfBudget
    /// </summary>
	public class JustificationOfBudgetMap : EntityTypeConfiguration<JustificationOfBudget>
    {
        /// <summary>
        /// Дефолтный конструктор
        /// </summary>
		public JustificationOfBudgetMap()
        {
            // Primary Key
            this.HasKey(t => t.Id);

            // Properties
            // Table & Column Mappings
            this.ToTable("JustificationOfBudget", "rep");
			this.Property(t => t.ReportProfileCaption).HasColumnName("ReportProfileCaption");
			this.Property(t => t.IdReportProfileType).HasColumnName("idReportProfileType");
			this.Property(t => t.IdReportProfileUser).HasColumnName("idReportProfileUser");
			this.Property(t => t.Id).HasColumnName("id");
			this.Property(t => t.IsTemporary).HasColumnName("isTemporary");
			this.Property(t => t.IdPublicLegalFormation).HasColumnName("idPublicLegalFormation");
			this.Property(t => t.IdBudget).HasColumnName("idBudget");
			this.Property(t => t.DateReport).HasColumnName("DateReport");
			this.Property(t => t.ByApproved).HasColumnName("byApproved");
			this.Property(t => t.IdVersion).HasColumnName("idVersion");
			this.Property(t => t.RepeatTableHeader).HasColumnName("repeatTableHeader");
			this.Property(t => t.HasAdditionalNeed).HasColumnName("HasAdditionalNeed");
			this.Property(t => t.IdSBP).HasColumnName("idSBP");
			
            // Relationships
			this.HasOptional(t => t.ReportProfileUser).WithMany().HasForeignKey(d => d.IdReportProfileUser);
			this.HasRequired(t => t.PublicLegalFormation).WithMany().HasForeignKey(d => d.IdPublicLegalFormation);
			this.HasRequired(t => t.Budget).WithMany().HasForeignKey(d => d.IdBudget);
			this.HasRequired(t => t.Version).WithMany().HasForeignKey(d => d.IdVersion);
			this.HasRequired(t => t.SBP).WithMany().HasForeignKey(d => d.IdSBP);
			this.HasMany(t => t.ListRemovedField).WithMany().Map(m => m.MapLeftKey("idJustificationOfBudget").MapRightKey("idListRemovedFields").ToTable("JustificationOfBudget_ListRemovedField", "ml"));
			this.HasMany(t => t.RZPR).WithMany().Map(m => m.MapLeftKey("idJustificationOfBudget").MapRightKey("idRZPR").ToTable("JustificationOfBudget_RZPR", "ml"));
			this.HasMany(t => t.KCSR).WithMany().Map(m => m.MapLeftKey("idJustificationOfBudget").MapRightKey("idKCSR").ToTable("JustificationOfBudget_KCSR", "ml"));
			this.HasMany(t => t.KVR).WithMany().Map(m => m.MapLeftKey("idJustificationOfBudget").MapRightKey("idKVR").ToTable("JustificationOfBudget_KVR", "ml"));
			this.HasMany(t => t.KOSGU).WithMany().Map(m => m.MapLeftKey("idJustificationOfBudget").MapRightKey("idKOSGU").ToTable("JustificationOfBudget_KOSGU", "ml"));
			this.HasMany(t => t.AuthorityOfExpenseObligation).WithMany().Map(m => m.MapLeftKey("idJustificationOfBudget").MapRightKey("idAuthorityOfExpenseObligation").ToTable("JustificationOfBudget_AuthorityOfExpenseObligation", "ml"));
			
        }
    }
}
