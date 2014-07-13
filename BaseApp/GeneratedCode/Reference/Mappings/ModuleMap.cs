using System.Data.Entity.ModelConfiguration;
using System.ComponentModel.DataAnnotations.Schema;

namespace BaseApp.Reference.Mappings
{
    /// <summary>
    /// Класс описывающий маппинг для сущности Module
    /// </summary>
	public class ModuleMap : EntityTypeConfiguration<Module>
    {
        /// <summary>
        /// Дефолтный конструктор
        /// </summary>
		public ModuleMap()
        {
            // Primary Key
            this.HasKey(t => t.Id);

            // Properties
            // Table & Column Mappings
            this.ToTable("Module", "ref");
			this.Property(t => t.Id).HasColumnName("id");
			this.Property(t => t.Caption).HasColumnName("Caption");
			this.Property(t => t.Code).HasColumnName("Code");
			this.Property(t => t.On).HasColumnName("On");
			
            // Relationships
			this.HasMany(t => t.Entity).WithMany().Map(m => m.MapLeftKey("idModule").MapRightKey("idEntity").ToTable("Module_Entity", "ml"));
			
        }
    }
}
