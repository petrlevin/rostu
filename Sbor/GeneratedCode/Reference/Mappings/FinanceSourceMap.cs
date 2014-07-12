using System.Data.Entity.ModelConfiguration;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sbor.Reference.Mappings
{
    /// <summary>
    /// Класс описывающий маппинг для сущности FinanceSource
    /// </summary>
	public class FinanceSourceMap : EntityTypeConfiguration<FinanceSource>
    {
        /// <summary>
        /// Дефолтный конструктор
        /// </summary>
		public FinanceSourceMap()
        {
            // Primary Key
            this.HasKey(t => t.Id);

            // Properties
            // Table & Column Mappings
            this.ToTable("FinanceSource", "ref");
			this.Property(t => t.Id).HasColumnName("id");
			this.Property(t => t.Code).HasColumnName("Code");
			this.Property(t => t.Caption).HasColumnName("Caption");
			this.Property(t => t.IdFinanceSourceType).HasColumnName("idFinanceSourceType");
			this.Property(t => t.IdRefStatus).HasColumnName("idRefStatus");
			
            // Relationships
			
        }
    }
}
