using System.Data.Entity.ModelConfiguration;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sbor.Reports.Report.Mappings
{
    /// <summary>
    /// Класс описывающий маппинг для сущности WordCommonReport
    /// </summary>
	public class WordCommonReportMap : EntityTypeConfiguration<WordCommonReport>
    {
        /// <summary>
        /// Дефолтный конструктор
        /// </summary>
		public WordCommonReportMap()
        {
            // Primary Key
            this.HasKey(t => t.Id);

            // Properties
            // Table & Column Mappings
            this.ToTable("WordCommonReport", "rep");
			this.Property(t => t.Id).HasColumnName("id");
			this.Property(t => t.ReportProfileCaption).HasColumnName("ReportProfileCaption");
			this.Property(t => t.IdReportProfileType).HasColumnName("idReportProfileType");
			this.Property(t => t.IdReportProfileUser).HasColumnName("idReportProfileUser");
			this.Property(t => t.IsTemporary).HasColumnName("isTemporary");
			this.Property(t => t.IdTemplateFile).HasColumnName("idTemplateFile");
			
            // Relationships
			this.HasOptional(t => t.ReportProfileUser).WithMany().HasForeignKey(d => d.IdReportProfileUser);
			
        }
    }
}
