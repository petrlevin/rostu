using System.Data.Entity.ModelConfiguration;
using System.ComponentModel.DataAnnotations.Schema;

namespace BaseApp.Reference.Mappings
{
    /// <summary>
    /// Класс описывающий маппинг для сущности ProjectSettings
    /// </summary>
	public class ProjectSettingsMap : EntityTypeConfiguration<ProjectSettings>
    {
        /// <summary>
        /// Дефолтный конструктор
        /// </summary>
		public ProjectSettingsMap()
        {
            // Primary Key
            this.HasKey(t => t.Id);

            // Properties
            // Table & Column Mappings
            this.ToTable("ProjectSettings", "ref");
			this.Property(t => t.Id).HasColumnName("id");
			this.Property(t => t.Code).HasColumnName("Code");
			this.Property(t => t.Caption).HasColumnName("Caption");
			
            // Relationships
			
        }
    }
}
