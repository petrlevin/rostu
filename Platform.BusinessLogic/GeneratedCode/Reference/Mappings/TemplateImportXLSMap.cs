using System.Data.Entity.ModelConfiguration;
using System.ComponentModel.DataAnnotations.Schema;

namespace Platform.BusinessLogic.Reference.Mappings
{
    /// <summary>
    /// Класс описывающий маппинг для сущности TemplateImportXLS
    /// </summary>
	public class TemplateImportXLSMap : EntityTypeConfiguration<TemplateImportXLS>
    {
        /// <summary>
        /// Дефолтный конструктор
        /// </summary>
		public TemplateImportXLSMap()
        {
            // Primary Key
            this.HasKey(t => t.Id);

            // Properties
            // Table & Column Mappings
            this.ToTable("TemplateImportXLS", "ref");
			this.Property(t => t.IdOwner).HasColumnName("idOwner");
			this.Property(t => t.Id).HasColumnName("id");
			this.Property(t => t.Caption).HasColumnName("Caption");
			this.Property(t => t.IdEntity).HasColumnName("idEntity");
			this.Property(t => t.IdImportType).HasColumnName("idImportType");
			this.Property(t => t.IsPerformSingleTransaction).HasColumnName("IsPerformSingleTransaction");
			this.Property(t => t.IsIgnoreSoftControl).HasColumnName("IsIgnoreSoftControl");
			this.Property(t => t.Description).HasColumnName("Description");
			this.Property(t => t.IdAccessGroup).HasColumnName("idAccessGroup");
			this.Property(t => t.IdExecImportMode).HasColumnName("idExecImportMode");
			this.Property(t => t.IdRefStatus).HasColumnName("idRefStatus");
			
            // Relationships
			this.HasOptional(t => t.Owner).WithMany(t => t.TemplatesByEntity).HasForeignKey(d => d.IdOwner);
			this.HasRequired(t => t.Entity).WithMany().HasForeignKey(d => d.IdEntity);
			this.HasOptional(t => t.AccessGroup).WithMany().HasForeignKey(d => d.IdAccessGroup);
			
        }
    }
}
