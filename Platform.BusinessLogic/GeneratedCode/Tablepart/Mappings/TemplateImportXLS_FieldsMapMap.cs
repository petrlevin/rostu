using System.Data.Entity.ModelConfiguration;
using System.ComponentModel.DataAnnotations.Schema;

namespace Platform.BusinessLogic.Tablepart.Mappings
{
    /// <summary>
    /// Класс описывающий маппинг для сущности TemplateImportXLS_FieldsMap
    /// </summary>
	public class TemplateImportXLS_FieldsMapMap : EntityTypeConfiguration<TemplateImportXLS_FieldsMap>
    {
        /// <summary>
        /// Дефолтный конструктор
        /// </summary>
		public TemplateImportXLS_FieldsMapMap()
        {
            // Primary Key
            this.HasKey(t => t.Id);

            // Properties
            // Table & Column Mappings
            this.ToTable("TemplateImportXLS_FieldsMap", "tp");
			this.Property(t => t.Id).HasColumnName("id");
			this.Property(t => t.IdOwner).HasColumnName("idOwner");
			this.Property(t => t.IdEntityField).HasColumnName("idEntityField");
			this.Property(t => t.NameColumn).HasColumnName("NameColumn");
			this.Property(t => t.ValueColumn).HasColumnName("ValueColumn");
			this.Property(t => t.MaskFinding).HasColumnName("MaskFinding");
			this.Property(t => t.MaskReplacing).HasColumnName("MaskReplacing");
			
            // Relationships
			this.HasRequired(t => t.Owner).WithMany(t => t.FieldsMap).HasForeignKey(d => d.IdOwner);
			this.HasRequired(t => t.EntityField).WithMany().HasForeignKey(d => d.IdEntityField);
			
        }
    }
}
