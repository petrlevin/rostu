using System.Data.Entity.ModelConfiguration;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sbor.Reports.Report.Mappings
{
    /// <summary>
    /// Класс описывающий маппинг для сущности InterBudgetaryTransfers
    /// </summary>
	public class InterBudgetaryTransfersMap : EntityTypeConfiguration<InterBudgetaryTransfers>
    {
        /// <summary>
        /// Дефолтный конструктор
        /// </summary>
		public InterBudgetaryTransfersMap()
        {
            // Primary Key
            this.HasKey(t => t.Id);

            // Properties
            // Table & Column Mappings
            this.ToTable("InterBudgetaryTransfers", "rep");
			this.Property(t => t.ReportProfileCaption).HasColumnName("ReportProfileCaption");
			this.Property(t => t.IdReportProfileType).HasColumnName("idReportProfileType");
			this.Property(t => t.IdReportProfileUser).HasColumnName("idReportProfileUser");
			this.Property(t => t.Id).HasColumnName("id");
			this.Property(t => t.IsTemporary).HasColumnName("isTemporary");
			this.Property(t => t.IdPublicLegalFormation).HasColumnName("idPublicLegalFormation");
			this.Property(t => t.IdBudget).HasColumnName("idBudget");
			this.Property(t => t.IdVersion).HasColumnName("idVersion");
			this.Property(t => t.ByApproved).HasColumnName("byApproved");
			this.Property(t => t.DateReport).HasColumnName("DateReport");
			this.Property(t => t.IdUnitDimension).HasColumnName("idUnitDimension");
			this.Property(t => t.IdSourcesDataReports).HasColumnName("idSourcesDataReports");
			this.Property(t => t.RepeatTableHeader).HasColumnName("repeatTableHeader");
			this.Property(t => t.Caption).HasColumnName("Caption");
			this.Property(t => t.IsShowYear).HasColumnName("IsShowYear");
			this.Property(t => t.IsShowYear1).HasColumnName("IsShowYear1");
			this.Property(t => t.IsShowYear2).HasColumnName("IsShowYear2");
			this.Property(t => t.IsShowUnallocatedReserve).HasColumnName("IsShowUnallocatedReserve");
			this.Property(t => t.Header).HasColumnName("Header");
			
            // Relationships
			this.HasOptional(t => t.ReportProfileUser).WithMany().HasForeignKey(d => d.IdReportProfileUser);
			this.HasRequired(t => t.PublicLegalFormation).WithMany().HasForeignKey(d => d.IdPublicLegalFormation);
			this.HasRequired(t => t.Budget).WithMany().HasForeignKey(d => d.IdBudget);
			this.HasRequired(t => t.Version).WithMany().HasForeignKey(d => d.IdVersion);
			this.HasRequired(t => t.UnitDimension).WithMany().HasForeignKey(d => d.IdUnitDimension);
			this.HasMany(t => t.BudgetLevel).WithMany().Map(m => m.MapLeftKey("idInterBudgetaryTransfers").MapRightKey("BudgetLevel").ToTable("InterBudgetaryTransfers_BudgetLevel", "ml"));
			
        }
    }
}
