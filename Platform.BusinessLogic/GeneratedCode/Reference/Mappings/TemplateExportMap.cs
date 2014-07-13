using System.Data.Entity.ModelConfiguration;
using System.ComponentModel.DataAnnotations.Schema;

namespace Platform.BusinessLogic.Reference.Mappings
{
    /// <summary>
    /// Класс описывающий маппинг для сущности TemplateExport
    /// </summary>
	public class TemplateExportMap : EntityTypeConfiguration<TemplateExport>
    {
        /// <summary>
        /// Дефолтный конструктор
        /// </summary>
		public TemplateExportMap()
        {
            // Primary Key
            this.HasKey(t => t.Id);

            // Properties
            // Table & Column Mappings
            this.ToTable("TemplateExport", "ref");
			this.Property(t => t.Id).HasColumnName("id");
			this.Property(t => t.Caption).HasColumnName("Caption");
			this.Property(t => t.IdSelectionType).HasColumnName("idSelectionType");
			this.Property(t => t.EntitiesSql).HasColumnName("EntitiesSql");
			this.Property(t => t.LinkedEntitiesSql).HasColumnName("LinkedEntitiesSql");
			this.Property(t => t.IdLinkedSelectionType).HasColumnName("idLinkedSelectionType");
			
            // Relationships
			this.HasMany(t => t.LinkedEntities).WithMany().Map(m => m.MapLeftKey("idTemplateExport").MapRightKey("idEntity").ToTable("TemplateExport_LinkedEntity", "ml"));
			
        }
    }
}
