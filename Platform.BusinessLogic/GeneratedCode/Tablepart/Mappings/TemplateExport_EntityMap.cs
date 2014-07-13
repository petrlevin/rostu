using System.Data.Entity.ModelConfiguration;
using System.ComponentModel.DataAnnotations.Schema;

namespace Platform.BusinessLogic.Tablepart.Mappings
{
    /// <summary>
    /// Класс описывающий маппинг для сущности TemplateExport_Entity
    /// </summary>
	public class TemplateExport_EntityMap : EntityTypeConfiguration<TemplateExport_Entity>
    {
        /// <summary>
        /// Дефолтный конструктор
        /// </summary>
		public TemplateExport_EntityMap()
        {
            // Primary Key
            this.HasKey(t => t.Id);

            // Properties
            // Table & Column Mappings
            this.ToTable("TemplateExport_Entity", "tp");
			this.Property(t => t.Id).HasColumnName("id");
			this.Property(t => t.IdOwner).HasColumnName("idOwner");
			this.Property(t => t.IdEntity).HasColumnName("idEntity");
			this.Property(t => t.Sql).HasColumnName("Sql");
			
            // Relationships
			this.HasRequired(t => t.Owner).WithMany(t => t.Entities).HasForeignKey(d => d.IdOwner);
			this.HasRequired(t => t.Entity).WithMany().HasForeignKey(d => d.IdEntity);
			
        }
    }
}
