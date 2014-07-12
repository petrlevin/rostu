using System.Data.Entity.ModelConfiguration;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sbor.Reports.Tool.Mappings
{
    /// <summary>
    /// Класс описывающий маппинг для сущности ReportBatch
    /// </summary>
	public class ReportBatchMap : EntityTypeConfiguration<ReportBatch>
    {
        /// <summary>
        /// Дефолтный конструктор
        /// </summary>
		public ReportBatchMap()
        {
            // Primary Key
            this.HasKey(t => t.Id);

            // Properties
            // Table & Column Mappings
            this.ToTable("ReportBatch", "tool");
			this.Property(t => t.Id).HasColumnName("id");
			this.Property(t => t.IdPublicLegalFormation).HasColumnName("idPublicLegalFormation");
			this.Property(t => t.Caption).HasColumnName("caption");
			this.Property(t => t.Description).HasColumnName("description");
			
            // Relationships
			this.HasRequired(t => t.PublicLegalFormation).WithMany().HasForeignKey(d => d.IdPublicLegalFormation);
			
        }
    }
}
