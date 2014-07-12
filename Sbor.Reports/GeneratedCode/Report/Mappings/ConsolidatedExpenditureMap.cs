using System.Data.Entity.ModelConfiguration;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sbor.Reports.Report.Mappings
{
    /// <summary>
    /// Класс описывающий маппинг для сущности ConsolidatedExpenditure
    /// </summary>
	public class ConsolidatedExpenditureMap : EntityTypeConfiguration<ConsolidatedExpenditure>
    {
        /// <summary>
        /// Дефолтный конструктор
        /// </summary>
		public ConsolidatedExpenditureMap()
        {
            // Primary Key
            this.HasKey(t => t.Id);

            // Properties
            // Table & Column Mappings
            this.ToTable("ConsolidatedExpenditure", "rep");
			this.Property(t => t.Id).HasColumnName("id");
			this.Property(t => t.ReportProfileCaption).HasColumnName("ReportProfileCaption");
			this.Property(t => t.IdReportProfileType).HasColumnName("idReportProfileType");
			this.Property(t => t.IdReportProfileUser).HasColumnName("idReportProfileUser");
			this.Property(t => t.IsTemporary).HasColumnName("isTemporary");
			this.Property(t => t.IdPublicLegalFormation).HasColumnName("idPublicLegalFormation");
			this.Property(t => t.IdHierarchyPeriod).HasColumnName("idHierarchyPeriod");
			this.Property(t => t.IdUnitDimension).HasColumnName("idUnitDimension");
			this.Property(t => t.IdSourcesDataReports).HasColumnName("idSourcesDataReports");
			this.Property(t => t.IsApprovedOnly).HasColumnName("isApprovedOnly");
			this.Property(t => t.ReportDate).HasColumnName("ReportDate");
			this.Property(t => t.Caption).HasColumnName("Caption");
			this.Property(t => t.RepeatTableHeader).HasColumnName("repeatTableHeader");
			this.Property(t => t.IdCurrentPPO).HasColumnName("idCurrentPPO");
			
            // Relationships
			this.HasOptional(t => t.ReportProfileUser).WithMany().HasForeignKey(d => d.IdReportProfileUser);
			this.HasRequired(t => t.PublicLegalFormation).WithMany().HasForeignKey(d => d.IdPublicLegalFormation);
			this.HasRequired(t => t.HierarchyPeriod).WithMany().HasForeignKey(d => d.IdHierarchyPeriod);
			this.HasRequired(t => t.UnitDimension).WithMany().HasForeignKey(d => d.IdUnitDimension);
			this.HasRequired(t => t.CurrentPPO).WithMany().HasForeignKey(d => d.IdCurrentPPO);
			
        }
    }
}
