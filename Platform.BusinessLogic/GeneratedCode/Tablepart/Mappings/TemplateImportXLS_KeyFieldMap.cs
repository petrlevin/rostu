using System.Data.Entity.ModelConfiguration;
using System.ComponentModel.DataAnnotations.Schema;

namespace Platform.BusinessLogic.Tablepart.Mappings
{
    /// <summary>
    /// Класс описывающий маппинг для сущности TemplateImportXLS_KeyField
    /// </summary>
	public class TemplateImportXLS_KeyFieldMap : EntityTypeConfiguration<TemplateImportXLS_KeyField>
    {
        /// <summary>
        /// Дефолтный конструктор
        /// </summary>
		public TemplateImportXLS_KeyFieldMap()
        {
            // Primary Key
            this.HasKey(t => t.Id);

            // Properties
            // Table & Column Mappings
            this.ToTable("TemplateImportXLS_KeyField", "tp");
			this.Property(t => t.Id).HasColumnName("id");
			this.Property(t => t.IdOwner).HasColumnName("idOwner");
			this.Property(t => t.IdEntityField).HasColumnName("idEntityField");
			
            // Relationships
			this.HasRequired(t => t.Owner).WithMany(t => t.KeyField).HasForeignKey(d => d.IdOwner);
			this.HasRequired(t => t.EntityField).WithMany().HasForeignKey(d => d.IdEntityField);
			
        }
    }
}
