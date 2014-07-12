using System.Data.Entity.ModelConfiguration;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sbor.Reports.Report.Mappings
{
    /// <summary>
    /// Класс описывающий маппинг для сущности AnalysisBAofDirectAndIndirectCost
    /// </summary>
	public class AnalysisBAofDirectAndIndirectCostMap : EntityTypeConfiguration<AnalysisBAofDirectAndIndirectCost>
    {
        /// <summary>
        /// Дефолтный конструктор
        /// </summary>
		public AnalysisBAofDirectAndIndirectCostMap()
        {
            // Primary Key
            this.HasKey(t => t.Id);

            // Properties
            // Table & Column Mappings
            this.ToTable("AnalysisBAofDirectAndIndirectCost", "rep");
			this.Property(t => t.Id).HasColumnName("id");
			this.Property(t => t.ReportProfileCaption).HasColumnName("ReportProfileCaption");
			this.Property(t => t.IdReportProfileType).HasColumnName("idReportProfileType");
			this.Property(t => t.IdReportProfileUser).HasColumnName("idReportProfileUser");
			this.Property(t => t.IsTemporary).HasColumnName("isTemporary");
			this.Property(t => t.IdBudget).HasColumnName("idBudget");
			this.Property(t => t.IdVersion).HasColumnName("idVersion");
			this.Property(t => t.ByApproved).HasColumnName("byApproved");
			this.Property(t => t.DateReport).HasColumnName("DateReport");
			this.Property(t => t.RepeatTableHeader).HasColumnName("repeatTableHeader");
			this.Property(t => t.IdUnitDimension).HasColumnName("idUnitDimension");
			this.Property(t => t.ByActivity).HasColumnName("byActivity");
			this.Property(t => t.BySBP).HasColumnName("bySBP");
			
            // Relationships
			this.HasOptional(t => t.ReportProfileUser).WithMany().HasForeignKey(d => d.IdReportProfileUser);
			this.HasRequired(t => t.Budget).WithMany().HasForeignKey(d => d.IdBudget);
			this.HasRequired(t => t.Version).WithMany().HasForeignKey(d => d.IdVersion);
			this.HasRequired(t => t.UnitDimension).WithMany().HasForeignKey(d => d.IdUnitDimension);
			this.HasMany(t => t.SBPs).WithMany().Map(m => m.MapLeftKey("idAnalysisBAofDirectAndIndirectCost").MapRightKey("idSBP").ToTable("AnalysisBAofDirectAndIndirectCost_SBP", "ml"));
			
        }
    }
}
