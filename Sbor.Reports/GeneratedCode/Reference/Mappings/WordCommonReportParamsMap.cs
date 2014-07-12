using System.Data.Entity.ModelConfiguration;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sbor.Reports.Reference.Mappings
{
    /// <summary>
    /// Класс описывающий маппинг для сущности WordCommonReportParams
    /// </summary>
	public class WordCommonReportParamsMap : EntityTypeConfiguration<WordCommonReportParams>
    {
        /// <summary>
        /// Дефолтный конструктор
        /// </summary>
		public WordCommonReportParamsMap()
        {
            // Primary Key
            this.HasKey(t => t.Id);

            // Properties
            // Table & Column Mappings
            this.ToTable("WordCommonReportParams", "ref");
			this.Property(t => t.Id).HasColumnName("id");
			this.Property(t => t.Caption).HasColumnName("caption");
			this.Property(t => t.Description).HasColumnName("description");
			this.Property(t => t.IdOutputEntity).HasColumnName("idOutputEntity");
			this.Property(t => t.IdOutputEntityField).HasColumnName("idOutputEntityField");
			this.Property(t => t.SqlQuery).HasColumnName("sqlQuery");
			
            // Relationships
			this.HasOptional(t => t.OutputEntity).WithMany().HasForeignKey(d => d.IdOutputEntity);
			this.HasOptional(t => t.OutputEntityField).WithMany().HasForeignKey(d => d.IdOutputEntityField);
			
        }
    }
}
