using System.Data.Entity.ModelConfiguration;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sbor.Reports.Report.Mappings
{
    /// <summary>
    /// Класс описывающий маппинг для сущности Report1
    /// </summary>
	public class Report1Map : EntityTypeConfiguration<Report1>
    {
        /// <summary>
        /// Дефолтный конструктор
        /// </summary>
		public Report1Map()
        {
            // Primary Key
            this.HasKey(t => t.Id);

            // Properties
            // Table & Column Mappings
            this.ToTable("Report1", "rep");
			this.Property(t => t.Id).HasColumnName("id");
			this.Property(t => t.IsTemporary).HasColumnName("isTemporary");
			this.Property(t => t.Text).HasColumnName("Text");
			this.Property(t => t.SBP).HasColumnName("SBP");
			this.Property(t => t.Boolean).HasColumnName("Boolean");
			this.Property(t => t.Date).HasColumnName("Date");
			this.Property(t => t.ReportProfileCaption).HasColumnName("ReportProfileCaption");
			this.Property(t => t.IdReportProfileType).HasColumnName("idReportProfileType");
			this.Property(t => t.IdReportProfileUser).HasColumnName("idReportProfileUser");
			
            // Relationships
			this.HasRequired(t => t.P).WithMany().HasForeignKey(d => d.SBP);
			this.HasOptional(t => t.ReportProfileUser).WithMany().HasForeignKey(d => d.IdReportProfileUser);
			this.HasMany(t => t.FinanceSource).WithMany().Map(m => m.MapLeftKey("idReport1").MapRightKey("idFinanceSource").ToTable("Report1_FinanceSource", "ml"));
			
        }
    }
}
