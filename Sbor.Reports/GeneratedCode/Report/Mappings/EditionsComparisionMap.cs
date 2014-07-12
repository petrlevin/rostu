using System.Data.Entity.ModelConfiguration;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sbor.Reports.Report.Mappings
{
    /// <summary>
    /// Класс описывающий маппинг для сущности EditionsComparision
    /// </summary>
	public class EditionsComparisionMap : EntityTypeConfiguration<EditionsComparision>
    {
        /// <summary>
        /// Дефолтный конструктор
        /// </summary>
		public EditionsComparisionMap()
        {
            // Primary Key
            this.HasKey(t => t.Id);

            // Properties
            // Table & Column Mappings
            this.ToTable("EditionsComparision", "rep");
			this.Property(t => t.Id).HasColumnName("id");
			this.Property(t => t.IsTemporary).HasColumnName("isTemporary");
			this.Property(t => t.IdEditionA).HasColumnName("idEditionA");
			this.Property(t => t.IdEditionAEntity).HasColumnName("idEditionAEntity");
			this.Property(t => t.IdEditionB).HasColumnName("idEditionB");
			this.Property(t => t.IdEditionBEntity).HasColumnName("idEditionBEntity");
			this.Property(t => t.ReportProfileCaption).HasColumnName("ReportProfileCaption");
			this.Property(t => t.IdReportProfileType).HasColumnName("idReportProfileType");
			this.Property(t => t.IdReportProfileUser).HasColumnName("idReportProfileUser");
			
            // Relationships
			this.HasRequired(t => t.EditionAEntity).WithMany().HasForeignKey(d => d.IdEditionAEntity);
			this.HasRequired(t => t.EditionBEntity).WithMany().HasForeignKey(d => d.IdEditionBEntity);
			this.HasOptional(t => t.ReportProfileUser).WithMany().HasForeignKey(d => d.IdReportProfileUser);
			
        }
    }
}
