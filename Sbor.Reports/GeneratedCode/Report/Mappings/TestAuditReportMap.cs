using System.Data.Entity.ModelConfiguration;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sbor.Reports.Report.Mappings
{
    /// <summary>
    /// Класс описывающий маппинг для сущности TestAuditReport
    /// </summary>
	public class TestAuditReportMap : EntityTypeConfiguration<TestAuditReport>
    {
        /// <summary>
        /// Дефолтный конструктор
        /// </summary>
		public TestAuditReportMap()
        {
            // Primary Key
            this.HasKey(t => t.Id);

            // Properties
            // Table & Column Mappings
            this.ToTable("TestAuditReport", "rep");
			this.Property(t => t.Id).HasColumnName("id");
			this.Property(t => t.IsTemporary).HasColumnName("isTemporary");
			this.Property(t => t.DateFrom).HasColumnName("DateFrom");
			this.Property(t => t.DateTo).HasColumnName("DateTo");
			this.Property(t => t.IdUser).HasColumnName("idUser");
			this.Property(t => t.ReportProfileCaption).HasColumnName("ReportProfileCaption");
			this.Property(t => t.IdReportProfileType).HasColumnName("idReportProfileType");
			this.Property(t => t.IdReportProfileUser).HasColumnName("idReportProfileUser");
			
            // Relationships
			this.HasOptional(t => t.User).WithMany().HasForeignKey(d => d.IdUser);
			this.HasOptional(t => t.ReportProfileUser).WithMany().HasForeignKey(d => d.IdReportProfileUser);
			
        }
    }
}
