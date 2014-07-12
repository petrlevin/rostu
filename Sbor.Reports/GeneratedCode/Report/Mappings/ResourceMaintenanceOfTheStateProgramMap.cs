using System.Data.Entity.ModelConfiguration;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sbor.Reports.Report.Mappings
{
    /// <summary>
    /// Класс описывающий маппинг для сущности ResourceMaintenanceOfTheStateProgram
    /// </summary>
	public class ResourceMaintenanceOfTheStateProgramMap : EntityTypeConfiguration<ResourceMaintenanceOfTheStateProgram>
    {
        /// <summary>
        /// Дефолтный конструктор
        /// </summary>
		public ResourceMaintenanceOfTheStateProgramMap()
        {
            // Primary Key
            this.HasKey(t => t.Id);

            // Properties
            // Table & Column Mappings
            this.ToTable("ResourceMaintenanceOfTheStateProgram", "rep");
			this.Property(t => t.Id).HasColumnName("id");
			this.Property(t => t.IsTemporary).HasColumnName("isTemporary");
			this.Property(t => t.IdPublicLegalFormation).HasColumnName("idPublicLegalFormation");
			this.Property(t => t.IdBudget).HasColumnName("idBudget");
			this.Property(t => t.IdVersion).HasColumnName("idVersion");
			this.Property(t => t.Caption).HasColumnName("Caption");
			this.Property(t => t.IdProgram).HasColumnName("idProgram");
			this.Property(t => t.ByApproved).HasColumnName("byApproved");
			this.Property(t => t.IsRatingAdditionalNeeds).HasColumnName("isRatingAdditionalNeeds");
			this.Property(t => t.DateReport).HasColumnName("DateReport");
			this.Property(t => t.RepeatTableHeader).HasColumnName("repeatTableHeader");
			this.Property(t => t.ShowActivities).HasColumnName("showActivities");
			this.Property(t => t.BySource).HasColumnName("bySource");
			this.Property(t => t.IdSourcesDataReports).HasColumnName("idSourcesDataReports");
			this.Property(t => t.HasNoFunds).HasColumnName("HasNoFunds");
			this.Property(t => t.IdReportProfileType).HasColumnName("idReportProfileType");
			this.Property(t => t.IdReportProfileUser).HasColumnName("idReportProfileUser");
			this.Property(t => t.ReportProfileCaption).HasColumnName("ReportProfileCaption");
			
            // Relationships
			this.HasRequired(t => t.PublicLegalFormation).WithMany().HasForeignKey(d => d.IdPublicLegalFormation);
			this.HasRequired(t => t.Budget).WithMany().HasForeignKey(d => d.IdBudget);
			this.HasRequired(t => t.Version).WithMany().HasForeignKey(d => d.IdVersion);
			this.HasRequired(t => t.Program).WithMany().HasForeignKey(d => d.IdProgram);
			this.HasOptional(t => t.ReportProfileUser).WithMany().HasForeignKey(d => d.IdReportProfileUser);
			this.HasMany(t => t.FinanceSource).WithMany().Map(m => m.MapLeftKey("idResourceMaintenanceOfTheStateProgram").MapRightKey("idFinanceSource").ToTable("ResourceMaintenanceOfTheStateProgram_FinanceSource", "ml"));
			
        }
    }
}
