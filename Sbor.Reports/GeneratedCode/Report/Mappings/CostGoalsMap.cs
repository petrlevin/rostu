using System.Data.Entity.ModelConfiguration;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sbor.Reports.Report.Mappings
{
    /// <summary>
    /// Класс описывающий маппинг для сущности CostGoals
    /// </summary>
	public class CostGoalsMap : EntityTypeConfiguration<CostGoals>
    {
        /// <summary>
        /// Дефолтный конструктор
        /// </summary>
		public CostGoalsMap()
        {
            // Primary Key
            this.HasKey(t => t.Id);

            // Properties
            // Table & Column Mappings
            this.ToTable("CostGoals", "rep");
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
			
            // Relationships
			this.HasOptional(t => t.ReportProfileUser).WithMany().HasForeignKey(d => d.IdReportProfileUser);
			this.HasRequired(t => t.PublicLegalFormation).WithMany().HasForeignKey(d => d.IdPublicLegalFormation);
			this.HasRequired(t => t.Budget).WithMany().HasForeignKey(d => d.IdBudget);
			this.HasRequired(t => t.Version).WithMany().HasForeignKey(d => d.IdVersion);
			
        }
    }
}
