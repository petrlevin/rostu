using System.Data.Entity.ModelConfiguration;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sbor.Reports.Report.Mappings
{
    /// <summary>
    /// Класс описывающий маппинг для сущности BudgetExpenseStructure
    /// </summary>
	public class BudgetExpenseStructureMap : EntityTypeConfiguration<BudgetExpenseStructure>
    {
        /// <summary>
        /// Дефолтный конструктор
        /// </summary>
		public BudgetExpenseStructureMap()
        {
            // Primary Key
            this.HasKey(t => t.Id);

            // Properties
            // Table & Column Mappings
            this.ToTable("BudgetExpenseStructure", "rep");
			this.Property(t => t.ReportProfileCaption).HasColumnName("ReportProfileCaption");
			this.Property(t => t.IdReportProfileType).HasColumnName("idReportProfileType");
			this.Property(t => t.IdReportProfileUser).HasColumnName("idReportProfileUser");
			this.Property(t => t.ReportCap).HasColumnName("ReportCap");
			this.Property(t => t.Id).HasColumnName("id");
			this.Property(t => t.IsTemporary).HasColumnName("isTemporary");
			this.Property(t => t.IdPublicLegalFormation).HasColumnName("idPublicLegalFormation");
			this.Property(t => t.IdBudget).HasColumnName("idBudget");
			this.Property(t => t.IdVersion).HasColumnName("idVersion");
			this.Property(t => t.Caption).HasColumnName("Caption");
			this.Property(t => t.IsApprovedOnly).HasColumnName("isApprovedOnly");
			this.Property(t => t.ReportDate).HasColumnName("ReportDate");
			this.Property(t => t.IdUnitDimension).HasColumnName("idUnitDimension");
			this.Property(t => t.IdPeriodOption).HasColumnName("idPeriodOption");
			this.Property(t => t.IdSourcesDataReports).HasColumnName("idSourcesDataReports");
			this.Property(t => t.RepeatTableHeader).HasColumnName("repeatTableHeader");
			this.Property(t => t.ShowGoals).HasColumnName("showGoals");
			this.Property(t => t.ShowProgram).HasColumnName("showProgram");
			this.Property(t => t.ShowActivities).HasColumnName("showActivities");
			this.Property(t => t.IdDocType).HasColumnName("idDocType");
			
            // Relationships
			this.HasOptional(t => t.ReportProfileUser).WithMany().HasForeignKey(d => d.IdReportProfileUser);
			this.HasRequired(t => t.PublicLegalFormation).WithMany().HasForeignKey(d => d.IdPublicLegalFormation);
			this.HasRequired(t => t.Budget).WithMany().HasForeignKey(d => d.IdBudget);
			this.HasRequired(t => t.Version).WithMany().HasForeignKey(d => d.IdVersion);
			this.HasRequired(t => t.UnitDimension).WithMany().HasForeignKey(d => d.IdUnitDimension);
			this.HasOptional(t => t.DocType).WithMany().HasForeignKey(d => d.IdDocType);
			
        }
    }
}
