using System.Data.Entity.ModelConfiguration;
using System.ComponentModel.DataAnnotations.Schema;

namespace Platform.BusinessLogic.Reference.Mappings
{
    /// <summary>
    /// Класс описывающий маппинг для сущности TemplateImport
    /// </summary>
	public class TemplateImportMap : EntityTypeConfiguration<TemplateImport>
    {
        /// <summary>
        /// Дефолтный конструктор
        /// </summary>
		public TemplateImportMap()
        {
            // Primary Key
            this.HasKey(t => t.Id);

            // Properties
            // Table & Column Mappings
            this.ToTable("TemplateImport", "ref");
			this.Property(t => t.Id).HasColumnName("id");
			this.Property(t => t.Caption).HasColumnName("Caption");
			
            // Relationships
			
        }
    }
}
