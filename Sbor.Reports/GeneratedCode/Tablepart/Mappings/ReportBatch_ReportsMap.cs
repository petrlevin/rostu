using System.Data.Entity.ModelConfiguration;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sbor.Reports.Tablepart.Mappings
{
    /// <summary>
    /// Класс описывающий маппинг для сущности ReportBatch_Reports
    /// </summary>
	public class ReportBatch_ReportsMap : EntityTypeConfiguration<ReportBatch_Reports>
    {
        /// <summary>
        /// Дефолтный конструктор
        /// </summary>
		public ReportBatch_ReportsMap()
        {
            // Primary Key
            this.HasKey(t => t.Id);

            // Properties
            // Table & Column Mappings
            this.ToTable("ReportBatch_Reports", "tp");
			this.Property(t => t.Id).HasColumnName("id");
			this.Property(t => t.IdOwner).HasColumnName("idOwner");
			this.Property(t => t.Number).HasColumnName("number");
			
            // Relationships
			this.HasRequired(t => t.Owner).WithMany(t => t.Reports).HasForeignKey(d => d.IdOwner);
			
        }
    }
}
