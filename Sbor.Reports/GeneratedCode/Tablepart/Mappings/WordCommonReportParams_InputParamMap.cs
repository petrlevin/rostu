using System.Data.Entity.ModelConfiguration;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sbor.Reports.Tablepart.Mappings
{
    /// <summary>
    /// Класс описывающий маппинг для сущности WordCommonReportParams_InputParam
    /// </summary>
	public class WordCommonReportParams_InputParamMap : EntityTypeConfiguration<WordCommonReportParams_InputParam>
    {
        /// <summary>
        /// Дефолтный конструктор
        /// </summary>
		public WordCommonReportParams_InputParamMap()
        {
            // Primary Key
            this.HasKey(t => t.Id);

            // Properties
            // Table & Column Mappings
            this.ToTable("WordCommonReportParams_InputParam", "tp");
			this.Property(t => t.Id).HasColumnName("id");
			this.Property(t => t.IdOwner).HasColumnName("idOwner");
			this.Property(t => t.Caption).HasColumnName("caption");
			this.Property(t => t.Description).HasColumnName("description");
			this.Property(t => t.DefaultValue).HasColumnName("defaultValue");
			
            // Relationships
			this.HasRequired(t => t.Owner).WithMany(t => t.InputParams).HasForeignKey(d => d.IdOwner);
			
        }
    }
}
