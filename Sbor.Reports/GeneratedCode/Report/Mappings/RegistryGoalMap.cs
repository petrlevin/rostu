using System.Data.Entity.ModelConfiguration;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sbor.Reports.Report.Mappings
{
    /// <summary>
    /// Класс описывающий маппинг для сущности RegistryGoal
    /// </summary>
	public class RegistryGoalMap : EntityTypeConfiguration<RegistryGoal>
    {
        /// <summary>
        /// Дефолтный конструктор
        /// </summary>
		public RegistryGoalMap()
        {
            // Primary Key
            this.HasKey(t => t.Id);

            // Properties
            // Table & Column Mappings
            this.ToTable("RegistryGoal", "rep");
			this.Property(t => t.ReportProfileCaption).HasColumnName("ReportProfileCaption");
			this.Property(t => t.IdReportProfileType).HasColumnName("idReportProfileType");
			this.Property(t => t.IdReportProfileUser).HasColumnName("idReportProfileUser");
			this.Property(t => t.Id).HasColumnName("id");
			this.Property(t => t.IsTemporary).HasColumnName("isTemporary");
			this.Property(t => t.IdSBP).HasColumnName("idSBP");
			this.Property(t => t.DateStart).HasColumnName("DateStart");
			this.Property(t => t.DateEnd).HasColumnName("DateEnd");
			this.Property(t => t.OutputGoalOperatingPeriod).HasColumnName("OutputGoalOperatingPeriod");
			this.Property(t => t.IdPublicLegalFormation).HasColumnName("idPublicLegalFormation");
			this.Property(t => t.IdSourcesDataReports).HasColumnName("idSourcesDataReports");
			this.Property(t => t.IdVersion).HasColumnName("idVersion");
			this.Property(t => t.ConstructReportApprovedData).HasColumnName("ConstructReportApprovedData");
			this.Property(t => t.DateReport).HasColumnName("DateReport");
			this.Property(t => t.DisplayReportCodeS).HasColumnName("DisplayReportCodeS");
			this.Property(t => t.DisplayReportDataGoal).HasColumnName("DisplayReportDataGoal");
			this.Property(t => t.DispleySelectedParameterValues).HasColumnName("DispleySelectedParameterValues");
			this.Property(t => t.RepeatTableHeader).HasColumnName("RepeatTableHeader");
			this.Property(t => t.GenerateValuesWithDetails).HasColumnName("GenerateValuesWithDetails");
			this.Property(t => t.IdBudget).HasColumnName("idBudget");
			this.Property(t => t.DisplayResourceProvision).HasColumnName("DisplayResourceProvision");
			this.Property(t => t.DisplayResourceSupport).HasColumnName("DisplayResourceSupport");
			this.Property(t => t.DisplayDataBudgetPeriod).HasColumnName("DisplayDataBudgetPeriod");
			
            // Relationships
			this.HasOptional(t => t.ReportProfileUser).WithMany().HasForeignKey(d => d.IdReportProfileUser);
			this.HasOptional(t => t.SBP).WithMany().HasForeignKey(d => d.IdSBP);
			this.HasRequired(t => t.PublicLegalFormation).WithMany().HasForeignKey(d => d.IdPublicLegalFormation);
			this.HasRequired(t => t.Version).WithMany().HasForeignKey(d => d.IdVersion);
			this.HasOptional(t => t.Budget).WithMany().HasForeignKey(d => d.IdBudget);
			this.HasMany(t => t.ElementTypeSystemGoal).WithMany().Map(m => m.MapLeftKey("idRegistryGoal").MapRightKey("idElementTypeSystemGoal").ToTable("RegistryGoal_ElementTypeSystemGoal", "ml"));
			
        }
    }
}
